using System.Collections.Generic;
using System.Threading.Tasks;

using Bet.CleanArchitecture.Core.Entities;

namespace Bet.CleanArchitecture.Core.Interfaces
{
    public interface IAsyncRepository<TRepository, TKey> where TRepository : BaseEntity<TKey>
    {
        Task<TRepository> GetByIdAsync(int id);

        Task<IReadOnlyList<TRepository>> ListAllAsync();

        Task<IReadOnlyList<TRepository>> ListAsync(ISpecification<TRepository> spec);

        Task<TRepository> AddAsync(TRepository entity);

        Task UpdateAsync(TRepository entity);

        Task DeleteAsync(TRepository entity);

        Task<int> CountAsync(ISpecification<TRepository> spec);
    }
}
