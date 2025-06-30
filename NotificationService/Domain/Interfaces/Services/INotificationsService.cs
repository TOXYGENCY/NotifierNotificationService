using NotifierNotificationService.NotificationService.Domain.Entities;
using NotifierNotificationService.NotificationService.Domain.Entities.Dto;

namespace NotifierNotificationService.NotificationService.Domain.Interfaces.Services
{
    public interface INotificationsService : IDtoConverter<Notification, NotificationDto>
    {
        Task AddNotificationAsync(NotificationDto newNotificationDto);
        Task<IEnumerable<NotificationDto>> GetAllNotificationsAsync();
        Task<NotificationDto?> GetNotificationByIdAsync(Guid notificationId);
        Task UpdateNotificationAsync(Guid id, NotificationDto updatedNotification);
    }
}
