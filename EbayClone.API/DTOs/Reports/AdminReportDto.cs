namespace EbayClone.API.DTOs.Reports;

public record AdminReportDto(
    DateTime? From,
    DateTime? To,
    DateTime GeneratedAtUtc,
    int TotalUsers,
    int TotalProducts,
    int TotalOrders,
    decimal PaidRevenue,
    IReadOnlyList<ReportBreakdownDto> OrderStatuses,
    IReadOnlyList<ReportBreakdownDto> DisputeStatuses);

public record ReportBreakdownDto(string Label, int Count, decimal Amount);
