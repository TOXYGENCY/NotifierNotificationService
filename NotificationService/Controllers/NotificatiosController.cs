using Microsoft.AspNetCore.Mvc;
using NotifierNotificationService.NotificationService.Domain.Interfaces.Repositories;
using NotifierNotificationService.NotificationService.Domain.Entities;
using NotifierNotificationService.NotificationService.Domain.Interfaces.Services;
using NotifierNotificationService.NotificationService.Domain.Entities.Dto;
using Npgsql;

namespace NotifierNotificationService.NotificationService.Controllers

{
    [Route("api/v1/notifications")]
    [ApiController]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationsRepository notificationsRepository;
        private readonly ILogger<NotificationsController> logger;
        private readonly INotificationsService notificationsService;

        public NotificationsController(INotificationsRepository notificationsRepository,
            ILogger<NotificationsController> logger, INotificationsService notificationsService)
        {
            this.notificationsRepository = notificationsRepository;
            this.notificationsService = notificationsService;
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

        [HttpGet("{notificationId}")]
        public async Task<ActionResult<NotificationDto>> GetNotificationById(Guid notificationId)
        {
            try
            {
                var notification = await notificationsService.GetNotificationByIdAsync(notificationId);
                if (notification == null) return StatusCode(StatusCodes.Status404NotFound);

                return Ok(notification);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An unexpected error occurred while searching for the notification");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Возникла непредвиденная ошибка при поиске уведомления. Обратитесь к администратору или попробуйте позже.");
            }
        }

        [HttpPut("{notificationId}")]
        public async Task<IActionResult> UpdateNotificationAsync(Guid notificationId, NotificationDto updatedNotification)
        {
            try
            {
                await notificationsService.UpdateNotificationAsync(notificationId, updatedNotification);
                logger.LogInformation($"notificationDto {notificationId} updated");

                return Ok();
            }
            catch (KeyNotFoundException ex)
            {
                logger.LogError(ex, "notificationDto not found");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Уведомление не найдено.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An unexpected error occurred while updating the notification");
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
                logger.LogInformation($"notificationDto with id = {notificationId} has been deleted");

                return Ok();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An unexpected error occurred while deleting the notification.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Возникла непредвиденная ошибка при удалении уведомления. Обратитесь к администратору или попробуйте позже.");
            }
        }

        [HttpPost]
        public async Task<ActionResult> AddNotificationAsync(NotificationDto notificationDto)
        {
            try
            {
                if (notificationDto is null)
                {
                    throw new ArgumentNullException(nameof(notificationDto));
                }

                await notificationsService.AddNotificationAsync(notificationDto);
                logger.LogInformation($"Notification created");

                return Ok();
            }
            catch (ArgumentNullException ex)
            {
                logger.LogError(ex, "Required data to add a notification is not received.");
                return StatusCode(StatusCodes.Status400BadRequest,
                    "Необходимые данные для добавления уведомления не получены. Обратитесь к администратору или попробуйте позже.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An unexpected error occurred while adding the notification.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Возникла непредвиденная ошибка при добавлении уведомления. Обратитесь к администратору или попробуйте позже.");
            }
        }
    }
}
