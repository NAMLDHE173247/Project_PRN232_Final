namespace EbayClone.API.DTOs.Orders;

public record OrderDetailAdminDto(
    int OrderId,
    int? BuyerId,
    string? BuyerName,
    string? BuyerEmail,
    string? Status,
    decimal TotalAmount,
    DateTime? OrderDate,
    IReadOnlyList<OrderItemAdminDto> Items,
    IReadOnlyList<OrderPaymentAdminDto> Payments,
    IReadOnlyList<OrderShippingAdminDto> Shipping);

public record OrderItemAdminDto(
    int Id,
    int? ProductId,
    string? ProductName,
    int Quantity,
    decimal UnitPrice);

public record OrderPaymentAdminDto(
    decimal Amount,
    string? Method,
    string? Status,
    DateTime? PaidAt);

public record OrderShippingAdminDto(
    string? Carrier,
    string? TrackingNumber,
    string? Status,
    DateTime? EstimatedArrival);
