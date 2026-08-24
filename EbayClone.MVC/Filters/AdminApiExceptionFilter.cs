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
        else if (exception.StatusCode == 503)
        {
            context.HttpContext.Session.SetString("OfflineMode", "true");
        }

        context.Result = exception.StatusCode is 401 or 403
            ? new RedirectToActionResult("Login", "Account", null)
            : exception.StatusCode == 503
                ? new RedirectToActionResult("Offline", "Dashboard", null)
                : new RedirectToActionResult("Index", "Dashboard", null);
        context.ExceptionHandled = true;
    }
}
