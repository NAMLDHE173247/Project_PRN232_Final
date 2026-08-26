namespace EbayClone.API.Models;

public class User
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = "User";
    public string ModerationStatus { get; set; } = "Active";
    public string? ModerationReason { get; set; }
    public int? ModeratedBy { get; set; }
    public DateTime? ModeratedAtUtc { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public ICollection<Product> Products { get; set; } = new List<Product>();
    public ICollection<OrderTable> Orders { get; set; } = new List<OrderTable>();
}
