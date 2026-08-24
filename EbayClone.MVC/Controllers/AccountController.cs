using EbayClone.MVC.Models;
using EbayClone.MVC.Services;
using Microsoft.AspNetCore.Mvc;

namespace EbayClone.MVC.Controllers;

public class AccountController(AdminApiClient apiClient) : Controller
{
    [HttpGet]
    public IActionResult Login()
    {
        if (!string.IsNullOrWhiteSpace(HttpContext.Session.GetString("AdminToken")))
            return RedirectToAction("Index", "Dashboard");
        var marketplaceRole = HttpContext.Session.GetString("MarketplaceAccountType");
        if (!string.IsNullOrWhiteSpace(HttpContext.Session.GetString("MarketplaceToken")))
            return RedirectToAction(marketplaceRole == "Seller" ? "SellerHome" : "UserHome", "MarketplaceAccount");
        return View(new LoginInputModel());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginInputModel input, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return View(input);
        try
        {
            var response = await apiClient.LoginAsync(input, cancellationToken);
            if (response is null || response.Role is not ("Admin" or "User" or "Seller"))
            {
                ModelState.AddModelError(string.Empty, "Tài khoản không có role hợp lệ.");
                return View(input);
            }

            if (response.Role == "Admin")
            {
                HttpContext.Session.Remove("MarketplaceToken");
                HttpContext.Session.Remove("MarketplaceEmail");
                HttpContext.Session.Remove("MarketplaceAccountType");
                HttpContext.Session.SetString("AdminToken", response.Token);
                HttpContext.Session.SetString("AdminEmail", response.Email);
                return RedirectToAction("Index", "Dashboard");
            }

            HttpContext.Session.Remove("AdminToken");
            HttpContext.Session.Remove("AdminEmail");
            HttpContext.Session.SetString("MarketplaceToken", response.Token);
            HttpContext.Session.SetString("MarketplaceEmail", response.Email);
            HttpContext.Session.SetString("MarketplaceAccountType", response.Role);
            return RedirectToAction(response.Role == "Seller" ? "SellerHome" : "UserHome", "MarketplaceAccount");
        }
        catch (AdminApiException)
        {
            ModelState.AddModelError(string.Empty, "Đăng nhập thất bại. Tài khoản có thể chưa được duyệt, đã bị khóa hoặc thông tin không đúng.");
            return View(input);
        }
        catch (HttpRequestException)
        {
            ModelState.AddModelError(string.Empty, "Không kết nối được Admin API.");
            return View(input);
        }
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction(nameof(Login));
    }
}
