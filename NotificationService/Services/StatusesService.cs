using System.Text.Json;
using NotifierNotificationService.NotificationService.Domain.Interfaces.Repositories;
using NotifierNotificationService.NotificationService.Domain.Entities;
using NotifierNotificationService.NotificationService.Domain.Entities.Dto;
using NotifierNotificationService.NotificationService.Domain.Interfaces.Services;
using NotifierNotificationService.NotificationService.Infrastructure;

namespace NotifierNotificationService.NotificationService.Services
{
    public class StatusesService : IStatusesService
    {
        private readonly IStatusesRepository statusesRepository;
        private readonly NotifierContext context;

        public StatusesService(IStatusesRepository statusesRepository, NotifierContext context)
        {
            this.statusesRepository = statusesRepository;
            this.context = context;
        }

        /// <summary>
        /// Конвертирование из объекта src типа SRC в объект типа DEST через сериализацию и десереализацию в JSON-объект (встроенный авто-маппинг).
        /// </summary>
        /// <typeparam name="SRC"></typeparam>
        /// <typeparam name="DEST"></typeparam>
        /// <param name="src"></param>
        /// <returns></returns>
        public DEST JsonSerializationConvert<SRC, DEST>(SRC src)
        {
            return JsonSerializer.Deserialize<DEST>(JsonSerializer.Serialize(src));
        }

        public async Task<Status> FromDtoAsync(StatusDto statusDto)
        {
            if (statusDto == null) throw new ArgumentNullException(nameof(statusDto));
            Status status;

            if (statusDto.Id == null)
                status = await statusesRepository.GetByEngNameAsync(statusDto.EngName);
            else
                status = await statusesRepository.GetByIdAsync(statusDto.Id.Value);

            status ??= JsonSerializationConvert<StatusDto, Status>(statusDto);
            return status;
        }

        public StatusDto ToDto(Status full)
        {
            if (full is null)
            {
                throw new ArgumentNullException(nameof(full));
            }

            return JsonSerializationConvert<Status, StatusDto>(full);
        }

        public IEnumerable<StatusDto> ToDtos(IEnumerable<Status> full)
        {
            if (full is null)
            {
                throw new ArgumentNullException(nameof(full));
            }

            var statusDtos = new List<StatusDto>();
            foreach (var status in full)
                statusDtos.Add(ToDto(status));

            return statusDtos;
        }

        public async Task UpdateServiceAsync(StatusDto updatedStatusDto)
        {
            if (updatedStatusDto is null)
            {
                throw new ArgumentNullException(nameof(updatedStatusDto));
            }

            var currentStatus = await FromDtoAsync(updatedStatusDto);
            var updatedStatus = currentStatus;

            updatedStatus.Name = updatedStatusDto.Name;
            updatedStatus.EngName = updatedStatusDto.EngName;

            await statusesRepository.UpdateAsync(updatedStatus);
        }

        public async Task AddStatusAsync(StatusDto newStatusDto)
        {
            if (newStatusDto is null)
            {
                throw new ArgumentNullException(nameof(newStatusDto));
            }

            var newStatus = await FromDtoAsync(newStatusDto);

            await statusesRepository.AddAsync(newStatus);
        }
    }
}
