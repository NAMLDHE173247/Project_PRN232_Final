using EbayClone.API.Data;
using EbayClone.API.DTOs.Reports;
using Microsoft.EntityFrameworkCore;

namespace EbayClone.API.Services;

public sealed class AdminReportService(AppDbContext dbContext)
{
    public async Task<AdminReportDto> GetAsync(DateTime? from, DateTime? to, CancellationToken cancellationToken = default)
    {
        var fromUtc = from?.Date;
        var toExclusiveUtc = to?.Date.AddDays(1);
        var orders = dbContext.Orders.AsNoTracking().AsQueryable();
        if (fromUtc.HasValue) orders = orders.Where(order => order.OrderDate >= fromUtc.Value);
        if (toExclusiveUtc.HasValue) orders = orders.Where(order => order.OrderDate < toExclusiveUtc.Value);

        var payments = dbContext.Payments.AsNoTracking().Where(payment => payment.Status == "Paid");
        if (fromUtc.HasValue) payments = payments.Where(payment => payment.PaidAt >= fromUtc.Value);
        if (toExclusiveUtc.HasValue) payments = payments.Where(payment => payment.PaidAt < toExclusiveUtc.Value);

        var orderRows = await orders
            .Select(order => new { order.Status, Amount = order.TotalPrice ?? 0m })
            .ToListAsync(cancellationToken);
        var orderStatuses = orderRows
            .GroupBy(order => order.Status ?? "Unknown")
            .Select(group => new ReportBreakdownDto(group.Key, group.Count(), group.Sum(order => order.Amount)))
            .OrderByDescending(item => item.Count)
            .ToList();
        var paidRevenue = await payments.SumAsync(payment => (decimal?)payment.Amount, cancellationToken) ?? 0m;

        var disputeRows = await dbContext.Disputes.AsNoTracking()
            .Select(dispute => dispute.Status)
            .ToListAsync(cancellationToken);
        var disputeStatuses = disputeRows
            .GroupBy(status => status ?? "Unknown")
            .Select(group => new ReportBreakdownDto(group.Key, group.Count(), 0m))
            .OrderByDescending(item => item.Count)
            .ToList();
        return new AdminReportDto(
            from,
            to,
            DateTime.UtcNow,
            await dbContext.Users.CountAsync(cancellationToken),
            await dbContext.Products.CountAsync(cancellationToken),
            await orders.CountAsync(cancellationToken),
            paidRevenue,
            orderStatuses,
            disputeStatuses);
    }
}
