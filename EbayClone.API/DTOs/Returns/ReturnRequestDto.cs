namespace EbayClone.API.DTOs.Returns;

public record ReturnRequestDto(
    int Id,
    int? OrderId,
    int? UserId,
    string? UserName,
    string? UserEmail,
    string? Reason,
    string? Status,
    DateTime? CreatedAt,
    string? OrderStatus,
    decimal OrderTotal,
    DateTime? OrderDate,
    string? PaymentMethod,
    string? PaymentStatus,
    string? ShippingCarrier,
    string? TrackingNumber,
    string? ShippingStatus);
