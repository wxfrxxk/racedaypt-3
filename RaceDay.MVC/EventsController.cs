using Microsoft.AspNetCore.Mvc;
using RaceDay.MVC.Filters;
using RaceDay.MVC.Models;
using RaceDay.MVC.Services;

namespace RaceDay.MVC.Controllers
{
    [RoleAuthorize("Participant")]
    public class EventsController : BaseController
    {
        public EventsController(ApiClient api) : base(api)
        {
        }

        // Full event information and the categories a participant can enter.
        public async Task<IActionResult> Details(int id)
        {
            var eventResult = await Api.GetAsync<EventDto>($"api/Events/{id}");

            var expired = HandleExpiredSession(eventResult);
            if (expired != null)
            {
                return expired;
            }

            if (!eventResult.Success || eventResult.Data == null)
            {
                Flash(false, eventResult.Message);
                return RedirectToAction("Index", "Home");
            }

            var categories = await Api.GetAsync<List<CategoryDto>>($"api/Categories/event/{id}");
            var mine = await Api.GetAsync<List<MyEnrolmentDto>>("api/Enrolments/my");

            var vm = new EventDetailsVm
            {
                Event = eventResult.Data,
                Categories = categories.Data ?? new List<CategoryDto>()
            };

            // Remember which categories this participant is already enrolled in.
            if (mine.Success && mine.Data != null)
            {
                var enrolledNames = mine.Data
                    .Where(m => m.EventID == id && m.Status != "Cancelled")
                    .Select(m => m.CategoryName)
                    .ToHashSet();

                foreach (var category in vm.Categories.Where(c => enrolledNames.Contains(c.CategoryName)))
                {
                    vm.EnrolledCategoryIds.Add(category.CategoryID);
                }
            }

            return View(vm);
        }

        // Enter an event by selecting a category. The API records the enrolment immediately.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Enrol(int eventId, int categoryId)
        {
            if (categoryId == 0)
            {
                Flash(false, "Please choose a category before entering.");
                return RedirectToAction(nameof(Details), new { id = eventId });
            }

            var result = await Api.PostAsync<object>("api/Registrations", new
            {
                eventID = eventId,
                categoryID = categoryId
            });

            var expired = HandleExpiredSession(result);
            if (expired != null)
            {
                return expired;
            }

            if (!result.Success)
            {
                Flash(false, result.Message);
                return RedirectToAction(nameof(Details), new { id = eventId });
            }

            Flash(true, "You are entered! Your enrolment is pending confirmation by the organiser.");
            return RedirectToAction("MyEvents", "Participant");
        }
    }
}
