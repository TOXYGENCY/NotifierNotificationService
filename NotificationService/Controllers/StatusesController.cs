using Microsoft.AspNetCore.Mvc;
using NotifierNotificationService.NotificationService.Domain.Interfaces.Repositories;
using NotifierNotificationService.NotificationService.Domain.Entities;
using NotifierNotificationService.NotificationService.Domain.Entities.Dto;
using NotifierNotificationService.NotificationService.Domain.Interfaces.Services;

namespace NotifierNotificationService.NotificationService.Controllers

{
    [Route("api/v1/statuses")]
    [ApiController]
    public class StatusesController : ControllerBase
    {
        private readonly IStatusesRepository statusesRepository;
        private readonly IStatusesService statusesService;
        private readonly ILogger<StatusesController> logger;

        public StatusesController(IStatusesRepository statusesRepository,
            ILogger<StatusesController> logger,
            IStatusesService statusesService)
        {
            this.statusesRepository = statusesRepository;
            this.logger = logger;
            this.statusesService = statusesService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Status>>> GetAllStatuses()
        {
            try
            {
                var statuses = await statusesRepository.GetAllAsync();
                return Ok(statuses);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An unexpected error occurred while retrieving all statuses.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Возникла непредвиденная ошибка при получении всех статусов. Обратитесь к администратору или попробуйте позже.");
            }
        }

        [HttpGet("{statusId}")]
        public async Task<ActionResult<Status>> GetStatusById(short statusId)
        {
            try
            {
                var status = await statusesRepository.GetByIdAsync(statusId);
                if (status == null) return StatusCode(StatusCodes.Status404NotFound);

                return Ok(status);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An unexpected error occurred while searching for the status");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Возникла непредвиденная ошибка при поиске статуса. Обратитесь к администратору или попробуйте позже.");
            }
        }

        [HttpPut("{statusId}")]
        public async Task<IActionResult> UpdateStatusAsync(StatusDto updatedStatusDto)
        {

            try
            {
                if (updatedStatusDto is null)
                {
                    throw new ArgumentNullException(nameof(updatedStatusDto));
                }

                await statusesService.UpdateServiceAsync(updatedStatusDto);
                logger.LogInformation($"statusDto {updatedStatusDto.EngName} updated");

                return Ok();
            }
            catch (KeyNotFoundException ex)
            {
                logger.LogError(ex, "statusDto not found");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Статус не найден.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An unexpected error occurred while updating the status");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Возникла непредвиденная ошибка при обновлении статуса. Обратитесь к администратору или попробуйте позже.");
            }
        }


        [HttpDelete("{statusId}")]
        public async Task<IActionResult> DeleteStatus(short statusId)
        {
            try
            {
                await statusesRepository.DeleteAsync(statusId);
                logger.LogInformation($"statusDto with id = {statusId} has been deleted");

                return Ok();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An unexpected error occurred while deleting the status.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Возникла непредвиденная ошибка при удалении статуса. Обратитесь к администратору или попробуйте позже.");
            }
        }

        [HttpPost]
        public async Task<ActionResult> AddStatusAsync(StatusDto statusDto)
        {
            try
            {
                if (statusDto is null)
                {
                    throw new ArgumentNullException(nameof(statusDto));
                }
                await statusesService.AddStatusAsync(statusDto);
                logger.LogInformation($"status {statusDto.EngName} created");

                return Ok();
            }
            catch (ArgumentNullException ex)
            {
                logger.LogError(ex, "Required data to add a status is not received.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Необходимые данные для добавления статуса не получены.Обратитесь к администратору или попробуйте позже.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An unexpected error occurred while adding the status.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Возникла непредвиденная ошибка при добавлении статуса. Обратитесь к администратору или попробуйте позже.");
            }
        }
    }
}
