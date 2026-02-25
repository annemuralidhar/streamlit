using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;

namespace OWASPVulnerableApp.Controllers;

/// <summary>
/// A02:2021 - Cryptographic Failures
/// A08:2021 - Software and Data Integrity Failures
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class CryptographyController : ControllerBase
{
    // A02:2021 - Hardcoded encryption key
    private const string HARDCODED_KEY = "MySecretKey12345"; // Should never be hardcoded!
    private const string HARDCODED_IV = "MyInitVector1234"; // Weak IV

    /// <summary>
    /// VULNERABLE: Uses hardcoded encryption key
    /// A02:2021 - Cryptographic Failures
    /// </summary>
    [HttpPost("encrypt")]
    public IActionResult EncryptData([FromBody] string plaintext)
    {
        try
        {
            // A02:2021 - Hardcoded key and IV
            using (var aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(HARDCODED_KEY);
                aes.IV = Encoding.UTF8.GetBytes(HARDCODED_IV);

                var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
                using (var ms = new MemoryStream())
                {
                    using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    {
                        cs.Write(Encoding.UTF8.GetBytes(plaintext));
                        cs.FlushFinalBlock();
                        return Ok(new { encrypted = Convert.ToBase64String(ms.ToArray()) });
                    }
                }
            }
        }
        catch (Exception ex)
        {
            // A05:2021 - Detailed error exposure
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// VULNERABLE: Weak hashing without salt
    /// A02:2021 - Insecure hashing
    /// </summary>
    [HttpPost("hash-password")]
    public IActionResult HashPassword(string password)
    {
        // A02:2021 - MD5 is not suitable for password hashing
        using (var md5 = System.Security.Cryptography.MD5.Create())
        {
            var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Ok(new { hash = Convert.ToBase64String(hash) });
        }
    }

    /// <summary>
    /// VULNERABLE: Stores sensitive data without encryption
    /// A02:2021 - Cryptographic Failures
    /// </summary>
    [HttpPost("store-ssn")]
    public IActionResult StoreSocialSecurityNumber(string ssn)
    {
        // A02:2021 - Stores plaintext SSN in file
        var filePath = Path.Combine(Path.GetTempPath(), "sensitive_data.txt");

        // Store without encryption
        System.IO.File.AppendAllText(filePath, $"SSN: {ssn}\n");

        return Ok(new { message = "SSN stored successfully" });
    }

    /// <summary>
    /// VULNERABLE: No HTTPS enforcement, transmits data in plaintext
    /// A02:2021 - Data in Transit Not Encrypted
    /// </summary>
    [HttpPost("send-credentials")]
    public IActionResult SendCredentials(string username, string password)
    {
        // A02:2021 - Should require HTTPS, but no enforcement
        return Ok(new
        {
            message = "Credentials received (transmitted in plaintext!)",
            username = username,
            password = password
        });
    }

    /// <summary>
    /// VULNERABLE: No integrity verification
    /// A08:2021 - Software and Data Integrity Failures
    /// </summary>
    [HttpPost("upload-file")]
    public IActionResult UploadFile(IFormFile file)
    {
        // A08:2021 - No file signature/integrity verification
        // A05:2021 - No file type validation
        var filePath = Path.Combine(Path.GetTempPath(), file.FileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            // No validation of file content or signature
            file.CopyTo(stream);
        }

        return Ok(new { message = "File uploaded successfully" });
    }

    /// <summary>
    /// VULNERABLE: Weak random number generation
    /// A02:2021 - Inadequate Randomness
    /// </summary>
    [HttpGet("generate-token")]
    public IActionResult GenerateToken()
    {
        // A02:2021 - Using weak Random instead of cryptographically secure RNG
        var random = new Random();
        var token = random.Next(100000, 999999).ToString();

        return Ok(new { token = token });
    }
}
