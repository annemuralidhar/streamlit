using Microsoft.AspNetCore.Mvc;
using OWASPVulnerableApp.Data;
using OWASPVulnerableApp.Models;
using System.Data;
using System.Diagnostics;

namespace OWASPVulnerableApp.Controllers;

/// <summary>
/// A03:2021 - Injection
/// A01:2021 - Broken Access Control
/// A09:2021 - Logging and Monitoring Failures
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly VulnerableDbContext _context;

    public ProductsController(VulnerableDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// VULNERABLE: SQL Injection via string concatenation
    /// A03:2021 - SQL Injection
    /// </summary>
    [HttpGet("search")]
    public IActionResult SearchByName(string name)
    {
        try
        {
            // A03:2021 - SQL Injection vulnerability
            // User input directly concatenated into query
            var products = _context.Products
                .FromSqlInterpolated($"SELECT * FROM Products WHERE Name LIKE '%{name}%'")
                .ToList();

            return Ok(products);
        }
        catch (Exception ex)
        {
            // A05:2021 - Detailed error messages exposed
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// VULNERABLE: No access control on sensitive operations
    /// A01:2021 - Broken Access Control
    /// A09:2021 - No logging of who deleted what
    /// </summary>
    [HttpDelete("{id}")]
    public IActionResult DeleteProduct(int id)
    {
        // A01:2021 - No authorization check, anyone can delete
        var product = _context.Products.Find(id);

        if (product == null)
            return NotFound();

        _context.Products.Remove(product);
        _context.SaveChanges();

        // A09:2021 - No security logging
        return Ok(new { message = "Product deleted" });
    }

    /// <summary>
    /// VULNERABLE: Allows arbitrary ID manipulation
    /// A01:2021 - Broken Access Control / IDOR
    /// </summary>
    [HttpGet("{id}")]
    public IActionResult GetProduct(int id)
    {
        // A01:2021 - No verification that user owns this resource
        var product = _context.Products.Find(id);

        if (product == null)
            return NotFound();

        return Ok(product);
    }

    /// <summary>
    /// VULNERABLE: Command injection via system execution
    /// A03:2021 - Command Injection
    /// </summary>
    [HttpPost("generate-report")]
    public IActionResult GenerateReport(string format)
    {
        try
        {
            // A03:2021 - Command Injection vulnerability
            var startInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c dir C:\\temp && echo Report format: {format}",
                UseShellExecute = true
            };

            Process.Start(startInfo);

            return Ok(new { message = "Report generation started" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// VULNERABLE: No input validation on bulk operations
    /// A01:2021 - Broken Access Control
    /// </summary>
    [HttpPost("bulk-update")]
    public IActionResult BulkUpdate([FromBody] List<int> productIds, [FromBody] decimal newPrice)
    {
        // A01:2021 - No verification of ownership
        // A06:2021 - No parameter validation
        foreach (var id in productIds)
        {
            var product = _context.Products.Find(id);
            if (product != null)
            {
                product.Price = newPrice;
            }
        }

        _context.SaveChanges();
        return Ok(new { message = $"Updated {productIds.Count} products" });
    }

    /// <summary>
    /// VULNERABLE: Exposes internal IDs and metadata
    /// A01:2021 - Information Disclosure
    /// </summary>
    [HttpGet("internal-metadata")]
    public IActionResult GetInternalMetadata()
    {
        var products = _context.Products.Select(p => new
        {
            p.Id,
            p.Name,
            p.Price,
            p.CreatedBy,
            p.StockQuantity,
            p.CreatedAt,
            InternalId = p.Id * 1000, // Unnecessary internal data
            DatabaseField = "This should be hidden"
        }).ToList();

        return Ok(products);
    }
}
