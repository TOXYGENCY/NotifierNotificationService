using Microsoft.EntityFrameworkCore;
using NotifierNotificationService.NotificationService.Domain.Entities;
using NotifierNotificationService.NotificationService.Domain.Interfaces.Repositories;

namespace NotifierNotificationService.NotificationService.Infrastructure
{
    public class NotificationsRepository : INotificationsRepository
    {
        private NotifierContext context;

        public NotificationsRepository(NotifierContext сontext)
        {
            this.context = сontext;
        }

        public async Task AddAsync(Notification notification)
        {
            context.Notifications.Add(notification);
            await context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var notification = await GetByIdAsync(id);
            if (notification != null)
            {
                context.Remove(notification);
                await context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Notification>> GetAllAsync()
        {
            return await context.Notifications.ToListAsync();
        }

        public async Task<Notification?> GetByIdAsync(Guid id)
        {
            return await context.Notifications.FirstOrDefaultAsync(notification => notification.Id == id);
        }

        public async Task UpdateAsync(Notification updatedNotification)
        {
            var currentNotification = await context.Notifications.FirstOrDefaultAsync(s => s.Id == updatedNotification.Id);
            if (currentNotification == null) throw new KeyNotFoundException($"Уведомление {updatedNotification.Id} не найдено");

            context.Entry(currentNotification).CurrentValues.SetValues(updatedNotification);
            await context.SaveChangesAsync();
        }
    }
}
