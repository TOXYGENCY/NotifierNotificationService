using NotifierNotificationService.NotificationService.Domain.Entities;
using NotifierNotificationService.NotificationService.Domain.Entities.Dto;

namespace NotifierNotificationService.NotificationService.Domain.Interfaces.Services
{
    public interface IStatusesService : IDtoConverter<Status, StatusDto>
    {
        Task<IEnumerable<StatusDto>> GetAllStatusesAsync();
        Task UpdateServiceAsync(StatusDto updatedStatusDto);
        Task AddStatusAsync(StatusDto newStatusDto);
    }
}
