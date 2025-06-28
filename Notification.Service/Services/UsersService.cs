using Microsoft.AspNetCore.Identity;
using Notifier.Notification.Service.Notification.Service.Domain.Interfaces.Repositories;
using Notifier.Notification.Service.Notification.Service.Domain.Interfaces.Services;
using NotifierNotificationService.NotificationService.Domain.Entities;

namespace NotifierNotificationService.NotificationService.Services
{
    public class UsersService : IUsersService
    {
        private readonly IUsersRepository usersRepository;
        private readonly PasswordHasher<object> hasher; // object, а не User, потому что в этой реализации .HashPassword аргумент user не используется

        public UsersService(IUsersRepository usersRepository)
        {
            this.usersRepository = usersRepository;
            hasher = new PasswordHasher<object>();
        }

        public bool VerifyHashedPassword(string hashedPassword, string providedPassword)
        {
            var result = hasher.VerifyHashedPassword(null, hashedPassword, providedPassword);
            // Проверка данных и выдача at + rt + данных пользователя, если успех
            if (result == PasswordVerificationResult.Success || result == PasswordVerificationResult.SuccessRehashNeeded)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        // хеширование пароля
        public string HashPassword(string password)
        {
            return hasher.HashPassword(null, password); // null, а не User, потому что в этой реализации .HashPassword аргумент user не используется
        }

        public async Task UpdateUserAsync(Guid userId, User updatedUser, string? newPassword = null)
        {
            if (updatedUser == null || userId == null) throw new ArgumentNullException("Не все аргументы переданы.");
            if (updatedUser.Id != userId) throw new ArgumentException("ID не совпадают.");

            // Получаем текущего пользователя из репозитория
            var user = await usersRepository.GetByIdAsync(userId);
            if (user == null)
                throw new InvalidOperationException($"Пользователь с Id = {userId} не найден.");

            // Обновляем поля, которые должны измениться
            user.Login = updatedUser.Login;

            // Обновляем пароль, если он был передан
            if (!string.IsNullOrWhiteSpace(newPassword))
            {
                // Хешируем пароль перед сохранением
                user.PasswordHash = HashPassword(newPassword);
            }

            await usersRepository.UpdateAsync(user);
        }

        public async Task AddUserAsync(User user, string password)
        {
            if (user is null) throw new ArgumentNullException(nameof(user));

            var passwordHash = HashPassword(password);

            user.PasswordHash = passwordHash;

            await usersRepository.AddAsync(user);
        }

    }
}