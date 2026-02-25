# Claude Code Prompt: ASP.NET Core Razor Pages with OWASP Top 10

## Complete Prompt for Claude Code

```
Build an ASP.NET Core 8.0 Razor Pages educational web application that
demonstrates all OWASP Top 10 (2021) security vulnerabilities with explanations.

PURPOSE: Educational learning platform for security professionals to understand
and identify common web vulnerabilities in a realistic application context.

## Project Requirements

### Technology Stack
- .NET 8.0 (latest)
- ASP.NET Core Razor Pages
- Entity Framework Core
- SQL Server LocalDB (or SQLite for portability)
- Bootstrap 5 for UI
- Razor syntax with C#

### Project Structure
```
OWASPVulnerableApp/
├── Pages/
│   ├── Shared/
│   │   ├── Layout.cshtml
│   │   └── _Layout.cshtml
│   ├── Index.cshtml
│   ├── Index.cshtml.cs
│   ├── Authentication/
│   │   ├── Login.cshtml
│   │   ├── Login.cshtml.cs
│   │   ├── Register.cshtml
│   │   └── Register.cshtml.cs
│   ├── Products/
│   │   ├── Index.cshtml
│   │   ├── Index.cshtml.cs
│   │   ├── Details.cshtml
│   │   └── Details.cshtml.cs
│   ├── Profile/
│   │   ├── MyProfile.cshtml
│   │   ├── MyProfile.cshtml.cs
│   │   ├── AdminPanel.cshtml
│   │   └── AdminPanel.cshtml.cs
│   ├── API/
│   │   ├── DataExport.cshtml.cs
│   │   └── ProcessFile.cshtml.cs
│   ├── Debug/
│   │   ├── ServerInfo.cshtml
│   │   ├── ServerInfo.cshtml.cs
│   │   ├── ViewLogs.cshtml
│   │   └── ViewLogs.cshtml.cs
│   └── Error.cshtml
├── Models/
│   ├── User.cs
│   ├── Product.cs
│   ├── AuditLog.cs
│   └── ApiResponse.cs
├── Data/
│   ├── VulnerableDbContext.cs
│   └── DbInitializer.cs
├── Migrations/
├── appsettings.json
├── appsettings.Development.json
├── Program.cs
├── OWASPVulnerableApp.csproj
├── README.md
├── VULNERABILITIES.md
├── TESTING_GUIDE.md
└── .gitignore
```

## OWASP Top 10 (2021) - Implementation Details

### A01:2021 - Broken Access Control

**Vulnerable Features:**
1. **Product Details - IDOR**
   - Pages/Products/Details.cshtml.cs
   - Allow users to view/edit ANY product by ID
   - No ownership check: if (product == null) return NotFound()
   - No role validation

2. **Admin Panel - Broken Authorization**
   - Pages/Profile/AdminPanel.cshtml.cs
   - No [Authorize(Roles = "Admin")] attribute
   - Check role in code: if (User.Identity.Name != "admin")
   - Allow any logged-in user to access

3. **Direct Account Access**
   - Pages/Profile/MyProfile.cshtml.cs
   - Access other users: GET /profile?userId=2
   - No validation that userId matches current user

**Code Example:**
```csharp
// VULNERABLE A01:2021 - Broken Access Control - No authorization check
public async Task<IActionResult> OnGetDetails(int id)
{
    // A01 - No ownership verification
    Product product = await _context.Products
        .FirstOrDefaultAsync(p => p.Id == id);

    if (product == null)
        return NotFound();

    return Page();
}

// CORRECT APPROACH (commented):
// if (!User.Identity.IsAuthenticated)
//     return Unauthorized();
// var product = await _context.Products
//     .FirstOrDefaultAsync(p => p.Id == id && p.CreatedBy == GetUserId());
```

---

### A02:2021 - Cryptographic Failures

**Vulnerable Features:**
1. **Password Storage**
   - User.cs: Store passwords as MD5 hash (not bcrypt)
   - No salt: Hash = MD5.HashData(password)
   - Display hashes in debug page

2. **Sensitive Data**
   - Store SSN plaintext in database
   - Store credit card plaintext
   - No encryption at rest

3. **Weak Token Generation**
   - Use System.Random for session tokens
   - Predictable reset tokens
   - No cryptographic randomness

4. **HTTP Transmission**
   - Accept credentials over HTTP
   - No HTTPS enforcement
   - Store credentials in query string

**Code Example:**
```csharp
// VULNERABLE A02:2021 - Cryptographic Failures
public async Task<IActionResult> OnPostLogin()
{
    var user = await _context.Users
        .FirstOrDefaultAsync(u => u.Username == Username);

    if (user != null && user.Password == Username) // Plaintext comparison!
    {
        // A02 - Weak token generation
        string token = new Random().Next(10000, 99999).ToString();
        // ... set session ...
    }

    return Page();
}

// VULNERABLE - MD5 Password Hashing
private string HashPassword(string password)
{
    // A02 - MD5 is cryptographically broken
    using (var md5 = MD5.Create())
    {
        var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToHexString(hash);
    }
}

// CORRECT APPROACH (commented):
// using (var scoped = new RNGCryptoServiceProvider())
// {
//     byte[] randomBytes = new byte[32];
//     scoped.GetBytes(randomBytes);
//     return Convert.ToBase64String(randomBytes);
// }
```

---

### A03:2021 - Injection

**Vulnerable Features:**
1. **SQL Injection in Search**
   - Pages/Products/Index.cshtml.cs
   - Use string interpolation in queries
   - No parameterized queries

2. **Command Injection**
   - Pages/API/ProcessFile.cshtml.cs
   - Pass filename to System.Diagnostics.Process
   - Execute arbitrary commands

3. **LDAP Injection (optional)**
   - Search user directory with user input

**Code Example:**
```csharp
// VULNERABLE A03:2021 - SQL Injection
public async Task<IActionResult> OnGetSearch(string searchTerm)
{
    // A03 - Direct string concatenation in query!
    string query = $"SELECT * FROM Products WHERE Name LIKE '%{searchTerm}%'";
    var products = await _context.Products
        .FromSqlRaw(query)  // Dangerous!
        .ToListAsync();

    return Page();
}

// VULNERABLE - Command Injection
public async Task OnPostGenerateReport(string format)
{
    // A03 - User input passed directly to command
    var process = new Process
    {
        StartInfo = new ProcessStartInfo
        {
            FileName = "cmd.exe",
            Arguments = $"/c dir > report.{format}"  // format could be "txt & del *.*"
        }
    };
    process.Start();
    await process.WaitForExitAsync();
}

// CORRECT APPROACH (commented):
// var products = await _context.Products
//     .Where(p => p.Name.Contains(searchTerm))
//     .ToListAsync();
```

---

### A04:2021 - Insecure Design

**Vulnerable Features:**
1. **No Rate Limiting on Login**
   - Pages/Authentication/Login.cshtml.cs
   - Unlimited login attempts
   - No account lockout

2. **No CSRF Protection**
   - POST forms without anti-forgery tokens
   - No verification: @Html.AntiForgeryToken()

3. **Weak Session Management**
   - Sessions never expire
   - No idle timeout
   - No re-authentication for sensitive operations

4. **No Input Validation Framework**
   - No data validation on form submission
   - Accept any input

**Code Example:**
```csharp
// VULNERABLE A04:2021 - Insecure Design - No rate limiting
public async Task<IActionResult> OnPostLogin()
{
    // A04 - No brute force protection
    var user = await _context.Users
        .FirstOrDefaultAsync(u => u.Username == Username);

    if (user == null)
    {
        // Log failed attempt? No!
        ModelState.AddModelError("", "Invalid credentials");
    }

    return Page();
}

// VULNERABLE - No CSRF Protection in Razor
<!-- A04:2021 - Insecure Design - Missing CSRF token -->
<form method="post">
    <!-- Missing: @Html.AntiForgeryToken() -->
    <input type="text" name="username" />
    <button type="submit">Transfer Money</button>
</form>

// CORRECT APPROACH (commented):
// [HttpPost]
// [ValidateAntiForgeryToken]  // Verify CSRF token
// public async Task<IActionResult> OnPostTransferMoney()
```

---

### A05:2021 - Security Misconfiguration

**Vulnerable Features:**
1. **Detailed Error Messages**
   - Return exception details in UI
   - Show stack traces
   - Expose database errors

2. **Debug Mode Enabled**
   - Use app.UseDeveloperExceptionPage() everywhere
   - Swagger UI exposed in production
   - Detailed exception pages

3. **CORS Misconfiguration**
   - Allow all origins
   - AllowAnyMethod, AllowAnyHeader

4. **Information Disclosure**
   - Pages/Debug/ServerInfo.cshtml
   - Expose .NET version
   - List dependency versions
   - Show environment variables

5. **Default Credentials**
   - Seed database with admin/admin123
   - Predictable test accounts

**Code Example:**
```csharp
// VULNERABLE A05:2021 - Security Misconfiguration
// Program.cs
var builder = WebApplicationBuilder.CreateBuilder(args);

// A05 - Enable detailed error pages everywhere
app.UseDeveloperExceptionPage();

// A05 - CORS allows all origins
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
        builder.AllowAnyOrigin()   // Allow any domain
               .AllowAnyMethod()   // Allow any HTTP method
               .AllowAnyHeader()); // Allow any header
});

// A05 - Swagger in production
app.UseSwagger();
app.UseSwaggerUI();

// VULNERABLE - Global exception handler
public async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context)
{
    try { }
    catch (Exception ex)
    {
        // A05 - Expose stack trace to users
        ErrorMessage = ex.ToString();
    }
}

// CORRECT APPROACH (commented):
// if (app.Environment.IsDevelopment())
// {
//     app.UseDeveloperExceptionPage();
// }
// else
// {
//     app.UseExceptionHandler("/Error");
//     app.UseHsts();
// }
```

---

### A06:2021 - Vulnerable and Outdated Components

**Vulnerable Features:**
1. **Outdated NuGet Packages**
   - Reference old versions in .csproj
   - Use deprecated APIs

2. **Weak Cryptography**
   - Use SHA1 (deprecated)
   - Use DES (broken)

3. **XXE Vulnerability**
   - Parse XML without DTD protection
   - Allow external entity resolution

4. **Unsafe Deserialization**
   - Comments about BinaryFormatter dangers
   - Unsafe JSON deserialization

**Code Example:**
```csharp
// VULNERABLE A06:2021 - Vulnerable Components
// In .csproj - comment about outdated packages
<ItemGroup>
  <!-- A06 - Using outdated version with known vulnerabilities -->
  <PackageReference Include="System.Net.Http" Version="4.3.0" />
</ItemGroup>

// VULNERABLE - XXE Attack
public async Task<IActionResult> OnPostUploadXml(IFormFile xmlFile)
{
    var doc = new XmlDocument();
    // A06 - Allow external entity resolution (XXE vulnerability)
    doc.Load(xmlFile.OpenReadStream());

    return Page();
}

// VULNERABLE - SHA1 (deprecated)
private string ComputeFileHash(byte[] fileData)
{
    // A06 - SHA1 is cryptographically broken
    using (var sha1 = SHA1.Create())
    {
        return Convert.ToHexString(sha1.ComputeHash(fileData));
    }
}

// CORRECT APPROACH (commented):
// XmlReaderSettings settings = new XmlReaderSettings
// {
//     DtdProcessing = DtdProcessing.Prohibit,  // Prevent XXE
//     XmlResolver = null
// };
// using (XmlReader reader = XmlReader.Create(stream, settings))
```

---

### A07:2021 - Identification and Authentication Failures

**Vulnerable Features:**
1. **Plaintext Password Storage**
   - User.cs: public string Password { get; set; }
   - Passwords visible in database
   - No hashing

2. **Weak Password Validation**
   - No minimum length requirement
   - No complexity requirements
   - Accept "123" as password

3. **Hardcoded Credentials**
   - Admin account in DbInitializer
   - password = "admin123"
   - Test user with predictable password

4. **No Account Lockout**
   - Unlimited failed login attempts
   - No brute force protection

5. **Insecure Session Management**
   - Session tokens stored in cookies plaintext
   - No HttpOnly flag
   - No Secure flag

**Code Example:**
```csharp
// VULNERABLE A07:2021 - Authentication Failures
// Models/User.cs
public class User
{
    public int Id { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }  // A07 - Stored plaintext!
    public string Email { get; set; }
    public string Role { get; set; }
    // A07 - Sensitive data plaintext
    public string SocialSecurityNumber { get; set; }
    public string CreditCardNumber { get; set; }
}

// VULNERABLE - Register without validation
public async Task<IActionResult> OnPostRegister()
{
    // A07 - No password complexity validation
    var user = new User
    {
        Username = Username,
        Password = Password  // A07 - Plaintext storage
    };

    // No validation:
    // - Minimum length?
    // - Uppercase, lowercase, numbers?
    // - Password != Username?

    _context.Users.Add(user);
    await _context.SaveChangesAsync();

    return RedirectToPage("./Login");
}

// VULNERABLE - DbInitializer with hardcoded password
public static void Initialize(VulnerableDbContext context)
{
    if (context.Users.Any()) return;

    // A07 - Hardcoded default credentials!
    context.Users.AddRange(
        new User { Username = "admin", Password = "admin123", Role = "Admin" },
        new User { Username = "user", Password = "user123", Role = "User" }
    );

    context.SaveChanges();
}

// CORRECT APPROACH (commented):
// using (var hasher = new PasswordHasher<IdentityUser>())
// {
//     string hashedPassword = hasher.HashPassword(user, password);
//     user.PasswordHash = hashedPassword;
// }
```

---

### A08:2021 - Software and Data Integrity Failures

**Vulnerable Features:**
1. **No File Validation**
   - Accept any uploaded file
   - No MIME type checking
   - No file extension validation

2. **No File Signature Verification**
   - Don't verify file magic bytes
   - Accept renamed executable files

3. **Unsafe File Processing**
   - Execute uploaded files
   - Process files without sanitization

4. **Insecure File Storage**
   - Store files with user-supplied names
   - Path traversal vulnerability possible

**Code Example:**
```csharp
// VULNERABLE A08:2021 - Software and Data Integrity
public async Task<IActionResult> OnPostUploadFile(IFormFile file)
{
    if (file == null || file.Length == 0)
        return BadRequest();

    // A08 - No file type validation
    // A08 - No file size limit
    // A08 - No content verification

    var fileName = Path.Combine(_uploadFolder, file.FileName);  // Unsafe!

    using (var stream = new FileStream(fileName, FileMode.Create))
    {
        await file.CopyToAsync(stream);  // Copy without verification
    }

    // A08 - Execute uploaded file?
    if (fileName.EndsWith(".exe"))
    {
        Process.Start(fileName);  // Dangerous!
    }

    return Ok("File uploaded");
}

// CORRECT APPROACH (commented):
// if (file.ContentType != "image/jpeg" && file.ContentType != "image/png")
//     return BadRequest("Invalid file type");
//
// if (file.Length > 5 * 1024 * 1024)  // 5MB max
//     return BadRequest("File too large");
//
// var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
// var extension = Path.GetExtension(file.FileName);
// if (!allowedExtensions.Contains(extension))
//     return BadRequest("File type not allowed");
```

---

### A09:2021 - Logging and Monitoring Failures

**Vulnerable Features:**
1. **No Security Logging**
   - Don't log failed login attempts
   - No audit trail for sensitive operations
   - No monitoring of data access

2. **Exposed Logs**
   - Pages/Debug/ViewLogs.cshtml
   - View logs without authentication
   - Logs contain sensitive data

3. **Logs Without Protection**
   - Store logs in plaintext files
   - No access control
   - No encryption

4. **No Alerting**
   - No detection of suspicious activities
   - No alerts for multiple failed logins
   - No alerts for unauthorized access

**Code Example:**
```csharp
// VULNERABLE A09:2021 - Logging and Monitoring Failures
public class DebugPageModel : PageModel
{
    public List<string> Logs { get; set; } = new();

    // A09 - No authentication check on logs endpoint!
    public IActionResult OnGet()
    {
        // Read logs without authorization
        if (System.IO.File.Exists("logs.txt"))
        {
            Logs = System.IO.File
                .ReadAllLines("logs.txt")
                .ToList();
        }

        return Page();
    }
}

// VULNERABLE - Login without logging failures
public async Task<IActionResult> OnPostLogin()
{
    var user = await _context.Users
        .FirstOrDefaultAsync(u => u.Username == Username);

    if (user == null || user.Password != Password)
    {
        // A09 - No failed login logging!
        // No brute force detection!
        ModelState.AddModelError("", "Invalid credentials");
        return Page();
    }

    return RedirectToPage("./Index");
}

// VULNERABLE - Unprotected audit logs
public void LogFailedLogin(string username)
{
    // A09 - Plaintext log file, world-readable
    string logEntry = $"{DateTime.Now}: Failed login for {username}";
    System.IO.File.AppendAllText("logs.txt", logEntry + Environment.NewLine);
}

// CORRECT APPROACH (commented):
// [Authorize]  // Require authentication
// [Authorize(Roles = "Admin")]  // Require admin role
// public IActionResult OnGetViewLogs()
// {
//     // ... protected logs ...
// }
```

---

### A10:2021 - Server-Side Request Forgery (SSRF)

**Vulnerable Features:**
1. **Unvalidated URL Requests**
   - Pages/API/DataExport.cshtml.cs
   - Accept any URL from user input
   - Fetch from arbitrary URLs

2. **Internal Network Access**
   - Allow requests to localhost
   - Access internal services
   - Request 127.0.0.1, 192.168.*, 10.*

3. **Port Scanning**
   - Check which ports are open
   - Discover running services

4. **Redirect Following**
   - Follow redirects to any destination
   - No whitelist validation

**Code Example:**
```csharp
// VULNERABLE A10:2021 - Server-Side Request Forgery (SSRF)
public async Task<IActionResult> OnGetExportData(string sourceUrl)
{
    // A10 - No URL validation!
    using (var client = new HttpClient())
    {
        try
        {
            // Fetch from ANY URL - including internal services!
            var response = await client.GetStringAsync(sourceUrl);

            // Could be:
            // sourceUrl = "http://localhost:8080/admin"
            // sourceUrl = "http://192.168.1.1"
            // sourceUrl = "http://169.254.169.254/latest/meta-data/"  (AWS metadata)

            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}

// VULNERABLE - Port scanning via SSRF
public async Task<IActionResult> OnGetCheckService(string host, int port)
{
    // A10 - No IP/hostname validation
    string url = $"http://{host}:{port}";

    using (var client = new HttpClient { Timeout = TimeSpan.FromSeconds(2) })
    {
        try
        {
            await client.GetAsync(url);
            return Ok($"Service running on {host}:{port}");  // Port is open!
        }
        catch
        {
            return Ok($"Service not running on {host}:{port}");
        }
    }
}

// VULNERABLE - File download from arbitrary URL
public async Task<IActionResult> OnGetDownloadFile(string fileUrl)
{
    // A10 - No URL validation
    using (var client = new HttpClient())
    {
        var fileData = await client.GetByteArrayAsync(fileUrl);
        return File(fileData, "application/octet-stream", "downloaded-file");
    }
}

// CORRECT APPROACH (commented):
// private bool IsUrlSafe(string url)
// {
//     var uri = new Uri(url);
//
//     // Whitelist allowed domains
//     var allowedDomains = new[] { "example.com", "api.example.com" };
//     if (!allowedDomains.Contains(uri.Host))
//         return false;
//
//     // Block private IP ranges
//     if (uri.Host == "localhost" || uri.Host == "127.0.0.1")
//         return false;
//
//     // Verify not private IP
//     if (IPAddress.TryParse(uri.Host, out var ip))
//     {
//         if (ip.IsPrivate || ip.IsLoopback)
//             return false;
//     }
//
//     return true;
// }
```

---

## Database Models

### User Model
```csharp
public class User
{
    public int Id { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }  // VULNERABLE: Plaintext
    public string Email { get; set; }
    public string Role { get; set; }      // "Admin" or "User"
    public string SocialSecurityNumber { get; set; }  // Plaintext
    public string CreditCardNumber { get; set; }      // Plaintext
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLogin { get; set; }
}
```

### Product Model
```csharp
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public int CreatedBy { get; set; }    // User ID - no ownership validation
    public DateTime CreatedAt { get; set; }
}
```

### AuditLog Model
```csharp
public class AuditLog
{
    public int Id { get; set; }
    public int? UserId { get; set; }
    public string Action { get; set; }
    public string Details { get; set; }
    public DateTime Timestamp { get; set; }
}
```

---

## Razor Pages Structure

### Authentication Pages
- **Login.cshtml** - Vulnerable login with plaintext password
- **Register.cshtml** - No password validation
- Both without CSRF protection

### Product Pages
- **Products/Index.cshtml** - Search with SQL injection
- **Products/Details.cshtml** - IDOR vulnerability
- **Products/Edit** - No ownership verification

### Profile Pages
- **MyProfile.cshtml** - Access other users' data
- **AdminPanel.cshtml** - No authorization check

### Debug Pages
- **ServerInfo.cshtml** - Expose system information
- **ViewLogs.cshtml** - View logs without authentication

### API Endpoints (Handler Methods)
- **DataExport** - SSRF vulnerability
- **ProcessFile** - File upload and command injection

---

## Configuration Files

### Program.cs Requirements
```
- app.UseDeveloperExceptionPage() in all environments
- CORS allowing all origins
- Swagger UI enabled everywhere
- No HTTPS redirection
- Detailed exception handling with stack traces
```

### appsettings.json
```
- Logging with Detailed level
- ConnectionString with obvious credentials
- Debug settings enabled
- Detailed error messages
```

---

## Documentation Requirements

### README.md (Educational Focus)
1. **Clear Educational Purpose**
   - Warning about legal usage
   - Recommended for authorized testing only

2. **Quick Start Guide**
   - Prerequisites
   - Installation steps
   - Running the application
   - Accessing Razor Pages

3. **Vulnerability Overview**
   - Table of all 10 vulnerabilities
   - Links to each page demonstrating vulnerability
   - Impact description for each

4. **Application Walkthrough**
   - User flows (Register, Login, Browse Products)
   - Admin flows (View logs, check server info)
   - API flows (Data export, file processing)

5. **Pages Guide**
   - Location of each vulnerable page
   - What vulnerability it demonstrates
   - How to interact with it

### VULNERABILITIES.md (Detailed Explanations)
1. **For Each OWASP Vulnerability (A01-A10)**
   - Full description
   - Location in code (file names, line numbers)
   - Vulnerable code snippet
   - Explanation of why it's vulnerable
   - Correct approach (code comparison)
   - Risk and impact
   - Related pages/endpoints

2. **Code Examples**
   - Side-by-side vulnerable vs. secure code
   - Comments in code explaining issues
   - CORRECT APPROACH sections showing fixes

### TESTING_GUIDE.md (Practical Exploitation)
1. **Prerequisites**
   - Required tools (browser, Postman, curl)
   - How to run the application
   - Test user credentials

2. **For Each Vulnerability**
   - Step-by-step exploitation instructions
   - How to identify the vulnerability
   - Expected results/responses
   - Screenshots or examples
   - Proof of concept

3. **Test Cases**
   - Use case for each vulnerability
   - Test data examples
   - Expected vs. actual behavior

4. **Common Payloads**
   - SQL injection payloads for A03
   - Search term examples
   - File names for bypass

---

## Code Style Requirements

1. **Educational Comments**
   - Format: // VULNERABLE A0X:2021 - Vulnerability Name
   - Explain WHY it's vulnerable
   - Location in code marked clearly

2. **Realistic Code**
   - Look like real business logic
   - Use meaningful variable names
   - Include proper error handling (with vulnerabilities)

3. **Side-by-Side Code**
   - Show vulnerable approach
   - Show correct approach (commented out)
   - Enable learning through comparison

4. **Inline Documentation**
   - What each controller does
   - What each page demonstrates
   - How vulnerabilities manifest

---

## Important Requirements

1. **Educational Only**
   - Clear warnings in README
   - Disclaimer about unauthorized access
   - Legal usage guidelines

2. **Completeness**
   - All 10 OWASP vulnerabilities demonstrated
   - Multiple pages/endpoints showing real-world usage
   - Every vulnerability testable

3. **Realistic UI**
   - Clean, professional-looking Razor Pages
   - Bootstrap styling
   - Real user workflows

4. **Proper Structure**
   - Follow ASP.NET Core conventions
   - Clean separation of concerns
   - Models, Pages, Data layers

---

## Deliverables Checklist

- [ ] Complete ASP.NET Core Razor Pages project
- [ ] Pages folder with all Razor pages created
- [ ] Models folder with User, Product, AuditLog
- [ ] Data folder with DbContext and DbInitializer
- [ ] Program.cs with vulnerable configuration
- [ ] appsettings.json with detailed logging
- [ ] All 10 OWASP vulnerabilities implemented
- [ ] Each vulnerability on separate page(s)
- [ ] Comprehensive README.md (1000+ words)
- [ ] Detailed VULNERABILITIES.md guide
- [ ] TESTING_GUIDE.md with test cases
- [ ] Inline code comments for all vulnerabilities
- [ ] Bootstrap UI styling applied
- [ ] Database initialization working
- [ ] Swagger/API documentation (if APIs included)
- [ ] .gitignore file

---

## Success Criteria

✅ Application builds with: dotnet build
✅ Runs with: dotnet run
✅ All pages load in browser
✅ Can register and login users
✅ Each vulnerability is demonstrable
✅ Code comments explain vulnerabilities
✅ Documentation is comprehensive
✅ UI is clean and professional
✅ Database initializes with seed data
✅ All test cases work as documented

---

## Notes for Claude Code

- Create realistic, working Razor Pages application
- Make intentional vulnerabilities obvious for learning
- Include comments explaining each vulnerability
- Show both vulnerable and correct approaches
- Ensure every page is functional and demonstrates real business logic
- Keep code readable and educational
- Make documentation comprehensive and practical
- Include warnings about educational use
- Ensure easy to test and understand
```

## End of Prompt

---

## How to Use This Prompt with Claude Code

### Option 1: Using Claude Code CLI
```bash
# Copy the prompt content and save to a file, then:
cat DOTNET_RAZOR_PAGES_OWASP_PROMPT.md | claude code
```

### Option 2: Using Web Interface
1. Go to https://claude.ai/code
2. Paste the prompt (between the triple backticks)
3. Click "Create" or "Generate"

### Option 3: Direct Paste
Copy everything between the outer triple backticks and paste into Claude Code.

---

## What You'll Get

A complete **ASP.NET Core 8.0 Razor Pages application** with:

✅ **Authentication Pages** - Register/Login with vulnerable auth
✅ **Product Pages** - IDOR, SQL injection vulnerabilities
✅ **Profile Pages** - Broken access control
✅ **Admin Pages** - No authorization checks
✅ **Debug Pages** - Information disclosure
✅ **API Pages** - SSRF, file upload vulnerabilities
✅ **Database** - User and Product models with plaintext data
✅ **Complete Documentation** - README, VULNERABILITIES, TESTING guides
✅ **Educational Comments** - Every vulnerability explained
✅ **Professional UI** - Bootstrap-styled Razor Pages
✅ **Working Seed Data** - Test accounts and products

---

## Customization Variants

### Variant 1: Add Advanced Features
Add this to the prompt:
```
Additional Requirements:
- Add Shopping Cart functionality (with A04 insufficient session validation)
- Add Order History (with A09 logging failures)
- Add Export to CSV (with A10 SSRF via CSV source)
- Add Two-Factor Authentication (vulnerable OTP implementation - A07)
- Add Notification System (with A05 configuration exposure)
```

### Variant 2: Add CTF Elements
```
Add CTF Features:
- Hidden flags in each vulnerability
- Point scoring system
- Difficulty ratings per vulnerability
- Progress tracking page
- Hints for each challenge
```

### Variant 3: Add Microservices
```
Convert to Microservices:
- Authentication Service
- Product Service
- Audit Logging Service
- Communication vulnerabilities between services (A10)
- Service-to-service authentication failures (A07)
```

---

## After Generation

1. **Review the code** - Understand each vulnerability
2. **Run the application** - See it working
3. **Test each page** - Follow the TESTING_GUIDE.md
4. **Read VULNERABILITIES.md** - Learn the concepts
5. **Study the code** - Compare vulnerable vs. correct approaches
6. **Modify and experiment** - Create your own test cases

---

Save this file and reuse for future .NET Core projects!
