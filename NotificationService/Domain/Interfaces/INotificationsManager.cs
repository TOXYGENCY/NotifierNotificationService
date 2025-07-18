using NotifierNotificationService.NotificationService.API.Dto;

namespace NotifierNotificationService.NotificationService.Domain.Interfaces
{
    public interface INotificationsManager
    {
        Task CreateNotificationWithStatusAsync(NotificationDto notificationDto);
        Task UpdateNotificationStatusAsync(Guid notificationId, short newStatusId);
    }
}
