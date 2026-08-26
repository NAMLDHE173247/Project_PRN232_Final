using EbayClone.API.Data;
using EbayClone.API.Models;
using Microsoft.EntityFrameworkCore;

namespace EbayClone.API.Repositories;

public class ReviewRepository(AppDbContext dbContext) : IReviewRepository
{
    public async Task<(int Total, IReadOnlyList<Review> Items)> GetPageAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.Reviews.AsNoTracking().OrderByDescending(review => review.Id);

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (total, items);
    }

    public Task<Review?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        dbContext.Reviews.SingleOrDefaultAsync(review => review.Id == id, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
