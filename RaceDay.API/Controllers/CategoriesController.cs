
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.Models;

namespace RaceDay.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly RaceDayContext _context;

        public CategoriesController(RaceDayContext context)
        {
            _context = context;
        }

        // GET: api/Categories
        [HttpGet]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await _context.Categories
                .AsNoTracking()
                .ToListAsync();

            return Ok(categories);
        }

        // GET: api/Categories/1
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCategory(int id)
        {
            var category = await _context.Categories
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.CategoryID == id);

            if (category == null)
            {
                return NotFound(new
                {
                    message = "Category not found."
                });
            }

            return Ok(category);
        }

        // GET: api/Categories/event/1
        [HttpGet("event/{eventId}")]
        public async Task<IActionResult> GetCategoriesByEvent(int eventId)
        {
            bool eventExists = await _context.Events
                .AnyAsync(e => e.EventID == eventId);

            if (!eventExists)
            {
                return NotFound(new
                {
                    message = "Event not found."
                });
            }

            var categories = await _context.Categories
                .AsNoTracking()
                .Where(c => c.EventID == eventId)
                .ToListAsync();

            return Ok(categories);
        }

        // POST: api/Categories
        // Only logged-in organisers can create categories.
        [HttpPost]
        public async Task<IActionResult> CreateCategory(
            [FromBody] CategoryRequest request)
        {
            if (!IsOrganiser())
            {
                return StatusCode(StatusCodes.Status403Forbidden, new
                {
                    message = "Only logged-in organisers can create categories."
                });
            }

            if (string.IsNullOrWhiteSpace(request.CategoryName))
            {
                return BadRequest(new
                {
                    message = "Category name is required."
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

            string categoryName = request.CategoryName.Trim();

            bool categoryExists = await _context.Categories
                .AnyAsync(c =>
                    c.EventID == request.EventID &&
                    c.CategoryName == categoryName);

            if (categoryExists)
            {
                return Conflict(new
                {
                    message = "This category already exists for the event."
                });
            }

            var category = new Category
            {
                CategoryName = categoryName,
                EventID = request.EventID
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetCategory),
                new { id = category.CategoryID },
                category);
        }

        // PUT: api/Categories/1
        // Only logged-in organisers can update categories.
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCategory(
            int id,
            [FromBody] CategoryRequest request)
        {
            if (!IsOrganiser())
            {
                return StatusCode(StatusCodes.Status403Forbidden, new
                {
                    message = "Only logged-in organisers can update categories."
                });
            }

            if (string.IsNullOrWhiteSpace(request.CategoryName))
            {
                return BadRequest(new
                {
                    message = "Category name is required."
                });
            }

            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.CategoryID == id);

            if (category == null)
            {
                return NotFound(new
                {
                    message = "Category not found."
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

            string categoryName = request.CategoryName.Trim();

            bool duplicate = await _context.Categories
                .AnyAsync(c =>
                    c.CategoryID != id &&
                    c.EventID == request.EventID &&
                    c.CategoryName == categoryName);

            if (duplicate)
            {
                return Conflict(new
                {
                    message = "This category already exists for the event."
                });
            }

            category.CategoryName = categoryName;
            category.EventID = request.EventID;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Category updated successfully.",
                category
            });
        }

        // DELETE: api/Categories/1
        // Only logged-in organisers can delete categories.
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            if (!IsOrganiser())
            {
                return StatusCode(StatusCodes.Status403Forbidden, new
                {
                    message = "Only logged-in organisers can delete categories."
                });
            }

            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.CategoryID == id);

            if (category == null)
            {
                return NotFound(new
                {
                    message = "Category not found."
                });
            }

            _context.Categories.Remove(category);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return Conflict(new
                {
                    message = "This category cannot be deleted because it has related registrations."
                });
            }

            return Ok(new
            {
                message = "Category deleted successfully."
            });
        }

        private bool IsOrganiser()
        {
            return HttpContext.Session.GetInt32("UserID") != null &&
                   HttpContext.Session.GetString("Role") == "Organiser";
        }
    }

    public class CategoryRequest
    {
        public string CategoryName { get; set; } = string.Empty;

        public int EventID { get; set; }
    }
}