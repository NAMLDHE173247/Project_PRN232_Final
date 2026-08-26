using EbayClone.API.DTOs.Users;
using EbayClone.API.DTOs.Moderation;
using System.Security.Claims;
using EbayClone.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EbayClone.API.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/admin/users")]
public class AdminUserController(IAdminUserService userService) : ControllerBase
{
    [HttpGet]
    public Task<PagedResultDto<AdminUserDto>> GetUsers(
        [FromQuery] string? search,
        [FromQuery] string? role,
        [FromQuery] string? status,
        [FromQuery] string? sort,
        [FromQuery] string? direction,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        return userService.GetUsersAsync(search, role, status, sort, direction, page, pageSize, cancellationToken);
    }

    [HttpPut("{id:int}/approve")]
    public Task<ActionResult<AdminUserDto>> Approve(int id, CancellationToken cancellationToken) =>
        ExecuteTransition(() => userService.ApproveAsync(id, GetAdminId(), cancellationToken));

    [HttpPut("{id:int}/ban")]
    public Task<ActionResult<AdminUserDto>> Ban(int id, ModerationReasonRequestDto request, CancellationToken cancellationToken) =>
        ExecuteTransition(() => userService.BanAsync(id, GetAdminId(), request.Reason, cancellationToken));

    [HttpPut("{id:int}/unban")]
    public Task<ActionResult<AdminUserDto>> Unban(int id, CancellationToken cancellationToken) =>
        ExecuteTransition(() => userService.UnbanAsync(id, GetAdminId(), cancellationToken));

    private static async Task<ActionResult<AdminUserDto>> ExecuteTransition(Func<Task<AdminUserDto?>> transition)
    {
        try { var user = await transition(); return user is null ? new NotFoundResult() : new OkObjectResult(user); }
        catch (InvalidOperationException exception) { return new ConflictObjectResult(new { message = exception.Message }); }
        catch (DbUpdateConcurrencyException) { return new ConflictObjectResult(new { message = "The user state changed before this action completed." }); }
    }

    private int GetAdminId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet("{id:int}")]
    public async Task<ActionResult<AdminUserDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var user = await userService.GetByIdAsync(id, cancellationToken);
        return user is null ? NotFound() : Ok(user);
    }

}
