using ParseService.Models.Response;
using ParseService.Models;
using ParseService.Options;
using ParseService.Services.MessangerService;
using System.Text.Json;
using ParseService.Repository.Parse;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Caching.Memory;

namespace ParseService.Services.NewsService
{
    public class NewsService(IHttpClientFactory httpClientFactory,
        IOptions<MainOptions> mainOptions,
        ILogger<NewsService> logger,
        IServiceScopeFactory serviceScopeFactory,
        IMessangerService messangerService,
        IMemoryCache memoryCache
            ) : INewsService
    {
        private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
        private readonly MainOptions _mainOptions = mainOptions.Value;
        private readonly ILogger<NewsService> _logger = logger;
        private readonly IMessangerService _messangerService = messangerService;
        private IParseRepository _parseRepository;
        private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;
        private readonly IMemoryCache _memoryCache = memoryCache;


        public async Task FetchLatestAnnouncements()
        {
            try
            {
                using var scope = _serviceScopeFactory.CreateScope();

                _parseRepository = scope.ServiceProvider.GetRequiredService<IParseRepository>();
                var client = _httpClientFactory.CreateClient();
                var url = _mainOptions.MAIN_URL;
                var response = await client.GetAsync(url);


                await JsonParser(response);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка при запросе к API:Значение конфигурации:{_mainOptions.MAIN_URL}{DateTime.UtcNow} {ex.Message}");
            }
        }

        

        private async Task JsonParser(HttpResponseMessage response)
        {
            string cacheKey = "announceMent";
            IEnumerable<AnnouncementItemResponse> announcementsDbEntity;

            // Проверяем, есть ли данные в кеше
            if (!_memoryCache.TryGetValue(cacheKey, out IEnumerable<AnnouncementItemResponse> announcementsData))
            {
                // Если данных нет в кеше, получаем из базы данных или другого источника
                announcementsData = await _parseRepository.GetAnnouncements(cancellationToken: default);

                // Если данные успешно получены, сохраняем их в кеш
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(2))  // Данные будут жить в кеше 2 минуты
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(2));  // Данные будут храниться в кеше 2 минуты

                _memoryCache.Set(cacheKey, announcementsData, cacheEntryOptions);
                _logger.LogInformation("Данные записаны в кэш");
            }
           
                 announcementsDbEntity = announcementsData;
            
         

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
                        foreach (var item in announcements.Data.Items)
                        {
                            await _parseRepository.AddAnnouncements(item, cancellationToken: default);
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
            }
            catch (Exception exception)
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
                catch (Exception exception)
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
}
