using Microsoft.EntityFrameworkCore;
using NotifierDeliveryWorker.DeliveryWorker.Infrastructure;
using NotifierNotificationService.NotificationService.Application.Services;
using NotifierNotificationService.NotificationService.Domain.Interfaces;
using NotifierNotificationService.NotificationService.Domain.Interfaces.Repositories;
using NotifierNotificationService.NotificationService.Domain.Interfaces.Services;
using NotifierNotificationService.NotificationService.Infrastructure;
using NotifierNotificationService.NotificationService.Infrastructure.Repositories;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Loki;
using StackExchange.Redis;
using System.Text.Json.Serialization;


namespace NotifierNotificationService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            static LoggerConfiguration ConfigureLogger(LoggerConfiguration cfg, 
                string outputTemplate, string filename, 
                LogEventLevel fileMinimumLvl = LogEventLevel.Warning, 
                LogEventLevel consoleMinimumLvl = LogEventLevel.Information)
            {
                cfg.MinimumLevel.Override("Microsoft", LogEventLevel.Fatal) // ������ ����������� ������ �� Microsoft-��������
                    .Enrich.FromLogContext()
                    .WriteTo.Console(
                        outputTemplate: outputTemplate,
                        restrictedToMinimumLevel: consoleMinimumLvl
                    )
                    .WriteTo.File(
                        "logs/notification-startup-log.txt",
                        outputTemplate: outputTemplate,
                        rollingInterval: RollingInterval.Day,
                        restrictedToMinimumLevel: fileMinimumLvl
                    );
                return cfg;
            }

            var tempLoggerOutputTemplate = 
                "[NOTIFICATION.SVC STARTUP LOGGER] {Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}";
            var tempLogFilename = "logs/notification-startup-log.txt";
            // ��������� ������ ��� ����� �� �������� �������
            Log.Logger = ConfigureLogger(new LoggerConfiguration(), tempLoggerOutputTemplate, tempLogFilename)
                .CreateBootstrapLogger(); // ��������� ������

            try
            {
                Log.Information("Starting to build the application...");
                var builder = WebApplication.CreateBuilder(args);

                Log.Information("Loading configurations...");
                // ���������� ��������.
                builder.Configuration
                    .AddEnvironmentVariables()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
                    .AddJsonFile($"appsettings.{Environment.UserName.ToLowerInvariant()}.json", optional: true, reloadOnChange: true)
                    .AddCommandLine(args);
                Log.Information("Configuration loaded.");

                // ����������� ��������� Serilog ������� (�� �������)
                builder.Host.UseSerilog((builderContext, serilogConfig) =>
                {
                    // ������������ �������
                    serilogConfig
                        // ���������������� ������������ �� appsettings (���� ����)
                        .ReadFrom.Configuration(builderContext.Configuration)
                        // ������ ��������� Loki
                        .WriteTo.Loki(new LokiSinkConfigurations()
                        {
                            Url = new Uri("http://loki:3100"),
                            Labels =
                            [
                                new LokiLabel("app", "notifier_notification") ,
                                new LokiLabel("app_full","notifier_full")
                            ]
                        });
                    var loggerOutputTemplate =
                        "[NOTIFICATION.SVC] {Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}";
                    var logFilename = "logs/notification-startup-log.txt";
                    ConfigureLogger(serilogConfig, loggerOutputTemplate, logFilename);
                });

                var connectionString = builder.Configuration.GetConnectionString("Main");

                // Add services to the container.
                builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                });

                // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
                builder.Services.AddEndpointsApiExplorer();
                builder.Services.AddSwaggerGen();
                builder.Services.AddHostedService<RabbitConsumer>();
                builder.Services.AddSingleton<IRabbitPublisher, RabbitPublisher>();
                builder.Services.AddScoped<IUsersRepository, UsersRepository>();
                builder.Services.AddScoped<IUsersService, UsersService>();
                builder.Services.AddScoped<INotificationsRepository, NotificationsRepository>();
                builder.Services.AddScoped<INotificationsService, NotificationsService>();
                builder.Services.AddScoped<INotificationsManager, NotificationsManager>();
                builder.Services.AddScoped<IDeliveryStatusManager, DeliveryStatusManager>();
                builder.Services.AddScoped<IStatusesRedisCache, StatusesRedisCache>();
                builder.Services.AddScoped<IAnalyticsManager, AnalyticsManagerRedis>();
                builder.Services.AddScoped<StatusesRepository>();
                builder.Services.AddScoped<IStatusesRepository>(provider =>
                {
                    // ��������� �� IStatusesRepository ������� � ����� Redis, ������� ���������� 
                    // ������� EF-����������� (repo) ����� DI
                    var cache = provider.GetRequiredService<IStatusesRedisCache>();
                    var repo = provider.GetRequiredService<StatusesRepository>();
                    var logger = provider.GetRequiredService<ILogger<CachedStatusesRepository>>();
                    return new CachedStatusesRepository(cache, repo, logger);
                });
                builder.Services.AddScoped<IStatusesService, StatusesService>();
                builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
                    ConnectionMultiplexer.Connect(builder.Configuration["Redis:ConnectionString"]));
                builder.Services.AddScoped<IDatabase>(provider =>
                    provider.GetRequiredService<IConnectionMultiplexer>().GetDatabase());
                builder.Services.AddDbContext<NotifierContext>(options =>
                    options.UseNpgsql(connectionString)
                        .UseLazyLoadingProxies());

                var app = builder.Build();

                var url = "http://*:6121";
                app.Urls.Add(url);
                Log.Information($"Added {url} to list of app's URLs.");

                // Configure the HTTP request pipeline.
                if (Boolean.TryParse(builder.Configuration["UseSwagger"],
                    out bool useSwagger) && useSwagger)
                {
                    app.UseSwagger();
                    app.UseSwaggerUI();
                    Log.Information("Using OpenAPI SwaggerUI.");
                }

                app.UseAuthorization();

                app.MapControllers();

                Log.Information("Application startup...");
                app.Run();
            }
            catch (Exception ex)
            {
                // � ������ ����� ���������� ��� ������� �������� ��������� ����:
                // 1. ������ � ���� � ������� ����������
                Log.Fatal(ex, "An unexpected Fatal error has occurred in the application.");
                try
                {
                    // 2. ������� ��������� ����������� ������ � Loki
                    using var tempLogger = new LoggerConfiguration()
                        .WriteTo.Loki(new LokiSinkConfigurations()
                        {
                            Url = new Uri("http://loki:3100"),
                            Labels =
                            [
                                new LokiLabel("app_startup", "notifier_notification_startup") ,
                                new LokiLabel("app_full","notifier_full")
                            ]
                        })
                        .CreateLogger();
                    tempLogger.Fatal(ex, "[NOTIFICATION.SVC TEMPORARY LOGGER FATAL] Application startup failed");
                }
                catch (Exception lokiEx)
                {
                    Log.Warning(lokiEx, "Failed to send log to Loki");
                }
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }
}