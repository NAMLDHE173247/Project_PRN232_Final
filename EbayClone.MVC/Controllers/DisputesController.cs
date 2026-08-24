using EbayClone.MVC.Filters;
using EbayClone.MVC.Models;
using EbayClone.MVC.Services;
using Microsoft.AspNetCore.Mvc;

namespace EbayClone.MVC.Controllers;

[AdminSession]
public class DisputesController(AdminApiClient apiClient, AdminNotificationService notifications) : AdminMvcController
{
    public async Task<IActionResult> Index(string? status, int page = 1, CancellationToken cancellationToken = default)
    {
        var query = $"api/admin/disputes?page={Math.Max(page, 1)}&pageSize=20";
        if (!string.IsNullOrWhiteSpace(status)) query += $"&status={Uri.EscapeDataString(status)}";
        ViewBag.Status = status;
        return View(await apiClient.GetAsync<PagedViewModel<DisputeViewModel>>(query, cancellationToken));
    }

    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        return View(await apiClient.GetAsync<DisputeViewModel>($"api/admin/disputes/{id}", cancellationToken));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public Task<IActionResult> Assign(int id, CancellationToken cancellationToken) =>
        RunAction(id, "assign", new { adminId = (int?)null }, cancellationToken);

    [HttpPost, ValidateAntiForgeryToken]
    public Task<IActionResult> Resolve(int id, string resolution, CancellationToken cancellationToken) =>
        RunAction(id, "resolve", new { resolution }, cancellationToken);

    [HttpPost, ValidateAntiForgeryToken]
    public Task<IActionResult> Reject(int id, string resolution, CancellationToken cancellationToken) =>
        RunAction(id, "reject", new { resolution }, cancellationToken);

    private async Task<IActionResult> RunAction(int id, string action, object body, CancellationToken cancellationToken)
    {
        try
        {
            await apiClient.PutAsync<DisputeViewModel>($"api/admin/disputes/{id}/{action}", body, cancellationToken);
            TempData["Success"] = "Cập nhật khiếu nại thành công.";
            await notifications.BroadcastAsync("Trạng thái khiếu nại đã được cập nhật.", cancellationToken: cancellationToken);
            return RedirectToAction(nameof(Details), new { id });
        }
        catch (AdminApiException exception)
        {
            if (exception.StatusCode is 401 or 403)
                return HandleApiFailure(exception);
            TempData["Error"] = "API không xử lý được yêu cầu. Vui lòng kiểm tra trạng thái khiếu nại.";
            return RedirectToAction(nameof(Details), new { id });
        }
    }
}
