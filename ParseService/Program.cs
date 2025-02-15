using Microsoft.EntityFrameworkCore;
using ParseService.Data;
using ParseService.Options;
using ParseService.Repository.Parse;
using ParseService.Services.MessangerService;
using ParseService.Services.NewsService;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((context, config) =>
    {
        // Добавляем конфигурацию из файла secret.json
        var env = context.HostingEnvironment;
        config.SetBasePath(env.ContentRootPath)
              .AddJsonFile("secret.json", optional: true, reloadOnChange: true); // Добавляем файл secret.json
    })
    .ConfigureServices((context , services) =>
    {
        // Регистрируем HttpClient, фоновой сервис, мессенджер и другие сервисы
        services.AddHttpClient();
        services.AddHostedService<NewsBackgroundService>();
        services.AddSingleton<IMessangerService, TelegramService>();
        services.AddSingleton<INewsService, NewsService>();
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