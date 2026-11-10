using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.Models;
using RaceDay.API.Services;

namespace RaceDay.API.Controllers
{
    /// <summary>Organiser dashboard data and event banner uploads.</summary>
    [ApiController]
    [Route("api/Events")]
    public class EventExtrasController : ControllerBase // This controller handles organiser dashboard data and event banner uploads
    {
        private readonly RaceDayContext _context; // This is the database context used to access the database
        private readonly IFileStorageService _storage; // This is the file storage service used to upload images

        public EventExtrasController(RaceDayContext context, IFileStorageService storage)
        {
            _context = context;
            _storage = storage;
        }

        
        [HttpGet("mine")]
        public async Task<IActionResult> GetMyEvents()
        {
            if (!IsOrganiser())
            {
                return StatusCode(StatusCodes.Status403Forbidden, new // This checks if the user is an organiser and returns a 403 Forbidden status if not
                {
                    message = "Only logged-in organisers can view their events."
                });
            }

            int organiserID = HttpContext.Session.GetInt32("UserID")!.Value; // This gets the organiser ID from the session

            var events = await _context.Events
                .AsNoTracking()
                .Where(e => e.OrganiserID == organiserID)
                .OrderBy(e => e.EventDate)
                .Select(e => new
                {
                    e.EventID,
                    e.EventName,
                    e.Description,
                    e.EventDate,
                    e.Location,
                    e.EventType,
                    e.Distance,
                    e.BannerUrl,
                    EnrolmentCount = _context.Registrations.Count(r => r.EventID == e.EventID && r.Status != "Cancelled"),
                    CategoryCount = _context.Categories.Count(c => c.EventID == e.EventID)
                })
                .ToListAsync();

            return Ok(events);
        }

        
        [HttpPost("{id:int}/banner")]
        public async Task<IActionResult> UploadBanner(int id, [FromForm] FileUploadRequest request) // This endpoint allows organisers to upload a banner for their event
        {
            if (!IsOrganiser())
            {
                return StatusCode(StatusCodes.Status403Forbidden, new
                {
                    message = "Only logged-in organisers can upload banners."
                });
            }

            var eventItem = await _context.Events.FirstOrDefaultAsync(e => e.EventID == id);

            if (eventItem == null)
            {
                return NotFound(new { message = "Event not found." });
            }

            int organiserID = HttpContext.Session.GetInt32("UserID")!.Value;

            if (eventItem.OrganiserID != organiserID)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new
                {
                    message = "You can only upload banners for events that you organise."
                });
            }

            string? error = ImageValidator.Validate(request.File);

            if (error != null)
            {
                return BadRequest(new { message = error });
            }

            eventItem.BannerUrl = await _storage.UploadImageAsync(request.File, "banners");
            await _context.SaveChangesAsync();

            return Ok(new { message = "Banner uploaded.", url = eventItem.BannerUrl });
        }

        private bool IsOrganiser()
        {
            return HttpContext.Session.GetInt32("UserID") != null &&
                   HttpContext.Session.GetString("Role") == "Organiser";
        }
    }
}