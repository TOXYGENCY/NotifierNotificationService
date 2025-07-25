using NotifierNotificationService.NotificationService.Domain.Entities;
using StackExchange.Redis;

namespace NotifierNotificationService.NotificationService.Domain.Interfaces.Services
{
    public interface IStatusesRedisCache
    {
        Task SetStatusAsync(RedisKey statusKey, Status status, TimeSpan? timespan = null);
        Task SetStatusAsync(Status status, TimeSpan? timespan = null);
        Task<Status?> GetStatusAsync(RedisKey statusKey);
        Task<IEnumerable<Status>> GetAllStatusesAsync();
        Task SetStatusesAsync(IEnumerable<Status> statuses);
        Task<Status?> GetStatusByIdAsync(short statusId);
        Task ClearStatusByIdAsync(short statusId);
    }
}
