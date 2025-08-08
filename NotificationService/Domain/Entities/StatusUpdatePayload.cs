using NotifierNotificationService.NotificationService.API.Dto;

namespace NotifierNotificationService.NotificationService.Domain.Entities
{
    public class StatusUpdatePayload
    {
        public NotificationDto Notification { get; set; }
        public short NewStatusId { get; set; }
    }
}
