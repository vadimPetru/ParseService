using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using ParseService.Models;
using ParseService.Models.Response;
using ParseService.Options;
using ParseService.Repository;
using ParseService.Services;
using PuppeteerSharp;

public class NewsBackgroundService : BackgroundService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<NewsBackgroundService> _logger;
    private readonly MainOptions _mainOptions;
    private readonly IMessangerService _messangerService;
    private readonly IMemoryCache _memoryCache;
    private readonly IServiceScopeFactory _scopeFactory;
    private IParseRepository _parseRepository;
    public NewsBackgroundService(IHttpClientFactory httpClientFactory,
                                ILogger<NewsBackgroundService> logger,
                                IOptions<MainOptions> options,
                                IMessangerService messangerService,
                                IMemoryCache memoryCache,
                                IServiceScopeFactory scopeFactory)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _mainOptions = options.Value;
        _messangerService = messangerService;
        _memoryCache = memoryCache;
        _scopeFactory = scopeFactory;
      
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
     
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                _parseRepository = scope.ServiceProvider.GetRequiredService<IParseRepository>();
                await FetchLatestAnnouncements();
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }catch(Exception ex)
            {
                _logger.LogError($"Ошибка рабты сервиса {DateTime.UtcNow}");
            }    
        }
    }

    private async Task FetchLatestAnnouncements()
    {
        try
        {
            var client = _httpClientFactory.CreateClient();
            var url = _mainOptions.MainUrl;
            var response = await client.GetAsync(url);


            await JsonParser(response);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Ошибка при запросе к API: {ex.Message}");
        }
    }

    private async Task JsonParser(HttpResponseMessage response)
    {
        var announcementsDbEntity = await _parseRepository.GetAnnouncements(cancellationToken: default);

        if (response.IsSuccessStatusCode)
        {
            var jsonResponse = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var announcements = JsonSerializer.Deserialize<AnnouncementsResponse>(jsonResponse, options);

            if (announcements != null && announcements.Data != null)
            {
                _logger.LogInformation($"Получено {announcements.Data.TotalNum} новостей.");

                if (announcementsDbEntity.Count() == 0)
                {
                    foreach(var item in announcements.Data.Items)
                    {
                        await _parseRepository.AddAnnouncements(item,cancellationToken:default);
                    }
                    await ProcessAnnouncements(announcements);
                }
                else
                {
                    await PublishNewNews(announcementsDbEntity, announcements);
                }
            }
            else
            {
                _logger.LogWarning("Не удалось получить новости.");
            }
        }
        else
        {
            _logger.LogError($"Ошибка API: {response.StatusCode}");
        }
    }

    private async Task PublishNewNews(IEnumerable<AnnouncementItemResponse> announcementsDbEntity, AnnouncementsResponse? announcements)
    {
        try
        {
            // Работаем с новыми объявлениями
            var latestAnnouncements = announcements.Data.Items
                .Where(item => !announcementsDbEntity.Any(db => db.AnnId == item.AnnId))
                .ToList();

            if (latestAnnouncements.Any())
            {
                foreach (var item in latestAnnouncements)
                {
                    await _parseRepository.AddAnnouncements(item, cancellationToken: default);

                    var responeTelegram = new AnnouncementItemResponse(item.AnnId, item.AnnTitle, item.AnnDesc, item.AnnUrl);

                    await ProcessAnnouncements(responeTelegram);
                }
            }
            else
            {
                _logger.LogInformation("Нет новых новостей.");
            }
        }catch(Exception exception)
        {
            _logger.LogError($"Ошибка при публикации новостей {DateTime.UtcNow} - {exception.Message}");
        }
    }

    private async Task ProcessAnnouncements(AnnouncementsResponse announcements)
    {
        foreach (var item in announcements.Data.Items)
        {
            try
            {
                await _messangerService.SendToTelegram(item);
                _logger.LogInformation($"Заголовок: {item.AnnTitle}\nСсылка: {item.AnnUrl}\nОписание: {item.AnnDesc}");
            }
            catch(Exception exception)
            {
                _logger.LogError($"Ошибка при отправке данных в телеграм {DateTime.UtcNow} - {exception.Message}");
            }
           
        }
    }

    private async Task ProcessAnnouncements(AnnouncementItemResponse announcementResponse)
    {
        try
        {
            var annoncement = new AnnouncementItem
            {
                AnnId = announcementResponse.AnnId,
                AnnDesc = announcementResponse.AnnDesc,
                AnnTitle = announcementResponse.AnnTitle,
                AnnUrl = announcementResponse.AnnUrl,
                CTime = DateTime.Now.Microsecond,
                Language = "Ru_ru"
            };
            await _messangerService.SendToTelegram(annoncement);
            _logger.LogInformation($"Заголовок: {announcementResponse.AnnTitle}\nСсылка: {announcementResponse.AnnUrl}\nОписание: {announcementResponse.AnnDesc}");
        }
        catch (Exception exception)
        {
            _logger.LogError($"Ошибка при отправке данных в телеграм {DateTime.UtcNow} - {exception.Message}");
        }
    }

}
