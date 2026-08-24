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
    int ActiveUsers,
    int BannedUsers,
    int HiddenProducts,
    int PendingDisputes,
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
    IReadOnlyList<ReportBreakdownViewModel> UserStatuses,
    IReadOnlyList<ReportBreakdownViewModel> ProductStatuses,
    IReadOnlyList<ReportBreakdownViewModel> OrderStatuses,
    IReadOnlyList<ReportBreakdownViewModel> DisputeStatuses,
    IReadOnlyList<ReportBreakdownViewModel> AuditActions);

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
    string Status,
    string ApprovalStatus,
    string? BannedReason,
    DateTime? ApprovedAt,
    DateTime? BannedAt);

public record AdminProductViewModel(int Id, string Name, decimal Price, int SellerId, string Status);

public record AdminFeedbackViewModel(
    int Id,
    int? SellerId,
    decimal? AverageRating,
    int? TotalReviews,
    decimal? PositiveRate);

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
public record OrderPaymentViewModel(decimal Amount, string? Status, DateTime? PaidAt);
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
    string? AssignedAdminName,
    DateTime? AssignedAt,
    int? ResolvedBy,
    DateTime? ResolvedAt);

public record AuditLogViewModel(
    long Id,
    int? ActorId,
    string Action,
    string Resource,
    int? ResourceId,
    string? Metadata,
    DateTime CreatedAtUtc);
