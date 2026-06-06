using System.Threading.Tasks;

namespace Application.UseCases.Ports
{
    public interface IRepositoryPort<T> where T : class
    {
        Task<T?> CreateAsync(T entity);
        Task<T?> RetrieveAsync(T entity);
        Task<T?> DeleteAsync(T entity);
        Task<T?> UpdateAsync(T entity);
    }
}
