namespace EbayClone.API.DTOs.Dashboard;

public record DashboardDto(
    int TotalUsers,
    int TotalProducts,
    int TotalOrders,
    decimal Revenue,
    int ActiveUsers,
    int BannedUsers,
    int HiddenProducts,
    int PendingDisputes,
    IReadOnlyList<DashboardAlertDto> Alerts);

public record DashboardAlertDto(
    string Severity,
    string Title,
    string Message,
    string Controller,
    string Action);
