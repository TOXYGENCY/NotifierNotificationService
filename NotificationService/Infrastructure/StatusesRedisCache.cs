using NotifierNotificationService.NotificationService.Domain.Entities;
using NotifierNotificationService.NotificationService.Domain.Interfaces.Services;
using StackExchange.Redis;
using System.Text.Json;

namespace NotifierNotificationService.NotificationService.Infrastructure
{
    public class StatusesRedisCache : IStatusesRedisCache
    {
        private readonly ILogger<StatusesRedisCache> logger;
        private readonly IConfiguration config;
        private readonly IDatabase redis;
        private string redisInstanceName;
        private string keyCategory;
        private string keyPattern;

        public StatusesRedisCache(IDatabase redis, IConfiguration config,
            ILogger<StatusesRedisCache> logger)
        {
            this.redis = redis;
            this.logger = logger;
            this.config = config;
            if (!config.GetSection("Redis").Exists())
                throw new ArgumentException("Configuration section 'Redis' not found");

            keyCategory = config["Redis:Statuses:CategoryName"];
            redisInstanceName = config["Redis:InstanceName"];
            keyPattern = $"{redisInstanceName}:{keyCategory}:*";
        }

        private string MakeKeyById(short statusId)
        {
            return $"{redisInstanceName}:{keyCategory}:{statusId}";
        }

        public async Task<IEnumerable<Status>> GetAllStatusesAsync()
        {
            var statuses = new List<Status>();

            var server = redis.Multiplexer.GetServer(redis.Multiplexer.GetEndPoints().First());
            await foreach (var key in server.KeysAsync(pattern: keyPattern))
            {
                var statusJson = await redis.StringGetAsync(key);
                if (!statusJson.IsNullOrEmpty)
                {
                    try
                    {
                        statuses.Add(JsonSerializer.Deserialize<Status>(statusJson));
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, $"Could not deserialize from key {key}: {statusJson}");
                    }
                }
            }
            ;

            return statuses;
        }

        public async Task<Status?> GetStatusAsync(RedisKey statusKey)
        {
            var statusJson = await redis.StringGetAsync(statusKey);
            if (statusJson.HasValue)
            {
                try
                {
                    var status = JsonSerializer.Deserialize<Status>(statusJson);
                    return status;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"Could not deserialize from key {statusKey}: {statusJson}");
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        public async Task<Status?> GetStatusByIdAsync(short statusId)
        {
            var key = MakeKeyById(statusId);
            return await GetStatusAsync(key);
        }


        public async Task SetStatusAsync(RedisKey statusKey, Status status, TimeSpan? timespan = null)
        {
            timespan ??= TimeSpan.FromHours(1);
            var statusJson = JsonSerializer.Serialize(status);

            await redis.StringSetAsync(statusKey, statusJson, timespan);
        }

        public async Task SetStatusAsync(Status status, TimeSpan? timespan = null)
        {
            ArgumentNullException.ThrowIfNull(status);

            await SetStatusAsync(MakeKeyById(status.Id), status, timespan);
        }

        public async Task SetStatusesAsync(IEnumerable<Status> statuses)
        {
            foreach (var status in statuses)
            {
                if (status != null)
                    await SetStatusAsync(status);
            }
        }

        public async Task ClearStatusByIdAsync(short statusId)
        {
            await redis.KeyDeleteAsync(MakeKeyById(statusId));
        }
    }
}
