using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace NotifierNotificationService.NotificationService.Domain.Entities;


public partial class Notification
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    public Guid RecipientUserId { get; set; }

    public Guid SenderUserId { get; set; }

    public string Message { get; set; } = null!;

    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public DateTime CreatedAt { get; set; }

    [JsonIgnore]
    public virtual ICollection<NotificationStatusLog> NotificationStatusLogs { get; set; } = new List<NotificationStatusLog>();

    [JsonIgnore]
    public virtual User RecipientUser { get; set; } = null!;

    [JsonIgnore]
    public virtual User SenderUser { get; set; } = null!;
}
