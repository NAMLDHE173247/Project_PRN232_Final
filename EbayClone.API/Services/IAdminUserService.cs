using EbayClone.API.DTOs.Users;
using EbayClone.API.Models;

namespace EbayClone.API.Services;

public interface IAdminUserService
{
    Task<PagedResultDto<AdminUserDto>> GetUsersAsync(
        string? search,
        string? role,
        UserStatus? status,
        string? sort,
        string? direction,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
    Task<AdminUserDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<AdminUserDto?> ApproveAsync(int userId, int adminId, CancellationToken cancellationToken = default);
    Task<AdminUserDto?> BlockAsync(int userId, int adminId, string reason, CancellationToken cancellationToken = default);
    Task<AdminUserDto?> UnblockAsync(int userId, int adminId, CancellationToken cancellationToken = default);
}
