using Microsoft.AspNetCore.Mvc;

namespace OWASPVulnerableApp.Controllers;

/// <summary>
/// A10:2021 - Server-Side Request Forgery (SSRF)
/// A01:2021 - Broken Access Control
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class SSRFController : ControllerBase
{
    /// <summary>
    /// VULNERABLE: Accepts arbitrary URLs without validation
    /// A10:2021 - Server-Side Request Forgery
    /// </summary>
    [HttpPost("fetch-url")]
    public async Task<IActionResult> FetchUrl([FromBody] string url)
    {
        try
        {
            // A10:2021 - No URL validation or whitelist
            // User can request internal resources like http://localhost:8000/admin
            using (var client = new HttpClient())
            {
                var response = await client.GetAsync(url);
                var content = await response.Content.ReadAsStringAsync();

                return Ok(new { url = url, content = content });
            }
        }
        catch (Exception ex)
        {
            // A05:2021 - Detailed error exposure reveals internal information
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// VULNERABLE: Downloads files from user-supplied URLs
    /// A10:2021 - SSRF via file downloads
    /// A08:2021 - Software Integrity Issues
    /// </summary>
    [HttpPost("download-file")]
    public async Task<IActionResult> DownloadFile(string fileUrl)
    {
        try
        {
            // A10:2021 - No URL validation
            // Can be used to download files from internal network
            using (var client = new HttpClient())
            {
                var response = await client.GetAsync(fileUrl);
                var fileBytes = await response.Content.ReadAsByteArrayAsync();

                var fileName = Path.GetFileName(fileUrl);
                return File(fileBytes, "application/octet-stream", fileName);
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// VULNERABLE: Pings internal servers without validation
    /// A10:2021 - SSRF / Internal Network Scanning
    /// </summary>
    [HttpPost("check-service")]
    public async Task<IActionResult> CheckService(string host, int port)
    {
        try
        {
            // A10:2021 - Allows scanning of internal network
            // User can discover running services on internal IPs
            using (var client = new TcpClient())
            {
                var connectTask = client.ConnectAsync(host, port);
                var completedTask = await Task.WhenAny(connectTask, Task.Delay(3000));

                if (completedTask == connectTask)
                {
                    return Ok(new { host = host, port = port, status = "open" });
                }
                else
                {
                    return Ok(new { host = host, port = port, status = "closed" });
                }
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// VULNERABLE: Allows proxying requests through the server
    /// A10:2021 - SSRF via request proxying
    /// </summary>
    [HttpPost("proxy-request")]
    public async Task<IActionResult> ProxyRequest([FromBody] ProxyRequest request)
    {
        try
        {
            // A10:2021 - Proxy request without destination validation
            using (var client = new HttpClient())
            {
                var httpRequest = new HttpRequestMessage
                {
                    Method = new HttpMethod(request.Method),
                    RequestUri = new Uri(request.Url)
                };

                if (!string.IsNullOrEmpty(request.Body))
                {
                    httpRequest.Content = new StringContent(request.Body);
                }

                var response = await client.SendAsync(httpRequest);
                var content = await response.Content.ReadAsStringAsync();

                return Ok(new { statusCode = response.StatusCode, content = content });
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }
}

public class ProxyRequest
{
    public string Url { get; set; } = string.Empty;
    public string Method { get; set; } = "GET";
    public string Body { get; set; } = string.Empty;
}
