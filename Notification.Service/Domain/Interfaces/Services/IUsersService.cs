using NotifierNotificationService.NotificationService.Domain.Entities;

namespace Notifier.Notification.Service.Notification.Service.Domain.Interfaces.Services
{
    public interface IUsersService
    {
        string HashPassword(string password);
        Task AddUserAsync(User user, string password);
        Task UpdateUserAsync(Guid userId, User updatedUser, string newPassword = null);
        bool VerifyHashedPassword(string hashedPassword, string providedPassword);
    }
}
