using EbayClone.MVC.Filters;
using EbayClone.MVC.Models;
using EbayClone.MVC.Services;
using Microsoft.AspNetCore.Mvc;

namespace EbayClone.MVC.Controllers;

[AdminSession]
public class ReviewsController(AdminApiClient apiClient) : AdminMvcController
{
    public async Task<IActionResult> Index(int page = 1, CancellationToken cancellationToken = default) =>
        View(await apiClient.GetAsync<PagedViewModel<AdminReviewViewModel>>(
            $"api/admin/reviews?page={Math.Max(page, 1)}&pageSize=20", cancellationToken));

    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken) =>
        View(await apiClient.GetAsync<AdminReviewViewModel>($"api/admin/reviews/{id}", cancellationToken));

    [HttpPost, ValidateAntiForgeryToken]
    public Task<IActionResult> Hide(int id, string reason, CancellationToken cancellationToken) => RunAction(id, "hide", new { reason }, cancellationToken);

    [HttpPost, ValidateAntiForgeryToken]
    public Task<IActionResult> Restore(int id, CancellationToken cancellationToken) => RunAction(id, "restore", null, cancellationToken);

    private async Task<IActionResult> RunAction(int id, string action, object? body, CancellationToken cancellationToken)
    {
        try { await apiClient.PutAsync<AdminReviewViewModel>($"api/admin/reviews/{id}/{action}", body, cancellationToken); TempData["Success"] = $"Review #{id}: {action}."; }
        catch (AdminApiException exception) { return HandleApiFailure(exception, nameof(Details)); }
        return RedirectToAction(nameof(Details), new { id });
    }
}
