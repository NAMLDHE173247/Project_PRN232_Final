using EbayClone.API.Data;
using EbayClone.API.DTOs.Returns;
using EbayClone.API.Models;
using Microsoft.EntityFrameworkCore;

namespace EbayClone.API.Repositories;

public class ReturnRequestRepository(AppDbContext dbContext) : IReturnRequestRepository
{
    public async Task<(int Total, IReadOnlyList<ReturnRequestDto> Items)> GetPageAsync(
        string? status, int? userId, int? orderId, DateTime? from, DateTime? to,
        int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = dbContext.ReturnRequests.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(status)) query = query.Where(item => item.Status == status.Trim());
        if (userId.HasValue) query = query.Where(item => item.UserId == userId.Value);
        if (orderId.HasValue) query = query.Where(item => item.OrderId == orderId.Value);
        if (from.HasValue) query = query.Where(item => item.CreatedAt >= from.Value.Date);
        if (to.HasValue) query = query.Where(item => item.CreatedAt < to.Value.Date.AddDays(1));

        var total = await query.CountAsync(cancellationToken);
        var items = await BuildDtoQuery(query.OrderByDescending(item => item.CreatedAt).ThenByDescending(item => item.Id))
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
        return (total, items);
    }

    public Task<ReturnRequestDto?> GetDtoByIdAsync(int id, CancellationToken cancellationToken = default) =>
        BuildDtoQuery(dbContext.ReturnRequests.AsNoTracking().Where(item => item.Id == id))
            .FirstOrDefaultAsync(cancellationToken);

    public Task<ReturnRequest?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        dbContext.ReturnRequests.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        dbContext.SaveChangesAsync(cancellationToken);

    private IQueryable<ReturnRequestDto> BuildDtoQuery(IQueryable<ReturnRequest> requests) =>
        from request in requests
        join user in dbContext.Users.AsNoTracking() on request.UserId equals (int?)user.Id into users
        from user in users.DefaultIfEmpty()
        join order in dbContext.Orders.AsNoTracking() on request.OrderId equals (int?)order.Id into orders
        from order in orders.DefaultIfEmpty()
        select new ReturnRequestDto(
            request.Id,
            request.OrderId,
            request.UserId,
            user == null ? null : user.FullName,
            user == null ? null : user.Email,
            request.Reason,
            request.Status,
            request.CreatedAt,
            order == null ? null : order.Status,
            order == null ? 0m : order.TotalPrice ?? 0m,
            order == null ? null : order.OrderDate,
            dbContext.Payments.AsNoTracking().Where(payment => payment.OrderId == request.OrderId).OrderByDescending(payment => payment.Id).Select(payment => payment.Method).FirstOrDefault(),
            dbContext.Payments.AsNoTracking().Where(payment => payment.OrderId == request.OrderId).OrderByDescending(payment => payment.Id).Select(payment => payment.Status).FirstOrDefault(),
            dbContext.ShippingInfos.AsNoTracking().Where(info => info.OrderId == request.OrderId).OrderByDescending(info => info.Id).Select(info => info.Carrier).FirstOrDefault(),
            dbContext.ShippingInfos.AsNoTracking().Where(info => info.OrderId == request.OrderId).OrderByDescending(info => info.Id).Select(info => info.TrackingNumber).FirstOrDefault(),
            dbContext.ShippingInfos.AsNoTracking().Where(info => info.OrderId == request.OrderId).OrderByDescending(info => info.Id).Select(info => info.Status).FirstOrDefault());
}
