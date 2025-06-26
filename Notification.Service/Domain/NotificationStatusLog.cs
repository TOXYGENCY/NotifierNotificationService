using System;
using System.Collections.Generic;

namespace Notifier.Notification.Service;

public partial class NotificationStatusLog
{
    public Guid Id { get; set; }

    public short StatusId { get; set; }

    public Guid NotificationId { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Notification Notification { get; set; } = null!;

    public virtual Status Status { get; set; } = null!;
}
