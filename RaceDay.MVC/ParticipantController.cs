using Microsoft.AspNetCore.Mvc;
using RaceDay.MVC.Filters;
using RaceDay.MVC.Models;
using RaceDay.MVC.Services;

namespace RaceDay.MVC.Controllers
{
    [RoleAuthorize("Participant")]
    public class ParticipantController : BaseController
    {
        public ParticipantController(ApiClient api) : base(api)
        {
        }

        // All events the participant has entered, with enrolment status.
        public async Task<IActionResult> MyEvents()
        {
            var result = await Api.GetAsync<List<MyEnrolmentDto>>("api/Enrolments/my");

            var expired = HandleExpiredSession(result);
            if (expired != null)
            {
                return expired;
            }

            if (!result.Success)
            {
                Flash(false, result.Message);
            }

            return View(result.Data ?? new List<MyEnrolmentDto>());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var result = await Api.DeleteAsync($"api/Registrations/{id}");

            var expired = HandleExpiredSession(result);
            if (expired != null)
            {
                return expired;
            }

            Flash(result.Success, result.Success ? "Your enrolment was cancelled." : result.Message);
            return RedirectToAction(nameof(MyEvents));
        }

        // Personal race history across completed events.
        public async Task<IActionResult> MyResults()
        {
            var result = await Api.GetAsync<List<MyResultDto>>("api/Enrolments/results/my");

            var expired = HandleExpiredSession(result);
            if (expired != null)
            {
                return expired;
            }

            if (!result.Success)
            {
                Flash(false, result.Message);
            }

            return View(result.Data ?? new List<MyResultDto>());
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var result = await Api.GetAsync<ProfileDto>("api/Users/me");

            var expired = HandleExpiredSession(result);
            if (expired != null)
            {
                return expired;
            }

            if (!result.Success || result.Data == null)
            {
                Flash(false, result.Message);
                return RedirectToAction("Index", "Home");
            }

            return View(ToVm(result.Data));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(ProfileVm model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await Api.PutAsync<object>("api/Users/me", new
            {
                firstName = model.FirstName.Trim(),
                lastName = model.LastName.Trim(),
                phoneNumber = model.PhoneNumber.Trim()
            });

            var expired = HandleExpiredSession(result);
            if (expired != null)
            {
                return expired;
            }

            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                return View(model);
            }

            HttpContext.Session.SetString("FullName", $"{model.FirstName.Trim()} {model.LastName.Trim()}");
            Flash(true, "Your profile was updated.");
            return RedirectToAction(nameof(Profile));
        }

        // The file is sent to the API, which stores it in Azure Blob Storage.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProfilePicture(IFormFile? picture)
        {
            if (picture == null || picture.Length == 0)
            {
                Flash(false, "Please choose an image to upload.");
                return RedirectToAction(nameof(Profile));
            }

            var result = await Api.PostFileAsync("api/Users/me/picture", picture);

            var expired = HandleExpiredSession(result);
            if (expired != null)
            {
                return expired;
            }

            Flash(result.Success, result.Success ? "Profile picture updated." : result.Message);
            return RedirectToAction(nameof(Profile));
        }

        private static ProfileVm ToVm(ProfileDto dto) => new()
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            PhoneNumber = dto.PhoneNumber,
            Email = dto.Email,
            ProfilePictureUrl = dto.ProfilePictureUrl,
            Role = dto.Role
        };
    }
}
