using System.Text.Json.Serialization;

namespace NotifierNotificationService.NotificationService.Domain.Entities;

public partial class User
{
    public Guid Id { get; set; }

    public string? Login { get; set; }

    public string PasswordHash { get; set; } = null!;

    [JsonIgnore]
    public virtual ICollection<Notification> NotificationRecipientUsers { get; set; } = new List<Notification>();

    [JsonIgnore]
    public virtual ICollection<Notification> NotificationSenderUsers { get; set; } = new List<Notification>();
}
