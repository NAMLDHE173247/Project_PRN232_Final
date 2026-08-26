namespace EbayClone.API.Models;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int SellerId { get; set; }
    public string ModerationStatus { get; set; } = "Active";
    public string? ModerationReason { get; set; }
    public int? ModeratedBy { get; set; }
    public DateTime? ModeratedAtUtc { get; set; }
    public User Seller { get; set; } = null!;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
