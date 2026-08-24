using System.Security.Claims;
using EbayClone.API.DTOs.Users;
using EbayClone.API.Models;
using EbayClone.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
        [FromQuery] UserStatus? status,
        [FromQuery] string? sort,
        [FromQuery] string? direction,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        return userService.GetUsersAsync(search, role, status, sort, direction, page, pageSize, cancellationToken);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<AdminUserDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var user = await userService.GetByIdAsync(id, cancellationToken);
        return user is null ? NotFound() : Ok(user);
    }

    [HttpPut("{id:int}/approve")]
    public Task<ActionResult<AdminUserDto>> Approve(int id, CancellationToken cancellationToken)
    {
        return ExecuteTransition(
            () => userService.ApproveAsync(id, GetAdminId(), cancellationToken));
    }

    [HttpPut("{id:int}/block")]
    public Task<ActionResult<AdminUserDto>> Block(
        int id,
        BlockUserRequestDto request,
        CancellationToken cancellationToken)
    {
        return ExecuteTransition(
            () => userService.BlockAsync(id, GetAdminId(), request.Reason, cancellationToken));
    }

    [HttpPut("{id:int}/unblock")]
    public Task<ActionResult<AdminUserDto>> Unblock(int id, CancellationToken cancellationToken)
    {
        return ExecuteTransition(
            () => userService.UnblockAsync(id, GetAdminId(), cancellationToken));
    }

    private static async Task<ActionResult<AdminUserDto>> ExecuteTransition(
        Func<Task<AdminUserDto?>> transition)
    {
        try
        {
            var user = await transition();
            return user is null ? new NotFoundResult() : new OkObjectResult(user);
        }
        catch (InvalidOperationException exception)
        {
            return new BadRequestObjectResult(new { message = exception.Message });
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
