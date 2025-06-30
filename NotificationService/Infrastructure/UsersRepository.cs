using Microsoft.EntityFrameworkCore;
using NotifierNotificationService.NotificationService.Domain.Entities;
using NotifierNotificationService.NotificationService.Domain.Interfaces.Repositories;

namespace NotifierNotificationService.NotificationService.Infrastructure
{
    public class UsersRepository : IUsersRepository
    {
        private NotifierContext context;

        public UsersRepository(NotifierContext сontext)
        {
            this.context = сontext;
        }

        public async Task AddAsync(User user)
        {
            context.Users.Add(user);
            await context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var user = await GetByIdAsync(id);
            if (user != null)
            {
                context.Remove(user);
                await context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            return await context.Users.ToListAsync();
        }

        public async Task<User?> GetByIdAsync(Guid id)
        {
            return await context.Users.FirstOrDefaultAsync(user => user.Id == id);
        }

        public async Task<User?> GetByLoginAsync(string login)
        {
            return await context.Users.FirstOrDefaultAsync(user => user.Login == login);
        }

        public async Task UpdateAsync(User updatedUser)
        {
            var currentUser = await context.Users.FirstOrDefaultAsync(s => s.Id == updatedUser.Id);
            if (currentUser == null) throw new KeyNotFoundException($"Пользователь {updatedUser.Id} не найден");

            context.Entry(currentUser).CurrentValues.SetValues(updatedUser);
            await context.SaveChangesAsync();
        }
    }
}
