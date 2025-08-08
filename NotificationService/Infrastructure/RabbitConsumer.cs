using NotifierNotificationService.NotificationService.Domain.Entities;
using NotifierNotificationService.NotificationService.Domain.Interfaces.Services;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace NotifierDeliveryWorker.DeliveryWorker.Infrastructure
{
    public class RabbitConsumer : BackgroundService
    {
        private readonly IConfiguration config;
        IServiceScopeFactory scopeFactory;
        private string hostname;
        private string username;
        private string password;
        private string queue;
        private ConnectionFactory connectionFactory;
        private IChannel channel;
        private IConnection connection;
        private AsyncEventingBasicConsumer consumer;
        private readonly ILogger<RabbitConsumer> logger;

        public RabbitConsumer(IConfiguration configuration, IServiceScopeFactory scopeFactory,
            ILogger<RabbitConsumer> logger)
        {
            this.logger = logger;
            logger.LogDebug($"RabbitConsumer constructor start...");
            this.config = configuration;

            hostname = config["RabbitMq:HostName"] ?? "rabbitmq";
            logger.LogDebug($"RabbitMQ HostName is \"{hostname}\"");

            username = config["RabbitMq:UserName"] ?? "admin";
            password = config["RabbitMq:Password"] ?? "admin";

            queue = config["RabbitMq:StatusUpdatesQueueName"] ?? "status_updates";
            logger.LogDebug($"RabbitMQ consumer queue is \"{queue}\"");

            this.scopeFactory = scopeFactory;
            logger.LogDebug($"RabbitConsumer constructor finish.");
        }

        private async Task Init()
        {
            connectionFactory = new ConnectionFactory
            {
                HostName = hostname,
                UserName = username,
                Password = password
            };

            connection = await connectionFactory.CreateConnectionAsync();
            channel = await connection.CreateChannelAsync();
            consumer = new AsyncEventingBasicConsumer(channel);

            logger.LogDebug($"Initialized rabbit consumer.");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Инициализация переменных соединений, каналов и тд
            await Init();

            // Идемпотентное создание очереди (существует или нет - будет существовать)
            await channel.QueueDeclareAsync(queue: queue, durable: false,
                exclusive: false, autoDelete: false, arguments: null);

            logger.LogInformation($"Waiting for messages.");

            // Подписываемся на событие получения сообщения
            consumer.ReceivedAsync += async (model, eventArgs) =>
            {
                // Обрабатываем полученное сообщение
                await ProcessMessageAsync(eventArgs);
            };

            // Начинаем потребление сообщений из очереди
            await channel.BasicConsumeAsync(queue, autoAck: true, consumer: consumer);

            // Бесконечная работа:
            while (!stoppingToken.IsCancellationRequested)
                await Task.Delay(1000, stoppingToken);
        }

        private async Task ProcessMessageAsync(BasicDeliverEventArgs eventArgs)
        {
            try
            {
                var body = eventArgs.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                logger.LogInformation($"Received {message}.");


                var statusUpdate = JsonSerializer.Deserialize<StatusUpdatePayload>(message);
                if (statusUpdate == null)
                {
                    logger.LogError($"Deserialization returned null.");
                    return;
                }

                using var scope = scopeFactory.CreateAsyncScope();
                var deliveryStatusManager = scope.ServiceProvider.GetRequiredService<IDeliveryStatusManager>();

                await deliveryStatusManager.UpdateDeliveryStatusAsync(statusUpdate);
                logger.LogInformation($"Update completed.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"CRITICAL ERROR.");
            }
        }
    }
}
