using Ambev.DeveloperEvaluation.Domain.Entities;

namespace Ambev.DeveloperEvaluation.Domain.Repositories
{
    public interface ISaleRepository
    {
        Task AddAsync(Sale sale);
        Task<Sale?> GetByIdAsync(Guid id);
        Task UpdateAsync(Sale sale);
        Task<IEnumerable<Sale>> ListAsync(
            int pageNumber,
            int pageSize,
            string? orderBy = null,
            string? filter = null
        );
        Task<int> CountAsync(string? filter = null);
    }
}