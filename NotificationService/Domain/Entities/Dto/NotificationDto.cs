namespace NotifierNotificationService.NotificationService.Domain.Entities.Dto;


public partial class NotificationDto
{

    public Guid RecipientUserId { get; set; }

    public Guid SenderUserId { get; set; }

    public string Message { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

}
