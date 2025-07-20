using Microsoft.EntityFrameworkCore;
using NotifierDeliveryWorker.DeliveryWorker.Infrastructure;
using NotifierNotificationService.NotificationService.Application.Services;
using NotifierNotificationService.NotificationService.Domain.Interfaces;
using NotifierNotificationService.NotificationService.Domain.Interfaces.Repositories;
using NotifierNotificationService.NotificationService.Domain.Interfaces.Services;
using NotifierNotificationService.NotificationService.Infrastructure;
using NotifierNotificationService.NotificationService.Infrastructure.Repositories;
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
            builder.Services.AddTransient<IUsersRepository, UsersRepository>();
            builder.Services.AddTransient<IUsersService, UsersService>();
            builder.Services.AddTransient<IStatusesRepository, StatusesRepository>();
            builder.Services.AddTransient<IStatusesService, StatusesService>();
            builder.Services.AddTransient<INotificationsRepository, NotificationsRepository>();
            builder.Services.AddTransient<INotificationsService, NotificationsService>();
            builder.Services.AddTransient<INotificationsManager, NotificationsManager>();
            builder.Services.AddTransient<IDeliveryStatusManager, DeliveryStatusManager>();
            builder.Services.AddDbContext<NotifierContext>(options =>
                options.UseNpgsql(connectionString)
                    .UseLazyLoadingProxies());

            var app = builder.Build();

            app.Urls.Add("http://*:6121");

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
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
