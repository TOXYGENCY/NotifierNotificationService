using NotifierNotificationService.NotificationService.Domain.Entities;
using NotifierNotificationService.NotificationService.Domain.Interfaces;
using System.Text.Json;

namespace NotifierNotificationService.NotificationService.Application.Services
{
    public class DeliveryStatusManager : IDeliveryStatusManager
    {
        private readonly INotificationsManager notificationsManager;

        public DeliveryStatusManager(INotificationsManager notificationsManager)
        {
            this.notificationsManager = notificationsManager;
        }

        public async Task UpdateDeliveryStatusAsync(StatusUpdatePayload statusUpdate)
        {
            ArgumentNullException.ThrowIfNull(statusUpdate);

            try
            {
                await UpdateStatusAsync(statusUpdate);
                Console.WriteLine($"[v] Updates status of {JsonSerializer.Serialize(statusUpdate.Notification)}");
            }
            catch (Exception)
            {
                Console.WriteLine($"[XXX] Error during updating status.");
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
