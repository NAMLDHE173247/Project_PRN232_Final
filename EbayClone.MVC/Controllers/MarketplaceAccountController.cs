using EbayClone.MVC.Models;
using EbayClone.MVC.Services;
using Microsoft.AspNetCore.Mvc;

namespace EbayClone.MVC.Controllers;

public sealed class MarketplaceAccountController : Controller
{
    [HttpGet("/marketplace/login")]
    public IActionResult Login() => RedirectToAction("Login", "Account");

    [HttpGet("/marketplace/user")]
    public IActionResult UserHome() =>
        HasMarketplaceSession("User") ? View("Home", "User") : RedirectToAction(nameof(Login));

    [HttpGet("/marketplace/seller")]
    public IActionResult SellerHome() =>
        HasMarketplaceSession("Seller") ? View("Home", "Seller") : RedirectToAction(nameof(Login));

    [HttpPost("/marketplace/logout"), ValidateAntiForgeryToken]
    public IActionResult Logout()
    {
        HttpContext.Session.Remove("MarketplaceToken");
        HttpContext.Session.Remove("MarketplaceEmail");
        HttpContext.Session.Remove("MarketplaceAccountType");
        return RedirectToAction(nameof(Login));
    }

    private bool HasMarketplaceSession(string role) =>
        string.Equals(HttpContext.Session.GetString("MarketplaceAccountType"), role, StringComparison.Ordinal)
        && !string.IsNullOrWhiteSpace(HttpContext.Session.GetString("MarketplaceToken"));
}
