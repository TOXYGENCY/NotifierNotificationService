
using NotifierNotificationService.NotificationService.Domain.Entities;

namespace Notifier.Notification.Service.Notification.Service.Domain.Interfaces.Repositories
{
    public interface IStatusesRepository : ICrud<Status, short>
    {
        Task<Status> GetByEngNameAsync(string engName);
    }
}