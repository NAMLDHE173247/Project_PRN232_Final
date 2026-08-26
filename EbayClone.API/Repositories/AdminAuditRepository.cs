using EbayClone.API.Data;
using EbayClone.API.Models;
using EbayClone.API.DTOs.Audit;
using Microsoft.EntityFrameworkCore;

namespace EbayClone.API.Repositories;

public class AdminAuditRepository(AppDbContext dbContext) : IAdminAuditRepository
{
    public void Add(int adminUserId, string action, string resourceType, int resourceId, string? reason = null) =>
        dbContext.AdminAuditLogs.Add(new AdminAuditLog
        {
            AdminUserId = adminUserId,
            Action = action,
            ResourceType = resourceType,
            ResourceId = resourceId,
            Reason = reason,
            CreatedAtUtc = DateTime.UtcNow
        });

    public async Task<(int Total, IReadOnlyList<AdminAuditLogDto> Items)> GetPageAsync(string? action, string? resourceType, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var logs = dbContext.AdminAuditLogs.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(action)) logs = logs.Where(x => x.Action == action.Trim());
        if (!string.IsNullOrWhiteSpace(resourceType)) logs = logs.Where(x => x.ResourceType == resourceType.Trim());
        var total = await logs.CountAsync(cancellationToken);
        var items = await (
            from log in logs
            join admin in dbContext.Users.AsNoTracking() on log.AdminUserId equals admin.Id
            orderby log.CreatedAtUtc descending, log.Id descending
            select new AdminAuditLogDto(log.Id, log.AdminUserId, admin.FullName, log.Action, log.ResourceType, log.ResourceId, log.Reason, log.CreatedAtUtc))
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return (total, items);
    }
}
