using EbayClone.API.Models;

namespace EbayClone.API.Repositories;

public interface IAuditRepository
{
    Task AddAsync(AuditLog auditLog, CancellationToken cancellationToken = default);
    Task<(int Total, IReadOnlyList<AuditLog> Items)> GetPageAsync(
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
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AuditLog>> GetAllAsync(
        string? search = null,
        string? action = null,
        string? resource = null,
        int? actorId = null,
        DateTime? from = null,
        DateTime? to = null,
        string? sort = null,
        string? direction = null,
        CancellationToken cancellationToken = default);
}
