using System.Linq.Expressions;

namespace AngularAuthApi.Core.Repository
{
    
        public interface IRepository<T> where T : class
        {
            Task<int> InsertAsync(T entity);
            Task<T> UpdateAsync(T entity);
            Task<T> DeleteAsync(int id);
            Task<IEnumerable<T>> GetAllAsync();
            Task<IEnumerable<T>> GetByConditionAsync(Expression<Func<T, bool>> filter);
            Task<T> GetByIdAsync(int id);
            Task<T> AddAsync(T entity);
            Task<int> SaveChangesAsync();

        }
    }

