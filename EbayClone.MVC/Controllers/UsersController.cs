using EbayClone.MVC.Filters;
using EbayClone.MVC.Models;
using EbayClone.MVC.Services;
using Microsoft.AspNetCore.Mvc;

namespace EbayClone.MVC.Controllers;

[AdminSession]
public class UsersController(AdminApiClient apiClient, AdminNotificationService notifications) : AdminMvcController
{
    public async Task<IActionResult> Index(string? search, string? role, string? status, string? sort, string? direction, int page = 1, CancellationToken cancellationToken = default)
    {
        var query = $"api/admin/users?page={Math.Max(page, 1)}&pageSize=20";
        if (!string.IsNullOrWhiteSpace(search)) query += $"&search={Uri.EscapeDataString(search)}";
        if (!string.IsNullOrWhiteSpace(role)) query += $"&role={Uri.EscapeDataString(role)}";
        if (!string.IsNullOrWhiteSpace(status)) query += $"&status={Uri.EscapeDataString(status)}";
        if (!string.IsNullOrWhiteSpace(sort)) query += $"&sort={Uri.EscapeDataString(sort)}";
        if (!string.IsNullOrWhiteSpace(direction)) query += $"&direction={Uri.EscapeDataString(direction)}";
        ViewBag.Search = search;
        ViewBag.Role = role;
        ViewBag.Status = status;
        ViewBag.Sort = sort;
        ViewBag.Direction = direction;
        return View(await apiClient.GetAsync<PagedViewModel<AdminUserViewModel>>(query, cancellationToken));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public Task<IActionResult> Approve(int id, CancellationToken cancellationToken) =>
        RunAction($"api/admin/users/{id}/approve", null, cancellationToken);

    [HttpPost, ValidateAntiForgeryToken]
    public Task<IActionResult> Block(int id, string reason, CancellationToken cancellationToken) =>
        RunAction($"api/admin/users/{id}/block", new { reason }, cancellationToken);

    [HttpPost, ValidateAntiForgeryToken]
    public Task<IActionResult> Unblock(int id, CancellationToken cancellationToken) =>
        RunAction($"api/admin/users/{id}/unblock", null, cancellationToken);

    private async Task<IActionResult> RunAction(string path, object? body, CancellationToken cancellationToken)
    {
        try
        {
            await apiClient.PutAsync<AdminUserViewModel>(path, body, cancellationToken);
            TempData["Success"] = "Cập nhật người dùng thành công.";
            await notifications.BroadcastAsync("Danh sách người dùng đã được cập nhật.", cancellationToken: cancellationToken);
            return RedirectToAction(nameof(Index));
        }
        catch (AdminApiException exception)
        {
            return HandleApiFailure(exception);
        }
    }
}
