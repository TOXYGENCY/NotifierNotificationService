using NotifierNotificationService.NotificationService.Domain.Entities;

namespace NotifierNotificationService.NotificationService.Domain.Interfaces
{
    public interface IAnalyticsManager
    {
        Task SendNotificationStatusAsync(NotificationStatusLog log);
        Task SendNotificationStatusAsync(NotificationStatusEntry entry);
        Task SendNotificationStatusAsync(Guid notificationId, short statusId);
        Task SendNotificationStatusAsync(string notificationId, string statusId);
    }
}
