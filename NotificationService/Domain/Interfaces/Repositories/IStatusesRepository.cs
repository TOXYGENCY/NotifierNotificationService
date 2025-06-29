using NotifierNotificationService.NotificationService.Domain.Entities;

namespace NotifierNotificationService.NotificationService.Domain.Interfaces.Repositories
{
    public interface IStatusesRepository : ICrud<Status, short>
    {
        Task<Status> GetByEngNameAsync(string engName);
    }
}