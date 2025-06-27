
using NotifierNotificationService.NotificationService.Domain.Entities;

namespace NotifierNotificationService.NotificationService.Domain.Interfaces
{
    public interface IUsersService
    {
        string HashPassword(string password);
        Task AddUserAsync(User user, string password);
        Task UpdateUserAsync(Guid userId, User updatedUser, string newPassword = null);
        bool VerifyHashedPassword(string hashedPassword, string providedPassword);
    }
}
