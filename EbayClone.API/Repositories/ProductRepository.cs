using EbayClone.API.Data;
using EbayClone.API.Models;
using Microsoft.EntityFrameworkCore;

namespace EbayClone.API.Repositories;

public class ProductRepository(AppDbContext dbContext) : IProductRepository
{
    public async Task<(int Total, IReadOnlyList<Product> Items)> GetPageAsync(
        string? search,
        int? sellerId,
        string? status,
        string? sort,
        string? direction,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.Products.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(product => product.Name.Contains(search.Trim()));
        if (sellerId.HasValue)
            query = query.Where(product => product.SellerId == sellerId.Value);
        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(product => product.ModerationStatus == status.Trim());

        var descending = string.Equals(direction, "desc", StringComparison.OrdinalIgnoreCase);
        var orderedQuery = (sort?.Trim().ToLowerInvariant()) switch
        {
            "name" => descending ? query.OrderByDescending(product => product.Name).ThenByDescending(product => product.Id) : query.OrderBy(product => product.Name).ThenBy(product => product.Id),
            "price" => descending ? query.OrderByDescending(product => product.Price).ThenByDescending(product => product.Id) : query.OrderBy(product => product.Price).ThenBy(product => product.Id),
            "seller" => descending ? query.OrderByDescending(product => product.SellerId).ThenByDescending(product => product.Id) : query.OrderBy(product => product.SellerId).ThenBy(product => product.Id),
            _ => descending ? query.OrderByDescending(product => product.Id) : query.OrderBy(product => product.Id)
        };

        var total = await orderedQuery.CountAsync(cancellationToken);
        var items = await orderedQuery.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return (total, items);
    }

    public Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        dbContext.Products.FirstOrDefaultAsync(product => product.Id == id, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
