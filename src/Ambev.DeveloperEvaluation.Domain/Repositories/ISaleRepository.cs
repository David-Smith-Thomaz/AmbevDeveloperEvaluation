using Ambev.DeveloperEvaluation.Domain.Entities;

namespace Ambev.DeveloperEvaluation.Domain.Repositories
{
    /// <summary>
    /// Repository interface for Sale entity operations.
    /// </summary>
    public interface ISaleRepository
    {
        /// <summary>
        /// Adds a new sale to the repository.
        /// </summary>
        /// <param name="sale">The sale to add.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A task that represents the asynchronous add operation.</returns>
        Task AddAsync(Sale sale, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves a sale by its unique identifier, including its associated items.
        /// </summary>
        /// <param name="id">The unique identifier of the sale.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The sale if found, null otherwise.</returns>
        Task<Sale?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates an existing sale in the repository.
        /// </summary>
        /// <param name="sale">The sale to update.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A task that represents the asynchronous update operation.</returns>
        Task UpdateAsync(Sale sale, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves a list of sales with optional pagination, ordering, and filtering.
        /// </summary>
        /// <param name="pageNumber">The page number (1-based).</param>
        /// <param name="pageSize">The number of items per page.</param>
        /// <param name="orderBy">Comma-separated list of properties to order by (e.g., "SaleDate desc, TotalAmount asc").</param>
        /// <param name="filter">String representing filter criteria (e.g., "CustomerName=Test*&_minTotalAmount=50").</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A task that represents the asynchronous operation, returning an enumerable collection of sales.</returns>
        Task<IEnumerable<Sale>> ListAsync(
            int pageNumber,
            int pageSize,
            string? orderBy = null,
            string? filter = null,
            CancellationToken cancellationToken = default
        );

        /// <summary>
        /// Counts the total number of sales based on optional filtering criteria.
        /// </summary>
        /// <param name="filter">String representing filter criteria.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A task that represents the asynchronous count operation, returning the total count.</returns>
        Task<int> CountAsync(string? filter = null, CancellationToken cancellationToken = default);
    }
}