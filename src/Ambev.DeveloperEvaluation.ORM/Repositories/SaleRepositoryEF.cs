using Microsoft.EntityFrameworkCore;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using System.Linq.Dynamic.Core;

namespace Ambev.DeveloperEvaluation.ORM.Repositories
{
    /// <summary>
    /// Implementation of ISaleRepository using Entity Framework Core for sales data operations.
    /// </summary>
    public class SaleRepositoryEF : ISaleRepository
    {
        private readonly DefaultContext _context;

        /// <summary>
        /// Initializes a new instance of SaleRepositoryEF.
        /// </summary>
        /// <param name="context">The database context.</param>
        public SaleRepositoryEF(DefaultContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Adds a new sale to the database.
        /// </summary>
        /// <param name="sale">The sale entity to add.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A task that represents the asynchronous add operation.</returns>
        public async Task AddAsync(Sale sale, CancellationToken cancellationToken = default)
        {
            await _context.Sales.AddAsync(sale, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Retrieves a sale by its unique identifier, including its associated items.
        /// </summary>
        /// <param name="id">The unique identifier of the sale.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The sale if found, null otherwise.</returns>
        public async Task<Sale?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Sales
                                 .Include(s => s.SaleItems)
                                 .AsNoTracking()
                                 .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
        }

        /// <summary>
        /// Updates an existing sale in the database, including its items and handling concurrency.
        /// This method explicitly manages the state of the sale and its child items.
        /// </summary>
        /// <param name="sale">The sale entity with updated information.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A task that represents the asynchronous update operation.</returns>
        public async Task UpdateAsync(Sale sale, CancellationToken cancellationToken = default)
        {
            _context.Entry(sale).State = EntityState.Modified;

            var originalItemsInDb = await _context.SaleItems
                                                  .AsNoTracking()
                                                  .Where(si => si.SaleId == sale.Id)
                                                  .ToListAsync(cancellationToken);

            foreach (var originalItem in originalItemsInDb)
            {
                if (!sale.SaleItems.Any(newItem => newItem.Id == originalItem.Id))
                {
                    _context.Entry(originalItem).State = EntityState.Deleted;
                }
            }

            foreach (var incomingItem in sale.SaleItems)
            {
                if (incomingItem.Id == Guid.Empty || _context.Entry(incomingItem).State == EntityState.Detached)
                {
                    var existsInDb = originalItemsInDb.Any(oi => oi.Id == incomingItem.Id);

                    if (existsInDb)
                    {
                        _context.SaleItems.Attach(incomingItem);
                        _context.Entry(incomingItem).State = EntityState.Modified;
                        _context.Entry(incomingItem).CurrentValues.SetValues(incomingItem);
                    }
                    else
                    {
                        _context.SaleItems.Attach(incomingItem);
                        _context.Entry(incomingItem).State = EntityState.Added;
                        incomingItem.SetSaleId(sale.Id);
                    }
                }
                else if (_context.Entry(incomingItem).State == EntityState.Unchanged)
                {
                    _context.Entry(incomingItem).State = EntityState.Modified;
                }
            }

            await _context.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Retrieves a list of sales with optional pagination, ordering, and filtering.
        /// </summary>
        /// <param name="pageNumber">The page number (1-based).</param>
        /// <param name="pageSize">The number of items per page.</param>
        /// <param name="orderBy">Comma-separated list of properties to order by (e.g., "SaleDate desc, TotalAmount asc").</param>
        /// <param name="filter">String representing filter criteria (e.g., "CustomerName=Test*&_minTotalAmount=50").</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A task that represents the asynchronous operation, returning an enumerable collection of sales.</returns>
        public async Task<IEnumerable<Sale>> ListAsync(
            int pageNumber,
            int pageSize,
            string? orderBy = null,
            string? filter = null,
            CancellationToken cancellationToken = default
        )
        {
            IQueryable<Sale> query = _context.Sales.Include(s => s.SaleItems);

            if (!string.IsNullOrWhiteSpace(filter))
            {
                var predicates = new List<string>();
                var args = new List<object>();

                var filterSegments = filter.Split('&', StringSplitOptions.RemoveEmptyEntries);
                foreach (var segment in filterSegments)
                {
                    if (segment.Contains("="))
                    {
                        var parts = segment.Split('=', 2);
                        if (parts.Length == 2)
                        {
                            var field = parts[0].Trim();
                            var value = parts[1].Trim();

                            if (value.StartsWith("*") && value.EndsWith("*"))
                            {
                                predicates.Add($"{field}.Contains(@{args.Count})");
                                args.Add(value.Trim('*'));
                            }
                            else if (value.StartsWith("*"))
                            {
                                predicates.Add($"{field}.EndsWith(@{args.Count})");
                                args.Add(value.TrimStart('*'));
                            }
                            else if (value.EndsWith("*"))
                            {
                                predicates.Add($"{field}.StartsWith(@{args.Count})");
                                args.Add(value.TrimEnd('*'));
                            }
                            else
                            {
                                predicates.Add($"{field} == @{args.Count}");
                                args.Add(value);
                            }
                        }
                    }
                    else if (segment.StartsWith("_min", StringComparison.OrdinalIgnoreCase) ||
                             segment.StartsWith("_max", StringComparison.OrdinalIgnoreCase))
                    {
                        var operatorChar = segment.StartsWith("_min", StringComparison.OrdinalIgnoreCase) ? ">=" : "<=";
                        var parts = segment.Split('=', 2);
                        if (parts.Length == 2)
                        {
                            var fieldName = parts[0].Substring(4).Trim();
                            var valueString = parts[1].Trim();

                            object convertedValue;

                            if (decimal.TryParse(valueString, out var decVal))
                            {
                                convertedValue = decVal;
                            }
                            else if (DateTime.TryParse(valueString, out var dateVal))
                            {
                                convertedValue = dateVal;
                            }
                            else
                            {
                                convertedValue = valueString;
                            }

                            predicates.Add($"{fieldName} {operatorChar} @{args.Count}");
                            args.Add(convertedValue);
                        }
                    }
                }

                if (predicates.Any())
                {
                    try
                    {
                        query = query.Where(string.Join(" AND ", predicates), args.ToArray());
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Erro ao aplicar filtro para contagem: {ex.Message}");
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(orderBy))
            {
                try
                {
                    query = query.OrderBy(orderBy);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erro ao aplicar ordenação dinâmica: {ex.Message}");
                    query = query.OrderByDescending(s => s.CreatedAt);
                }
            }
            else
            {
                query = query.OrderByDescending(s => s.CreatedAt);
            }

            var skip = (pageNumber - 1) * pageSize;
            query = query.Skip(skip).Take(pageSize);

            return await query.ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Counts the total number of sales based on optional filtering criteria.
        /// </summary>
        /// <param name="filter">String representing filter criteria.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A task that represents the asynchronous count operation, returning the total count.</returns>
        public async Task<int> CountAsync(string? filter = null, CancellationToken cancellationToken = default)
        {
            IQueryable<Sale> query = _context.Sales;

            if (!string.IsNullOrWhiteSpace(filter))
            {
                var predicates = new List<string>();
                var args = new List<object>();

                var filterSegments = filter.Split('&', StringSplitOptions.RemoveEmptyEntries);
                foreach (var segment in filterSegments)
                {
                    if (segment.Contains("="))
                    {
                        var parts = segment.Split('=', 2);
                        if (parts.Length == 2)
                        {
                            var field = parts[0].Trim();
                            var value = parts[1].Trim();

                            if (value.StartsWith("*") && value.EndsWith("*"))
                            {
                                predicates.Add($"{field}.Contains(@{args.Count})");
                                args.Add(value.Trim('*'));
                            }
                            else if (value.StartsWith("*"))
                            {
                                predicates.Add($"{field}.EndsWith(@{args.Count})");
                                args.Add(value.TrimStart('*'));
                            }
                            else if (value.EndsWith("*"))
                            {
                                predicates.Add($"{field}.StartsWith(@{args.Count})");
                                args.Add(value.TrimEnd('*'));
                            }
                            else
                            {
                                predicates.Add($"{field} == @{args.Count}");
                                args.Add(value);
                            }
                        }
                    }
                    else if (segment.StartsWith("_min", StringComparison.OrdinalIgnoreCase) ||
                             segment.StartsWith("_max", StringComparison.OrdinalIgnoreCase))
                    {
                        var operatorChar = segment.StartsWith("_min", StringComparison.OrdinalIgnoreCase) ? ">=" : "<=";
                        var parts = segment.Split('=', 2);
                        if (parts.Length == 2)
                        {
                            var fieldName = parts[0].Substring(4).Trim();
                            var valueString = parts[1].Trim();

                            object convertedValue;

                            if (decimal.TryParse(valueString, out var decVal))
                            {
                                convertedValue = decVal;
                            }
                            else if (DateTime.TryParse(valueString, out var dateVal))
                            {
                                convertedValue = dateVal;
                            }
                            else
                            {
                                convertedValue = valueString;
                            }

                            predicates.Add($"{fieldName} {operatorChar} @{args.Count}");
                            args.Add(convertedValue);
                        }
                    }
                }

                if (predicates.Any())
                {
                    try
                    {
                        query = query.Where(string.Join(" AND ", predicates), args.ToArray());
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Erro ao aplicar filtro para contagem: {ex.Message}");
                    }
                }
            }

            return await query.CountAsync(cancellationToken);
        }
    }
}