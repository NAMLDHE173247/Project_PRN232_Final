using EbayClone.API.DTOs.Audit;

namespace EbayClone.API.Services;

public interface IAuditService
{
    Task<PagedAuditResultDto> GetPageAsync(
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
    Task<byte[]> ExportAsync(
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
