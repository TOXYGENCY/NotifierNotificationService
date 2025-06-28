namespace Notifier.Notification.Service.Notification.Service.Domain.Interfaces.Repositories
{
    public interface ICrud<T, ID> where T : class where ID : struct
    {
        Task<IEnumerable<T>> GetAllAsync();

        Task<T> GetByIdAsync(ID id);

        Task AddAsync(T obj);

        Task UpdateAsync(T obj);

        Task DeleteAsync(ID id);
    }
}
