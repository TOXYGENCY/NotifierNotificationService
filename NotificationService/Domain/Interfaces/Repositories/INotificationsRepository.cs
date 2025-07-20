using NotifierNotificationService.NotificationService.Domain.Entities;

namespace NotifierNotificationService.NotificationService.Domain.Interfaces.Repositories
{
    public interface INotificationsRepository : ICrud<Notification, Guid>
    {
        Task<Notification?> GetByUsersAndTimestamp(Guid senderId, Guid recipientId, DateTime createdAt);
        //Task<IEnumerable<Notification>> GetBySenderIdAsync(Guid senderId);
        //Task<IEnumerable<Notification>> GetByRecipientIdAsync(Guid recipientId);
    }
}