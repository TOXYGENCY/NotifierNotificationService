using Microsoft.AspNetCore.Mvc;
using NotifierNotificationService.NotificationService.Domain.Entities;
using NotifierNotificationService.NotificationService.Domain.Entities.Dto;
using NotifierNotificationService.NotificationService.Domain.Interfaces.Repositories;
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
            this.statusesService = statusesService;
            this.logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Status>>> GetAllStatuses()
        {
            try
            {
                var statuses = await statusesService.GetAllStatusesAsync();
                return Ok(statuses);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An unexpected error occurred while retrieving all statuses.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Возникла непредвиденная ошибка при получении всех статусов. Обратитесь к администратору или попробуйте позже.");
            }
        }

        [HttpPost]
        public async Task<ActionResult> AddStatusAsync(StatusDto statusDto)
        {
            try
            {
                if (statusDto is null) throw new ArgumentNullException(nameof(statusDto));

                await statusesService.AddStatusAsync(statusDto);
                logger.LogInformation($"Status {statusDto.EngName} ({statusDto.Id}) created.");

                return Ok();
            }
            catch (Exception ex) // ArgumentNullException тоже считается непредвиденной в данном случае
            {
                logger.LogError(ex, "An unexpected error occurred while adding the status.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Возникла непредвиденная ошибка при добавлении статуса. Обратитесь к администратору или попробуйте позже.");
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
        public async Task<IActionResult> UpdateStatusAsync(short statusId, StatusDto updatedStatusDto)
        {
            try
            {
                if (updatedStatusDto is null) throw new ArgumentNullException(nameof(updatedStatusDto));
                if (statusId != updatedStatusDto.Id) return StatusCode(StatusCodes.Status403Forbidden, "Id не совпадают.");

                await statusesService.UpdateServiceAsync(updatedStatusDto);
                logger.LogInformation($"Status {updatedStatusDto.EngName} ({updatedStatusDto.Id}) updated.");

                return Ok();
            }
            catch (KeyNotFoundException ex)
            {
                logger.LogError(ex, $"Status ({updatedStatusDto.Id}) not found");
                return StatusCode(StatusCodes.Status404NotFound,
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
                logger.LogInformation($"Status with id = {statusId} has been deleted");

                return Ok();
            }
            catch (KeyNotFoundException ex)
            {
                logger.LogError(ex, $"Status ({statusId}) not found");
                return StatusCode(StatusCodes.Status404NotFound,
                    "Статус не найден.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An unexpected error occurred while deleting the status.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Возникла непредвиденная ошибка при удалении статуса. Обратитесь к администратору или попробуйте позже.");
            }
        }
    }
}
