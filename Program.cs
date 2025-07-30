using Microsoft.EntityFrameworkCore;
using NotifierDeliveryWorker.DeliveryWorker.Infrastructure;
using NotifierNotificationService.NotificationService.Application.Services;
using NotifierNotificationService.NotificationService.Domain.Interfaces;
using NotifierNotificationService.NotificationService.Domain.Interfaces.Repositories;
using NotifierNotificationService.NotificationService.Domain.Interfaces.Services;
using NotifierNotificationService.NotificationService.Infrastructure;
using NotifierNotificationService.NotificationService.Infrastructure.Repositories;
using StackExchange.Redis;
using System.Text.Json.Serialization;


namespace NotifierNotificationService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
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

            app.Urls.Add("http://*:6121");

            // Configure the HTTP request pipeline.
            if (Boolean.TryParse(builder.Configuration["UseSwagger"], 
                out bool useSwagger) && useSwagger)
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
