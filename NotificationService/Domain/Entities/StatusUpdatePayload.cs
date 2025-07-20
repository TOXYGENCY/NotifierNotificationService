using NotifierNotificationService.NotificationService.API.Dto;
using System.Text.Json.Serialization;

namespace NotifierNotificationService.NotificationService.Domain.Entities
{
    public class StatusUpdatePayload
    {
        public NotificationDto Notification { get; set; }
        public short NewStatusId { get; set; }
    }
}
