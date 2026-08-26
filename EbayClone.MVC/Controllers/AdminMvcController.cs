using EbayClone.MVC.Services;
using Microsoft.AspNetCore.Mvc;

namespace EbayClone.MVC.Controllers;

public abstract class AdminMvcController : Controller
{
    protected IActionResult HandleApiFailure(AdminApiException exception, string action = "Index")
    {
        if (exception.StatusCode is 401 or 403)
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Account");
        }

        if (exception.StatusCode is 502 or 503 or 504)
            return RedirectToAction("Offline", "Dashboard");

        TempData["Error"] = exception.StatusCode switch
        {
            400 => "Dữ liệu nhập không hợp lệ. Vui lòng kiểm tra lại.",
            404 => "Không tìm thấy dữ liệu được yêu cầu.",
            409 => "Dữ liệu đã thay đổi hoặc thao tác không còn hợp lệ.",
            _ => "API không xử lý được yêu cầu. Vui lòng thử lại."
        };
        return RedirectToAction(action);
    }
}
