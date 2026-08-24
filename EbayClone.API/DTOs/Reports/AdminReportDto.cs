namespace EbayClone.API.DTOs.Reports;

public record AdminReportDto(
    DateTime? From,
    DateTime? To,
    DateTime GeneratedAtUtc,
    int TotalUsers,
    int TotalProducts,
    int TotalOrders,
    decimal PaidRevenue,
    IReadOnlyList<ReportBreakdownDto> UserStatuses,
    IReadOnlyList<ReportBreakdownDto> ProductStatuses,
    IReadOnlyList<ReportBreakdownDto> OrderStatuses,
    IReadOnlyList<ReportBreakdownDto> DisputeStatuses,
    IReadOnlyList<ReportBreakdownDto> AuditActions);

public record ReportBreakdownDto(string Label, int Count, decimal Amount);
