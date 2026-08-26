namespace EbayClone.API.DTOs.Audit;

public record AdminAuditLogDto(
    int Id,
    int AdminUserId,
    string AdminName,
    string Action,
    string ResourceType,
    int ResourceId,
    string? Reason,
    DateTime CreatedAtUtc);

public record PagedAdminAuditLogDto(int Page, int PageSize, int Total, IReadOnlyList<AdminAuditLogDto> Items);
