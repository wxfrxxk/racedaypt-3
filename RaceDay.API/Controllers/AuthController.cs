
using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.Models;

namespace RaceDay.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly RaceDayContext _context;

        public AuthController(RaceDayContext context)
        {
            _context = context;
        }

        // POST: api/Auth/register
        [HttpPost("register")]
        public async Task<IActionResult> Register(
            [FromBody] RegisterRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.FirstName) ||
                string.IsNullOrWhiteSpace(request.LastName) ||
                string.IsNullOrWhiteSpace(request.Email) ||
                string.IsNullOrWhiteSpace(request.Password) ||
                string.IsNullOrWhiteSpace(request.PhoneNumber))
            {
                return BadRequest(new
                {
                    message = "All fields are required."
                });
            }

            if (request.Password.Length < 8)
            {
                return BadRequest(new
                {
                    message = "Password must contain at least 8 characters."
                });
            }

            string email = request.Email.Trim().ToLowerInvariant();

            bool emailExists = await _context.Users
                .AnyAsync(u => u.Email == email);

            if (emailExists)
            {
                return Conflict(new
                {
                    message = "Email address already exists."
                });
            }

            var participantRole = await _context.Roles
                .FirstOrDefaultAsync(r => r.RoleName == "Participant");

            if (participantRole == null)
            {
                return StatusCode(500, new
                {
                    message = "Participant role was not found in the database."
                });
            }

            var user = new User
            {
                FirstName = request.FirstName.Trim(),
                LastName = request.LastName.Trim(),
                Email = email,
                Password = HashPassword(request.Password),
                PhoneNumber = request.PhoneNumber.Trim(),
                RoleID = participantRole.RoleID
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Registration successful.",
                user.UserID,
                user.FirstName,
                user.LastName,
                user.Email,
                Role = participantRole.RoleName
            });
        }

        // POST: api/Auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login(
            [FromBody] LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) ||
                string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new
                {
                    message = "Email and password are required."
                });
            }

            string email = request.Email.Trim().ToLowerInvariant();

            var user = await _context.Users
                .Join(
                    _context.Roles,
                    u => u.RoleID,
                    r => r.RoleID,
                    (u, r) => new
                    {
                        User = u,
                        RoleName = r.RoleName
                    })
                .FirstOrDefaultAsync(x => x.User.Email == email);

            if (user == null ||
                !VerifyPassword(request.Password, user.User.Password))
            {
                return Unauthorized(new
                {
                    message = "Invalid email or password."
                });
            }

            // Upgrade a legacy plain-text password after successful login.
            if (!user.User.Password.Contains(':'))
            {
                user.User.Password = HashPassword(request.Password);
                await _context.SaveChangesAsync();
            }

            // Store the authenticated user's details in the session.
            HttpContext.Session.SetInt32("UserID", user.User.UserID);
            HttpContext.Session.SetString("Role", user.RoleName);
            HttpContext.Session.SetString("Email", user.User.Email);

            return Ok(new
            {
                message = "Login successful.",
                user.User.UserID,
                user.User.FirstName,
                user.User.LastName,
                user.User.Email,
                Role = user.RoleName
            });
        }

        // POST: api/Auth/logout
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();

            return Ok(new
            {
                message = "Logout successful."
            });
        }

        // Hash a password using PBKDF2 with SHA-256.
        private static string HashPassword(string password)
        {
            byte[] salt = RandomNumberGenerator.GetBytes(16);

            using var pbkdf2 = new Rfc2898DeriveBytes(
                password,
                salt,
                100000,
                HashAlgorithmName.SHA256);

            byte[] hash = pbkdf2.GetBytes(32);

            return $"{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}";
        }

        // Verify both legacy seed passwords and hashed passwords.
        private static bool VerifyPassword(
            string password,
            string storedPassword)
        {
            try
            {
                // Temporary support for the plain-text seed passwords.
                if (!storedPassword.Contains(':'))
                {
                    return string.Equals(
                        password,
                        storedPassword,
                        StringComparison.Ordinal);
                }

                string[] parts = storedPassword.Split(':');

                if (parts.Length != 2)
                {
                    return false;
                }

                byte[] salt = Convert.FromBase64String(parts[0]); // this line of code is converting the first part of the stored password (the salt) from a Base64-encoded string to a byte array. The salt is used in the password hashing process to add randomness and make it more secure against attacks like rainbow tables.
                byte[] storedHash = Convert.FromBase64String(parts[1]);

                using var pbkdf2 = new Rfc2898DeriveBytes( //  this line of code is creating a new instance of the Rfc2898DeriveBytes class, which is used to derive a cryptographic key from a password using the PBKDF2 algorithm. The constructor takes four parameters: the password, the salt, the number of iterations (100000), and the hash algorithm (SHA256). This means that the password will be hashed 100000 times using SHA256, with the provided salt, to produce a derived key.
                    password,
                    salt,
                    100000,
                    HashAlgorithmName.SHA256);

                byte[] computedHash = pbkdf2.GetBytes(32);

                return CryptographicOperations.FixedTimeEquals(
                    computedHash,
                    storedHash);
            }
            catch (FormatException)
            {
                return false;
            }
        }
    }

    public class RegisterRequest
    {
        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        public string PhoneNumber { get; set; } = string.Empty;
    }

    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;
    }
}