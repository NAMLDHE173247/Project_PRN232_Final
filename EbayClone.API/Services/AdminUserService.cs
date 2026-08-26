using EbayClone.API.DTOs.Users;
using EbayClone.API.Models;
using EbayClone.API.Repositories;

namespace EbayClone.API.Services;

public class AdminUserService(IUserRepository userRepository, IAdminAuditRepository auditRepository) : IAdminUserService
{
    public async Task<PagedResultDto<AdminUserDto>> GetUsersAsync(
        string? search, string? role, string? status, string? sort, string? direction,
        int page, int pageSize, CancellationToken cancellationToken = default)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);
        var result = await userRepository.GetPageAsync(search, role, status, sort, direction, page, pageSize, cancellationToken);
        return new(page, pageSize, result.Total, result.Items.Select(Map).ToList());
    }

    public async Task<AdminUserDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByIdAsync(id, cancellationToken);
        return user is null ? null : Map(user);
    }

    public Task<AdminUserDto?> ApproveAsync(int id, int adminId, CancellationToken cancellationToken = default) =>
        ChangeStatusAsync(id, adminId, "Pending", "Active", "APPROVE_USER", null, cancellationToken);

    public Task<AdminUserDto?> BanAsync(int id, int adminId, string reason, CancellationToken cancellationToken = default) =>
        ChangeStatusAsync(id, adminId, "Active", "Banned", "BAN_USER", reason.Trim(), cancellationToken);

    public Task<AdminUserDto?> UnbanAsync(int id, int adminId, CancellationToken cancellationToken = default) =>
        ChangeStatusAsync(id, adminId, "Banned", "Active", "UNBAN_USER", null, cancellationToken);

    private async Task<AdminUserDto?> ChangeStatusAsync(int id, int adminId, string expected, string next, string action, string? reason, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(id, cancellationToken);
        if (user is null) return null;
        if (user.Role == "Admin") throw new InvalidOperationException("Admin accounts cannot be moderated through user operations.");
        if (user.ModerationStatus != expected) throw new InvalidOperationException($"Only {expected} users can perform this transition.");
        user.ModerationStatus = next;
        user.ModerationReason = reason;
        user.ModeratedBy = adminId;
        user.ModeratedAtUtc = DateTime.UtcNow;
        auditRepository.Add(adminId, action, "User", id, reason);
        await userRepository.SaveChangesAsync(cancellationToken);
        return Map(user);
    }

    private static AdminUserDto Map(User user) =>
        new(user.Id, user.Email, user.FullName, user.Role, user.ModerationStatus, user.ModerationReason, user.ModeratedBy, user.ModeratedAtUtc);
}
