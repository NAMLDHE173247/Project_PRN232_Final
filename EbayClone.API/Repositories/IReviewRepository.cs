using EbayClone.API.Models;

namespace EbayClone.API.Repositories;

public interface IReviewRepository
{
    Task<(int Total, IReadOnlyList<Review> Items)> GetPageAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
    Task<Review?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
