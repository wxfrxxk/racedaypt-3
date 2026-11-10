using Microsoft.AspNetCore.Mvc;
using RaceDay.MVC.Models;
using RaceDay.MVC.Services;

namespace RaceDay.MVC.Controllers
{
    public class AccountController : BaseController
    {
        public AccountController(ApiClient api) : base(api)
        {
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (CurrentUserId != 0)
            {
                return RedirectToAction("Index", "Home");
            }

            return View(new LoginVm { ReturnUrl = returnUrl });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginVm model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Start from a clean session, then let the API create its own session.
            HttpContext.Session.Clear();

            var result = await Api.PostAsync<LoginResponseDto>("api/Auth/login", new
            {
                email = model.Email.Trim(),
                password = model.Password
            });

            if (!result.Success || result.Data == null)
            {
                ModelState.AddModelError(string.Empty,
                    result.Status == System.Net.HttpStatusCode.Unauthorized
                        ? "Invalid email or password."
                        : result.Message);
                return View(model);
            }

            // Store who is logged in and their role for all following pages.
            HttpContext.Session.SetInt32("UserID", result.Data.UserID);
            HttpContext.Session.SetString("Role", result.Data.Role);
            HttpContext.Session.SetString("FullName", $"{result.Data.FirstName} {result.Data.LastName}");

            if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            {
                return LocalRedirect(model.ReturnUrl);
            }

            return result.Data.Role == "Organiser"
                ? RedirectToAction("Dashboard", "Organiser")
                : RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Register()
        {
            if (CurrentUserId != 0)
            {
                return RedirectToAction("Index", "Home");
            }

            return View(new RegisterVm());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterVm model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await Api.PostAsync<object>("api/Auth/register", new
            {
                firstName = model.FirstName.Trim(),
                lastName = model.LastName.Trim(),
                email = model.Email.Trim(),
                phoneNumber = model.PhoneNumber.Trim(),
                password = model.Password
            });

            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                return View(model);
            }

            Flash(true, "Account created! You can now log in.");
            return RedirectToAction(nameof(Login));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            // End the API session too, then clear the MVC session.
            await Api.PostEmptyAsync("api/Auth/logout");
            HttpContext.Session.Clear();

            Flash(true, "You have been logged out.");
            return RedirectToAction(nameof(Login));
        }
    }
}
