using EbayClone.API.DTOs.Audit;
using EbayClone.API.Repositories;

namespace EbayClone.API.Services;

public class AuditService(IAuditRepository auditRepository) : IAuditService
{
    public async Task<PagedAuditResultDto> GetPageAsync(
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
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);
        var result = await auditRepository.GetPageAsync(search, action, resource, actorId, from, to, sort, direction, page, pageSize, cancellationToken);
        var items = result.Items.Select(log => new AuditLogDto(
            log.Id,
            log.ActorId,
            log.Action,
            log.Resource,
            log.ResourceId,
            log.Metadata,
            log.CreatedAtUtc)).ToList();
        return new PagedAuditResultDto(page, pageSize, result.Total, items);
    }

    public async Task<byte[]> ExportAsync(
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
        var logs = await auditRepository.GetAllAsync(search, action, resource, actorId, from, to, sort, direction, cancellationToken);
        var items = logs.Select(log => new AuditLogDto(
            log.Id,
            log.ActorId,
            log.Action,
            log.Resource,
            log.ResourceId,
            log.Metadata,
            log.CreatedAtUtc));
        return AuditLogExcelExporter.Create(items);
    }
}
