using ParseService.Options;
using ParseService.Services;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context , services) =>
    {
        services.AddHttpClient();
        services.AddHostedService<NewsBackgroundService>();
        services.AddSingleton<IMessangerService, TelegramService>();
        services.Configure<MainOptions>(context.Configuration.GetSection("MainOptions"));
        services.AddMemoryCache(); 
    })
    .ConfigureLogging(logging =>
    {
        logging.ClearProviders();
        logging.AddConsole();
    })
    .Build();

await host.RunAsync();