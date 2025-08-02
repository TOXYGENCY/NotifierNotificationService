using NotifierNotificationService.NotificationService.API.Dto;
using NotifierNotificationService.NotificationService.Domain.Interfaces;
using NotifierNotificationService.NotificationService.Domain.Interfaces.Services;
using StackExchange.Redis;
using System.Text.Json;

namespace NotifierNotificationService.NotificationService.Application.Services
{
    public class NotificationsManager : INotificationsManager
    {
        private readonly INotificationsService notificationsService;
        private readonly IStatusesService statusesService;
        private readonly IRabbitPublisher rabbitmq;
        private readonly IDatabase redis;

        public NotificationsManager(INotificationsService notificationsService,
            IStatusesService statusesService, IRabbitPublisher rabbitmq, IDatabase redis)
        {
            this.notificationsService = notificationsService;
            this.statusesService = statusesService;
            this.rabbitmq = rabbitmq;
            this.redis = redis;
        }

        public async Task CreateNotificationWithStatusAsync(NotificationDto notificationDto)
        {
            ArgumentNullException.ThrowIfNull(notificationDto);

            var newNotification = await notificationsService.AddNotificationAsync(notificationDto);
            if (newNotification != null)
            {
                var newNotificationDto = notificationsService.ToDto(newNotification);
                await statusesService.AssignStatusCreatedAsync(newNotification.Id);
                await redis.StreamAddAsync("analytics", new NameValueEntry[]
                    {
                        new("notificationId", newNotification.Id.ToString()),
                        new("status", "created")
                    });
                try
                {
                    await statusesService.AssignStatusPendingAsync(newNotification.Id);
                    await rabbitmq.PublishAsync(newNotificationDto);
                    await redis.StreamAddAsync("analytics", new NameValueEntry[]
                    {
                        new("notificationId", newNotification.Id.ToString()),
                        new("status", "pending")
                    });
                }
                catch (Exception)
                {
                    await statusesService.AssignStatusErrorAsync(newNotification.Id);
                    await redis.StreamAddAsync("analytics", new NameValueEntry[]
                    {
                        new("notificationId", newNotification.Id.ToString()),
                        new("status", "error")
                    });
                    throw;
                }

                // TODO: позже убрать
                Console.WriteLine($"Sent message into RabbitMQ with: \n {JsonSerializer.Serialize(newNotification)}");
            }
            else
            {
                throw new ArgumentNullException
                    ($"Created Notification ({nameof(newNotification)}) is null. Cannot create status and send message.");
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
            catch (Exception)
            {
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
                try
                {
                    await statusesService.AssignStatusToNotificationAsync(notificationId, newStatusId);
                    await redis.StreamAddAsync("analytics", $"{notificationId}.status", $"status.id={newStatusId}");
                }
                catch (Exception)
                {
                    await redis.StreamAddAsync("analytics", $"{notificationId}.status", "error");
                    throw;
                }
            }
            else
            {
                throw new ArgumentNullException
                    ($"Notification ({nameof(notification)}) is null.");
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
