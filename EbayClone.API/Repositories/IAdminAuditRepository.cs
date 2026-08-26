using EbayClone.API.Models;
using EbayClone.API.DTOs.Audit;

namespace EbayClone.API.Repositories;

public interface IAdminAuditRepository
{
    void Add(int adminUserId, string action, string resourceType, int resourceId, string? reason = null);
    Task<(int Total, IReadOnlyList<AdminAuditLogDto> Items)> GetPageAsync(string? action, string? resourceType, int page, int pageSize, CancellationToken cancellationToken = default);
}
