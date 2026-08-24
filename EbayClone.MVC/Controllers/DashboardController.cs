using EbayClone.MVC.Filters;
using EbayClone.MVC.Models;
using EbayClone.MVC.Services;
using Microsoft.AspNetCore.Mvc;

namespace EbayClone.MVC.Controllers;

[AdminSession]
public class DashboardController(AdminApiClient apiClient) : AdminMvcController
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        try
        {
            var model = await apiClient.GetAsync<DashboardViewModel>("api/admin/dashboard", cancellationToken);
            return View(model);
        }
        catch (AdminApiException exception)
        {
            return HandleApiFailure(exception);
        }
    }

    public IActionResult Offline() => View();
}
