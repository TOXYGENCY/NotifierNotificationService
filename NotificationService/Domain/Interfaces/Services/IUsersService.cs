using NotifierNotificationService.NotificationService.Domain.Entities;
using NotifierNotificationService.NotificationService.Domain.Entities.Dto;

namespace NotifierNotificationService.NotificationService.Domain.Interfaces.Services
{
    public interface IUsersService : IDtoConverter<User, UserDto>
    {
        string HashPassword(string password);
        Task AddUserAsync(UserDto user, string password);
        Task UpdateUserAsync(Guid userId, UserDto updatedUser, string? newPassword = null);
        Task<UserDto> GetUserByIdAsync(Guid userId);
        Task<IEnumerable<UserDto>> GetAllUsersAsync();
        bool VerifyHashedPassword(string hashedPassword, string providedPassword);
    }
}
