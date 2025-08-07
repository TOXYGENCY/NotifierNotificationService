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
        private readonly ILogger<NotificationsManager> logger;

        public NotificationsManager(INotificationsService notificationsService,
            IStatusesService statusesService, IRabbitPublisher rabbitmq, ILogger<NotificationsManager> logger)
        {
            this.logger = logger;
            logger.LogDebug($"NotificationsManager constructor start...");
            this.notificationsService = notificationsService;
            this.statusesService = statusesService;
            this.rabbitmq = rabbitmq;
            logger.LogDebug($"NotificationsManager constructor finish.");
        }

        public async Task CreateNotificationWithStatusAsync(NotificationDto notificationDto)
        {
            ArgumentNullException.ThrowIfNull(notificationDto);

            var newNotification = await notificationsService.AddNotificationAsync(notificationDto);
            if (newNotification != null)
            {
                var newNotificationDto = notificationsService.ToDto(newNotification);
                await statusesService.AssignStatusCreatedAsync(newNotification.Id);
                try
                {
                    await statusesService.AssignStatusPendingAsync(newNotification.Id);
                    await rabbitmq.PublishAsync(newNotificationDto);
                }
                catch (Exception ex)
                {
                    await statusesService.AssignStatusErrorAsync(newNotification.Id);
                    logger.LogError(ex, $"Could not finish the creation of notification ({newNotification.Id}).");
                    throw;
                }

                logger.LogInformation($"Sent message into RabbitMQ.");
                logger.LogDebug($"RabbitMQ message content: \n {JsonSerializer.Serialize(newNotification)}");
            }
            else
            {
                var mes = 
                    $"Created Notification ({nameof(newNotification)}) is null. Cannot create status and send message.";
                logger.LogError(mes);
                throw new ArgumentNullException(mes);
            }
        }

        public async Task UpdateNotificationAsync(Guid notificationId, NotificationDto updatedNotification)
        {
            ArgumentNullException.ThrowIfNull(updatedNotification);
            if (notificationId == Guid.Empty) throw new ArgumentException(nameof(notificationId));
            var notification = await notificationsService.GetNotificationByIdAsync(notificationId);
            if (notification == null) throw new KeyNotFoundException(nameof(notification));

            try
            {
                await notificationsService.UpdateNotificationAsync(notificationId, updatedNotification);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Could not update notification ({notificationId}).");
                await statusesService.AssignStatusUpdateErrorAsync(notificationId);
                throw;
            }
        }

        public async Task UpdateNotificationStatusAsync(Guid notificationId, short newStatusId)
        {
            if (notificationId == Guid.Empty) throw new ArgumentException(nameof(notificationId));
            if (newStatusId < 0) throw new ArgumentOutOfRangeException(nameof(newStatusId));

            var notification = await notificationsService.GetNotificationByIdAsync(notificationId);
            if (notification != null)
            {
                await statusesService.AssignStatusToNotificationAsync(notificationId, newStatusId);
            }
            else
            {
                var mes = $"Notification ({nameof(notification)}) is null.";
                logger.LogError(mes);
                throw new ArgumentNullException(mes);
            }
        }

        public async Task UpdateNotificationStatusAsync(NotificationDto notificationDto, short newStatusId)
        {
            ArgumentNullException.ThrowIfNull(notificationDto);
            if (newStatusId < 0) throw new ArgumentOutOfRangeException(nameof(newStatusId));

            var notification = await notificationsService.FromDtoFindEntityAsync(notificationDto);

            await UpdateNotificationStatusAsync(notification.Id, newStatusId);
        }
    }
}
