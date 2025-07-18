using NotifierNotificationService.NotificationService.API.Dto;
using NotifierNotificationService.NotificationService.Domain.Entities;

namespace NotifierNotificationService.NotificationService.Domain.Interfaces.Services
{
    public interface IStatusesService : IDtoConverter<Status, StatusDto, short>
    {
        Task<IEnumerable<StatusDto>> GetAllStatusesAsync();
        Task UpdateStatusAsync(StatusDto updatedStatusDto);
        //Task AddStatusAsync(StatusDto newStatusDto);
        Task AddStatusAsync(string statusName, string statusEngName);
        Task<StatusDto?> GetStatusByIdAsync(short statusId);
        Task AssignStatusToNotificationAsync(short statusId, Notification notification);
        Task AssignStatusCreatedAsync(Notification notification);
        Task AssignStatusSentAsync(Notification notification);
        Task AssignStatusErrorAsync(Notification notification);
    }
}
