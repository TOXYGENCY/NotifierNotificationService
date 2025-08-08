namespace NotifierNotificationService.NotificationService.Domain.Interfaces
{
    public interface IMessageBrokerPublisher
    {
        Task PublishAsync<T>(T message, string queue);
        Task PublishAsync<T>(T message);
    }
}
