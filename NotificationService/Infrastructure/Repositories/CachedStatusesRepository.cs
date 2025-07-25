using Microsoft.EntityFrameworkCore;
using NotifierNotificationService.NotificationService.Domain.Entities;
using NotifierNotificationService.NotificationService.Domain.Interfaces.Repositories;
using NotifierNotificationService.NotificationService.Domain.Interfaces.Services;

namespace NotifierNotificationService.NotificationService.Infrastructure.Repositories
{
    public class CachedStatusesRepository : IStatusesRepository
    {
        private readonly IStatusesRedisCache cache;
        private readonly IStatusesRepository repo;
        private readonly ILogger<CachedStatusesRepository> logger;

        public CachedStatusesRepository(IStatusesRedisCache cache,
            IStatusesRepository repo,
            ILogger<CachedStatusesRepository> logger)
        {
            this.cache = cache;
            this.repo = repo;
            this.logger = logger;
        }

        public async Task AddAsync(Status status)
        {
            ArgumentNullException.ThrowIfNull(status);

            await repo.AddAsync(status);

            try
            {
                await cache.SetStatusAsync(status);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, $"Couldn't write new status (statusId = {status.Id}) in cache.");
            }
        }

        public async Task<IEnumerable<Status>> GetAllAsync()
        {
            var statuses = await cache.GetAllStatusesAsync();
            if (!statuses.Any())
            {
                statuses = await repo.GetAllAsync();
                await cache.SetStatusesAsync(statuses);
            }

            return statuses;
        }

        public async Task<Status?> GetByIdAsync(short statusId)
        {
            var status = await cache.GetStatusByIdAsync(statusId);
            if (status == null)
            {
                status = await repo.GetByIdAsync(statusId);
                await cache.SetStatusAsync(status);
            }

            return status;
        }

        public async Task<Status?> GetByEngNameAsync(string engName)
        {
            return await repo.GetByEngNameAsync(engName);
        }

        public async Task UpdateAsync(Status updatedStatus)
        {
            ArgumentNullException.ThrowIfNull(updatedStatus);

            await repo.UpdateAsync(updatedStatus);
            await cache.SetStatusAsync(updatedStatus);
        }

        public async Task DeleteAsync(short statusId)
        {
            await repo.DeleteAsync(statusId);
            await cache.ClearStatusByIdAsync(statusId);
        }

        public async Task AssignNotificationStatusAsync(Guid notificationId, short statusId)
        {
            await repo.AssignNotificationStatusAsync(notificationId, statusId);
        }
    }
}
