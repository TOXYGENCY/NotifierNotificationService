using NotifierNotificationService.NotificationService.API.Dto;
using NotifierNotificationService.NotificationService.Domain.Entities;
using NotifierNotificationService.NotificationService.Domain.Interfaces.Repositories;
using NotifierNotificationService.NotificationService.Domain.Interfaces.Services;
using System.Text.Json;

namespace NotifierNotificationService.NotificationService.Application.Services
{
    public class StatusesService : IStatusesService
    {
        private readonly IStatusesRepository statusesRepository;

        public StatusesService(IStatusesRepository statusesRepository)
        {
            this.statusesRepository = statusesRepository;
        }

        public async Task AddStatusAsync(StatusDto newStatusDto)
        {
            if (newStatusDto is null) throw new ArgumentNullException(nameof(newStatusDto));

            var newStatus = FromDto(newStatusDto);

            await statusesRepository.AddAsync(newStatus);
        }

        public async Task AddStatusAsync(string statusName, string statusEngName)
        {
            if (string.IsNullOrEmpty(statusName))
                throw new ArgumentException($"'{nameof(statusName)}' cannot be null or empty.", nameof(statusName));

            if (string.IsNullOrEmpty(statusEngName))
                throw new ArgumentException($"'{nameof(statusEngName)}' cannot be null or empty.", nameof(statusEngName));

            var newStatus = new StatusDto { Name = statusName, EngName = statusEngName };

            await statusesRepository.AddAsync(FromDto(newStatus));
        }

        public async Task<StatusDto?> GetStatusByIdAsync(short statusId)
        {
            var status = await statusesRepository.GetByIdAsync(statusId);
            var statusDto = ToDto(status);
            return statusDto;
        }

        public async Task UpdateStatusAsync(StatusDto updatedStatusDto)
        {
            if (updatedStatusDto is null) throw new ArgumentNullException(nameof(updatedStatusDto));
            if (updatedStatusDto.Id is null) throw new ArgumentNullException(nameof(updatedStatusDto.Id));
            if (updatedStatusDto.Id < 0) throw new ArgumentOutOfRangeException(nameof(updatedStatusDto.Id));

            var currentStatus = await FromDtoToEntityAsync(updatedStatusDto.Id.Value, updatedStatusDto);
            if (currentStatus is null) throw new KeyNotFoundException("Cannot update object that does not exist.");
            var updatedStatus = currentStatus;

            updatedStatus.Name = updatedStatusDto.Name.Trim();
            updatedStatus.EngName = updatedStatusDto.EngName.Trim();

            await statusesRepository.UpdateAsync(updatedStatus);
        }

        public async Task<IEnumerable<StatusDto>> GetAllStatusesAsync()
        {
            return ToDtos(await statusesRepository.GetAllAsync());
        }

        public Status? FromDto(StatusDto? statusDto, Status? baseForDto = null)
        {
            if (statusDto is null) return null;

            // Проверка обязательного поля
            if (string.IsNullOrWhiteSpace(statusDto.EngName))
                throw new ArgumentException("EngName cannot be empty", nameof(statusDto));

            var status = baseForDto ?? new Status();

            // Обновление полей
            status.Name = !string.IsNullOrWhiteSpace(statusDto.Name)
                ? statusDto.Name.Trim()
                : statusDto.EngName.Trim();
            status.EngName = statusDto.EngName.Trim(); // Очистка пробелов

            return status;
        }

        public async Task<Status?> FromDtoToEntityAsync(short statusId, StatusDto? statusDto = null)
        {
            Status? status = null;

            // Поиск по приоритетам: ID > EngName
            if (statusId >= 0)
                status = await statusesRepository.GetByIdAsync(statusId);
            else if (statusDto != null && !string.IsNullOrWhiteSpace(statusDto.EngName))
                status = await statusesRepository.GetByEngNameAsync(statusDto.EngName);

            // Если dto не передан, то с ним работать не можем - возвращаем что есть
            if (statusDto is null)
                return status;

            // Если dto передан - обновляем поля
            FromDto(statusDto, status);

            return status;
        }

        public StatusDto? ToDto(Status? full)
        {
            return JsonSerializationConvert<Status, StatusDto>(full);
        }

        public IEnumerable<StatusDto>? ToDtos(IEnumerable<Status>? full)
        {
            if (full is null) return null;
            var statusDtos = new List<StatusDto>();

            foreach (var status in full)
                statusDtos.Add(ToDto(status));

            return statusDtos;
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
            if (src == null) return default;
            return JsonSerializer.Deserialize<DEST>(JsonSerializer.Serialize(src));
        }

        public async Task AssignStatusToNotificationAsync(Guid notificationId, short statusId)
        {
            if (statusId < 0) throw new ArgumentOutOfRangeException(nameof(statusId));
            if (notificationId == Guid.Empty) throw new ArgumentException(nameof(notificationId));

            var status = await statusesRepository.GetByIdAsync(statusId);
            if (status is null) throw new KeyNotFoundException();

            await statusesRepository.AssignNotificationStatusAsync(notificationId, statusId);
        }

        public async Task AssignStatusCreatedAsync(Guid notificationId)
        {
            var status = await statusesRepository.GetByEngNameAsync("Created");
            ArgumentNullException.ThrowIfNull(status);
            await AssignStatusToNotificationAsync(notificationId, status.Id);
        }

        public async Task AssignStatusPendingAsync(Guid notificationId)
        {
            var status = await statusesRepository.GetByEngNameAsync("Pending send");
            ArgumentNullException.ThrowIfNull(status);
            await AssignStatusToNotificationAsync(notificationId, status.Id);
        }

        public async Task AssignStatusSentAsync(Guid notificationId)
        {
            var status = await statusesRepository.GetByEngNameAsync("Sent");
            ArgumentNullException.ThrowIfNull(status);
            await AssignStatusToNotificationAsync(notificationId, status.Id);
        }

        public async Task AssignStatusErrorAsync(Guid notificationId)
        {
            var status = await statusesRepository.GetByEngNameAsync("Error");
            ArgumentNullException.ThrowIfNull(status);
            await AssignStatusToNotificationAsync(notificationId, status.Id);
        }

        public async Task AssignStatusCreationErrorAsync(Guid notificationId)
        {
            var status = await statusesRepository.GetByEngNameAsync("Creation error");
            ArgumentNullException.ThrowIfNull(status);
            await AssignStatusToNotificationAsync(notificationId, status.Id);
        }

        public async Task AssignStatusUpdateErrorAsync(Guid notificationId)
        {
            var status = await statusesRepository.GetByEngNameAsync("Update error");
            ArgumentNullException.ThrowIfNull(status);
            await AssignStatusToNotificationAsync(notificationId, status.Id);
        }
    }
}
