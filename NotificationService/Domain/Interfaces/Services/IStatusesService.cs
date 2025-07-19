using NotifierNotificationService.NotificationService.API.Dto;
using NotifierNotificationService.NotificationService.Domain.Entities;

namespace NotifierNotificationService.NotificationService.Domain.Interfaces.Services
{
    public interface IStatusesService : IDtoConverter<Status, StatusDto, short>
    {
        Task<IEnumerable<StatusDto>> GetAllStatusesAsync();
        Task UpdateStatusAsync(StatusDto updatedStatusDto);
        Task AddStatusAsync(string statusName, string statusEngName);
        Task<StatusDto?> GetStatusByIdAsync(short statusId);
        Task AssignStatusToNotificationAsync(Guid notificationId, short statusId);
        Task AssignStatusCreatedAsync(Guid notificationId);
        Task AssignStatusSentAsync(Guid notificationId);
        Task AssignStatusPendingAsync(Guid notificationId);
        Task AssignStatusErrorAsync(Guid notificationId);
        Task AssignStatusCreationErrorAsync(Guid notificationId);
        Task AssignStatusUpdateErrorAsync(Guid notificationId);
    }
}
