using NotifierNotificationService.NotificationService.Domain.Entities;

namespace NotifierNotificationService.NotificationService.Domain.Interfaces.Services
{
    public interface IDeliveryStatusManager
    {
        Task UpdateDeliveryStatusAsync(StatusUpdatePayload statusUpdate);
    }
}
