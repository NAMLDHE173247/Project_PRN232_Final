using EbayClone.API.DTOs.Audit;
using EbayClone.API.Repositories;

namespace EbayClone.API.Services;

public class AdminAuditService(IAdminAuditRepository repository) : IAdminAuditService
{
    public async Task<PagedAdminAuditLogDto> GetPageAsync(string? action, string? resourceType, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);
        var result = await repository.GetPageAsync(action, resourceType, page, pageSize, cancellationToken);
        return new(page, pageSize, result.Total, result.Items);
    }
}
