using NotifierNotificationService.NotificationService.Domain.Entities;

namespace NotifierNotificationService.NotificationService.Domain.Interfaces.Repositories
{
    public interface IUsersRepository : ICrud<User, Guid>
    {

    }
}