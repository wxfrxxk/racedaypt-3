using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;

namespace RaceDay.API.Controllers
{
    /// <summary>Enrolment views (with participant, event and category details) and personal results.</summary>
    [ApiController]
    [Route("api/Enrolments")]
    public class EnrolmentsController : ControllerBase
    {
        private static readonly string[] ValidStatuses = { "Pending", "Confirmed", "Cancelled" }; // This array defines the valid enrolment statuses that can be set for a registration

        private readonly RaceDayContext _context;

        public EnrolmentsController(RaceDayContext context)
        {
            _context = context;
        }

        
        [HttpGet("event/{eventId:int}")]
        public async Task<IActionResult> GetForEvent(int eventId)
        {
            if (!IsOrganiser())
            {
                return StatusCode(StatusCodes.Status403Forbidden, new
                {
                    message = "Only logged-in organisers can view event enrolments."
                });
            }

            var eventItem = await _context.Events.AsNoTracking() // This ensures the retrieved event is not tracked by the context, improving performance for read-only operations
                .FirstOrDefaultAsync(e => e.EventID == eventId);

            if (eventItem == null)
            {
                return NotFound(new { message = "Event not found." });
            }

            if (eventItem.OrganiserID != HttpContext.Session.GetInt32("UserID"))
            {
                return StatusCode(StatusCodes.Status403Forbidden, new
                {
                    message = "You can only view enrolments for events that you organise."
                });
            }

            var items = await (
                from r in _context.Registrations
                where r.EventID == eventId
                join u in _context.Users on r.UserID equals u.UserID
                join c in _context.Categories on r.CategoryID equals c.CategoryID
                join res in _context.Results on r.RegistrationID equals res.RegistrationID into resultGroup
                from res in resultGroup.DefaultIfEmpty()
                orderby r.RegistrationDate, r.RegistrationID
                select new
                {
                    RegistrationID = r.RegistrationID,
                    ParticipantName = u.FirstName + " " + u.LastName,
                    Email = u.Email,
                    CategoryID = c.CategoryID,
                    CategoryName = c.CategoryName,
                    RegistrationDate = r.RegistrationDate,
                    Status = r.Status,
                    ResultID = (int?)res.ResultID,
                    FinishTime = (TimeSpan?)res.FinishTime,
                    Position = (int?)res.Position
                }).AsNoTracking().ToListAsync();

            return Ok(items);
        }

        
        [HttpGet("my")]
        public async Task<IActionResult> GetMine()
        {
            int? userID = HttpContext.Session.GetInt32("UserID");

            if (userID == null)
            {
                return Unauthorized(new { message = "Please log in first." });
            }

            var items = await (
                from r in _context.Registrations
                where r.UserID == userID.Value
                join e in _context.Events on r.EventID equals e.EventID
                join c in _context.Categories on r.CategoryID equals c.CategoryID
                orderby e.EventDate
                select new
                {
                    RegistrationID = r.RegistrationID,
                    EventID = e.EventID,
                    EventName = e.EventName,
                    EventDate = e.EventDate,
                    Location = e.Location,
                    EventType = e.EventType,
                    BannerUrl = e.BannerUrl,
                    CategoryName = c.CategoryName,
                    RegistrationDate = r.RegistrationDate,
                    Status = r.Status
                }).AsNoTracking().ToListAsync();

            return Ok(items);
        }

        
        [HttpPut("{id:int}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] StatusRequest request)
        {
            if (!IsOrganiser())
            {
                return StatusCode(StatusCodes.Status403Forbidden, new
                {
                    message = "Only logged-in organisers can change enrolment statuses."
                });
            }

            if (!ValidStatuses.Contains(request.Status))
            {
                return BadRequest(new { message = "Status must be Pending, Confirmed or Cancelled." });
            }

            var registration = await _context.Registrations
                .FirstOrDefaultAsync(r => r.RegistrationID == id);

            if (registration == null)
            {
                return NotFound(new { message = "Enrolment not found." });
            }

            var eventItem = await _context.Events.AsNoTracking() // This ensures the retrieved event is not tracked by the context, improving performance for read-only operations
                .FirstOrDefaultAsync(e => e.EventID == registration.EventID);

            if (eventItem == null || eventItem.OrganiserID != HttpContext.Session.GetInt32("UserID"))
            {
                return StatusCode(StatusCodes.Status403Forbidden, new
                {
                    message = "You can only manage enrolments for events that you organise."
                });
            }

            registration.Status = request.Status;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Enrolment status updated." });
        }

        
        [HttpGet("results/my")]
        public async Task<IActionResult> GetMyResults()
        {
            int? userID = HttpContext.Session.GetInt32("UserID");

            if (userID == null)
            {
                return Unauthorized(new { message = "Please log in first." });
            }

            var items = await (
                from res in _context.Results
                join r in _context.Registrations on res.RegistrationID equals r.RegistrationID
                where r.UserID == userID.Value
                join e in _context.Events on r.EventID equals e.EventID
                join c in _context.Categories on r.CategoryID equals c.CategoryID
                orderby e.EventDate descending
                select new
                {
                    ResultID = res.ResultID,
                    EventID = e.EventID,
                    EventName = e.EventName,
                    EventDate = e.EventDate,
                    CategoryName = c.CategoryName,
                    FinishTime = res.FinishTime,
                    Position = res.Position,
                    TotalFinishers = (
                        from r2 in _context.Registrations
                        join res2 in _context.Results on r2.RegistrationID equals res2.RegistrationID
                        where r2.EventID == e.EventID
                        select res2.ResultID).Count()
                }).AsNoTracking().ToListAsync();

            return Ok(items);
        }

        private bool IsOrganiser()
        {
            return HttpContext.Session.GetInt32("UserID") != null &&
                   HttpContext.Session.GetString("Role") == "Organiser";
        }
    }

    public class StatusRequest
    {
        public string Status { get; set; } = string.Empty;
    }
}
