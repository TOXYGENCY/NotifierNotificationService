using NotifierNotificationService.NotificationService.API.Dto;
using NotifierNotificationService.NotificationService.Domain.Entities;

namespace NotifierNotificationService.NotificationService.Domain.Interfaces.Services
{
    public interface INotificationsService : IDtoConverter<Notification, NotificationDto, Guid>
    {
        Task<Notification> AddNotificationAsync(NotificationDto newNotificationDto);
        Task<IEnumerable<NotificationDto>> GetAllNotificationsAsync();
        Task<NotificationDto?> GetNotificationByIdAsync(Guid notificationId);
        Task UpdateNotificationAsync(Guid id, NotificationDto updatedNotification);
    }
}
