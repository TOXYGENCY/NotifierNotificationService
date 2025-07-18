namespace NotifierNotificationService.NotificationService.Domain.Interfaces
{
    public interface IDtoConverter<Full, Dto, IdType>
    {
        Task<Full?> FromDtoToEntityAsync(IdType id, Dto? dto);
        Full? FromDto(Dto? dto, Full? baseForDto);

        Dto? ToDto(Full? full);
        IEnumerable<Dto>? ToDtos(IEnumerable<Full> full);
    }
}
