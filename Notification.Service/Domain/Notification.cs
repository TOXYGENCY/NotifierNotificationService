using System;
using System.Collections.Generic;

namespace Notifier.Notification.Service;

public partial class Notification
{
    public Guid Id { get; set; }

    public Guid RecipientUserId { get; set; }

    public Guid SenderUserId { get; set; }

    public string Message { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<NotificationStatusLog> NotificationStatusLogs { get; set; } = new List<NotificationStatusLog>();

    public virtual User RecipientUser { get; set; } = null!;

    public virtual User SenderUser { get; set; } = null!;
}
