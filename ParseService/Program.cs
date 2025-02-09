using Microsoft.EntityFrameworkCore;
using ParseService.Data;
using ParseService.Options;
using ParseService.Repository.Parse;
using ParseService.Services.MessangerService;
using ParseService.Services.NewsService;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context , services) =>
    {
        // ������������ HttpClient, ������� ������, ���������� � ������ �������
        services.AddHttpClient();
        services.AddHostedService<NewsBackgroundService>();
        services.AddSingleton<IMessangerService, TelegramService>();
        services.AddSingleton<INewsService, NewsService>();
        services.AddScoped<IParseRepository, ParseRepository>();

        // ������������ ���������������� �����
        services.Configure<MainOptions>(context.Configuration.GetSection("MainOptions"));


        services.AddMemoryCache();

        // ������������ �������� ���� ������ � SQLite.
        // ���� ���� ������ app.db ����� ������ � �������� ������� ����������.
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