using EbayClone.MVC.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace EbayClone.MVC.Filters;

public sealed class AdminApiExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        if (context.Exception is not AdminApiException exception) return;

        if (exception.StatusCode is 401 or 403)
            context.HttpContext.Session.Clear();
        else if (exception.StatusCode is 502 or 503 or 504)
        {
            context.HttpContext.Session.SetString("OfflineMode", "true");
        }

        if (exception.StatusCode is 400 or 404 or 409)
        {
            context.HttpContext.Session.SetString("LastAdminError", exception.StatusCode switch
            {
                400 => "Dữ liệu nhập không hợp lệ.",
                404 => "Không tìm thấy dữ liệu được yêu cầu.",
                _ => "Thao tác không còn hợp lệ vì dữ liệu đã được xử lý."
            });
        }

        context.Result = exception.StatusCode is 401 or 403
            ? new RedirectToActionResult("Login", "Account", null)
            : exception.StatusCode is 502 or 503 or 504
                ? new RedirectToActionResult("Offline", "Dashboard", null)
                : new RedirectToActionResult("Index", "Dashboard", null);
        context.ExceptionHandled = true;
    }
}
