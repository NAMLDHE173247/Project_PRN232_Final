using EbayClone.MVC.Filters;
using EbayClone.MVC.Models;
using EbayClone.MVC.Services;
using Microsoft.AspNetCore.Mvc;

namespace EbayClone.MVC.Controllers;

[AdminSession]
public class ProductsController(AdminApiClient apiClient) : AdminMvcController
{
    public async Task<IActionResult> Index(string? search, int? sellerId, string? status, string? sort, string? direction, int page = 1, CancellationToken cancellationToken = default)
    {
        var query = $"api/admin/products?page={Math.Max(page, 1)}&pageSize=20";
        if (!string.IsNullOrWhiteSpace(search)) query += $"&search={Uri.EscapeDataString(search)}";
        if (sellerId.HasValue) query += $"&sellerId={sellerId.Value}";
        if (!string.IsNullOrWhiteSpace(status)) query += $"&status={Uri.EscapeDataString(status)}";
        if (!string.IsNullOrWhiteSpace(sort)) query += $"&sort={Uri.EscapeDataString(sort)}";
        if (!string.IsNullOrWhiteSpace(direction)) query += $"&direction={Uri.EscapeDataString(direction)}";
        ViewBag.Search = search;
        ViewBag.SellerId = sellerId;
        ViewBag.Status = status;
        ViewBag.Sort = sort;
        ViewBag.Direction = direction;
        return View(await apiClient.GetAsync<PagedViewModel<AdminProductViewModel>>(query, cancellationToken));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public Task<IActionResult> Hide(int id, string reason, CancellationToken cancellationToken) => RunAction(id, "hide", new { reason }, cancellationToken);

    [HttpPost, ValidateAntiForgeryToken]
    public Task<IActionResult> Restore(int id, CancellationToken cancellationToken) => RunAction(id, "restore", null, cancellationToken);

    private async Task<IActionResult> RunAction(int id, string action, object? body, CancellationToken cancellationToken)
    {
        try { await apiClient.PutAsync<AdminProductViewModel>($"api/admin/products/{id}/{action}", body, cancellationToken); TempData["Success"] = $"Product #{id}: {action}."; }
        catch (AdminApiException exception) { return HandleApiFailure(exception); }
        return RedirectToAction(nameof(Index));
    }

}
