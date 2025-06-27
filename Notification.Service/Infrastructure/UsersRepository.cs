using Microsoft.EntityFrameworkCore;
using NotifierNotificationService.NotificationService.Domain.Entities;
using NotifierNotificationService.NotificationService.Domain.Interfaces;

namespace NotifierNotificationService.NotificationService.Infrastructure
{
    public class UsersRepository : IUsersRepository
    {
        private NotifierContext сontext;

        public UsersRepository(NotifierContext сontext)
        {
            this.сontext = сontext;
        }

        public async Task AddAsync(User user)
        {
            сontext.Users.Add(user);
            await сontext.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            сontext.Remove(await GetByIdAsync(id));
            await сontext.SaveChangesAsync();
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            return await сontext.Users.ToListAsync();
        }

        public async Task<User> GetByIdAsync(Guid id)
        {
            return await сontext.Users.FirstOrDefaultAsync(user => user.Id == id);
        }

        public async Task<User> GetByLoginAsync(string login)
        {
            return await сontext.Users.FirstOrDefaultAsync(user => user.Login == login);
        }

        public async Task UpdateAsync(User updatedUser)
        {
            var currentUser = await сontext.Users.FirstOrDefaultAsync(s => s.Id == updatedUser.Id);
            if (currentUser == null) throw new KeyNotFoundException($"Пользователь {updatedUser.Id} не найден");

            сontext.Entry(currentUser).CurrentValues.SetValues(updatedUser);
            await сontext.SaveChangesAsync();
        }
    }
}
