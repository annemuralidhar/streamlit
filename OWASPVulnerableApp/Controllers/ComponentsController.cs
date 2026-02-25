using Microsoft.AspNetCore.Mvc;

namespace OWASPVulnerableApp.Controllers;

/// <summary>
/// A06:2021 - Vulnerable and Outdated Components
/// A02:2021 - Cryptographic Failures
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ComponentsController : ControllerBase
{
    /// <summary>
    /// VULNERABLE: Uses outdated serialization (vulnerable to deserialization attacks)
    /// A06:2021 - Vulnerable Components
    /// </summary>
    [HttpPost("deserialize-object")]
    public IActionResult DeserializeObject([FromBody] string serializedData)
    {
        try
        {
            // A06:2021 - Using BinaryFormatter (deprecated and unsafe)
            // This demonstrates using outdated, vulnerable components
            // NOTE: BinaryFormatter is dangerous and removed in .NET 5+

            return Ok(new
            {
                message = "Object deserialized (UNSAFE in production)",
                warning = "BinaryFormatter is vulnerable to deserialization attacks"
            });
        }
        catch (Exception ex)
        {
            // A05:2021 - Detailed error exposure
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// VULNERABLE: Dependency information disclosure
    /// A06:2021 - Outdated components exposed in metadata
    /// A05:2021 - Information disclosure
    /// </summary>
    [HttpGet("dependency-info")]
    public IActionResult GetDependencyInfo()
    {
        // A06:2021 - Exposes version information of dependencies
        // A05:2021 - Information disclosure

        return Ok(new
        {
            dependencies = new Dictionary<string, string>
            {
                { "EntityFramework", "8.0.0" }, // Hypothetical outdated version
                { "JsonSerializerDep", "1.0.0" }, // Old version with CVEs
                { "XmlParser", "2.1.0" }, // Known vulnerable version
                { "Newtonsoft.Json", "12.0.1" } // Outdated version
            },
            framework = ".NET 8.0"
        });
    }

    /// <summary>
    /// VULNERABLE: Uses weak XML parsing
    /// A06:2021 - Vulnerable Component (XXE Attack)
    /// </summary>
    [HttpPost("parse-xml")]
    public IActionResult ParseXml([FromBody] string xmlContent)
    {
        try
        {
            // A06:2021 - Weak XML parsing vulnerable to XXE
            var xmlDoc = new System.Xml.XmlDocument();

            // A06:2021 - XXE vulnerability - not disabling DTD processing
            xmlDoc.LoadXml(xmlContent);

            return Ok(new { message = "XML parsed successfully" });
        }
        catch (Exception ex)
        {
            // A05:2021 - Error details exposure
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// VULNERABLE: Uses outdated encryption methods
    /// A06:2021 - Vulnerable Cryptography
    /// A02:2021 - Weak Encryption
    /// </summary>
    [HttpPost("encrypt-legacy")]
    public IActionResult EncryptWithLegacyMethod(string data)
    {
        // A06:2021 - DES is outdated and weak (only 56-bit key)
        // A02:2021 - Should use AES instead

        using (var des = System.Security.Cryptography.DES.Create())
        {
            // DES has a 56-bit key, is very weak
            var key = System.Text.Encoding.UTF8.GetBytes("12345678"); // 8 bytes for DES
            des.Key = key;
            des.Mode = System.Security.Cryptography.CipherMode.ECB; // ECB is insecure!

            var encryptor = des.CreateEncryptor();
            var inputBytes = System.Text.Encoding.UTF8.GetBytes(data);
            var encrypted = encryptor.TransformFinalBlock(inputBytes, 0, inputBytes.Length);

            return Ok(new
            {
                message = "Encrypted with legacy DES (INSECURE!)",
                encrypted = Convert.ToBase64String(encrypted),
                warning = "DES is deprecated and should not be used"
            });
        }
    }

    /// <summary>
    /// VULNERABLE: Vulnerable JSON deserialization
    /// A06:2021 - Outdated vulnerable library
    /// </summary>
    [HttpPost("deserialize-json")]
    public IActionResult DeserializeJson([FromBody] string jsonData)
    {
        try
        {
            // A06:2021 - Using vulnerable JSON deserialization
            // Depending on library and settings, could be vulnerable to type confusion

            var obj = System.Text.Json.JsonSerializer.Deserialize<dynamic>(jsonData);
            return Ok(new { message = "JSON deserialized", data = obj });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// VULNERABLE: Tells attacker about server software
    /// A05:2021 - Security Misconfiguration
    /// </summary>
    [HttpGet("server-info")]
    public IActionResult GetServerInfo()
    {
        // A05:2021 - Exposes server version information
        return Ok(new
        {
            server = "ASP.NET Core 8.0",
            osVersion = System.Runtime.InteropServices.RuntimeInformation.OSDescription,
            dotnetVersion = System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription,
            message = "This information helps attackers identify vulnerabilities"
        });
    }
}
