using Microsoft.AspNetCore.Mvc;
using RaceDay.MVC.Services;

namespace RaceDay.MVC.Controllers
{
    public abstract class BaseController : Controller
    {
        protected readonly ApiClient Api;

        protected BaseController(ApiClient api)
        {
            Api = api;
        }

        protected int CurrentUserId => HttpContext.Session.GetInt32("UserID") ?? 0;
        protected string CurrentRole => HttpContext.Session.GetString("Role") ?? string.Empty;

        // If the API says the session expired, clear it and send the user to the login page.
        protected IActionResult? HandleExpiredSession<T>(ApiResult<T> result)
        {
            if (!result.IsUnauthorized)
            {
                return null;
            }

            HttpContext.Session.Clear();
            TempData["Error"] = "Your session has expired. Please log in again.";
            return RedirectToAction("Login", "Account");
        }

        protected void Flash(bool success, string message)
        {
            TempData[success ? "Success" : "Error"] = message;
        }
    }
}
