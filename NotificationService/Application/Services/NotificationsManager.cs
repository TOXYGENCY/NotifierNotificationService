using NotifierNotificationService.NotificationService.API.Dto;
using NotifierNotificationService.NotificationService.Domain.Interfaces;
using NotifierNotificationService.NotificationService.Domain.Interfaces.Services;
using System.Text.Json;

namespace NotifierNotificationService.NotificationService.Application.Services
{
    public class NotificationsManager : INotificationsManager
    {
        private readonly INotificationsService notificationsService;
        private readonly IStatusesService statusesService;
        private readonly IRabbitPublisher rabbitmq;

        public NotificationsManager(INotificationsService notificationsService, 
            IStatusesService statusesService, IRabbitPublisher rabbitmq)
        {
            this.notificationsService = notificationsService;
            this.statusesService = statusesService;
            this.rabbitmq = rabbitmq;
        }

        public async Task CreateNotificationWithStatusAsync(NotificationDto notificationDto)
        {
            ArgumentNullException.ThrowIfNull(notificationDto);

            var newNotification = await notificationsService.AddNotificationAsync(notificationDto);
            if (newNotification != null)
            {
                var newNotificationDto = notificationsService.ToDto(newNotification);
                await rabbitmq.PublishAsync(newNotificationDto);
                await statusesService.AssignStatusCreatedAsync(newNotification);

                // TODO: позже убрать
                Console.WriteLine($"Sent message into RabbitMQ with: \n {JsonSerializer.Serialize(newNotification)}");
            }
            else
            {
                throw new ArgumentNullException
                    ($"Created notification ({nameof(newNotification)}) is null. Cannot create status and send message.");
            }
        }

        public async Task UpdateNotificationStatusAsync(Guid notificationId, short newStatusId)
        {
            throw new NotImplementedException();
        }
    }
}
