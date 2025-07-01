namespace NotifierNotificationService.NotificationService.Domain.Entities.Dto;


public partial class NotificationDto
{
    // TODO: Guid? или Guid
    public Guid? Id { get; set; }

    public Guid RecipientUserId { get; set; }

    public Guid SenderUserId { get; set; }

    public string Message { get; set; } = null!;

    // TODO: DateTime? или DateTime
    public DateTime? CreatedAt { get; set; }

}
