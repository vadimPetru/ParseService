using ParseService.Services.NewsService;

public class NewsBackgroundService : BackgroundService
{
    private readonly ILogger<NewsBackgroundService> _logger;
    private readonly INewsService _newsService;
    public NewsBackgroundService(ILogger<NewsBackgroundService> logger,
                                INewsService newsService)
    {
        _logger = logger;
        _newsService = newsService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
     
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {

                await _newsService.FetchLatestAnnouncements();
                await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);
            }catch(Exception ex)
            {
                _logger.LogError($"Ошибка рабты сервиса {DateTime.UtcNow} - {ex.Message}");
            }    
        }
    }
}
