using Microsoft.AspNetCore.Mvc;
using RaceDay.MVC.Filters;
using RaceDay.MVC.Models;
using RaceDay.MVC.Services;

namespace RaceDay.MVC.Controllers
{
    [RoleAuthorize("Organiser")]
    public class OrganiserController : BaseController
    {
        public OrganiserController(ApiClient api) : base(api)
        {
        }

        // Dashboard: summary of the organiser's events, enrolments and upcoming dates.
        public async Task<IActionResult> Dashboard()
        {
            var result = await Api.GetAsync<List<OrganiserEventDto>>("api/Events/mine");

            var expired = HandleExpiredSession(result);
            if (expired != null)
            {
                return expired;
            }

            if (!result.Success)
            {
                Flash(false, result.Message);
            }

            return View(result.Data ?? new List<OrganiserEventDto>());
        }

        // ---------- Events ----------

        public async Task<IActionResult> Events()
        {
            var result = await Api.GetAsync<List<OrganiserEventDto>>("api/Events/mine");

            var expired = HandleExpiredSession(result);
            if (expired != null)
            {
                return expired;
            }

            if (!result.Success)
            {
                Flash(false, result.Message);
            }

            return View(result.Data ?? new List<OrganiserEventDto>());
        }

        [HttpGet]
        public IActionResult CreateEvent()
        {
            ViewBag.IsEdit = false;
            return View("EventForm", new EventFormVm { EventDate = DateTime.Today.AddDays(30) });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateEvent(EventFormVm model)
        {
            ViewBag.IsEdit = false;

            if (!ModelState.IsValid)
            {
                return View("EventForm", model);
            }

            var result = await Api.PostAsync<EventDto>("api/Events", ToRequest(model));

            var expired = HandleExpiredSession(result);
            if (expired != null)
            {
                return expired;
            }

            if (!result.Success || result.Data == null)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                return View("EventForm", model);
            }

            string message = "Event created.";

            if (model.Banner != null && model.Banner.Length > 0)
            {
                var upload = await Api.PostFileAsync($"api/Events/{result.Data.EventID}/banner", model.Banner);
                message += upload.Success ? " Banner uploaded." : $" The banner could not be uploaded: {upload.Message}";
            }

            Flash(true, message);
            return RedirectToAction(nameof(Events));
        }

        [HttpGet]
        public async Task<IActionResult> EditEvent(int id)
        {
            var result = await Api.GetAsync<EventDto>($"api/Events/{id}");

            var expired = HandleExpiredSession(result);
            if (expired != null)
            {
                return expired;
            }

            if (!result.Success || result.Data == null)
            {
                Flash(false, result.Message);
                return RedirectToAction(nameof(Events));
            }

            var e = result.Data;
            ViewBag.IsEdit = true;

            return View("EventForm", new EventFormVm
            {
                EventID = e.EventID,
                EventName = e.EventName,
                Description = e.Description,
                EventDate = e.EventDate,
                Location = e.Location,
                EventType = e.EventType,
                Distance = e.Distance,
                ExistingBannerUrl = e.BannerUrl
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditEvent(int id, EventFormVm model)
        {
            ViewBag.IsEdit = true;
            model.EventID = id;

            if (!ModelState.IsValid)
            {
                return View("EventForm", model);
            }

            var result = await Api.PutAsync<object>($"api/Events/{id}", ToRequest(model));

            var expired = HandleExpiredSession(result);
            if (expired != null)
            {
                return expired;
            }

            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                return View("EventForm", model);
            }

            string message = "Event updated.";

            if (model.Banner != null && model.Banner.Length > 0)
            {
                var upload = await Api.PostFileAsync($"api/Events/{id}/banner", model.Banner);
                message += upload.Success ? " Banner uploaded." : $" The banner could not be uploaded: {upload.Message}";
            }

            Flash(true, message);
            return RedirectToAction(nameof(Events));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var result = await Api.DeleteAsync($"api/Events/{id}");

            var expired = HandleExpiredSession(result);
            if (expired != null)
            {
                return expired;
            }

            Flash(result.Success, result.Success ? "Event deleted." : result.Message);
            return RedirectToAction(nameof(Events));
        }

        // ---------- Categories ----------

        public async Task<IActionResult> Categories(int eventId)
        {
            var eventResult = await Api.GetAsync<EventDto>($"api/Events/{eventId}");

            var expired = HandleExpiredSession(eventResult);
            if (expired != null)
            {
                return expired;
            }

            if (!eventResult.Success || eventResult.Data == null)
            {
                Flash(false, eventResult.Message);
                return RedirectToAction(nameof(Events));
            }

            var categories = await Api.GetAsync<List<CategoryDto>>($"api/Categories/event/{eventId}");

            return View(new CategoriesVm
            {
                Event = eventResult.Data,
                Categories = categories.Data ?? new List<CategoryDto>()
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddCategory(int eventId, string? categoryName)
        {
            if (string.IsNullOrWhiteSpace(categoryName) || categoryName.Trim().Length > 50)
            {
                Flash(false, "Enter a category name of up to 50 characters (for example 10km or Under 20).");
                return RedirectToAction(nameof(Categories), new { eventId });
            }

            var result = await Api.PostAsync<object>("api/Categories", new
            {
                categoryName = categoryName.Trim(),
                eventID = eventId
            });

            var expired = HandleExpiredSession(result);
            if (expired != null)
            {
                return expired;
            }

            Flash(result.Success, result.Success ? "Category added." : result.Message);
            return RedirectToAction(nameof(Categories), new { eventId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCategory(int id, int eventId)
        {
            var result = await Api.DeleteAsync($"api/Categories/{id}");

            var expired = HandleExpiredSession(result);
            if (expired != null)
            {
                return expired;
            }

            Flash(result.Success, result.Success ? "Category removed." : result.Message);
            return RedirectToAction(nameof(Categories), new { eventId });
        }

        // ---------- Enrolments ----------

        public async Task<IActionResult> Enrolments(int eventId)
        {
            var vm = await LoadEnrolmentsAsync(eventId);

            if (vm.Redirect != null)
            {
                return vm.Redirect;
            }

            return View(vm.Model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetStatus(int id, int eventId, string status)
        {
            var result = await Api.PutAsync<object>($"api/Enrolments/{id}/status", new { status });

            var expired = HandleExpiredSession(result);
            if (expired != null)
            {
                return expired;
            }

            Flash(result.Success, result.Success ? $"Enrolment marked as {status}." : result.Message);
            return RedirectToAction(nameof(Enrolments), new { eventId });
        }

        // ---------- Results ----------

        public async Task<IActionResult> Results(int eventId)
        {
            var vm = await LoadEnrolmentsAsync(eventId);

            if (vm.Redirect != null)
            {
                return vm.Redirect;
            }

            return View(vm.Model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveResult(int eventId, int registrationId, int? resultId, string? finishTime, int? position)
        {
            if (!TimeSpan.TryParse(finishTime, out var time) || time <= TimeSpan.Zero || time >= TimeSpan.FromDays(1))
            {
                Flash(false, "Enter a finish time in the format hh:mm:ss, for example 00:42:15.");
                return RedirectToAction(nameof(Results), new { eventId });
            }

            if (position == null || position <= 0)
            {
                Flash(false, "Enter a finishing position of 1 or higher.");
                return RedirectToAction(nameof(Results), new { eventId });
            }

            var body = new
            {
                finishTime = time.ToString(@"hh\:mm\:ss"),
                position = position.Value,
                registrationID = registrationId
            };

            ApiResult<object> result = resultId.HasValue
                ? await Api.PutAsync<object>($"api/Results/{resultId.Value}", body)
                : await Api.PostAsync<object>("api/Results", body);

            var expired = HandleExpiredSession(result);
            if (expired != null)
            {
                return expired;
            }

            Flash(result.Success, result.Success ? "Result saved and published." : result.Message);
            return RedirectToAction(nameof(Results), new { eventId });
        }

        // ---------- Helpers ----------

        private async Task<(EnrolmentsVm? Model, IActionResult? Redirect)> LoadEnrolmentsAsync(int eventId)
        {
            var eventResult = await Api.GetAsync<EventDto>($"api/Events/{eventId}");

            var expired = HandleExpiredSession(eventResult);
            if (expired != null)
            {
                return (null, expired);
            }

            if (!eventResult.Success || eventResult.Data == null)
            {
                Flash(false, eventResult.Message);
                return (null, RedirectToAction(nameof(Events)));
            }

            var enrolments = await Api.GetAsync<List<EnrolmentDto>>($"api/Enrolments/event/{eventId}");

            if (!enrolments.Success)
            {
                Flash(false, enrolments.Message);
                return (null, RedirectToAction(nameof(Events)));
            }

            return (new EnrolmentsVm
            {
                Event = eventResult.Data,
                Enrolments = enrolments.Data ?? new List<EnrolmentDto>()
            }, null);
        }

        private static object ToRequest(EventFormVm model) => new
        {
            eventName = model.EventName.Trim(),
            description = model.Description.Trim(),
            eventDate = model.EventDate!.Value.Date,
            location = model.Location.Trim(),
            eventType = model.EventType,
            distance = model.Distance!.Value
        };
    }
}
