using Microsoft.AspNetCore.Mvc;

namespace OWASPVulnerableApp.Controllers;

/// <summary>
/// A04:2021 - Insecure Design
/// A09:2021 - Logging and Monitoring Failures
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class InsecureDesignController : ControllerBase
{
    // A09:2021 - No proper logging mechanism
    private static List<string> _applicationLogs = new();

    /// <summary>
    /// VULNERABLE: No rate limiting or account lockout
    /// A04:2021 - Insecure Design / Brute Force
    /// </summary>
    [HttpPost("unlimited-login")]
    public IActionResult UnlimitedLoginAttempts(string username, string password)
    {
        // A04:2021 - No rate limiting, allows unlimited brute force attempts
        // A09:2021 - No logging of failed attempts

        if (username == "admin" && password == "admin123")
        {
            return Ok(new { message = "Login successful" });
        }

        return Unauthorized(new { message = "Invalid credentials" });
    }

    /// <summary>
    /// VULNERABLE: No CSRF protection
    /// A04:2021 - Missing security controls
    /// </summary>
    [HttpPost("transfer-money")]
    public IActionResult TransferMoney(int fromAccount, int toAccount, decimal amount)
    {
        // A04:2021 - No CSRF token validation
        // An attacker can craft a request that executes on behalf of authenticated user

        // Simulate money transfer
        return Ok(new
        {
            message = "Transfer successful",
            from = fromAccount,
            to = toAccount,
            amount = amount
        });
    }

    /// <summary>
    /// VULNERABLE: No session timeout or expiration
    /// A04:2021 - Weak session management
    /// </summary>
    [HttpPost("create-permanent-session")]
    public IActionResult CreatePermanentSession(string userId)
    {
        // A04:2021 - Sessions never expire
        var sessionToken = Guid.NewGuid().ToString();

        return Ok(new
        {
            sessionToken = sessionToken,
            expiresIn = "Never", // Bad design!
            userId = userId
        });
    }

    /// <summary>
    /// VULNERABLE: No input validation or filtering
    /// A04:2021 - Insecure design allows XXS
    /// </summary>
    [HttpPost("store-comment")]
    public IActionResult StoreComment(string comment)
    {
        // A04:2021 - No input validation
        // A09:2021 - No logging
        _applicationLogs.Add($"Comment: {comment}");

        return Ok(new { message = "Comment stored", logCount = _applicationLogs.Count });
    }

    /// <summary>
    /// VULNERABLE: Exposes internal logs with sensitive information
    /// A09:2021 - Logging and Monitoring Failures
    /// </summary>
    [HttpGet("view-logs")]
    public IActionResult ViewLogs()
    {
        // A09:2021 - No authentication/authorization on logs
        // A05:2021 - Exposes internal application logs

        return Ok(new { logs = _applicationLogs });
    }

    /// <summary>
    /// VULNERABLE: No audit trail for critical operations
    /// A09:2021 - Missing audit logging
    /// </summary>
    [HttpPost("delete-user-account")]
    public IActionResult DeleteUserAccount(int userId)
    {
        // A09:2021 - No audit trail of who deleted what and when
        // A01:2021 - No authorization check

        // Simulate user deletion
        _applicationLogs.Add($"User {userId} deleted (no timestamp, no actor info)");

        return Ok(new { message = "User account deleted" });
    }

    /// <summary>
    /// VULNERABLE: No monitoring of suspicious activity
    /// A09:2021 - Monitoring Failures
    /// </summary>
    [HttpGet("suspicious-check")]
    public IActionResult CheckSuspiciousActivity()
    {
        // A09:2021 - No detection of suspicious patterns
        // No alerting mechanism for unusual behavior

        return Ok(new
        {
            anomaliesDetected = 0,
            suspiciousActivities = "None monitored",
            alertsEnabled = false
        });
    }

    /// <summary>
    /// VULNERABLE: Information leakage in error messages
    /// A05:2021 - Security Misconfiguration
    /// A09:2021 - Inadequate logging and monitoring
    /// </summary>
    [HttpPost("database-query")]
    public IActionResult ExecuteQuery(string query)
    {
        try
        {
            // Simulate database query execution
            if (query.Contains("DROP"))
            {
                throw new Exception("Database error: Cannot execute DROP TABLE statement");
            }

            return Ok(new { message = "Query executed" });
        }
        catch (Exception ex)
        {
            // A05:2021 - Exposes database error details
            // A09:2021 - Doesn't log properly
            return StatusCode(500, new
            {
                error = ex.Message,
                database = "MySecureDB",
                timestamp = DateTime.UtcNow
            });
        }
    }
}
