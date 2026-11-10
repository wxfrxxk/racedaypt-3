
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.Models;

namespace RaceDay.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ResultsController : ControllerBase
    {
        private readonly RaceDayContext _context;

        public ResultsController(RaceDayContext context)
        {
            _context = context;
        }

        // GET: api/Results
        // View all results.
        [HttpGet]
        public async Task<IActionResult> GetResults()
        {
            var results = await _context.Results
                .AsNoTracking()
                .ToListAsync();

            return Ok(results);
        }

        // GET: api/Results/1
        [HttpGet("{id}")]
        public async Task<IActionResult> GetResult(int id)
        {
            var result = await _context.Results
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.ResultID == id);

            if (result == null)
            {
                return NotFound(new
                {
                    message = "Result not found."
                });
            }

            return Ok(result);
        }

        // GET: api/Results/registration/1
        [HttpGet("registration/{registrationID}")]
        public async Task<IActionResult> GetResultByRegistration(
            int registrationID)
        {
            var result = await _context.Results
                .AsNoTracking()
                .FirstOrDefaultAsync(r =>
                    r.RegistrationID == registrationID);

            if (result == null)
            {
                return NotFound(new
                {
                    message = "Result not found for this registration."
                });
            }

            return Ok(result);
        }

        // POST: api/Results
        // Only organisers can record results.
        [HttpPost]
        public async Task<IActionResult> CreateResult(
            [FromBody] ResultRequest request)
        {
            if (!IsOrganiser())
            {
                return StatusCode(StatusCodes.Status403Forbidden, new
                {
                    message = "Only logged-in organisers can record results."
                });
            }

            if (request.FinishTime < TimeSpan.Zero ||
                request.FinishTime >= TimeSpan.FromDays(1))
            {
                return BadRequest(new
                {
                    message = "Finish time must be between 00:00:00 and 23:59:59."
                });
            }

            if (request.Position <= 0)
            {
                return BadRequest(new
                {
                    message = "Position must be greater than zero."
                });
            }

            bool registrationExists = await _context.Registrations
                .AnyAsync(r => r.RegistrationID == request.RegistrationID);

            if (!registrationExists)
            {
                return BadRequest(new
                {
                    message = "The specified registration does not exist."
                });
            }

            bool resultExists = await _context.Results
                .AnyAsync(r =>
                    r.RegistrationID == request.RegistrationID);

            if (resultExists)
            {
                return Conflict(new
                {
                    message = "A result already exists for this registration."
                });
            }

            var result = new Result
            {
                FinishTime = request.FinishTime,
                Position = request.Position,
                RegistrationID = request.RegistrationID
            };

            _context.Results.Add(result);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetResult),
                new { id = result.ResultID },
                result);
        }

        // PUT: api/Results/1
        // Only organisers can update results.
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateResult(
            int id,
            [FromBody] ResultRequest request)
        {
            if (!IsOrganiser())
            {
                return StatusCode(StatusCodes.Status403Forbidden, new
                {
                    message = "Only logged-in organisers can update results."
                });
            }

            if (request.FinishTime < TimeSpan.Zero ||
                request.FinishTime >= TimeSpan.FromDays(1))
            {
                return BadRequest(new
                {
                    message = "Finish time must be between 00:00:00 and 23:59:59."
                });
            }

            if (request.Position <= 0)
            {
                return BadRequest(new
                {
                    message = "Position must be greater than zero."
                });
            }

            var result = await _context.Results
                .FirstOrDefaultAsync(r => r.ResultID == id);

            if (result == null)
            {
                return NotFound(new
                {
                    message = "Result not found."
                });
            }

            bool registrationExists = await _context.Registrations
                .AnyAsync(r => r.RegistrationID == request.RegistrationID);

            if (!registrationExists)
            {
                return BadRequest(new
                {
                    message = "The specified registration does not exist."
                });
            }

            bool duplicate = await _context.Results
                .AnyAsync(r =>
                    r.RegistrationID == request.RegistrationID &&
                    r.ResultID != id);

            if (duplicate)
            {
                return Conflict(new
                {
                    message = "A result already exists for this registration."
                });
            }

            result.FinishTime = request.FinishTime;
            result.Position = request.Position;
            result.RegistrationID = request.RegistrationID;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Result updated successfully.",
                result
            });
        }

        // DELETE: api/Results/1
        // Only organisers can delete results.
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteResult(int id)
        {
            if (!IsOrganiser())
            {
                return StatusCode(StatusCodes.Status403Forbidden, new
                {
                    message = "Only logged-in organisers can delete results."
                });
            }

            var result = await _context.Results
                .FirstOrDefaultAsync(r => r.ResultID == id);

            if (result == null)
            {
                return NotFound(new
                {
                    message = "Result not found."
                });
            }

            _context.Results.Remove(result);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Result deleted successfully."
            });
        }

        private bool IsOrganiser()
        {
            return HttpContext.Session.GetInt32("UserID") != null &&
                   HttpContext.Session.GetString("Role") == "Organiser";
        }
    }

    public class ResultRequest
    {
        public TimeSpan FinishTime { get; set; }

        public int Position { get; set; }

        public int RegistrationID { get; set; }
    }
}