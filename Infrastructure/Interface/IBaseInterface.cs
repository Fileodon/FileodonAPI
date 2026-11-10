namespace Domain.Interface;

public interface IBaseInterface<T> where T : class
{
    Task<T?> GetByIdAsync(int id);
    Task<IQueryable<T>> GetAllAsync();
    Task<T> PostAsync(T entity);
    Task<T> PutAsync(T entity);
    Task<T> UpdateAsync(T entity);
    Task<T?> DeleteAsync(int id);
}