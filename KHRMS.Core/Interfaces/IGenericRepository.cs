namespace KHRMS.Core
{
    public interface IGenericRepository<T> where T : class
    {
        Task<T> GetById(long id);
        Task<IEnumerable<T>> GetAll();
        Task Add(T entity);
        void Delete(T entity);
        void Update(T entity);
        Task DeleteAsync(long id);
        Task<T> GetByEmployeeIdAsync(long employeeId);

        Task<T> GetByIdAsync(long id);
        IQueryable<T> Query();

    }
}
