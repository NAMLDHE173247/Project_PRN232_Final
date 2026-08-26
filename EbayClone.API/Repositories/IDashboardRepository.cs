namespace EbayClone.API.Repositories;

public interface IDashboardRepository
{
    Task<int> CountUsersAsync(CancellationToken cancellationToken = default);
    Task<int> CountProductsAsync(CancellationToken cancellationToken = default);
    Task<int> CountOrdersAsync(CancellationToken cancellationToken = default);
    Task<decimal> SumRevenueAsync(CancellationToken cancellationToken = default);
    Task<int> CountPendingDisputesAsync(CancellationToken cancellationToken = default);
    Task<int> CountPendingUsersAsync(CancellationToken cancellationToken = default);
    Task<int> CountHiddenProductsAsync(CancellationToken cancellationToken = default);
    Task<int> CountHiddenReviewsAsync(CancellationToken cancellationToken = default);
}
