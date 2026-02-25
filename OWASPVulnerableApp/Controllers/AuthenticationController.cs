using Microsoft.AspNetCore.Mvc;
using OWASPVulnerableApp.Data;
using OWASPVulnerableApp.Models;

namespace OWASPVulnerableApp.Controllers;

/// <summary>
/// A07:2021 - Identification and Authentication Failures
/// A05:2021 - Security Misconfiguration
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthenticationController : ControllerBase
{
    private readonly VulnerableDbContext _context;

    public AuthenticationController(VulnerableDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// VULNERABLE: Plaintext password comparison, no hashing
    /// A07:2021 - Weak password storage
    /// </summary>
    [HttpPost("login")]
    public IActionResult Login(string username, string password)
    {
        try
        {
            // A03:2021 - Injection vulnerability: No parameterized query
            var user = _context.Users
                .FirstOrDefault(u => u.Username == username && u.Password == password);

            if (user == null)
                return Unauthorized(new { message = "Invalid credentials" });

            // A07:2021 - No JWT or secure session token
            return Ok(new { id = user.Id, username = user.Username, role = user.Role });
        }
        catch (Exception ex)
        {
            // A05:2021 - Detailed error exposure
            return StatusCode(500, new { error = ex.Message, stackTrace = ex.StackTrace });
        }
    }

    /// <summary>
    /// VULNERABLE: Hardcoded credentials
    /// A07:2021 - Hardcoded secrets
    /// </summary>
    [HttpPost("admin-login")]
    public IActionResult AdminLogin(string password)
    {
        // A07:2021 - Hardcoded credentials
        const string HARDCODED_ADMIN_PASSWORD = "SecurePassword123";

        if (password == HARDCODED_ADMIN_PASSWORD)
        {
            return Ok(new { message = "Admin login successful", token = "hardcoded-token-12345" });
        }

        return Unauthorized();
    }

    /// <summary>
    /// VULNERABLE: No password validation
    /// A07:2021 - Weak password requirements
    /// </summary>
    [HttpPost("register")]
    public IActionResult Register(string username, string password)
    {
        // A07:2021 - No validation, accepts weak passwords
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            return BadRequest();

        var user = new User
        {
            Username = username,
            Password = password, // Stored in plaintext!
            Email = $"{username}@example.com"
        };

        _context.Users.Add(user);
        _context.SaveChanges();

        return Ok(new { message = "User registered", userId = user.Id });
    }

    /// <summary>
    /// VULNERABLE: No authentication on debug endpoint
    /// A05:2021 - Exposed sensitive configuration
    /// </summary>
    [HttpGet("debug-users")]
    public IActionResult GetAllUsers()
    {
        // A01:2021 - No authentication/authorization check
        // A05:2021 - Exposes all users with passwords
        var users = _context.Users.ToList();
        return Ok(users);
    }
}
