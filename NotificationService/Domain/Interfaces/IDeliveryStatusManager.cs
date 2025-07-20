using NotifierNotificationService.NotificationService.Domain.Entities;

namespace NotifierNotificationService.NotificationService.Domain.Interfaces
{
    public interface IDeliveryStatusManager
    {
        Task UpdateDeliveryStatusAsync(StatusUpdatePayload statusUpdate);
    }
}
