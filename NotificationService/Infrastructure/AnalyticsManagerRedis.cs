using NotifierNotificationService.NotificationService.Domain.Entities;
using NotifierNotificationService.NotificationService.Domain.Interfaces;
using StackExchange.Redis;

namespace NotifierNotificationService.NotificationService.Infrastructure
{
    public class AnalyticsManagerRedis : IAnalyticsManager
    {
        private readonly IConfiguration config;
        private readonly IDatabase redis;
        private string latestStatusesStream;

        public AnalyticsManagerRedis(IConfiguration config, IDatabase redis)
        {
            this.config = config;
            this.redis = redis;
            latestStatusesStream = config["Redis:Streams:Statuses"] ?? "status_log";
        }

        private async Task SendAsync(string notificationId, string statusId)
        {
            await redis.StreamAddAsync(latestStatusesStream, new NameValueEntry[]
            {
                new("notificationId", notificationId),
                new("statusId", statusId),
            });
        }

        public async Task SendNotificationStatusAsync(NotificationStatusLog log)
        {
            ArgumentNullException.ThrowIfNull(log);

            await SendAsync(log.NotificationId.ToString(), log.StatusId.ToString());
        }

        public async Task SendNotificationStatusAsync(NotificationStatusEntry entry)
        {
            ArgumentNullException.ThrowIfNull(entry);

            await SendAsync(entry.NotificationId.ToString(), entry.StatusId.ToString());
        }

        public async Task SendNotificationStatusAsync(Guid notificationId, short statusId)
        {
            await SendAsync(notificationId.ToString(), statusId.ToString());
        }

        public async Task SendNotificationStatusAsync(string notificationId, string statusId)
        {
            ArgumentException.ThrowIfNullOrEmpty(notificationId);
            ArgumentException.ThrowIfNullOrEmpty(statusId);

            await SendAsync(notificationId, statusId);
        }
    }
}
