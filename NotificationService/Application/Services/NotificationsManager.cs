﻿using NotifierNotificationService.NotificationService.API.Dto;
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
                await statusesService.AssignStatusCreatedAsync(newNotification.Id);
                try
                {
                    await statusesService.AssignStatusPendingAsync(newNotification.Id);
                    await rabbitmq.PublishAsync(newNotificationDto);
                }
                catch (Exception)
                {
                    await statusesService.AssignStatusErrorAsync(newNotification.Id);
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
                await statusesService.AssignStatusToNotificationAsync(notificationId, newStatusId);
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
