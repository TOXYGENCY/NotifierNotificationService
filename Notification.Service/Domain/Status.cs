using System;
using System.Collections.Generic;

namespace Notifier.Notification.Service;

public partial class Status
{
    public short Id { get; set; }

    public string? Name { get; set; }

    public string EngName { get; set; } = null!;

    public virtual ICollection<NotificationStatusLog> NotificationStatusLogs { get; set; } = new List<NotificationStatusLog>();
}
