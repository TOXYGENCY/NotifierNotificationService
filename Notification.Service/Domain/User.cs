using System;
using System.Collections.Generic;

namespace Notifier.Notification.Service;

public partial class User
{
    public Guid Id { get; set; }

    public string? Login { get; set; }

    public string PasswordHash { get; set; } = null!;

    public virtual ICollection<Notification> NotificationRecipientUsers { get; set; } = new List<Notification>();

    public virtual ICollection<Notification> NotificationSenderUsers { get; set; } = new List<Notification>();
}
