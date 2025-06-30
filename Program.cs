using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using NotifierNotificationService.NotificationService.Domain.Interfaces.Repositories;
using NotifierNotificationService.NotificationService.Infrastructure;
using NotifierNotificationService.NotificationService.Services;
using NotifierNotificationService.NotificationService.Domain.Interfaces.Services;


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
            builder.Services.AddTransient<IUsersRepository, UsersRepository>();
            builder.Services.AddTransient<IUsersService, UsersService>();
            builder.Services.AddTransient<IStatusesRepository, StatusesRepository>();
            builder.Services.AddTransient<IStatusesService, StatusesService>();
            builder.Services.AddTransient<INotificationsRepository, NotificationsRepository>();
            builder.Services.AddTransient<INotificationsService, NotificationsService>();
            builder.Services.AddDbContext<NotifierContext>(options =>
                options.UseNpgsql(connectionString)
                    .UseLazyLoadingProxies());

            var app = builder.Build();

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
