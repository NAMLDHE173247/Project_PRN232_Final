using EbayClone.API.DTOs.Audit;

namespace EbayClone.API.Services;

public interface IAdminAuditService
{
    Task<PagedAdminAuditLogDto> GetPageAsync(string? action, string? resourceType, int page, int pageSize, CancellationToken cancellationToken = default);
}
