using EbayClone.API.DTOs.Returns;
using EbayClone.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace EbayClone.API.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/admin/return-requests")]
public class AdminReturnRequestController(IAdminReturnRequestService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedReturnRequestResultDto>> GetPage(
        [FromQuery] string? status, [FromQuery] int? userId, [FromQuery] int? orderId,
        [FromQuery] DateTime? from, [FromQuery] DateTime? to,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        if (from.HasValue && to.HasValue && from.Value.Date > to.Value.Date)
            return BadRequest(new { message = "From date cannot be after To date." });
        return Ok(await service.GetPageAsync(status, userId, orderId, from, to, page, pageSize, cancellationToken));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ReturnRequestDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var request = await service.GetByIdAsync(id, cancellationToken);
        return request is null ? NotFound() : Ok(request);
    }

    [HttpPut("{id:int}/approve")]
    public Task<ActionResult<ReturnRequestDto>> Approve(int id, CancellationToken cancellationToken) =>
        ExecuteTransition(() => service.ApproveAsync(id, GetAdminId(), cancellationToken));

    [HttpPut("{id:int}/reject")]
    public Task<ActionResult<ReturnRequestDto>> Reject(int id, CancellationToken cancellationToken) =>
        ExecuteTransition(() => service.RejectAsync(id, GetAdminId(), cancellationToken));

    private static async Task<ActionResult<ReturnRequestDto>> ExecuteTransition(Func<Task<ReturnRequestDto?>> transition)
    {
        try
        {
            var request = await transition();
            return request is null ? new NotFoundResult() : new OkObjectResult(request);
        }
        catch (InvalidOperationException exception)
        {
            return new ConflictObjectResult(new { message = exception.Message });
        }
        catch (DbUpdateConcurrencyException)
        {
            return new ConflictObjectResult(new { message = "The return request state changed before this action completed." });
        }
    }

    private int GetAdminId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
