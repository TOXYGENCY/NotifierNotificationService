using Microsoft.AspNetCore.Diagnostics;
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
using System.Text.Json;
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
                cfg.MinimumLevel.Override("Microsoft", LogEventLevel.Error) // Только ошибки из Microsoft-сервисов
                    .Enrich.FromLogContext()
                    .WriteTo.Console(
                        outputTemplate: outputTemplate,
                        restrictedToMinimumLevel: consoleMinimumLvl
                    )
                    .WriteTo.File(
                        filename,
                        outputTemplate: outputTemplate,
                        rollingInterval: RollingInterval.Day,
                        restrictedToMinimumLevel: fileMinimumLvl
                    );
                return cfg;
            }

            // Вывод установленных значений уровней логгирования Serilog
            static void LogCurrentLoggingLevels(IConfiguration serilogConfig)
            {
                var sect = serilogConfig.GetSection("WriteTo");
                var globalLvl = serilogConfig.GetValue<string>("MinimumLevel:Default");
                Log.Information($"Logging level globally is set to {globalLvl}");

                foreach (var sink in sect.GetChildren())
                {
                    var name = sink["Name"];
                    var lvl = sink.GetValue<string>("Args:restrictedToMinimumLevel");
                    if (string.IsNullOrEmpty(lvl)) lvl = $"{globalLvl} (inherited)";

                    Log.Information($"Logging level for sink {name} is set to {lvl}");
                }
            }

            var tempLoggerOutputTemplate =
                "[NOTIFICATION.SVC STARTUP LOGGER] {Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}";
            var tempLogFilename = "logs/notification-startup-log.txt";
            // Временный логгер для этапа до создания билдера
            Log.Logger = ConfigureLogger(new LoggerConfiguration(), tempLoggerOutputTemplate, tempLogFilename)
                .CreateBootstrapLogger(); // временный логгер

            try
            {
                Log.Information("Starting to build the application...");
                var builder = WebApplication.CreateBuilder(args);

                Log.Information("Loading configurations...");
                // Применение конфигов.
                builder.Configuration
                    .AddEnvironmentVariables()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
                    .AddJsonFile($"appsettings.{Environment.UserName.ToLowerInvariant()}.json", optional: true, reloadOnChange: true)
                    .AddCommandLine(args);
                Log.Information("Configuration loaded.");

                // Полноценная настройка Serilog логгера (из конфига)
                builder.Host.UseSerilog((builder, serilogConfig) =>
                {
                    // Конфигурация логгера
                    serilogConfig
                        // Перезаписываение конфигурации из appsettings (если есть)
                        .ReadFrom.Configuration(builder.Configuration)
                        // Ручная настройка Loki
                        .WriteTo.Loki(new LokiSinkConfigurations()
                        {
                            Url = new Uri("http://loki:3100"),
                            Labels =
                            [
                                new LokiLabel("app", "notifier_notification") ,
                                new LokiLabel("app_full","notifier_full")
                            ]
                        });
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
                    // Установка на IStatusesRepository обертку с кешем Redis, который использует 
                    // обычный EF-репозиторий (repo) через DI
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

                // Выводим текущие уровни логгирования
                LogCurrentLoggingLevels(app.Configuration.GetSection("Serilog"));

                app.UseExceptionHandler(errorApp =>
                {
                    errorApp.Run(async ctx =>
                    {
                        var errorFeature = ctx.Features.Get<IExceptionHandlerFeature>();
                        var exception = errorFeature.Error;
                        var logger = ctx.RequestServices.GetRequiredService<ILogger<Program>>();
                        logger.LogError(exception, "Global exception handler caught exception");

                        ctx.Response.StatusCode = StatusCodes.Status500InternalServerError;
                        ctx.Response.ContentType = "application/json";

                        await ctx.Response.WriteAsync(JsonSerializer.Serialize(new
                        {
                            error = "Internal Server Error",
                            message = exception.Message
                        }));
                    });
                });

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
                // В случае краха приложения при запуске пытаемся отправить логи:
                // 1. Запись в файл и консоль контейнера
                Log.Fatal(ex, "An unexpected Fatal error has occurred in the application.");
                try
                {
                    // 2. Попытка отправить критическую ошибку в Loki
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