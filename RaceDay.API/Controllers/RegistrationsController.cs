
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.Models;

namespace RaceDay.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RegistrationsController : ControllerBase
    {
        private readonly RaceDayContext _context;

        public RegistrationsController(RaceDayContext context)
        {
            _context = context;
        }

        // GET: api/Registrations
        // Organisers can view all registrations.
        [HttpGet]
        public async Task<IActionResult> GetRegistrations()
        {
            if (!IsOrganiser())
            {
                return StatusCode(StatusCodes.Status403Forbidden, new
                {
                    message = "Only logged-in organisers can view all registrations."
                });
            }

            var registrations = await _context.Registrations
                .AsNoTracking()
                .ToListAsync();

            return Ok(registrations);
        }

        // GET: api/Registrations/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetRegistration(int id)
        {
            int? userID = HttpContext.Session.GetInt32("UserID");

            if (userID == null)
            {
                return Unauthorized(new
                {
                    message = "Please log in first."
                });
            }

            var registration = await _context.Registrations
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.RegistrationID == id);

            if (registration == null)
            {
                return NotFound(new
                {
                    message = "Registration not found."
                });
            }

            bool isOrganiser = HttpContext.Session.GetString("Role") == "Organiser";

            if (!isOrganiser && registration.UserID != userID.Value)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new
                {
                    message = "You can only view your own registrations."
                });
            }

            return Ok(registration);
        }

        // GET: api/Registrations/my
        // Participants can view their own registrations.
        [HttpGet("my")]
        public async Task<IActionResult> GetMyRegistrations()
        {
            int? userID = HttpContext.Session.GetInt32("UserID");

            if (userID == null)
            {
                return Unauthorized(new
                {
                    message = "Please log in first."
                });
            }

            var registrations = await _context.Registrations
                .AsNoTracking()
                .Where(r => r.UserID == userID.Value)
                .ToListAsync();

            return Ok(registrations);
        }

        // POST: api/Registrations
        // Participants register themselves for an event.
        [HttpPost]
        public async Task<IActionResult> CreateRegistration(
            [FromBody] RegistrationRequest request)
        {
            int? userID = HttpContext.Session.GetInt32("UserID");

            if (userID == null)
            {
                return Unauthorized(new
                {
                    message = "Please log in before registering for an event."
                });
            }

            if (HttpContext.Session.GetString("Role") != "Participant")
            {
                return StatusCode(StatusCodes.Status403Forbidden, new
                {
                    message = "Only participants can register for events."
                });
            }

            bool eventExists = await _context.Events
                .AnyAsync(e => e.EventID == request.EventID);

            if (!eventExists)
            {
                return BadRequest(new
                {
                    message = "The specified event does not exist."
                });
            }

            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.CategoryID == request.CategoryID);

            if (category == null || category.EventID != request.EventID)
            {
                return BadRequest(new
                {
                    message = "The selected category does not belong to this event."
                });
            }

            bool alreadyRegistered = await _context.Registrations
                .AnyAsync(r =>
                    r.UserID == userID.Value &&
                    r.EventID == request.EventID &&
                    r.CategoryID == request.CategoryID);

            if (alreadyRegistered)
            {
                return Conflict(new
                {
                    message = "You are already registered for this event category."
                });
            }

            var registration = new Registration
            {
                RegistrationDate = DateTime.Today,
                UserID = userID.Value,
                EventID = request.EventID,
                CategoryID = request.CategoryID
            };

            _context.Registrations.Add(registration);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Registration successful.",
                registration
            });
        }

        // DELETE: api/Registrations/5
        // Participants can cancel their own registrations.
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRegistration(int id)
        {
            int? userID = HttpContext.Session.GetInt32("UserID");

            if (userID == null)
            {
                return Unauthorized(new
                {
                    message = "Please log in first."
                });
            }

            var registration = await _context.Registrations
                .FirstOrDefaultAsync(r => r.RegistrationID == id);

            if (registration == null)
            {
                return NotFound(new
                {
                    message = "Registration not found."
                });
            }

            bool isOrganiser = HttpContext.Session.GetString("Role") == "Organiser";

            if (!isOrganiser && registration.UserID != userID.Value)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new
                {
                    message = "You can only cancel your own registrations."
                });
            }

            _context.Registrations.Remove(registration);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return Conflict(new
                {
                    message = "This registration cannot be deleted because it has a related result."
                });
            }

            return Ok(new
            {
                message = "Registration cancelled successfully."
            });
        }

        private bool IsOrganiser()
        {
            return HttpContext.Session.GetInt32("UserID") != null &&
                   HttpContext.Session.GetString("Role") == "Organiser";
        }
    }

    public class RegistrationRequest
    {
        public int EventID { get; set; }

        public int CategoryID { get; set; }
    }
}