using EbayClone.API.Data;
using EbayClone.API.Models;
using Microsoft.EntityFrameworkCore;

namespace EbayClone.API.Repositories;

public class AuditRepository(AppDbContext dbContext) : IAuditRepository
{
    public async Task AddAsync(AuditLog auditLog, CancellationToken cancellationToken = default)
    {
        dbContext.AuditLogs.Add(auditLog);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<(int Total, IReadOnlyList<AuditLog> Items)> GetPageAsync(
        string? search,
        string? action,
        string? resource,
        int? actorId,
        DateTime? from,
        DateTime? to,
        string? sort,
        string? direction,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = BuildQuery(search, action, resource, actorId, from, to, sort, direction);
        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
        return (total, items);
    }

    public async Task<IReadOnlyList<AuditLog>> GetAllAsync(
        string? search = null,
        string? action = null,
        string? resource = null,
        int? actorId = null,
        DateTime? from = null,
        DateTime? to = null,
        string? sort = null,
        string? direction = null,
        CancellationToken cancellationToken = default)
    {
        return await BuildQuery(search, action, resource, actorId, from, to, sort, direction)
            .ToListAsync(cancellationToken);
    }

    private IQueryable<AuditLog> BuildQuery(
        string? search,
        string? action,
        string? resource,
        int? actorId,
        DateTime? from,
        DateTime? to,
        string? sort,
        string? direction)
    {
        var query = dbContext.AuditLogs.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var keyword = search.Trim();
            query = query.Where(log => log.Action.Contains(keyword) || log.Resource.Contains(keyword) || (log.Metadata != null && log.Metadata.Contains(keyword)));
        }
        if (!string.IsNullOrWhiteSpace(action)) query = query.Where(log => log.Action == action.Trim());
        if (!string.IsNullOrWhiteSpace(resource)) query = query.Where(log => log.Resource == resource.Trim());
        if (actorId.HasValue) query = query.Where(log => log.ActorId == actorId.Value);
        if (from.HasValue) query = query.Where(log => log.CreatedAtUtc >= from.Value.Date);
        if (to.HasValue) query = query.Where(log => log.CreatedAtUtc < to.Value.Date.AddDays(1));

        var descending = !string.Equals(direction, "asc", StringComparison.OrdinalIgnoreCase);
        return (sort?.Trim().ToLowerInvariant()) switch
        {
            "id" => descending ? query.OrderByDescending(log => log.Id).ThenByDescending(log => log.CreatedAtUtc) : query.OrderBy(log => log.Id).ThenBy(log => log.CreatedAtUtc),
            _ => descending ? query.OrderByDescending(log => log.CreatedAtUtc).ThenByDescending(log => log.Id) : query.OrderBy(log => log.CreatedAtUtc).ThenBy(log => log.Id)
        };
    }
}
