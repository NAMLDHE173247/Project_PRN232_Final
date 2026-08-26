using EbayClone.MVC.Filters;
using EbayClone.MVC.Models;
using EbayClone.MVC.Services;
using Microsoft.AspNetCore.Mvc;

namespace EbayClone.MVC.Controllers;

[AdminSession]
public class ReturnRequestsController(AdminApiClient apiClient, AdminNotificationService notifications) : AdminMvcController
{
    public async Task<IActionResult> Index(
        string? status, int? userId, int? orderId, DateTime? from, DateTime? to,
        int page = 1, CancellationToken cancellationToken = default)
    {
        var query = $"api/admin/return-requests?page={Math.Max(page, 1)}&pageSize=20";
        if (!string.IsNullOrWhiteSpace(status)) query += $"&status={Uri.EscapeDataString(status)}";
        if (userId.HasValue) query += $"&userId={userId.Value}";
        if (orderId.HasValue) query += $"&orderId={orderId.Value}";
        if (from.HasValue) query += $"&from={from.Value:yyyy-MM-dd}";
        if (to.HasValue) query += $"&to={to.Value:yyyy-MM-dd}";
        ViewBag.Status = status;
        ViewBag.UserId = userId;
        ViewBag.OrderId = orderId;
        ViewBag.From = from?.ToString("yyyy-MM-dd");
        ViewBag.To = to?.ToString("yyyy-MM-dd");
        return View(await apiClient.GetAsync<PagedViewModel<ReturnRequestViewModel>>(query, cancellationToken));
    }

    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken) =>
        View(await apiClient.GetAsync<ReturnRequestViewModel>($"api/admin/return-requests/{id}", cancellationToken));

    [HttpPost, ValidateAntiForgeryToken]
    public Task<IActionResult> Approve(int id, CancellationToken cancellationToken) =>
        RunAction(id, "approve", "approved", cancellationToken);

    [HttpPost, ValidateAntiForgeryToken]
    public Task<IActionResult> Reject(int id, CancellationToken cancellationToken) =>
        RunAction(id, "reject", "rejected", cancellationToken);

    private async Task<IActionResult> RunAction(int id, string action, string result, CancellationToken cancellationToken)
    {
        try
        {
            await apiClient.PutAsync<ReturnRequestViewModel>($"api/admin/return-requests/{id}/{action}", null, cancellationToken);
            TempData["Success"] = $"Return request #{id} was {result}.";
            await notifications.BroadcastAsync($"Return request #{id} was {result}.", cancellationToken: cancellationToken);
        }
        catch (AdminApiException exception)
        {
            if (exception.StatusCode is 401 or 403) return HandleApiFailure(exception);
            TempData["Error"] = exception.StatusCode == 409
                ? "Yêu cầu hoàn trả đã được xử lý trước đó."
                : "API không xử lý được yêu cầu hoàn trả.";
        }
        return RedirectToAction(nameof(Details), new { id });
    }
}
