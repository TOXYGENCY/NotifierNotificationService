namespace NotifierNotificationService.NotificationService.Domain.Entities
{
    public class NotificationStatusEntry
    {
        public string Id { get; set; }
        public Guid NotificationId { get; set; }
        public short StatusId { get; set; }

        public NotificationStatusEntry(string id, Guid notificationId, short statusId)
        {
            Id = id;
            NotificationId = notificationId;
            StatusId = statusId;
        }

        public NotificationStatusEntry(string id, string notificationId, string statusId)
        {
            Id = id;
            NotificationId = Guid.Parse(notificationId);
            StatusId = short.Parse(statusId);
        }
    }
}
