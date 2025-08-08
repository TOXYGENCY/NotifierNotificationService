using NotifierNotificationService.NotificationService.Domain.Entities;
using NotifierNotificationService.NotificationService.Domain.Interfaces.Services;
using System.Text.Json;

namespace NotifierNotificationService.NotificationService.Application.Services
{
    public class DeliveryStatusManager : IDeliveryStatusManager
    {
        private readonly INotificationsManager notificationsManager;
        private readonly ILogger<DeliveryStatusManager> logger;

        public DeliveryStatusManager(INotificationsManager notificationsManager,
            ILogger<DeliveryStatusManager> logger)
        {
            this.logger = logger;
            logger.LogDebug($"DeliveryStatusManager constructor start...");
            this.notificationsManager = notificationsManager;
            logger.LogDebug($"DeliveryStatusManager constructor finish.");
        }

        public async Task UpdateDeliveryStatusAsync(StatusUpdatePayload statusUpdate)
        {
            ArgumentNullException.ThrowIfNull(statusUpdate);

            try
            {
                await UpdateStatusAsync(statusUpdate);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error during updating notification status.");
                logger.LogDebug($"Error during updating notification status {JsonSerializer.Serialize(statusUpdate)}.");
                throw;
            }

        }

        private async Task UpdateStatusAsync(StatusUpdatePayload statusUpdate)
        {
            await notificationsManager.UpdateNotificationStatusAsync
                (statusUpdate.Notification, statusUpdate.NewStatusId);
        }
    }
}
