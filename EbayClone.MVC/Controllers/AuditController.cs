using EbayClone.MVC.Filters;
using EbayClone.MVC.Models;
using EbayClone.MVC.Services;
using Microsoft.AspNetCore.Mvc;

namespace EbayClone.MVC.Controllers;

[AdminSession]
public class AuditController(AdminApiClient apiClient) : AdminMvcController
{
    public async Task<IActionResult> Index(string? auditAction, string? resourceType, int page = 1, CancellationToken cancellationToken = default)
    {
        var query = $"api/admin/audit?page={Math.Max(page, 1)}&pageSize=30";
        if (!string.IsNullOrWhiteSpace(auditAction)) query += $"&action={Uri.EscapeDataString(auditAction)}";
        if (!string.IsNullOrWhiteSpace(resourceType)) query += $"&resourceType={Uri.EscapeDataString(resourceType)}";
        ViewBag.AuditAction = auditAction; ViewBag.ResourceType = resourceType;
        return View(await apiClient.GetAsync<PagedViewModel<AdminAuditLogViewModel>>(query, cancellationToken));
    }
}
