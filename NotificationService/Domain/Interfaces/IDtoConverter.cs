namespace NotifierNotificationService.NotificationService.Domain.Interfaces
{
    public interface IDtoConverter<Full, Dto>
    {
        Task<Full?> FromDtoAsync(Dto? dto);
        Full? FromDto(Dto? dto);

        Dto? ToDto(Full? full);
        IEnumerable<Dto>? ToDtos(IEnumerable<Full> full);
    }
}
