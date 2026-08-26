using System.Security.Claims;
using EbayClone.API.DTOs.Disputes;
using EbayClone.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EbayClone.API.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/admin/disputes")]
public class AdminDisputeController(IAdminDisputeService disputeService) : ControllerBase
{
    [HttpGet]
    public Task<PagedDisputeResultDto> GetDisputes(
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default) =>
        disputeService.GetDisputesAsync(status, page, pageSize, cancellationToken);

    [HttpGet("{id:int}")]
    public async Task<ActionResult<DisputeDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var dispute = await disputeService.GetByIdAsync(id, cancellationToken);
        return dispute is null ? NotFound() : Ok(dispute);
    }

    [HttpPut("{id:int}/resolve")]
    public Task<ActionResult<DisputeDto>> Resolve(
        int id,
        ResolveDisputeRequestDto request,
        CancellationToken cancellationToken) =>
        ExecuteTransition(() => disputeService.ResolveAsync(id, GetAdminId(), request.Resolution, cancellationToken));

    [HttpPut("{id:int}/reject")]
    public Task<ActionResult<DisputeDto>> Reject(
        int id,
        ResolveDisputeRequestDto request,
        CancellationToken cancellationToken) =>
        ExecuteTransition(() => disputeService.RejectAsync(id, GetAdminId(), request.Resolution, cancellationToken));

    [HttpPut("{id:int}/assign")]
    public Task<ActionResult<DisputeDto>> Assign(int id, AssignDisputeRequestDto request, CancellationToken cancellationToken) =>
        ExecuteTransition(() => disputeService.AssignAsync(id, GetAdminId(), request.AdminUserId, cancellationToken));

    [HttpPut("{id:int}/start-review")]
    public Task<ActionResult<DisputeDto>> StartReview(int id, CancellationToken cancellationToken) =>
        ExecuteTransition(() => disputeService.StartReviewAsync(id, GetAdminId(), cancellationToken));

    private static async Task<ActionResult<DisputeDto>> ExecuteTransition(Func<Task<DisputeDto?>> transition)
    {
        try
        {
            var dispute = await transition();
            return dispute is null ? new NotFoundResult() : new OkObjectResult(dispute);
        }
        catch (InvalidOperationException exception)
        {
            return new ConflictObjectResult(new { message = exception.Message });
        }
        catch (ArgumentException exception)
        {
            return new BadRequestObjectResult(new { message = exception.Message });
        }
        catch (DbUpdateConcurrencyException)
        {
            return new ConflictObjectResult(new { message = "The dispute state changed before this action completed." });
        }
    }

    private int GetAdminId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(value, out var adminId)
            ? adminId
            : throw new InvalidOperationException("Authenticated user id is missing.");
    }
}
