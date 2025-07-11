using NotifierNotificationService.NotificationService.Domain.Entities;
using NotifierNotificationService.NotificationService.Domain.Entities.Dto;
using NotifierNotificationService.NotificationService.Domain.Interfaces.Repositories;
using NotifierNotificationService.NotificationService.Domain.Interfaces.Services;
using NotifierNotificationService.NotificationService.Infrastructure;
using System.Text.Json;

namespace NotifierNotificationService.NotificationService.Services
{
    public class NotificationsService : INotificationsService
    {
        private readonly INotificationsRepository notificationsRepository;

        public NotificationsService(INotificationsRepository notificationsRepository)
        {
            this.notificationsRepository = notificationsRepository;
        }
        public async Task AddNotificationAsync(NotificationDto newNotificationDto)
        {
            if (newNotificationDto is null) throw new ArgumentNullException(nameof(newNotificationDto));

            if (newNotificationDto.CreatedAt <= DateTime.MinValue)
                throw new ArgumentException($"Неверное время DateTime: {newNotificationDto.CreatedAt}");

            var newNotification = FromDto(newNotificationDto);

            await notificationsRepository.AddAsync(newNotification);
        }

        public async Task UpdateNotificationAsync(Guid notificationId, NotificationDto updatedNotificationDto)
        {
            if (updatedNotificationDto == null || notificationId.Equals(Guid.Empty))
                throw new ArgumentNullException("Не все аргументы переданы.");

            var notification = await notificationsRepository.GetByIdAsync(notificationId);
            if (notification == null) throw new InvalidOperationException($"Уведомление с Id = {notificationId} не найдено.");

            var updatedNotification = FromDto(updatedNotificationDto, notification);
            await notificationsRepository.UpdateAsync(updatedNotification);
        }

        public async Task<NotificationDto?> GetNotificationByIdAsync(Guid notificationId)
        {
            var notification = await notificationsRepository.GetByIdAsync(notificationId);
            if (notification is null) return null;
            return ToDto(notification);
        }

        public async Task<IEnumerable<NotificationDto>> GetAllNotificationsAsync()
        {
            var notifications = await notificationsRepository.GetAllAsync();
            return ToDtos(notifications);
        }

        /// <summary>
        /// Ищет EF-объект по Id в базе данных (EF-контексте).
        /// </summary>
        /// <param name="notificationId">обязательный параметр Id для поиска.</param>
        /// <param name="notificationDto">(опционально) объект, который стоит конвертировать в новый, 
        /// при отсустствии в контексте.</param>
        /// <returns> EF-объект из базы данных (EF-контекста). В случае, если он не найден, 
        /// то конвертирует notificationDto (при наличии) в новый объект (вне контекста).
        /// Возвращает null, если объект не найден в контексте И если DTO-объект отсутствует.
        /// </returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task<Notification?> FromDtoToEntityAsync(Guid notificationId, NotificationDto? notificationDto)
        {

            if (notificationId.Equals(Guid.Empty))
                throw new ArgumentException(nameof(notificationId));

            var notification = await notificationsRepository.GetByIdAsync(notificationId);

            // Если dto не передан, то с ним работать не можем - возвращаем что есть
            if (notificationDto == null)
                return notification;

            // если dto есть и нет объекта в БД - конвертируем
            notification ??= FromDto(notificationDto);

            return notification;
        }

        /// <summary>
        /// Конвертирует NotificationDto в Notification
        /// </summary>
        /// <param name="notificationDto"></param>
        /// <param name="baseForDto">Основа для установки значений из DTO.</param>
        /// <returns>
        /// Новый (вне EF) объект Notification на основе baseForDto (при наличии).
        /// null если NotificationDto = null.
        /// </returns>
        public Notification? FromDto(NotificationDto? notificationDto, Notification? baseForDto = null)
        {
            if (notificationDto is null) return null;
            Notification notification;

            notification = new Notification
            {
                // Id не ставим - в любом случае он либо не нужен, либо БД сгенерирует (при добавлении)
                RecipientUserId = notificationDto.RecipientUserId,
                SenderUserId = notificationDto.SenderUserId,
                Message = notificationDto.Message,
                CreatedAt = notificationDto.CreatedAt ?? default(DateTime), // либо передан, либо стандартное значение
                NotificationStatusLogs = new List<NotificationStatusLog>(),
                RecipientUser = null!, // Будет заполнено при загрузке
                SenderUser = null! // Будет заполнено при загрузке
            };
            if (baseForDto != null)
            {
                notification.Id = baseForDto.Id;
                notification.CreatedAt = baseForDto.CreatedAt;
                notification.SenderUser = baseForDto.SenderUser;
                notification.RecipientUser = baseForDto.RecipientUser;
                notification.NotificationStatusLogs = baseForDto.NotificationStatusLogs;
            }
            
            return notification;
        }

        public NotificationDto? ToDto(Notification? notification)
        {
            return JsonSerializationConvert<Notification, NotificationDto>(notification);
        }

        /// <summary>
        /// Конвертирует коллекцию Notification в коллекцию NotificationDto
        /// </summary>
        /// <param name="notifications">Исходная коллекция</param>
        /// <returns>
        /// null если входной параметр null,
        /// коллекцию DTO (пропускает null-элементы)
        /// </returns>
        public IEnumerable<NotificationDto>? ToDtos(IEnumerable<Notification>? notifications)
        {
            if (notifications is null) return null;
            var notificationDtos = new List<NotificationDto>();

            foreach (var notification in notifications)
            {
                var notificationDto = ToDto(notification);
                if (notificationDto is null) continue;
                notificationDtos.Add(notificationDto);
            }

            return notificationDtos;
        }

        /// <summary>
        /// Конвертирование из объекта src типа SRC в объект типа DEST через сериализацию и десереализацию в JSON-объект (встроенный авто-маппинг).
        /// </summary>
        /// <typeparam name="SRC"></typeparam>
        /// <typeparam name="DEST"></typeparam>
        /// <param name="src"></param>
        /// <returns></returns>
        private DEST? JsonSerializationConvert<SRC, DEST>(SRC? src)
        {
            if (src == null) return default(DEST);
            return JsonSerializer.Deserialize<DEST>(JsonSerializer.Serialize(src));
        }
    }
}