using Microsoft.EntityFrameworkCore;
using NotifierNotificationService.NotificationService.Domain.Entities;
using NotifierNotificationService.NotificationService.Domain.Interfaces.Repositories;

namespace NotifierNotificationService.NotificationService.Infrastructure.Repositories
{
    public class StatusesRepository : IStatusesRepository
    {
        private readonly NotifierContext context;

        public StatusesRepository(NotifierContext сontext)
        {
            this.context = сontext;
        }

        public async Task AddAsync(Status status)
        {
            context.Statuses.Add(status);
            await context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Status>> GetAllAsync()
        {
            return await context.Statuses.ToListAsync();
        }

        public async Task<Status?> GetByIdAsync(short id)
        {
            return await context.Statuses.FirstOrDefaultAsync(status => status.Id == id);
        }

        public async Task<Status?> GetByEngNameAsync(string engName)
        {
            return await context.Statuses.FirstOrDefaultAsync(status => status.EngName == engName);
        }

        public async Task UpdateAsync(Status updatedStatus)
        {
            var currentStatus = await context.Statuses.FirstOrDefaultAsync(s => s.Id == updatedStatus.Id);
            if (currentStatus == null) throw new KeyNotFoundException($"Статус {updatedStatus.Id} - {updatedStatus.Name} не найден");

            context.Entry(currentStatus).CurrentValues.SetValues(updatedStatus);
            await context.SaveChangesAsync();
        }

        public async Task DeleteAsync(short id)
        {
            var status = await GetByIdAsync(id);
            if (status != null)
            {
                context.Remove(status);
                await context.SaveChangesAsync();
            }
            else throw new KeyNotFoundException();
        }

        public async Task AssignNotificationStatusAsync(Guid notificationId, short statusId)
        {
            var log = new NotificationStatusLog
            {
                NotificationId = notificationId,
                StatusId = statusId
            };

            context.NotificationStatusLogs.Add(log);
            await context.SaveChangesAsync();
        }
    }
}
