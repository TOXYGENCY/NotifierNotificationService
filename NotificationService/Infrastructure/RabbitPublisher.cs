using NotifierNotificationService.NotificationService.Domain.Interfaces;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace NotifierNotificationService.NotificationService.Infrastructure
{
    public class RabbitPublisher : IRabbitPublisher
    {
        private readonly IConfiguration config;
        private string queue;

        public RabbitPublisher(IConfiguration configuration)
        {
            this.config = configuration;
            queue = config["RabbitMq:NotificationsQueueName"] ?? "notifications";
        }
        public async Task PublishAsync<T>(T content)
        {
            await PublishAsync<T>(content, queue);
        }

        public async Task PublishAsync<T>(T content, string queue)
        {
            var connfact = new ConnectionFactory
            {
                HostName = config["RabbitMq:HostName"],
                UserName = config["RabbitMq:UserName"],
                Password = config["RabbitMq:Password"],
            };

            using var conn = await connfact.CreateConnectionAsync();
            using var channel = await conn.CreateChannelAsync();

            await channel.QueueDeclareAsync(queue: queue, durable: false,
                exclusive: false, autoDelete: false, arguments: null);

            string json = JsonSerializer.Serialize(content);
            var body = Encoding.UTF8.GetBytes(json);

            await channel.BasicPublishAsync(exchange: string.Empty, routingKey: queue, body: body);

        }
    }
}
