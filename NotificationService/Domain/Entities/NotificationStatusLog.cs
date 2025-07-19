using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace NotifierNotificationService.NotificationService.Domain.Entities;


public partial class NotificationStatusLog
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    public short StatusId { get; set; }

    public Guid NotificationId { get; set; }

    public DateTime CreatedAt { get; set; }

    [JsonIgnore]
    public virtual Notification Notification { get; set; } = null!;

    [JsonIgnore]
    public virtual Status Status { get; set; } = null!;
}
