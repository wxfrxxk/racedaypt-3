using Microsoft.AspNetCore.Mvc;
using RaceDay.MVC.Models;
using RaceDay.MVC.Services;

namespace RaceDay.MVC.Controllers
{
    public class HomeController : BaseController
    {
        public HomeController(ApiClient api) : base(api)
        {
        }

        // Participant home page: all upcoming events with filtering.
        public async Task<IActionResult> Index(string? search, string? type, decimal? maxDistance)
        {
            if (CurrentUserId == 0)
            {
                return RedirectToAction("Login", "Account");
            }

            if (CurrentRole == "Organiser")
            {
                return RedirectToAction("Dashboard", "Organiser");
            }

            var result = await Api.GetAsync<List<EventDto>>("api/Events");

            var expired = HandleExpiredSession(result);
            if (expired != null)
            {
                return expired;
            }

            var vm = new HomeVm { Search = search, Type = type, MaxDistance = maxDistance };

            if (!result.Success || result.Data == null)
            {
                Flash(false, result.Message);
                return View(vm);
            }

            var upcoming = result.Data
                .Where(e => e.EventDate.Date >= DateTime.Today)
                .OrderBy(e => e.EventDate)
                .ToList();

            vm.TotalUpcoming = upcoming.Count;

            IEnumerable<EventDto> filtered = upcoming;

            if (!string.IsNullOrWhiteSpace(search))
            {
                filtered = filtered.Where(e =>
                    e.EventName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    e.Location.Contains(search, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(type))
            {
                filtered = filtered.Where(e => e.EventType.Equals(type, StringComparison.OrdinalIgnoreCase));
            }

            if (maxDistance.HasValue)
            {
                filtered = filtered.Where(e => e.Distance <= maxDistance.Value);
            }

            vm.Events = filtered.ToList();
            return View(vm);
        }

        public IActionResult Error(int? code)
        {
            ViewBag.Code = code;
            ViewBag.Message = code switch
            {
                404 => "We could not find that page.",
                403 => "You do not have access to that page.",
                _ => "Something went wrong. Please try again."
            };

            return View();
        }
    }
}
