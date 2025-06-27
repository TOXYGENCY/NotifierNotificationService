using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace NotifierNotificationService.NotificationService.Domain.Entities;

public partial class Status
{
    public short Id { get; set; }

    public string? Name { get; set; }

    public string EngName { get; set; } = null!;

    [JsonIgnore]
    public virtual ICollection<NotificationStatusLog> NotificationStatusLogs { get; set; } = new List<NotificationStatusLog>();
}
