
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.Models;

namespace RaceDay.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventsController : ControllerBase
    {
        private readonly RaceDayContext _context;

        public EventsController(RaceDayContext context)
        {
            _context = context;
        }

        // GET: api/Events
        // Anyone can view all events.
        [HttpGet]
        public async Task<IActionResult> GetEvents()
        {
            var events = await _context.Events
                .AsNoTracking()
                .ToListAsync();

            return Ok(events);
        }

        // GET: api/Events/1
        // Anyone can view a specific event.
        [HttpGet("{id}")]
        public async Task<IActionResult> GetEvent(int id)
        {
            var eventItem = await _context.Events
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.EventID == id);

            if (eventItem == null)
            {
                return NotFound(new
                {
                    message = "Event not found."
                });
            }

            return Ok(eventItem);
        }

        // POST: api/Events
        // Only logged-in organisers can create events.
        [HttpPost]
        public async Task<IActionResult> CreateEvent(
            [FromBody] EventRequest request)
        {
            if (!IsOrganiser())
            {
                return StatusCode(StatusCodes.Status403Forbidden, new
                {
                    message = "Only logged-in organisers can create events."
                });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (string.IsNullOrWhiteSpace(request.EventName) ||
                string.IsNullOrWhiteSpace(request.Description) ||
                string.IsNullOrWhiteSpace(request.Location) ||
                string.IsNullOrWhiteSpace(request.EventType))
            {
                return BadRequest(new
                {
                    message = "All event fields are required."
                });
            }

            if (request.EventDate == default)
            {
                return BadRequest(new
                {
                    message = "A valid event date is required."
                });
            }

            if (request.EventDate.Date < DateTime.Today)
            {
                return BadRequest(new
                {
                    message = "The event date cannot be in the past."
                });
            }

            if (!IsValidEventType(request.EventType))
            {
                return BadRequest(new
                {
                    message = "Event type must be Run, Walk, or Cycle."
                });
            }

            int organiserID = HttpContext.Session.GetInt32("UserID")!.Value;

            var newEvent = new Event
            {
                EventName = request.EventName.Trim(),
                Description = request.Description.Trim(),
                EventDate = request.EventDate.Date,
                Location = request.Location.Trim(),
                EventType = request.EventType.Trim(),
                Distance = request.Distance,
                OrganiserID = organiserID
            };

            _context.Events.Add(newEvent);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetEvent),
                new { id = newEvent.EventID },
                newEvent);
        }

        // PUT: api/Events/1
        // Only logged-in organisers can update events.
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEvent(
            int id,
            [FromBody] EventRequest request)
        {
            if (!IsOrganiser())
            {
                return StatusCode(StatusCodes.Status403Forbidden, new
                {
                    message = "Only logged-in organisers can update events."
                });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (string.IsNullOrWhiteSpace(request.EventName) ||
                string.IsNullOrWhiteSpace(request.Description) ||
                string.IsNullOrWhiteSpace(request.Location) ||
                string.IsNullOrWhiteSpace(request.EventType))
            {
                return BadRequest(new
                {
                    message = "All event fields are required."
                });
            }

            if (request.EventDate == default)
            {
                return BadRequest(new
                {
                    message = "A valid event date is required."
                });
            }

            if (request.EventDate.Date < DateTime.Today)
            {
                return BadRequest(new
                {
                    message = "The event date cannot be in the past."
                });
            }

            if (!IsValidEventType(request.EventType))
            {
                return BadRequest(new
                {
                    message = "Event type must be Run, Walk, or Cycle."
                });
            }

            var eventItem = await _context.Events
                .FirstOrDefaultAsync(e => e.EventID == id);

            if (eventItem == null)
            {
                return NotFound(new
                {
                    message = "Event not found."
                });
            }

            int organiserID = HttpContext.Session.GetInt32("UserID")!.Value;

            if (eventItem.OrganiserID != organiserID)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new
                {
                    message = "You can only update events that you organise."
                });
            }

            eventItem.EventName = request.EventName.Trim();
            eventItem.Description = request.Description.Trim();
            eventItem.EventDate = request.EventDate.Date;
            eventItem.Location = request.Location.Trim();
            eventItem.EventType = request.EventType.Trim();
            eventItem.Distance = request.Distance;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Event updated successfully.",
                eventItem
            });
        }

        // DELETE: api/Events/1
        // Only the organiser of the event can delete it.
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            if (!IsOrganiser())
            {
                return StatusCode(StatusCodes.Status403Forbidden, new
                {
                    message = "Only logged-in organisers can delete events."
                });
            }

            var eventItem = await _context.Events
                .FirstOrDefaultAsync(e => e.EventID == id);

            if (eventItem == null)
            {
                return NotFound(new
                {
                    message = "Event not found."
                });
            }

            int organiserID = HttpContext.Session.GetInt32("UserID")!.Value;

            if (eventItem.OrganiserID != organiserID)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new
                {
                    message = "You can only delete events that you organise."
                });
            }

            _context.Events.Remove(eventItem);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return Conflict(new
                {
                    message = "This event cannot be deleted because it has related categories or registrations."
                });
            }

            return Ok(new
            {
                message = "Event deleted successfully."
            });
        }

        // Check whether the current session belongs to an organiser.
        private bool IsOrganiser()
        {
            return HttpContext.Session.GetInt32("UserID") != null &&
                   HttpContext.Session.GetString("Role") == "Organiser";
        }

        // Check the event types allowed by the database.
        private static bool IsValidEventType(string eventType)
        {
            return eventType.Trim().Equals(
                       "Run",
                       StringComparison.OrdinalIgnoreCase) ||
                   eventType.Trim().Equals(
                       "Walk",
                       StringComparison.OrdinalIgnoreCase) ||
                   eventType.Trim().Equals(
                       "Cycle",
                       StringComparison.OrdinalIgnoreCase);
        }
    }

    public class EventRequest
    {
        public string EventName { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public DateTime EventDate { get; set; }

        public string Location { get; set; } = string.Empty;

        public string EventType { get; set; } = string.Empty;
        public decimal Distance { get; set; }
    }
}