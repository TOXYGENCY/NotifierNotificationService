using Microsoft.EntityFrameworkCore;
using NotifierNotificationService.NotificationService.Domain.Entities;
using NotifierNotificationService.NotificationService.Domain.Interfaces.Repositories;

namespace NotifierNotificationService.NotificationService.Infrastructure
{
    public class StatusesRepository : IStatusesRepository
    {
        private NotifierContext сontext;

        public StatusesRepository(NotifierContext сontext)
        {
            this.сontext = сontext;
        }

        public async Task AddAsync(Status status)
        {
            сontext.Statuses.Add(status);
            await сontext.SaveChangesAsync();
        }

        public async Task<IEnumerable<Status>> GetAllAsync()
        {
            return await сontext.Statuses.ToListAsync();
        }

        public async Task<Status?> GetByIdAsync(short id)
        {
            return await сontext.Statuses.FirstOrDefaultAsync(status => status.Id == id);
        }

        public async Task<Status?> GetByEngNameAsync(string engName)
        {
            return await сontext.Statuses.FirstOrDefaultAsync(status => status.EngName == engName);
        }

        public async Task UpdateAsync(Status updatedStatus)
        {
            var currentStatus = await сontext.Statuses.FirstOrDefaultAsync(s => s.Id == updatedStatus.Id);
            if (currentStatus == null) throw new KeyNotFoundException($"Статус {updatedStatus.Id} - {updatedStatus.Name} не найден");

            сontext.Entry(currentStatus).CurrentValues.SetValues(updatedStatus);
            await сontext.SaveChangesAsync();
        }

        public async Task DeleteAsync(short id)
        {
            var status = await GetByIdAsync(id);
            if (status != null)
            {
                сontext.Remove(status);
                await сontext.SaveChangesAsync();
            }
            else throw new KeyNotFoundException();
        }
    }
}
