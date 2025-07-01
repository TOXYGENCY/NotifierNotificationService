using NotifierNotificationService.NotificationService.Domain.Entities;
using NotifierNotificationService.NotificationService.Domain.Entities.Dto;
using NotifierNotificationService.NotificationService.Domain.Interfaces.Repositories;
using NotifierNotificationService.NotificationService.Domain.Interfaces.Services;
using System.Text.Json;

namespace NotifierNotificationService.NotificationService.Services
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

        public async Task UpdateServiceAsync(StatusDto updatedStatusDto)
        {
            if (updatedStatusDto is null) throw new ArgumentNullException(nameof(updatedStatusDto));

            var currentStatus = await FromDtoAsync(updatedStatusDto);
            var updatedStatus = currentStatus;

            updatedStatus.Name = updatedStatusDto.Name;
            updatedStatus.EngName = updatedStatusDto.EngName;

            await statusesRepository.UpdateAsync(updatedStatus);
        }

        public async Task<IEnumerable<StatusDto>> GetAllStatusesAsync()
        {
            return ToDtos(await statusesRepository.GetAllAsync());
        }

        public Status? FromDto(StatusDto? statusDto)
        {
            if (statusDto is null) return null;

            // TODO: так можно?
            statusDto.Id ??= -1;
            statusDto.Name ??= statusDto.EngName;

            var status = JsonSerializationConvert<StatusDto, Status>(statusDto);
            return status;
        }

        public async Task<Status?> FromDtoAsync(StatusDto? statusDto)
        {
            if (statusDto == null) return null;
            Status? status;

            if (statusDto.Id == null)
                status = await statusesRepository.GetByEngNameAsync(statusDto.EngName);
            else
                status = await statusesRepository.GetByIdAsync(statusDto.Id.Value);

            status ??= FromDto(statusDto);
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
            if (src == null) return default(DEST);
            return JsonSerializer.Deserialize<DEST>(JsonSerializer.Serialize(src));
        }
    }
}
