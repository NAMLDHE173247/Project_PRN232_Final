using EbayClone.API.DTOs.Orders;
using EbayClone.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EbayClone.API.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/admin/orders")]
public class AdminOrderController(IAdminOrderService orderService) : ControllerBase
{
    [HttpGet]
    public Task<PagedOrderResultDto> GetOrders(
        [FromQuery] string? status,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int? buyerId,
        [FromQuery] string? sort,
        [FromQuery] string? direction,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default) =>
        orderService.GetOrdersAsync(status, from, to, buyerId, sort, direction, page, pageSize, cancellationToken);

    [HttpGet("{id:int}")]
    public async Task<ActionResult<OrderDetailAdminDto>> GetDetail(int id, CancellationToken cancellationToken)
    {
        var order = await orderService.GetDetailAsync(id, cancellationToken);
        return order is null ? NotFound() : Ok(order);
    }
}
