using EbayClone.API.DTOs.Users;

namespace EbayClone.API.Services;

public interface IAdminUserService
{
    Task<PagedResultDto<AdminUserDto>> GetUsersAsync(
        string? search,
        string? role,
        string? status,
        string? sort,
        string? direction,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
    Task<AdminUserDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<AdminUserDto?> ApproveAsync(int id, int adminId, CancellationToken cancellationToken = default);
    Task<AdminUserDto?> BanAsync(int id, int adminId, string reason, CancellationToken cancellationToken = default);
    Task<AdminUserDto?> UnbanAsync(int id, int adminId, CancellationToken cancellationToken = default);
}
