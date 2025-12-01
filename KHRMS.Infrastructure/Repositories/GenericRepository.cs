using KHRMS.Core;
using Microsoft.EntityFrameworkCore;

namespace KHRMS.Infrastructure
{
    public abstract class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        protected readonly KHRMSContextClass _dbContext;

        protected GenericRepository(KHRMSContextClass context)
        {
            _dbContext = context;
        }

        public async Task<T> GetById(long id)
        {
            return await _dbContext.Set<T>().FindAsync(id);
        }
      
        public async Task<IEnumerable<T>> GetAll()
        {
            return await _dbContext.Set<T>().ToListAsync();
        }

        public async Task Add(T entity)
        {
            await _dbContext.Set<T>().AddAsync(entity);
        }

        public void Delete(T entity)
        {
            _dbContext.Set<T>().Remove(entity);
        }

        public void Update(T entity)
        {
            _dbContext.Set<T>().Update(entity);
        }

        public async Task DeleteAsync(long id)
        {
            var attendanceRequest = await _dbContext.AttendanceRequests.FindAsync(id);
            if (attendanceRequest != null)
            {
                _dbContext.AttendanceRequests.Remove(attendanceRequest);
            }
        }

        public async Task<T> GetByIdAsync(long id)
        {
            return await _dbContext.Set<T>().FindAsync(id);
        }
        public async Task<T> GetByEmployeeIdAsync(long id)
        {
            return await _dbContext.Set<T>().FindAsync(id);
        }

        public  IQueryable<T> Query()
        {
            return  _dbContext.Set<T>();
        }
    }
}
