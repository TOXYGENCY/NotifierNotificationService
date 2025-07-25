using Microsoft.AspNetCore.Mvc;
using NotifierNotificationService.NotificationService.API.Dto;
using NotifierNotificationService.NotificationService.Domain.Entities;
using NotifierNotificationService.NotificationService.Domain.Interfaces.Repositories;
using NotifierNotificationService.NotificationService.Domain.Interfaces.Services;

namespace NotifierNotificationService.NotificationService.API.Controllers

{
    [Route("api/v1/notifications")]
    [ApiController]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationsRepository notificationsRepository;
        private readonly INotificationsService notificationsService;
        private readonly INotificationsManager notificationsManager;
        private readonly IUsersService usersService;
        private readonly ILogger<NotificationsController> logger;

        public NotificationsController(INotificationsRepository notificationsRepository,
            ILogger<NotificationsController> logger, INotificationsService notificationsService,
            IUsersService usersService, INotificationsManager notificationsManager)
        {
            this.notificationsRepository = notificationsRepository;
            this.notificationsService = notificationsService;
            this.notificationsManager = notificationsManager;
            this.usersService = usersService;
            this.logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<NotificationDto>>> GetAllNotifications()
        {
            try
            {
                var notifications = await notificationsService.GetAllNotificationsAsync();
                return Ok(notifications);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An unexpected error occurred while retrieving all notifications.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Возникла непредвиденная ошибка при получении всех уведомлений. Обратитесь к администратору или попробуйте позже.");
            }
        }

        [HttpPost]
        public async Task<ActionResult> CreateAndSendNotificationAsync(NotificationDto notificationDto)
        {
            try
            {
                if (notificationDto is null) throw new ArgumentNullException(nameof(notificationDto));
                if (await usersService.GetUserByIdAsync(notificationDto.RecipientUserId) == null
                    || await usersService.GetUserByIdAsync(notificationDto.SenderUserId) == null)
                    throw new KeyNotFoundException();

                await notificationsManager.CreateNotificationWithStatusAsync(notificationDto);
                logger.LogInformation($"Notification created and sent to the RabbitMQ queue.");

                return Ok();
            }
            catch (KeyNotFoundException ex)
            {
                logger.LogError(ex,
                    $"Recipient ({notificationDto.RecipientUserId}) or Sender ({notificationDto.SenderUserId}) is not found");
                return StatusCode(StatusCodes.Status404NotFound,
                    "Получателя не существует в системе.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An unexpected error occurred while adding the Notification.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Возникла непредвиденная ошибка при добавлении уведомления. Обратитесь к администратору или попробуйте позже.");
            }
        }

        [HttpGet("{notificationId}")]
        public async Task<ActionResult<NotificationDto>> GetNotificationById(Guid notificationId)
        {
            try
            {
                var notification = await notificationsService.GetNotificationDtoByIdAsync(notificationId);
                if (notification == null) return StatusCode(StatusCodes.Status404NotFound);

                return Ok(notification);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An unexpected error occurred while searching for the Notification");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Возникла непредвиденная ошибка при поиске уведомления. Обратитесь к администратору или попробуйте позже.");
            }
        }

        [HttpPut("{notificationId}")]
        public async Task<IActionResult> UpdateNotificationAsync(Guid notificationId, NotificationDto updatedNotification)
        {
            try
            {
                if (notificationId == Guid.Empty) throw new ArgumentException(nameof(notificationId));
                ArgumentNullException.ThrowIfNull(updatedNotification);

                await notificationsManager.UpdateNotificationAsync(notificationId, updatedNotification);
                logger.LogInformation($"Notification {notificationId} updated");

                return Ok();
            }
            catch (KeyNotFoundException ex)
            {
                logger.LogError(ex, $"Notification {notificationId} not found");
                return StatusCode(StatusCodes.Status404NotFound,
                    "Уведомление не найдено.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An unexpected error occurred while updating the Notification");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Возникла непредвиденная ошибка при обновлении уведомления. Обратитесь к администратору или попробуйте позже.");
            }
        }


        [HttpDelete("{notificationId}")]
        public async Task<IActionResult> DeleteNotificationById(Guid notificationId)
        {
            try
            {
                await notificationsRepository.DeleteAsync(notificationId);
                logger.LogInformation($"Notification with id = {notificationId} has been deleted");

                return Ok();
            }
            catch (KeyNotFoundException ex)
            {
                logger.LogError(ex, $"Notification {notificationId} not found");
                return StatusCode(StatusCodes.Status404NotFound,
                    "Уведомление не найдено.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An unexpected error occurred while deleting the Notification.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Возникла непредвиденная ошибка при удалении уведомления. Обратитесь к администратору или попробуйте позже.");
            }
        }
    }
}
