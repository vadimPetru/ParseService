using Microsoft.EntityFrameworkCore;
using ParseService.Data;
using ParseService.Options;
using ParseService.Repository;
using ParseService.Services;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context , services) =>
    {
        // Регистрируем HttpClient, фоновой сервис, мессенджер и другие сервисы
        services.AddHttpClient();
        services.AddHostedService<NewsBackgroundService>();
        services.AddSingleton<IMessangerService, TelegramService>();
        services.AddScoped<IParseRepository, ParseRepository>();

        // Регистрируем конфигурационные опции
        services.Configure<MainOptions>(context.Configuration.GetSection("MainOptions"));


        services.AddMemoryCache();

        // Регистрируем контекст базы данных с SQLite.
        // Файл базы данных app.db будет создан в каталоге запуска приложения.
        services.AddDbContext<ParseDbContext>(options =>
           options.UseSqlite(context.Configuration.GetConnectionString("Sqlite")));
    })
    .ConfigureLogging(logging =>
    {
        logging.ClearProviders();
        logging.AddConsole();
    })
    .Build();

await host.RunAsync();