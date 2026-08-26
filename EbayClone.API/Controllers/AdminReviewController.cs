using EbayClone.API.DTOs.Reviews;
using EbayClone.API.DTOs.Moderation;
using System.Security.Claims;
using EbayClone.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EbayClone.API.Controllers;

/// <summary>Moderation endpoints for product reviews.</summary>
[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/admin/reviews")]
public class AdminReviewController(IAdminReviewService reviewService) : ControllerBase
{
    [HttpGet]
    public Task<PagedReviewResultDto> GetReviews(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default) =>
        reviewService.GetReviewsAsync(page, pageSize, cancellationToken);

    [HttpGet("{id:int}")]
    public async Task<ActionResult<AdminReviewDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var review = await reviewService.GetByIdAsync(id, cancellationToken);
        return review is null ? NotFound() : Ok(review);
    }

    [HttpPut("{id:int}/hide")]
    public Task<ActionResult<AdminReviewDto>> Hide(int id, ModerationReasonRequestDto request, CancellationToken cancellationToken) =>
        ExecuteTransition(() => reviewService.HideAsync(id, GetAdminId(), request.Reason, cancellationToken));

    [HttpPut("{id:int}/restore")]
    public Task<ActionResult<AdminReviewDto>> Restore(int id, CancellationToken cancellationToken) =>
        ExecuteTransition(() => reviewService.RestoreAsync(id, GetAdminId(), cancellationToken));

    private static async Task<ActionResult<AdminReviewDto>> ExecuteTransition(Func<Task<AdminReviewDto?>> transition)
    {
        try { var review = await transition(); return review is null ? new NotFoundResult() : new OkObjectResult(review); }
        catch (InvalidOperationException exception) { return new ConflictObjectResult(new { message = exception.Message }); }
        catch (DbUpdateConcurrencyException) { return new ConflictObjectResult(new { message = "The review state changed before this action completed." }); }
    }

    private int GetAdminId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

}
