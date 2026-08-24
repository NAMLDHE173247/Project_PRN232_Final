using EbayClone.API.DTOs.Users;
using EbayClone.API.Models;
using EbayClone.API.Repositories;

namespace EbayClone.API.Services;

public class AdminUserService(IUserRepository userRepository, IAuditRepository auditRepository) : IAdminUserService
{
    public async Task<PagedResultDto<AdminUserDto>> GetUsersAsync(
        string? search,
        string? role,
        UserStatus? status,
        string? sort,
        string? direction,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);
        var result = await userRepository.GetPageAsync(search, role, status, sort, direction, page, pageSize, cancellationToken);
        return new PagedResultDto<AdminUserDto>(
            page,
            pageSize,
            result.Total,
            result.Items.Select(Map).ToList());
    }

    public async Task<AdminUserDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByIdAsync(id, cancellationToken);
        return user is null ? null : Map(user);
    }

    public async Task<AdminUserDto?> ApproveAsync(int userId, int adminId, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null) return null;
        if (user.Status != UserStatus.Pending)
            throw new InvalidOperationException("Only pending users can be approved.");

        user.Status = UserStatus.Active;
        user.ApprovalStatus = "Approved";
        user.ApprovedBy = adminId;
        user.ApprovedAt = DateTime.UtcNow;
        await userRepository.SaveChangesAsync(cancellationToken);
        await WriteAuditAsync(adminId, "APPROVE_USER", user.Id, cancellationToken);
        return Map(user);
    }

    public async Task<AdminUserDto?> BlockAsync(int userId, int adminId, string reason, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null) return null;
        if (user.Status != UserStatus.Active)
            throw new InvalidOperationException("Only active users can be banned.");

        user.Status = UserStatus.Banned;
        user.BannedReason = reason.Trim();
        user.BannedBy = adminId;
        user.BannedAt = DateTime.UtcNow;
        await userRepository.SaveChangesAsync(cancellationToken);
        await WriteAuditAsync(adminId, "BLOCK_USER", user.Id, cancellationToken);
        return Map(user);
    }

    public async Task<AdminUserDto?> UnblockAsync(int userId, int adminId, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null) return null;
        if (user.Status != UserStatus.Banned)
            throw new InvalidOperationException("Only banned users can be unblocked.");

        user.Status = UserStatus.Active;
        user.BannedReason = null;
        user.BannedBy = null;
        user.BannedAt = null;
        await userRepository.SaveChangesAsync(cancellationToken);
        await WriteAuditAsync(adminId, "UNBLOCK_USER", user.Id, cancellationToken);
        return Map(user);
    }

    private Task WriteAuditAsync(
        int adminId,
        string action,
        int userId,
        CancellationToken cancellationToken)
    {
        return auditRepository.AddAsync(new AuditLog
        {
            ActorId = adminId,
            Action = action,
            Resource = "USER",
            ResourceId = userId,
            CreatedAtUtc = DateTime.UtcNow
        }, cancellationToken);
    }

    private static AdminUserDto Map(User user) => new(
        user.Id,
        user.Email,
        user.FullName,
        user.Role,
        user.Status,
        user.ApprovalStatus,
        user.BannedReason,
        user.ApprovedAt,
        user.BannedAt);
}
