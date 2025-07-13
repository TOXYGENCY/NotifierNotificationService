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
        public async Task<ActionResult<IEnumerable<Status>>> GetAllStatusesAsync()
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
        public async Task<ActionResult> AddStatusAsync(string statusName, string statusEngName)
        {

            try
            {
                if (string.IsNullOrEmpty(statusName))
                    throw new ArgumentException($"'{nameof(statusName)}' cannot be null or empty.", nameof(statusName));

                if (string.IsNullOrEmpty(statusEngName))
                    throw new ArgumentException($"'{nameof(statusEngName)}' cannot be null or empty.", nameof(statusEngName));

                await statusesService.AddStatusAsync(statusName, statusEngName);
                logger.LogInformation($"Status {statusEngName} created.");

                return Ok();
            }
            catch (ArgumentException ex)
            {
                logger.LogError(ex, $"Got invalid string values (statusName={statusName} and statusEngName={statusEngName})");
                return StatusCode(StatusCodes.Status400BadRequest,
                    "Возникла непредвиденная ошибка при добавлении статуса на стороне клиента. Обратитесь к администратору или попробуйте позже.");
            }
            catch (Exception ex) // ArgumentException тоже считается непредвиденной в данном случае
            {
                logger.LogError(ex, "An unexpected error occurred while adding the status.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Возникла непредвиденная ошибка при добавлении статуса. Обратитесь к администратору или попробуйте позже.");
            }
        }

        [HttpGet("{statusId}")]
        public async Task<ActionResult<StatusDto>> GetStatusByIdAsync(short statusId)
        {
            try
            {
                if (statusId <= 0) throw new ArgumentException(nameof(statusId));
                var status = await statusesService.GetStatusByIdAsync(statusId);

                return Ok(status);
            }
            catch (ArgumentException ex)
            {
                logger.LogError(ex, $"Id must be > 0. Got id = {statusId}");
                return StatusCode(StatusCodes.Status400BadRequest,
                    "Возникла непредвиденная ошибка при поиске статуса на стороне клиента. Обратитесь к администратору или попробуйте позже.");
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
                if (statusId <= 0) throw new ArgumentException(nameof(statusId));
                if (updatedStatusDto is null) throw new ArgumentNullException(nameof(updatedStatusDto));
                if (statusId != updatedStatusDto.Id) return StatusCode(StatusCodes.Status400BadRequest, "Id не совпадают.");

                await statusesService.UpdateServiceAsync(updatedStatusDto);
                logger.LogInformation($"Status {updatedStatusDto.EngName} ({updatedStatusDto.Id}) updated.");

                return Ok();
            }
            catch (ArgumentNullException ex)
            {
                logger.LogError(ex, $"Got null value ({nameof(updatedStatusDto)})");
                return StatusCode(StatusCodes.Status400BadRequest,
                    "Возникла непредвиденная ошибка при обновлении статуса на стороне клиента. Обратитесь к администратору или попробуйте позже.");
            }
            catch (ArgumentException ex)
            {
                logger.LogError(ex, $"Id must be > 0. Got id = {statusId}");
                return StatusCode(StatusCodes.Status400BadRequest,
                    "Возникла непредвиденная ошибка при поиске статуса на стороне клиента. Обратитесь к администратору или попробуйте позже.");
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
        public async Task<IActionResult> DeleteStatusAsync(short statusId)
        {
            try
            {
                if (statusId <= 0) throw new ArgumentException(nameof(statusId));
                await statusesRepository.DeleteAsync(statusId);
                logger.LogInformation($"Status with id = {statusId} has been deleted");

                return Ok();
            }
            catch (ArgumentException ex)
            {
                logger.LogError(ex, $"Id must be > 0. Got id = {statusId}");
                return StatusCode(StatusCodes.Status400BadRequest,
                    "Возникла непредвиденная ошибка при поиске статуса на стороне клиента. Обратитесь к администратору или попробуйте позже.");
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
