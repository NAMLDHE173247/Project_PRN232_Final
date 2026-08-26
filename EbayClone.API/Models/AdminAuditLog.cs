namespace EbayClone.API.Models;

public class AdminAuditLog
{
    public int Id { get; set; }
    public int AdminUserId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string ResourceType { get; set; } = string.Empty;
    public int ResourceId { get; set; }
    public string? Reason { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
