namespace EbayClone.MVC.Models;

public record ReportSectionViewModel(string Title, IReadOnlyList<ReportBreakdownViewModel> Items, bool ShowAmount);
