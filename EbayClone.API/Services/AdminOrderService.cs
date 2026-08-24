using EbayClone.API.DTOs.Orders;
using EbayClone.API.Repositories;

namespace EbayClone.API.Services;

public class AdminOrderService(IOrderRepository orderRepository) : IAdminOrderService
{
    public async Task<PagedOrderResultDto> GetOrdersAsync(
        string? status,
        DateTime? from,
        DateTime? to,
        int? buyerId,
        string? sort,
        string? direction,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);
        var result = await orderRepository.GetPageAsync(
            status,
            from,
            to,
            buyerId,
            sort,
            direction,
            page,
            pageSize,
            cancellationToken);
        return new PagedOrderResultDto(page, pageSize, result.Total, result.Items);
    }

    public Task<OrderDetailAdminDto?> GetDetailAsync(int id, CancellationToken cancellationToken = default) =>
        orderRepository.GetDetailAsync(id, cancellationToken);
}
