using NotifierNotificationService.NotificationService.Domain.Interfaces;
using RabbitMQ.Client;
using System.Text;
using System.Text.Unicode;

namespace NotifierNotificationService.NotificationService.Infrastructure
{
    public class RabbitPublisher : IRabbitPublisher
    {
        private readonly IConfiguration configuration;

        public RabbitPublisher(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public async Task PublishAsync<T>(T message, string queue)
        {
            var connfact = new ConnectionFactory
            {
                HostName = configuration["RabbitMq:HostName"],
                UserName = configuration["RabbitMq:UserName"],
                Password = configuration["RabbitMq:Password"],
            };

            using var conn = await connfact.CreateConnectionAsync();
            using var channel = await conn.CreateChannelAsync();

            await channel.QueueDeclareAsync(queue: "hello world", durable: false, 
                exclusive: false, autoDelete: false, arguments: null);
            
            string mes = "Hello world!";
            var body = Encoding.UTF8.GetBytes(mes);
            
            await channel.BasicPublishAsync(exchange: string.Empty, routingKey: "hello world", body: body);

        }
    }
}
