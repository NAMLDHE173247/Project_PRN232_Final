using System.ComponentModel.DataAnnotations;

namespace EbayClone.MVC.Models;

public class LoginInputModel
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
}

public record LoginResponseModel(string Token, int UserId, string Email, string Role);

public record DashboardViewModel(
    int TotalUsers,
    int TotalProducts,
    int TotalOrders,
    decimal Revenue,
    int PendingDisputes,
    int PendingUsers,
    int HiddenProducts,
    int HiddenReviews,
    IReadOnlyList<DashboardAlertViewModel> Alerts);

public record DashboardAlertViewModel(string Severity, string Title, string Message, string Controller, string Action);

public record AdminReportViewModel(
    DateTime? From,
    DateTime? To,
    DateTime GeneratedAtUtc,
    int TotalUsers,
    int TotalProducts,
    int TotalOrders,
    decimal PaidRevenue,
    IReadOnlyList<ReportBreakdownViewModel> OrderStatuses,
    IReadOnlyList<ReportBreakdownViewModel> DisputeStatuses);

public record ReportBreakdownViewModel(string Label, int Count, decimal Amount);

public record PagedViewModel<T>(int Page, int PageSize, int Total, IReadOnlyList<T> Items)
{
    public int TotalPages => Math.Max(1, (int)Math.Ceiling(Total / (double)PageSize));
}

public record AdminUserViewModel(
    int Id,
    string Email,
    string FullName,
    string Role,
    string ModerationStatus,
    string? ModerationReason,
    int? ModeratedBy,
    DateTime? ModeratedAtUtc);

public record AdminProductViewModel(int Id, string Name, decimal Price, int SellerId, string ModerationStatus, string? ModerationReason, int? ModeratedBy, DateTime? ModeratedAtUtc);

public record AdminFeedbackViewModel(
    int Id,
    int? SellerId,
    decimal? AverageRating,
    int? TotalReviews,
    decimal? PositiveRate);

public record AdminReviewViewModel(
    int Id,
    int? ProductId,
    int? ReviewerId,
    int? Rating,
    string? Comment,
    DateTime? CreatedAt,
    string ModerationStatus,
    string? ModerationReason,
    int? ModeratedBy,
    DateTime? ModeratedAtUtc);

public record OrderViewModel(
    int OrderId,
    int? BuyerId,
    string? BuyerName,
    string? Status,
    decimal TotalAmount,
    DateTime? OrderDate);

public record OrderDetailViewModel(
    int OrderId,
    int? BuyerId,
    string? BuyerName,
    string? BuyerEmail,
    string? Status,
    decimal TotalAmount,
    DateTime? OrderDate,
    IReadOnlyList<OrderItemViewModel> Items,
    IReadOnlyList<OrderPaymentViewModel> Payments,
    IReadOnlyList<OrderShippingViewModel> Shipping);

public record OrderItemViewModel(int Id, int? ProductId, string? ProductName, int Quantity, decimal UnitPrice);
public record OrderPaymentViewModel(decimal Amount, string? Method, string? Status, DateTime? PaidAt);
public record OrderShippingViewModel(string? Carrier, string? TrackingNumber, string? Status, DateTime? EstimatedArrival);

public record DisputeViewModel(
    int Id,
    int? OrderId,
    int? RaisedBy,
    string? RaisedByName,
    string? Description,
    string? Status,
    string? Resolution,
    int? AssignedTo,
    string? AssignedToName,
    DateTime? AssignedAtUtc,
    DateTime? ReviewStartedAtUtc,
    int? ResolvedBy,
    DateTime? ResolvedAtUtc);

public record AdminAuditLogViewModel(int Id, int AdminUserId, string AdminName, string Action, string ResourceType, int ResourceId, string? Reason, DateTime CreatedAtUtc);

public record ReturnRequestViewModel(
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
