using EbayClone.MVC.Filters;
using EbayClone.MVC.Models;
using EbayClone.MVC.Services;
using Microsoft.AspNetCore.Mvc;

namespace EbayClone.MVC.Controllers;

[AdminSession]
public sealed class ReportsController(AdminApiClient apiClient) : AdminMvcController
{
    public async Task<IActionResult> Index(DateTime? from, DateTime? to, CancellationToken cancellationToken)
    {
        try
        {
            var query = "api/admin/reports/summary";
            var parameters = new List<string>();
            if (from.HasValue) parameters.Add($"from={from.Value:yyyy-MM-dd}");
            if (to.HasValue) parameters.Add($"to={to.Value:yyyy-MM-dd}");
            if (parameters.Count > 0) query += "?" + string.Join('&', parameters);
            ViewBag.From = from?.ToString("yyyy-MM-dd");
            ViewBag.To = to?.ToString("yyyy-MM-dd");
            return View(await apiClient.GetAsync<AdminReportViewModel>(query, cancellationToken));
        }
        catch (AdminApiException exception)
        {
            return HandleApiFailure(exception);
        }
    }
}
