using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using ParseService.Models;
using ParseService.Options;
using ParseService.Services;

public class NewsBackgroundService : BackgroundService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<NewsBackgroundService> _logger;
    private readonly MainOptions _mainOptions;
    private readonly IMessangerService _messangerService;
    private readonly IMemoryCache _memoryCache;
    public NewsBackgroundService(IHttpClientFactory httpClientFactory,
                                ILogger<NewsBackgroundService> logger,
                                IOptions<MainOptions> options,
                                IMessangerService messangerService,
                                IMemoryCache memoryCache)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _mainOptions = options.Value;
        _messangerService = messangerService;
        _memoryCache = memoryCache;
      
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await FetchLatestAnnouncements();
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken); 
        }
    }

    private async Task FetchLatestAnnouncements()
    {


        var client = _httpClientFactory.CreateClient();
        var url = _mainOptions.MainUrl;

        try
        {
            var response = await client.GetAsync(url);
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

                  
                   

                    await ProcessAnnouncements(announcements);
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
        catch (Exception ex)
        {
            _logger.LogError($"Ошибка при запросе к API: {ex.Message}");
        }
    }

    private async Task ProcessAnnouncements(AnnouncementsResponse announcements)
    {
        foreach (var item in announcements.Data.Items)
        {
            await _messangerService.SendToTelegram(item);
            _logger.LogInformation($"Заголовок: {item.AnnTitle}\nСсылка: {item.AnnUrl}\nОписание: {item.AnnDesc}");
        }
    }

}
