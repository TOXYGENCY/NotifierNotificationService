using NotifierNotificationService.NotificationService.Domain.Entities;
using NotifierNotificationService.NotificationService.Domain.Entities.Dto;
using NotifierNotificationService.NotificationService.Domain.Interfaces.Services;

namespace Notifier.Notification.Service.Notification.Service.Domain.Interfaces.Services
{
    public interface IStatusesService : IDtoConverter<Status, StatusDto>
    {
        Task UpdateServiceAsync(StatusDto updatedStatusDto);
        Task AddStatusAsync(StatusDto newStatusDto);
    }
}
