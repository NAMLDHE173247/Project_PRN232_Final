using EbayClone.MVC.Filters;
using EbayClone.MVC.Models;
using EbayClone.MVC.Services;
using Microsoft.AspNetCore.Mvc;

namespace EbayClone.MVC.Controllers;

[AdminSession]
public class AuditLogsController(AdminApiClient apiClient) : AdminMvcController
{
    public async Task<IActionResult> Index(string? search, string? action, string? resource, int? actorId, DateTime? from, DateTime? to, string? sort, string? direction, int page = 1, CancellationToken cancellationToken = default)
    {
        var query = $"api/admin/audit-logs?page={Math.Max(page, 1)}&pageSize=20";
        if (!string.IsNullOrWhiteSpace(search)) query += $"&search={Uri.EscapeDataString(search)}";
        if (!string.IsNullOrWhiteSpace(action)) query += $"&action={Uri.EscapeDataString(action)}";
        if (!string.IsNullOrWhiteSpace(resource)) query += $"&resource={Uri.EscapeDataString(resource)}";
        if (actorId.HasValue) query += $"&actorId={actorId.Value}";
        if (from.HasValue) query += $"&from={from.Value:yyyy-MM-dd}";
        if (to.HasValue) query += $"&to={to.Value:yyyy-MM-dd}";
        if (!string.IsNullOrWhiteSpace(sort)) query += $"&sort={Uri.EscapeDataString(sort)}";
        if (!string.IsNullOrWhiteSpace(direction)) query += $"&direction={Uri.EscapeDataString(direction)}";
        ViewBag.Search = search;
        ViewBag.Action = action;
        ViewBag.Resource = resource;
        ViewBag.ActorId = actorId;
        ViewBag.From = from;
        ViewBag.To = to;
        ViewBag.Sort = sort;
        ViewBag.Direction = direction;
        var model = await apiClient.GetAsync<PagedViewModel<AuditLogViewModel>>(query, cancellationToken);
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Export(string? search, string? action, string? resource, int? actorId, DateTime? from, DateTime? to, string? sort, string? direction, CancellationToken cancellationToken)
    {
        var query = "api/admin/audit-logs/export";
        var parameters = new List<string>();
        if (!string.IsNullOrWhiteSpace(search)) parameters.Add($"search={Uri.EscapeDataString(search)}");
        if (!string.IsNullOrWhiteSpace(action)) parameters.Add($"action={Uri.EscapeDataString(action)}");
        if (!string.IsNullOrWhiteSpace(resource)) parameters.Add($"resource={Uri.EscapeDataString(resource)}");
        if (actorId.HasValue) parameters.Add($"actorId={actorId.Value}");
        if (from.HasValue) parameters.Add($"from={from.Value:yyyy-MM-dd}");
        if (to.HasValue) parameters.Add($"to={to.Value:yyyy-MM-dd}");
        if (!string.IsNullOrWhiteSpace(sort)) parameters.Add($"sort={Uri.EscapeDataString(sort)}");
        if (!string.IsNullOrWhiteSpace(direction)) parameters.Add($"direction={Uri.EscapeDataString(direction)}");
        if (parameters.Count > 0) query += "?" + string.Join('&', parameters);
        var content = await apiClient.GetFileAsync(query, cancellationToken);
        return File(content,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"audit-logs-{DateTime.UtcNow:yyyyMMdd-HHmmss}.xlsx");
    }
}
