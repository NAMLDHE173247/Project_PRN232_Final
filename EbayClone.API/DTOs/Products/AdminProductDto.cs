namespace EbayClone.API.DTOs.Products;

public record AdminProductDto(
    int Id,
    string Name,
    decimal Price,
    int SellerId,
    string ModerationStatus,
    string? ModerationReason,
    int? ModeratedBy,
    DateTime? ModeratedAtUtc);
