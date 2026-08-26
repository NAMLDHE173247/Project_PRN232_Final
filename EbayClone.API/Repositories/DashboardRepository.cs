using EbayClone.API.Data;
using EbayClone.API.Models;
using Microsoft.EntityFrameworkCore;

namespace EbayClone.API.Repositories;

public class DashboardRepository(AppDbContext dbContext) : IDashboardRepository
{
    public Task<int> CountUsersAsync(CancellationToken cancellationToken = default) =>
        dbContext.Users.CountAsync(cancellationToken);

    public Task<int> CountProductsAsync(CancellationToken cancellationToken = default) =>
        dbContext.Products.CountAsync(cancellationToken);

    public Task<int> CountOrdersAsync(CancellationToken cancellationToken = default) =>
        dbContext.Orders.CountAsync(cancellationToken);

    public async Task<decimal> SumRevenueAsync(CancellationToken cancellationToken = default) =>
        await dbContext.Payments
            .Where(payment => payment.Status == "Paid")
            .SumAsync(payment => (decimal?)payment.Amount, cancellationToken) ?? 0m;

    public Task<int> CountPendingDisputesAsync(CancellationToken cancellationToken = default) =>
        dbContext.Disputes.CountAsync(dispute => dispute.Status == nameof(DisputeStatus.Open), cancellationToken);

    public Task<int> CountPendingUsersAsync(CancellationToken cancellationToken = default) => dbContext.Users.CountAsync(x => x.ModerationStatus == "Pending", cancellationToken);
    public Task<int> CountHiddenProductsAsync(CancellationToken cancellationToken = default) => dbContext.Products.CountAsync(x => x.ModerationStatus == "Hidden", cancellationToken);
    public Task<int> CountHiddenReviewsAsync(CancellationToken cancellationToken = default) => dbContext.Reviews.CountAsync(x => x.ModerationStatus == "Hidden", cancellationToken);
}
