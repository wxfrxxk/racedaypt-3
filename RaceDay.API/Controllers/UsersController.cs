using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.Models;
using RaceDay.API.Services;

namespace RaceDay.API.Controllers
{
    /// <summary>View and update the logged-in user's own profile (both roles).</summary>
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase // This controller handles viewing and updating the logged-in user's own profile
    {
        private readonly RaceDayContext _context; // This is the database context used to access the database
        private readonly IFileStorageService _storage; // This is the file storage service used to upload images

        public UsersController(RaceDayContext context, IFileStorageService storage)
        {
            _context = context;
            _storage = storage;
        }

        
        [HttpGet("me")]
        public async Task<IActionResult> GetMe() // This endpoint retrieves the logged-in user's profile information
        {
            int? userID = HttpContext.Session.GetInt32("UserID"); // Get the logged-in user's ID from the session

            if (userID == null)
            {
                return Unauthorized(new { message = "Please log in first." });
            }

            var user = await _context.Users
                .AsNoTracking() // This ensures the retrieved user is not tracked by the context, improving performance for read-only operations
                .FirstOrDefaultAsync(u => u.UserID == userID.Value);

            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }

            return Ok(new
            {
                user.UserID,
                user.FirstName,
                user.LastName,
                user.Email,
                user.PhoneNumber,
                user.ProfilePictureUrl,
                Role = HttpContext.Session.GetString("Role")
            });
        }

        
        [HttpPut("me")]
        public async Task<IActionResult> UpdateMe([FromBody] ProfileRequest request)
        {
            int? userID = HttpContext.Session.GetInt32("UserID");

            if (userID == null)
            {
                return Unauthorized(new { message = "Please log in first." });
            }

            if (string.IsNullOrWhiteSpace(request.FirstName) ||
                string.IsNullOrWhiteSpace(request.LastName) ||
                string.IsNullOrWhiteSpace(request.PhoneNumber))
            {
                return BadRequest(new { message = "First name, last name and phone number are required." });
            }

            if (request.FirstName.Length > 50 || request.LastName.Length > 50 || request.PhoneNumber.Length > 15)
            {
                return BadRequest(new { message = "One or more fields are too long." });
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserID == userID.Value); // This retrieves the logged-in user from the database

            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }

            user.FirstName = request.FirstName.Trim();
            user.LastName = request.LastName.Trim();
            user.PhoneNumber = request.PhoneNumber.Trim();

            await _context.SaveChangesAsync();

            return Ok(new { message = "Profile updated successfully." });
        }

       
        [HttpPost("me/picture")]
        public async Task<IActionResult> UploadPicture([FromForm] FileUploadRequest request)
        {
            int? userID = HttpContext.Session.GetInt32("UserID");

            if (userID == null)
            {
                return Unauthorized(new { message = "Please log in first." });
            }

            string? error = ImageValidator.Validate(request.File);

            if (error != null)
            {
                return BadRequest(new { message = error });
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserID == userID.Value);

            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }

            user.ProfilePictureUrl = await _storage.UploadImageAsync(request.File, "profiles");
            await _context.SaveChangesAsync();

            return Ok(new { message = "Profile picture updated.", url = user.ProfilePictureUrl });
        }
    }

    public class ProfileRequest // This class is used to represent a profile update request
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
    }
}