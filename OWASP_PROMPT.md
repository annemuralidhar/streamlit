# Claude Code Prompt: OWASP Top 10 Vulnerable .NET Core Application

## Complete Prompt to Use with Claude Code

```
Build a comprehensive .NET Core web application that intentionally demonstrates
all OWASP Top 10 (2021) security vulnerabilities for educational and authorized
security testing purposes.

## Project Requirements

### Technology Stack
- .NET 8.0 (latest stable)
- ASP.NET Core Web API
- Entity Framework Core for database operations
- SQL Server connection (local or remote)
- Swagger/OpenAPI for documentation

### Application Structure
Create the following folder structure:
```
OWASPVulnerableApp/
├── Controllers/
│   ├── AuthenticationController.cs
│   ├── ProductsController.cs
│   ├── CryptographyController.cs
│   ├── SSRFController.cs
│   ├── InsecureDesignController.cs
│   └── ComponentsController.cs
├── Models/
│   ├── User.cs
│   ├── Product.cs
│   └── ApiResponse.cs
├── Data/
│   └── VulnerableDbContext.cs
├── Migrations/
├── Program.cs
├── appsettings.json
├── OWASPVulnerableApp.csproj
├── README.md
├── TESTING.md
└── .gitignore
```

## OWASP Top 10 (2021) Vulnerabilities to Implement

### A01:2021 - Broken Access Control
Implement the following vulnerabilities:
1. **No Authentication on Sensitive Endpoints**
   - Create an endpoint that exposes user data without authentication
   - Example: GET /api/authentication/debug-users should return all users with sensitive info

2. **Insecure Direct Object References (IDOR)**
   - Implement product endpoints where users can access/modify any product by ID
   - No verification that the user owns the resource
   - Example: GET /api/products/{id} accessible with any ID

3. **No Authorization Checks**
   - Delete, update operations without role-based access control
   - Example: DELETE /api/products/{id} - anyone can delete anything

**Implementation Details:**
- AuthenticationController.cs: Add /api/authentication/debug-users endpoint
- ProductsController.cs: Add GET, DELETE endpoints without authorization
- Include comments explaining: "A01:2021 - Broken Access Control - No auth check"

---

### A02:2021 - Cryptographic Failures
Implement the following vulnerabilities:
1. **Hardcoded Encryption Keys**
   - Define encryption keys as constants in the code
   - Use the same key for all operations

2. **Weak Password Hashing**
   - Use MD5 for password hashing instead of bcrypt/PBKDF2
   - Store passwords in plaintext in the database
   - No salt for hashing

3. **Plaintext Sensitive Data Storage**
   - Store SSN, credit card numbers, personal information without encryption
   - Save sensitive data to files without protection

4. **Weak Random Number Generation**
   - Use System.Random instead of cryptographically secure RNG
   - Generate weak tokens/session IDs

5. **No HTTPS Enforcement**
   - Accept and process credentials over HTTP
   - Don't enforce secure transport

**Implementation Details:**
- CryptographyController.cs:
  - POST /api/cryptography/encrypt - use hardcoded key
  - POST /api/cryptography/hash-password - use MD5
  - POST /api/cryptography/store-ssn - save plaintext
  - GET /api/cryptography/generate-token - use weak Random
- User.cs: Password property stored as plaintext string
- Include comments: "A02:2021 - Cryptographic Failures"

---

### A03:2021 - Injection
Implement the following vulnerabilities:
1. **SQL Injection**
   - Concatenate user input directly into SQL queries
   - Use string interpolation or concatenation instead of parameterized queries
   - Example: SELECT * FROM Products WHERE Name LIKE '%{userInput}%'

2. **Command Injection**
   - Pass user input to system commands via Process.Start
   - No input validation or escaping
   - Example: cmd.exe /c dir {userInput}

3. **LDAP Injection** (optional)
   - Build LDAP queries from user input

**Implementation Details:**
- ProductsController.cs:
  - GET /api/products/search?name={name} - SQL injection via FromSqlInterpolated
  - POST /api/products/generate-report?format={format} - command injection via Process.Start
- Include comments: "A03:2021 - Injection - No parameterized query"
- Show unsafe code alongside what NOT to do

---

### A04:2021 - Insecure Design
Implement the following vulnerabilities:
1. **No Rate Limiting**
   - Allow unlimited login attempts
   - No brute force protection

2. **No CSRF Protection**
   - Accept state-changing requests without CSRF tokens
   - No token validation on POST/PUT/DELETE

3. **Weak Session Management**
   - Sessions that never expire
   - No timeout mechanism
   - Predictable session IDs

4. **Missing Security Controls**
   - No input validation framework
   - No security by design

**Implementation Details:**
- InsecureDesignController.cs:
  - POST /api/insecuredesign/unlimited-login - no rate limiting
  - POST /api/insecuredesign/transfer-money - no CSRF token
  - POST /api/insecuredesign/create-permanent-session - sessions never expire
- Include comments: "A04:2021 - Insecure Design"

---

### A05:2021 - Security Misconfiguration
Implement the following vulnerabilities:
1. **Detailed Error Messages**
   - Return stack traces in API responses
   - Expose internal file paths and database errors

2. **Debug Mode Enabled**
   - Swagger/OpenAPI exposed in all environments
   - Detailed exception pages enabled

3. **Unnecessary Features Enabled**
   - CORS allowing all origins
   - Debug endpoints accessible

4. **Default Credentials**
   - Hardcoded default admin accounts
   - Predictable credentials in database

5. **Information Disclosure**
   - Expose server version information
   - List all dependencies and versions

**Implementation Details:**
- Program.cs:
  - Add UseDeveloperExceptionPage() without environment check
  - Configure CORS: AllowAnyOrigin, AllowAnyMethod, AllowAnyHeader
  - Enable Swagger in all environments
- All Controllers: Return exception.Message and exception.StackTrace in catch blocks
- ComponentsController.cs:
  - GET /api/components/server-info - expose .NET version, OS info
  - GET /api/components/dependency-info - list package versions
- VulnerableDbContext.cs: Seed admin account with password "admin123"
- Include comments: "A05:2021 - Security Misconfiguration"

---

### A06:2021 - Vulnerable and Outdated Components
Implement the following vulnerabilities:
1. **Outdated NuGet Packages**
   - Reference old versions in .csproj
   - Use deprecated APIs

2. **Weak Cryptography Libraries**
   - Use DES instead of AES (only 56-bit key, ECB mode)
   - Deprecated encryption algorithms

3. **XXE Vulnerability**
   - Unsafe XML parsing without DTD disabled
   - Load external entities

4. **Unsafe Deserialization**
   - Comments about BinaryFormatter (removed in .NET 5+ but demonstrate vulnerability)
   - Unsafe JSON deserialization

**Implementation Details:**
- OWASPVulnerableApp.csproj:
  - Reference older versions of EntityFrameworkCore (8.0.0)
  - Include comment about vulnerable package versions
- ComponentsController.cs:
  - POST /api/components/encrypt-legacy - use DES encryption
  - POST /api/components/parse-xml - unsafe XML with DTD enabled
  - POST /api/components/deserialize-object - unsafe deserialization
  - GET /api/components/dependency-info - expose dependency versions
- Include comments: "A06:2021 - Vulnerable and Outdated Components"

---

### A07:2021 - Identification and Authentication Failures
Implement the following vulnerabilities:
1. **Plaintext Password Storage**
   - Store passwords in database as plaintext strings
   - No hashing or encryption

2. **Plaintext Password Comparison**
   - Compare passwords using simple string equality
   - No secure comparison methods

3. **Hardcoded Credentials**
   - Define admin password as const string
   - Hardcoded secrets in code

4. **No Password Validation**
   - Accept any password length/complexity
   - No minimum requirements

5. **No Secure Tokens**
   - No JWT or secure session tokens
   - Return basic tokens without signing

**Implementation Details:**
- AuthenticationController.cs:
  - POST /api/authentication/login - plaintext password comparison
  - POST /api/authentication/admin-login - hardcoded admin password
  - POST /api/authentication/register - no password validation
- User.cs:
  - public string Password { get; set; } // Stored plaintext
- VulnerableDbContext.cs:
  - Seed users with plaintext passwords
  - Include admin user with password "admin123"
- Include comments: "A07:2021 - Identification and Authentication Failures"

---

### A08:2021 - Software and Data Integrity Failures
Implement the following vulnerabilities:
1. **No File Signature Verification**
   - Accept and process any uploaded files
   - No file type validation

2. **No File Content Verification**
   - Don't verify file integrity or signatures
   - Execute/process untrusted files

3. **Unsafe Deserialization**
   - Deserialize data without type checking
   - Comment on BinaryFormatter dangers

4. **No Code Signing**
   - Comments about missing code signing verification
   - Demonstrate vulnerability in design

**Implementation Details:**
- CryptographyController.cs:
  - POST /api/cryptography/upload-file - accept any file
  - No MIME type validation
  - No file size limits
  - No virus scanning
- Include comments: "A08:2021 - Software and Data Integrity Failures"

---

### A09:2021 - Logging and Monitoring Failures
Implement the following vulnerabilities:
1. **No Security Logging**
   - Don't log failed login attempts
   - No audit trail for sensitive operations

2. **No Monitoring**
   - No detection of suspicious activities
   - No alerting mechanism

3. **Logs Without Protection**
   - Store logs in unencrypted files
   - No access control on logs

4. **Exposed Logs**
   - Endpoint to view logs without authentication
   - Exposes sensitive information in logs

**Implementation Details:**
- InsecureDesignController.cs:
  - Maintain a static List<string> for logs
  - GET /api/insecuredesign/view-logs - expose logs without auth
  - No logging on delete operations
  - POST /api/insecuredesign/delete-user-account - no audit trail
- Include comments: "A09:2021 - Logging and Monitoring Failures"

---

### A10:2021 - Server-Side Request Forgery (SSRF)
Implement the following vulnerabilities:
1. **Unvalidated URL Requests**
   - Accept any URL from user input
   - Fetch from arbitrary URLs

2. **Internal Network Access**
   - Allow requests to localhost, 127.0.0.1, private IPs
   - Access internal services and metadata

3. **Port Scanning**
   - Ping internal servers on various ports
   - Discover running services

4. **Request Proxying**
   - Act as proxy for arbitrary requests
   - Forward requests to any destination

5. **File Download from Arbitrary URLs**
   - Download and serve files from user-specified URLs

**Implementation Details:**
- SSRFController.cs:
  - POST /api/ssrf/fetch-url - fetch any URL without validation
  - POST /api/ssrf/download-file - download files without validation
  - POST /api/ssrf/check-service - port scan on host:port
  - POST /api/ssrf/proxy-request - proxy requests to any destination
  - No whitelist, no IP validation, no URL scheme validation
- Include comments: "A10:2021 - Server-Side Request Forgery (SSRF)"

---

## Database Models

### User Model
```csharp
public class User
{
    public int Id { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }  // Plaintext storage
    public string Email { get; set; }
    public string Role { get; set; }      // admin, user
    public string SensitiveData { get; set; }  // SSN, CC, etc
    public DateTime CreatedAt { get; set; }
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
    public int CreatedBy { get; set; }    // User ID (no validation)
    public DateTime CreatedAt { get; set; }
}
```

---

## Configuration Files

### appsettings.json
Include settings that demonstrate misconfiguration:
- Enable detailed error logging
- Expose stack traces
- Allow debug endpoints
- Default connection string

### Program.cs
Include vulnerable configuration:
- UseDeveloperExceptionPage() without environment check
- CORS policy allowing all origins
- No HTTPS redirection
- Swagger enabled in all environments
- Detailed error handling

---

## Documentation Requirements

### README.md (Comprehensive)
1. **Project Overview**
   - Clear warning this is for educational use only
   - Prohibited use cases

2. **Vulnerability Map**
   - For each OWASP vulnerability:
     - Affected files and endpoints
     - Description of vulnerability
     - Why it's dangerous
     - Test cases with curl examples

3. **Project Structure**
   - Folder and file organization
   - Purpose of each component

4. **Setup & Running Instructions**
   - Prerequisites (.NET 8.0, SQL Server)
   - Clone and setup steps
   - Database migration commands
   - How to run the application

5. **Access Points**
   - API base URL
   - Swagger UI location
   - Example endpoints

6. **Security Testing Checklist**
   - One checkbox per vulnerability
   - Testing steps

7. **Educational Value**
   - Learning outcomes
   - References to OWASP documentation

### TESTING.md (Detailed Testing Guide)
1. **Prerequisites and Tools**
   - Required tools (curl, Postman, Burp Suite, OWASP ZAP)
   - Installation instructions

2. **For Each Vulnerability (A01-A10)**
   - Detailed description
   - Step-by-step exploitation instructions
   - curl examples
   - Expected results
   - Impact explanation

3. **Common Payloads Reference**
   - SQL injection payloads
   - Command injection payloads
   - XXE payloads
   - SSRF target examples

4. **Tools & Techniques**
   - curl cheat sheet
   - Postman setup
   - Burp Suite setup
   - OWASP ZAP automated testing

5. **Reporting Findings**
   - What to document
   - How to write security reports
   - Evidence collection

---

## Code Quality Standards

1. **Inline Comments**
   - Add comments explaining each vulnerability
   - Format: "A0X:2021 - Vulnerability Type - Description"
   - Example: "// A03:2021 - SQL Injection - No parameterized query"

2. **Descriptive Names**
   - Controller/method names clearly indicate vulnerability
   - Example: GetAllUsersWithoutAuth(), SearchWithSQLInjection()

3. **Realistic Code**
   - Code should look like real vulnerable code
   - Use realistic variable names and business logic
   - Include realistic error handling (but with vulnerabilities)

4. **Educational Comments**
   - Explain WHY something is vulnerable
   - Suggest the correct approach (commented out)
   - Link to OWASP documentation

---

## Important Constraints

1. **Educational Purpose Only**
   - Add warnings in README and code
   - Include disclaimer about unauthorized access
   - Clear statement of legal usage

2. **No Real Exploitation Tools**
   - Don't include actual malware or exploits
   - Don't include payloads that compromise production systems
   - Focus on demonstrating vulnerability concepts

3. **Isolated Environment**
   - Emphasize this must run in isolated, controlled environment
   - Never connect to production internet
   - Only for authorized testing

4. **Completeness**
   - Every OWASP Top 10 vulnerability must be demonstrated
   - Every endpoint must have clear documentation
   - Every vulnerability must be testable

---

## Deliverables Checklist

- [ ] Complete ASP.NET Core project structure created
- [ ] 6 Controllers with all OWASP vulnerabilities implemented
- [ ] 3 Models (User, Product, etc) with vulnerable design
- [ ] DbContext with vulnerable configuration
- [ ] Program.cs with insecure configuration
- [ ] appsettings.json demonstrating misconfiguration
- [ ] OWASPVulnerableApp.csproj with proper dependencies
- [ ] Comprehensive README.md (1000+ words)
- [ ] Detailed TESTING.md with exploitation examples
- [ ] .gitignore file
- [ ] Inline code comments for all vulnerabilities
- [ ] At least 2 endpoints per vulnerability
- [ ] Curl test examples for each vulnerability
- [ ] Clear warnings about educational use only
- [ ] All files committed to git with descriptive messages

---

## Success Criteria

✅ Application builds successfully with `dotnet build`
✅ All endpoints are accessible via Swagger UI
✅ Each OWASP Top 10 vulnerability is demonstrable
✅ Test cases in TESTING.md work as documented
✅ Code comments explain each vulnerability
✅ README and TESTING docs are comprehensive
✅ Project structure is clean and organized
✅ Warnings about educational use are prominent

---

## Notes for Claude Code

- Create this as a real, working .NET Core 8.0 application
- Make code realistic but intentionally vulnerable
- Include both vulnerable code AND comments showing correct approaches
- Prioritize clarity and education over code quality
- Add warnings prominently about this being for authorized testing only
- Ensure every endpoint is documented and testable
- Keep file organization clean and logical
- Make documentation thorough and practical
```

## End of Prompt

---

## How to Use This Prompt with Claude Code

1. **Copy the entire prompt above** (everything between the triple backticks)

2. **Use with Claude Code CLI:**
```bash
claude code "paste-the-complete-prompt-here"
```

3. **Or paste into Claude Code Web Interface:**
   - Go to https://claude.ai/code
   - Paste the entire prompt
   - Click "Generate" or "Create"

4. **Or use with your IDE:**
   - Open Claude Code in VS Code
   - Create a new file with the prompt
   - Use the `/code` command to execute

---

## Prompt Variants for Specific Use Cases

### Variant 1: For CTF Challenges
Replace this section in the prompt:
```
## Important Constraints

1. **Educational Purpose Only**
```

With:
```
## Important Constraints

1. **CTF Challenge Focus**
   - Add difficulty levels to endpoints
   - Include hints in responses
   - Create flag extraction endpoints
   - Add scoring mechanism
   - Include challenge descriptions
```

### Variant 2: For Pentesting Practice
Add to the prompt:
```
## Additional Requirements for Pentesting

1. **Realistic Business Logic**
   - Implement actual user management workflows
   - Include order processing system
   - Add payment processing (vulnerable)
   - Implement file management (vulnerable)

2. **Metrics Collection**
   - Track which endpoints are accessed
   - Log exploitation attempts
   - Generate vulnerability reports

3. **Progressive Difficulty**
   - Easy exploits (obvious SQL injection)
   - Medium exploits (IDOR with ID obfuscation)
   - Hard exploits (SSRF + XXE combination)
```

### Variant 3: For Training Course
Add to the prompt:
```
## Training Material Requirements

1. **Lesson Plan Integration**
   - Add lesson IDs to vulnerabilities
   - Include difficulty ratings
   - Add estimated learning time per module

2. **Exercise Endpoints**
   - Beginner level exploits
   - Intermediate challenges
   - Advanced combination attacks

3. **Progress Tracking**
   - Student submission endpoints
   - Verification of completed exploits
   - Score calculation
```

---

## Tips for Best Results

1. **Be Specific** - The more detail you provide, the better the output
2. **Include Context** - Explain your use case (learning, testing, research)
3. **Request Examples** - Ask for test cases and curl commands
4. **Ask for Documentation** - Request comprehensive README and TESTING guides
5. **Set Expectations** - Specify file structure, file count, code quality standards

---

## After Generation

Once Claude Code generates the application:

1. **Review the code** - Verify all 10 vulnerabilities are present
2. **Test the endpoints** - Use the TESTING.md guide
3. **Add customizations** - Modify for your specific needs
4. **Create your own tests** - Build upon the provided examples
5. **Document findings** - Use for security training or research

---

## Reference: OWASP Top 10 (2021)

| ID | Vulnerability | Rank |
|---|---|---|
| A01 | Broken Access Control | 1st |
| A02 | Cryptographic Failures | 2nd |
| A03 | Injection | 3rd |
| A04 | Insecure Design | 4th |
| A05 | Security Misconfiguration | 5th |
| A06 | Vulnerable and Outdated Components | 6th |
| A07 | Identification and Authentication Failures | 7th |
| A08 | Software and Data Integrity Failures | 8th |
| A09 | Logging and Monitoring Failures | 9th |
| A10 | Server-Side Request Forgery (SSRF) | 10th |

---

**Save this file and reuse it for future .NET Core vulnerable application generation!**
