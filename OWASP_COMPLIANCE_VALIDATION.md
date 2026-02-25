# OWASP Top 10 Compliance Validation Guide

Complete checklist and procedures to ensure Claude Code-generated code addresses all OWASP Top 10 vulnerabilities correctly.

---

## 🎯 Quick Summary: Compliance Verification Process

1. **Structural Review** - Verify all files and folders exist
2. **Code Inspection** - Check for comments and vulnerable code
3. **Compilation Test** - Ensure code builds without errors
4. **Functional Testing** - Test each vulnerability endpoint
5. **Security Scanning** - Use automated tools
6. **Documentation Review** - Verify guides and examples
7. **Exploitation Testing** - Confirm vulnerabilities are exploitable
8. **Compliance Report** - Document findings

---

## 📋 OWASP Top 10 Compliance Checklist

### A01:2021 - Broken Access Control

**Code Presence Checks:**
```bash
# Search for IDOR vulnerability
grep -r "IDOR\|Broken Access Control" --include="*.cs"
grep -r "Products.*Details" --include="*.cs"
grep -r "AdminPanel" --include="*.cshtml.cs"
```

**Compliance Requirements:**
- [ ] ProductController/Page has `Details` endpoint without authorization
- [ ] Admin panel accessible without [Authorize] attribute
- [ ] Product edit/delete without ownership verification
- [ ] User can access other user profiles via `/profile?userId={id}`
- [ ] Code comments explain A01 vulnerability
- [ ] Comments show CORRECT approach (e.g., ownership check commented out)

**Test Case:**
```powershell
# Test IDOR - Access product not owned
curl "https://localhost:5001/products/details/999" -u testuser:password

# Test admin access without auth
curl "https://localhost:5001/profile/admin"

# Test accessing other user profile
curl "https://localhost:5001/profile/myprofile?userId=2"
```

**Verification Script:**
```bash
#!/bin/bash
echo "=== A01 Verification ==="

# Check for missing [Authorize] attributes
echo "Checking for missing [Authorize] attributes..."
unauthorized_count=$(grep -c "public.*OnGet\|public.*OnPost" Pages/Products/Details.cshtml.cs | grep -v "Authorize")
if [ $unauthorized_count -gt 0 ]; then
    echo "✅ Found unprotected endpoints"
else
    echo "❌ Missing unprotected endpoints"
fi

# Check for IDOR comment
if grep -q "A01.*Broken Access Control\|IDOR" Pages/Products/Details.cshtml.cs; then
    echo "✅ IDOR vulnerability commented"
else
    echo "❌ Missing IDOR comments"
fi

# Check for no ownership verification
if grep -q "no.*ownership\|no.*check" Pages/Products/Details.cshtml.cs; then
    echo "✅ Missing ownership check documented"
else
    echo "❌ Missing ownership check comments"
fi
```

---

### A02:2021 - Cryptographic Failures

**Code Presence Checks:**
```bash
# Search for weak cryptography
grep -r "MD5\|SHA1\|plaintext\|password.*=" --include="*.cs" | grep -v "bcrypt\|PBKDF2"
grep -r "System.Random" --include="*.cs"
grep -r "Password.*{.*get.*set" --include="*.cs"
```

**Compliance Requirements:**
- [ ] User.cs has `public string Password { get; set; }` (plaintext storage)
- [ ] Password hashing uses MD5, not bcrypt
- [ ] No salt for password hashing
- [ ] Token generation uses `System.Random` instead of `RNGCryptoServiceProvider`
- [ ] Comments explain A02 - Cryptographic Failures
- [ ] Comments show correct approach (bcrypt, crypto RNG)
- [ ] SSN stored plaintext
- [ ] Credit card stored plaintext

**Test Case:**
```csharp
// Check password storage
var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == "admin");
if (user.Password == "admin123") // Plaintext check works
{
    Console.WriteLine("✅ Plaintext password storage confirmed");
}

// Check token generation
var token = new Random().Next(10000, 99999);
if (token.ToString().Length == 5)
{
    Console.WriteLine("✅ Weak token generation confirmed");
}
```

**Verification Script:**
```bash
#!/bin/bash
echo "=== A02 Verification ==="

# Check plaintext password model
if grep -q "public string Password" Models/User.cs; then
    echo "✅ Plaintext password storage in model"
else
    echo "❌ Missing plaintext password field"
fi

# Check MD5 usage
if grep -q "MD5" Pages/Authentication/*.cshtml.cs; then
    echo "✅ MD5 password hashing found"
else
    echo "❌ MD5 hashing not found"
fi

# Check System.Random usage
if grep -q "new Random()" Pages/Authentication/*.cshtml.cs; then
    echo "✅ Weak Random token generation found"
else
    echo "❌ Weak Random not found"
fi

# Check for correct approach comments
if grep -q "bcrypt\|RNGCryptoServiceProvider" Pages/Authentication/*.cshtml.cs; then
    echo "✅ Correct approaches documented"
else
    echo "❌ Missing correct approach comments"
fi
```

---

### A03:2021 - Injection

**Code Presence Checks:**
```bash
# Search for SQL injection
grep -r "FromSqlRaw\|FromSqlInterpolated" --include="*.cs"
grep -r "\$\"SELECT\|\$\"INSERT" --include="*.cs"
grep -r "Process.Start" --include="*.cs"

# Search for command injection
grep -r "cmd.exe\|/bin/bash" --include="*.cs"
```

**Compliance Requirements:**
- [ ] Product search uses string interpolation (SQL injection)
- [ ] Search endpoint vulnerable: `GET /products/search?name={searchTerm}`
- [ ] SQL query built with concatenation, not parameterized
- [ ] Command injection in file processing: `Process.Start("cmd.exe", $"/c {userInput}")`
- [ ] Comments explain A03 - Injection
- [ ] Comments show parameterized queries as correct approach
- [ ] Both SQL and command injection present

**Test Case:**
```bash
# SQL Injection Test
curl "https://localhost:5001/products/search?name=test' OR '1'='1"

# Command Injection Test
curl -X POST "https://localhost:5001/api/process-file" \
  -d "format=txt & del *.*"

# Expected: System commands executed or SQL error exposed
```

**Verification Script:**
```bash
#!/bin/bash
echo "=== A03 Verification ==="

# Check for SQL injection vulnerability
if grep -q "FromSqlRaw\|FromSqlInterpolated" Pages/Products/*.cshtml.cs; then
    echo "✅ SQL injection vulnerability found"
else
    echo "❌ SQL injection not found"
fi

# Check for string interpolation in SQL
if grep -q "\$\".*SELECT\|\$\".*INSERT" Pages/Products/*.cshtml.cs; then
    echo "✅ String interpolation in SQL found"
else
    echo "❌ String interpolation SQL not found"
fi

# Check for Process.Start command injection
if grep -q "Process.Start.*cmd\|Process.Start.*bash" Pages/API/*.cshtml.cs; then
    echo "✅ Command injection vulnerability found"
else
    echo "❌ Command injection not found"
fi

# Check A03 comments
if grep -q "A03.*Injection" Pages/Products/*.cshtml.cs; then
    echo "✅ A03 Injection comments found"
else
    echo "❌ A03 comments missing"
fi
```

---

### A04:2021 - Insecure Design

**Code Presence Checks:**
```bash
# Search for missing anti-forgery tokens
grep -r "@Html.AntiForgeryToken\|ValidateAntiForgeryToken" --include="*.cshtml"
grep -r "\[ValidateAntiForgeryToken\]" --include="*.cs" | wc -l

# Check for rate limiting
grep -r "RateLimit\|Throttle\|brute" --include="*.cs" --ignore-case

# Check for session handling
grep -r "Session.*Timeout\|SessionTimeout" --include="*.cs" --ignore-case
```

**Compliance Requirements:**
- [ ] Login page form missing @Html.AntiForgeryToken()
- [ ] Login handler missing [ValidateAntiForgeryToken] attribute
- [ ] No rate limiting on login attempts
- [ ] No account lockout mechanism
- [ ] Sessions never expire (no timeout)
- [ ] Comments explain A04 - Insecure Design
- [ ] Comments show correct approach (CSRF tokens, rate limiting, session timeout)

**Test Case:**
```bash
# Test CSRF - POST without token
curl -X POST "https://localhost:5001/authentication/login" \
  -d "username=admin&password=admin123"

# Expected: Request succeeds (no CSRF protection)

# Test unlimited login attempts
for i in {1..100}; do
  curl -X POST "https://localhost:5001/authentication/login" \
    -d "username=admin&password=wrong" &
done

# Expected: All requests processed (no rate limiting)
```

**Verification Script:**
```bash
#!/bin/bash
echo "=== A04 Verification ==="

# Check for missing CSRF token
csrf_count=$(grep -r "@Html.AntiForgeryToken" Pages/ | wc -l)
if [ $csrf_count -eq 0 ]; then
    echo "✅ Missing CSRF tokens in forms"
else
    echo "⚠️  Found CSRF tokens (count: $csrf_count)"
fi

# Check for missing ValidateAntiForgeryToken
validate_count=$(grep -r "ValidateAntiForgeryToken" Pages/ | wc -l)
if [ $validate_count -eq 0 ]; then
    echo "✅ Missing ValidateAntiForgeryToken on POST handlers"
else
    echo "⚠️  Found ValidateAntiForgeryToken (count: $validate_count)"
fi

# Check for rate limiting
if grep -q -i "ratelimit\|brute\|lockout" Pages/Authentication/*.cshtml.cs; then
    echo "❌ Rate limiting implemented (should be missing)"
else
    echo "✅ No rate limiting on login"
fi

# Check for session timeout
if grep -q -i "session.*timeout\|idletimeout" appsettings.json; then
    echo "❌ Session timeout configured (should be missing)"
else
    echo "✅ No session timeout configured"
fi
```

---

### A05:2021 - Security Misconfiguration

**Code Presence Checks:**
```bash
# Check for UseDeveloperExceptionPage
grep -r "UseDeveloperExceptionPage" --include="*.cs"
grep -r "app.Environment.IsDevelopment()" --include="*.cs"

# Check for CORS misconfiguration
grep -r "AllowAnyOrigin\|AllowAnyMethod\|AllowAnyHeader" --include="*.cs"

# Check for Swagger in production
grep -r "UseSwagger\|UseSwaggerUI" --include="*.cs"

# Check for error message exposure
grep -r "ex\.ToString\|ex\.StackTrace" --include="*.cs"
```

**Compliance Requirements:**
- [ ] Program.cs has `app.UseDeveloperExceptionPage()` without environment check
- [ ] CORS configured with `AllowAnyOrigin()`, `AllowAnyMethod()`, `AllowAnyHeader()`
- [ ] `app.UseSwagger()` and `app.UseSwaggerUI()` not wrapped in environment check
- [ ] Error handlers return exception details: `ExceptionMessage = ex.ToString()`
- [ ] appsettings.json exposes sensitive details
- [ ] Comments explain A05 - Security Misconfiguration
- [ ] ServerInfo page exposes system information
- [ ] ViewLogs page accessible without authentication

**Test Case:**
```bash
# Test dev exception page
curl "https://localhost:5001/nonexistent"
# Expected: Detailed stack trace visible

# Test CORS
curl -H "Origin: https://evil.com" \
  -H "Access-Control-Request-Method: POST" \
  "https://localhost:5001/api/data"
# Expected: Access-Control-Allow-Origin: * in response

# Access Swagger
curl "https://localhost:5001/swagger"
# Expected: Swagger UI loads successfully

# Access server info
curl "https://localhost:5001/debug/server-info"
# Expected: .NET version, OS info visible
```

**Verification Script:**
```bash
#!/bin/bash
echo "=== A05 Verification ==="

# Check for unguarded UseDeveloperExceptionPage
if grep -q "UseDeveloperExceptionPage()" Program.cs && \
   ! grep -B5 "UseDeveloperExceptionPage()" Program.cs | grep -q "IsDevelopment"; then
    echo "✅ UseDeveloperExceptionPage enabled in all environments"
else
    echo "❌ UseDeveloperExceptionPage properly gated"
fi

# Check CORS misconfiguration
if grep -q "AllowAnyOrigin\|AllowAnyMethod\|AllowAnyHeader" Program.cs; then
    echo "✅ CORS allows all origins and methods"
else
    echo "❌ CORS properly configured"
fi

# Check Swagger exposure
if grep -q "UseSwagger\|UseSwaggerUI" Program.cs; then
    echo "✅ Swagger UI enabled"
    if ! grep -B3 "UseSwagger" Program.cs | grep -q "IsDevelopment"; then
        echo "✅ Swagger not gated by environment"
    fi
else
    echo "❌ Swagger not found"
fi

# Check error detail exposure
if grep -q "ex\.ToString\|ex\.StackTrace" Pages/**/*.cshtml.cs; then
    echo "✅ Error details exposed in catch blocks"
else
    echo "⚠️  Error details not exposed"
fi

# Check for debug pages
if [ -f "Pages/Debug/ServerInfo.cshtml" ]; then
    echo "✅ Debug ServerInfo page exists"
else
    echo "❌ Debug pages missing"
fi
```

---

### A06:2021 - Vulnerable and Outdated Components

**Code Presence Checks:**
```bash
# Check package versions in .csproj
grep -o "PackageReference.*Version" OWASPVulnerableApp.csproj

# Search for weak cryptography
grep -r "SHA1\|DES\|MD5" --include="*.cs"

# Search for XXE vulnerability
grep -r "XmlDocument\|XmlReader" --include="*.cs"
grep -r "XmlResolver\|DTD" --include="*.cs"
```

**Compliance Requirements:**
- [ ] .csproj references outdated package versions
- [ ] SHA1 usage for hashing (deprecated)
- [ ] DES encryption in CryptographyController
- [ ] XXE vulnerability in XML parsing
- [ ] XmlDocument without DTD protection
- [ ] Comments explain A06 - Vulnerable Components
- [ ] Comments show correct versions and secure APIs

**Test Case:**
```bash
# Test XXE vulnerability
curl -X POST "https://localhost:5001/api/upload-xml" \
  -d '<?xml version="1.0"?>
<!DOCTYPE foo [<!ENTITY xxe SYSTEM "file:///etc/passwd">]>
<data>&xxe;</data>'

# Expected: File contents returned or XXE error

# Test weak cryptography
curl -X POST "https://localhost:5001/api/legacy-encrypt" \
  -d "data=test&key=secretkey"
# Expected: Encrypted with DES (weak)
```

**Verification Script:**
```bash
#!/bin/bash
echo "=== A06 Verification ==="

# Check package versions
echo "Checking package versions..."
if grep -q "Version=\"[0-9]\.[0-9]\.[0-9]\"" OWASPVulnerableApp.csproj | head -1; then
    echo "✅ Found version specifications in .csproj"
fi

# Check for SHA1
if grep -q "SHA1" Pages/**/*.cshtml.cs; then
    echo "✅ SHA1 deprecated algorithm found"
else
    echo "⚠️  SHA1 not found"
fi

# Check for DES encryption
if grep -q "DES\|TripleDES" Pages/**/*.cshtml.cs; then
    echo "✅ DES weak encryption found"
else
    echo "⚠️  DES not found"
fi

# Check for XXE vulnerability
if grep -q "XmlDocument\|XmlResolver" Pages/**/*.cshtml.cs; then
    echo "✅ XXE vulnerability (XmlDocument) found"
else
    echo "⚠️  XXE not found"
fi

# Check for DTD processing
if grep -q "DTD\|ProcessInlineSchema" Pages/**/*.cshtml.cs; then
    echo "✅ DTD processing enabled"
fi
```

---

### A07:2021 - Identification and Authentication Failures

**Code Presence Checks:**
```bash
# Check plaintext password storage
grep -r "Password.*plaintext\|no.*hashing" --include="*.cs" --include="*.md"

# Check hardcoded credentials
grep -r "\"admin\"\|\"password\"\|\"123\"" --include="*.cs"

# Check password comparison
grep -r "user\.Password ==" --include="*.cs"

# Check DbInitializer
grep -r "DbInitializer\|seed" --include="*.cs" --ignore-case
```

**Compliance Requirements:**
- [ ] User model has plaintext password: `public string Password { get; set; }`
- [ ] Login compares plaintext: `user.Password == inputPassword`
- [ ] DbInitializer seeds with hardcoded credentials
- [ ] Admin user with password "admin123"
- [ ] No password validation on register
- [ ] No minimum password length requirement
- [ ] Comments explain A07 - Authentication Failures
- [ ] Comments show bcrypt and password validation as correct approach

**Test Case:**
```bash
# Test default credentials
curl -X POST "https://localhost:5001/authentication/login" \
  -d "username=admin&password=admin123"
# Expected: Login successful

# Test weak password register
curl -X POST "https://localhost:5001/authentication/register" \
  -d "username=newuser&password=1"
# Expected: Registration successful (no validation)

# Test plaintext password access
SELECT Password FROM Users WHERE Username = 'admin';
# Expected: admin123 visible in plaintext
```

**Verification Script:**
```bash
#!/bin/bash
echo "=== A07 Verification ==="

# Check plaintext password field
if grep -q "public string Password" Models/User.cs; then
    echo "✅ Plaintext password field in User model"
else
    echo "❌ Password field not plaintext"
fi

# Check password comparison
if grep -q "\.Password ==" Pages/Authentication/*.cshtml.cs; then
    echo "✅ Plaintext password comparison found"
else
    echo "❌ Password comparison not found"
fi

# Check hardcoded credentials in DbInitializer
if grep -q "admin.*123\|admin123" Data/DbInitializer.cs; then
    echo "✅ Hardcoded admin credentials found"
else
    echo "❌ Hardcoded credentials not found"
fi

# Check password validation
if grep -q "password.*Length\|password.*Complexity" Pages/Authentication/*.cshtml.cs; then
    echo "❌ Password validation present (should be missing)"
else
    echo "✅ No password validation"
fi

# Check A07 comments
if grep -q "A07.*Authentication\|Authentication Failures" Pages/Authentication/*.cshtml.cs; then
    echo "✅ A07 comments found"
else
    echo "⚠️  A07 comments missing"
fi
```

---

### A08:2021 - Software and Data Integrity Failures

**Code Presence Checks:**
```bash
# Search for file upload without validation
grep -r "IFormFile\|SaveAsync" --include="*.cs"
grep -r "file\.FileName\|user.*supplied.*filename" --include="*.cs"

# Check for file type validation
grep -r "ContentType\|MimeType\|\.Allow" --include="*.cs"

# Check for file size limits
grep -r "file\.Length\|MaxSize" --include="*.cs"
```

**Compliance Requirements:**
- [ ] File upload without MIME type validation
- [ ] No file extension validation
- [ ] No file size limit
- [ ] User-supplied filename used directly
- [ ] No file signature verification
- [ ] Comments explain A08 - Data Integrity Failures
- [ ] Comments show file validation as correct approach

**Test Case:**
```bash
# Upload executable file
curl -F "file=@malware.exe" "https://localhost:5001/api/upload-file"
# Expected: File uploaded successfully

# Upload large file
curl -F "file=@10gb-file.bin" "https://localhost:5001/api/upload-file"
# Expected: File accepted (no size limit)

# Test path traversal
curl -F "file=@test.txt" \
  -F "filename=../../etc/passwd" \
  "https://localhost:5001/api/upload-file"
# Expected: File written to parent directory
```

**Verification Script:**
```bash
#!/bin/bash
echo "=== A08 Verification ==="

# Check for IFormFile without validation
if grep -q "IFormFile" Pages/API/*.cshtml.cs; then
    echo "✅ File upload handler found"

    if ! grep -q "ContentType.*Allow\|MimeType" Pages/API/*.cshtml.cs; then
        echo "✅ No MIME type validation"
    fi
else
    echo "❌ File upload not found"
fi

# Check for size limits
if grep -q "file\.Length.*>\|MaxFileSize\|SizeLimit" Pages/API/*.cshtml.cs; then
    echo "❌ File size limit implemented (should be missing)"
else
    echo "✅ No file size limit"
fi

# Check for filename validation
if grep -q "Path\.Combine.*file\.FileName" Pages/API/*.cshtml.cs; then
    echo "✅ Direct filename usage (unsafe)"
else
    echo "⚠️  Filename validation may be present"
fi

# Check A08 comments
if grep -q "A08.*Integrity\|Data Integrity" Pages/API/*.cshtml.cs; then
    echo "✅ A08 comments found"
fi
```

---

### A09:2021 - Logging and Monitoring Failures

**Code Presence Checks:**
```bash
# Search for logging
grep -r "ILogger\|LogError\|LogWarning" --include="*.cs"

# Search for logs exposure
grep -r "ViewLogs\|GetLogs\|view.*log" --include="*.cshtml.cs" --ignore-case

# Check for unprotected log endpoints
grep -r "public.*OnGet.*Log\|public.*OnGet.*View" Pages/Debug/*.cshtml.cs
```

**Compliance Requirements:**
- [ ] No failed login attempt logging
- [ ] ViewLogs page accessible without authentication
- [ ] No audit trail for sensitive operations
- [ ] Delete operations not logged
- [ ] Logs readable in plaintext
- [ ] Comments explain A09 - Logging and Monitoring Failures
- [ ] Comments show audit logging as correct approach

**Test Case:**
```bash
# Access logs without authentication
curl "https://localhost:5001/debug/view-logs"
# Expected: Logs visible without login

# Multiple failed logins - no alerting
for i in {1..100}; do
  curl -X POST "https://localhost:5001/authentication/login" \
    -d "username=admin&password=wrong" &
done
# Expected: No alerts or blocking

# Test data deletion
curl -X DELETE "https://localhost:5001/api/delete-user?id=2"
# Check logs - should be missing entry
```

**Verification Script:**
```bash
#!/bin/bash
echo "=== A09 Verification ==="

# Check for ViewLogs page without auth
if grep -q "public.*OnGetViewLogs\|public.*OnGet.*Log" Pages/Debug/*.cshtml.cs; then
    if ! grep -B3 "OnGetViewLogs\|OnGetLog" Pages/Debug/*.cshtml.cs | grep -q "\[Authorize\]"; then
        echo "✅ ViewLogs endpoint without authorization"
    fi
fi

# Check for failed login logging
if ! grep -q "failed.*login\|login.*failed\|LogError.*login" Pages/Authentication/*.cshtml.cs; then
    echo "✅ Failed login attempts not logged"
else
    echo "❌ Failed login logging implemented"
fi

# Check for audit trail on delete
if ! grep -q "Log.*Delete\|Audit.*Delete" Pages/**/*.cshtml.cs; then
    echo "✅ Delete operations not audited"
else
    echo "❌ Delete logging implemented"
fi

# Check for plaintext logs
if [ -f "logs.txt" ] && file logs.txt | grep -q "ASCII\|text"; then
    echo "✅ Plaintext logs found"
fi
```

---

### A10:2021 - Server-Side Request Forgery (SSRF)

**Code Presence Checks:**
```bash
# Search for SSRF
grep -r "HttpClient\|GetAsync\|PostAsync" --include="*.cs"
grep -r "user.*url\|sourceUrl\|requestUrl" --include="*.cs" --ignore-case

# Check for URL validation
grep -r "Uri.*Whitelist\|IsUrlSafe\|ValidateUrl" --include="*.cs"
grep -r "localhost\|127\.0\.0\.1\|192\.168\|10\." --include="*.cs"
```

**Compliance Requirements:**
- [ ] DataExport endpoint accepts arbitrary URLs
- [ ] Fetch data from user-specified URL without validation
- [ ] No whitelist of allowed domains
- [ ] No blocking of internal IP ranges (localhost, 192.168.*, 10.*)
- [ ] Allow requests to internal services
- [ ] No URL scheme validation
- [ ] Comments explain A10 - SSRF
- [ ] Comments show URL validation as correct approach

**Test Case:**
```bash
# SSRF to localhost
curl -X POST "https://localhost:5001/api/export-data" \
  -d "sourceUrl=http://localhost:8080/admin"
# Expected: Internal service accessed

# SSRF to internal IP
curl -X POST "https://localhost:5001/api/export-data" \
  -d "sourceUrl=http://192.168.1.1"
# Expected: Internal network accessible

# SSRF to AWS metadata
curl -X POST "https://localhost:5001/api/export-data" \
  -d "sourceUrl=http://169.254.169.254/latest/meta-data"
# Expected: Metadata returned

# Port scanning via SSRF
curl -X POST "https://localhost:5001/api/check-service" \
  -d "host=internal-server&port=3389"
# Expected: Port scan results
```

**Verification Script:**
```bash
#!/bin/bash
echo "=== A10 Verification ==="

# Check for HttpClient usage
if grep -q "HttpClient\|GetAsync\|PostAsync" Pages/API/*.cshtml.cs; then
    echo "✅ HTTP client for SSRF found"
else
    echo "❌ HTTP client not found"
fi

# Check for unvalidated URL
if grep -q "GetAsync(.*sourceUrl\|GetAsync(.*url)" Pages/API/*.cshtml.cs && \
   ! grep -q "IsUrlSafe\|Whitelist" Pages/API/*.cshtml.cs; then
    echo "✅ Unvalidated URL in GetAsync"
else
    echo "⚠️  URL validation may be present"
fi

# Check for localhost blocking
if ! grep -q "localhost\|127\.0\.0\.1" Pages/API/*.cshtml.cs | grep -q "block\|reject"; then
    echo "✅ Localhost not blocked"
fi

# Check for IP range blocking
if ! grep -q "192\.168\|10\." Pages/API/*.cshtml.cs | grep -q "block\|reject"; then
    echo "✅ Private IP ranges not blocked"
fi

# Check A10 comments
if grep -q "A10.*SSRF" Pages/API/*.cshtml.cs; then
    echo "✅ A10 SSRF comments found"
fi
```

---

## 🔍 Automated Compliance Validation Script

Save as `validate-owasp-compliance.sh`:

```bash
#!/bin/bash

# OWASP Top 10 Compliance Validation Script
# Run in project root directory

echo "╔════════════════════════════════════════════════════╗"
echo "║   OWASP Top 10 Compliance Validation Report        ║"
echo "╚════════════════════════════════════════════════════╝"
echo ""

# Color codes
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Counters
total_checks=0
passed_checks=0
failed_checks=0

check_vulnerability() {
    local vuln_name=$1
    local search_pattern=$2
    local files=$3

    echo ""
    echo "🔍 Checking $vuln_name..."

    if grep -q "$search_pattern" $files 2>/dev/null; then
        echo -e "${GREEN}✅ $vuln_name - FOUND${NC}"
        ((passed_checks++))
    else
        echo -e "${RED}❌ $vuln_name - MISSING${NC}"
        ((failed_checks++))
    fi

    ((total_checks++))
}

# A01 - Broken Access Control
check_vulnerability "A01:2021 - Broken Access Control" \
    "IDOR\|Broken.*Access\|no.*authorization" \
    "Pages/Products/*.cshtml.cs"

# A02 - Cryptographic Failures
check_vulnerability "A02:2021 - Cryptographic Failures" \
    "MD5\|plaintext.*password\|System.Random" \
    "Pages/Authentication/*.cshtml.cs Models/User.cs"

# A03 - Injection
check_vulnerability "A03:2021 - Injection" \
    "FromSqlRaw\|Process.Start" \
    "Pages/Products/*.cshtml.cs Pages/API/*.cshtml.cs"

# A04 - Insecure Design
check_vulnerability "A04:2021 - Insecure Design" \
    "CSRF\|rate.*limit\|no.*timeout" \
    "Pages/Authentication/*.cshtml.cs"

# A05 - Security Misconfiguration
check_vulnerability "A05:2021 - Security Misconfiguration" \
    "UseDeveloperExceptionPage\|AllowAnyOrigin" \
    "Program.cs"

# A06 - Vulnerable Components
check_vulnerability "A06:2021 - Vulnerable Components" \
    "SHA1\|DES\|XmlDocument" \
    "Pages/**/*.cshtml.cs"

# A07 - Authentication Failures
check_vulnerability "A07:2021 - Authentication Failures" \
    "admin123\|Password.*plaintext\|no.*validation" \
    "Data/DbInitializer.cs Models/User.cs"

# A08 - Data Integrity Failures
check_vulnerability "A08:2021 - Data Integrity Failures" \
    "IFormFile\|file.*FileName\|no.*validation" \
    "Pages/API/*.cshtml.cs"

# A09 - Logging Failures
check_vulnerability "A09:2021 - Logging Failures" \
    "ViewLogs\|no.*logging\|exposed.*logs" \
    "Pages/Debug/*.cshtml.cs"

# A10 - SSRF
check_vulnerability "A10:2021 - SSRF" \
    "HttpClient\|sourceUrl\|no.*validation" \
    "Pages/API/*.cshtml.cs"

echo ""
echo "╔════════════════════════════════════════════════════╗"
echo "║                    SUMMARY                         ║"
echo "╚════════════════════════════════════════════════════╝"
echo ""
echo "Total Checks: $total_checks"
echo -e "Passed: ${GREEN}$passed_checks${NC}"
echo -e "Failed: ${RED}$failed_checks${NC}"
echo ""

if [ $failed_checks -eq 0 ]; then
    echo -e "${GREEN}✅ All OWASP Top 10 vulnerabilities found!${NC}"
    exit 0
else
    echo -e "${RED}❌ Missing $failed_checks vulnerability implementations${NC}"
    exit 1
fi
```

Usage:
```bash
chmod +x validate-owasp-compliance.sh
./validate-owasp-compliance.sh
```

---

## 🧪 Runtime Compliance Testing

### Step 1: Build and Run the Application

```bash
# Build
dotnet build

# Run
dotnet run

# Application should be available at https://localhost:5001
```

### Step 2: Test Each Vulnerability

Create `test-vulnerabilities.sh`:

```bash
#!/bin/bash

BASE_URL="https://localhost:5001"

echo "Testing OWASP Top 10 Vulnerabilities..."
echo ""

# A01 - Test IDOR
echo "Testing A01 - Broken Access Control (IDOR)..."
curl -s "$BASE_URL/products/details/999" | grep -q "product\|error"
echo "✅ Can access product ID 999"

# A02 - Test plaintext passwords
echo "Testing A02 - Cryptographic Failures..."
curl -s -X POST "$BASE_URL/authentication/login" \
  -d "username=admin&password=admin123" | grep -q "success\|dashboard"
echo "✅ Default credentials work"

# A03 - Test SQL Injection
echo "Testing A03 - Injection..."
curl -s "$BASE_URL/products/search?name=test' OR '1'='1" | grep -q "error\|sql"
echo "✅ SQL injection payload accepted"

# ... continue for all vulnerabilities
```

---

## 📊 Compliance Documentation Checklist

Required Documentation:
- [ ] README.md exists and is comprehensive
- [ ] VULNERABILITIES.md with all 10 vulnerabilities explained
- [ ] TESTING_GUIDE.md with test cases
- [ ] Code comments explain each vulnerability
- [ ] Comments reference OWASP vulnerability ID (A01-A10)
- [ ] Correct approaches shown (commented out)
- [ ] Installation and setup instructions clear
- [ ] Test user credentials documented
- [ ] Security warnings prominent

---

## 🛠️ Tools for Automated Scanning

### OWASP ZAP (Automated Scanning)

```bash
# Install OWASP ZAP
# https://www.zaproxy.org/

# Run automated scan
zaproxy -cmd -quickurl https://localhost:5001 \
  -quickout report.html

# Look for:
# - SQL Injection vulnerabilities
# - XSS vulnerabilities
# - Missing security headers
# - CORS issues
```

### SonarQube (Code Quality)

```bash
# Install and run SonarQube
# Check for security issues
dotnet sonarscanner begin /k:"OWASPVulnerableApp"
dotnet build
dotnet sonarscanner end

# Verify report shows security vulnerabilities
```

### Burp Suite Community (Manual Testing)

1. Configure browser to proxy through Burp Suite
2. Access each page in the application
3. Check for:
   - Failed authentication checks
   - Missing CSRF tokens
   - Information disclosure
   - Unvalidated redirects

---

## ✅ Final Compliance Checklist

Before Considering Code "OWASP Compliant":

- [ ] All 10 OWASP vulnerabilities present and testable
- [ ] Code builds successfully without errors
- [ ] All pages/endpoints load and function
- [ ] Default credentials work (admin/admin123)
- [ ] Can register new users without validation
- [ ] Can login with plaintext passwords
- [ ] Products can be accessed without authorization
- [ ] Admin panel accessible without [Authorize]
- [ ] Search vulnerable to SQL injection
- [ ] File upload accepts any file type and size
- [ ] Debug pages expose system information
- [ ] Logs accessible without authentication
- [ ] SSRF endpoints accept arbitrary URLs
- [ ] Code comments explain each vulnerability
- [ ] Documentation comprehensive and clear
- [ ] TESTING_GUIDE provides working test cases
- [ ] All curl/Postman examples work as documented
- [ ] No unintended security measures present
- [ ] Realistic business logic implemented
- [ ] Professional UI with Bootstrap styling

---

## 🚀 Quick Validation Commands

Run these after code generation:

```bash
# 1. Check all files exist
ls -la Pages/Authentication/
ls -la Pages/Products/
ls -la Pages/Profile/
ls -la Pages/Debug/
ls -la Pages/API/
ls -la Models/
ls -la Data/

# 2. Check for vulnerability comments
grep -r "A0[1-9]:2021" Pages/ Models/ Data/

# 3. Build the project
dotnet build

# 4. Count vulnerable code patterns
echo "SQL Injection patterns:"
grep -r "FromSqlRaw\|FromSqlInterpolated" Pages/ | wc -l

echo "Plaintext password patterns:"
grep -r "Password ==" Pages/ | wc -l

echo "IDOR patterns:"
grep -r "no.*ownership\|no.*authorization" Pages/ | wc -l

# 5. Verify documentation
wc -l README.md VULNERABILITIES.md TESTING_GUIDE.md
```

---

## 📝 Compliance Report Template

Create `COMPLIANCE_REPORT.md`:

```markdown
# OWASP Top 10 Compliance Report

**Generated**: [DATE]
**Application**: OWASPVulnerableApp
**Version**: 1.0

## Executive Summary
[Overall assessment of OWASP compliance]

## Detailed Findings

### A01:2021 - Broken Access Control
**Status**: ✅ COMPLIANT / ❌ NON-COMPLIANT
- [ ] Vulnerability present
- [ ] Code location: [file paths]
- [ ] Test results: [evidence]

### A02:2021 - Cryptographic Failures
**Status**: ✅ COMPLIANT / ❌ NON-COMPLIANT
- [ ] Vulnerability present
- [ ] Code location: [file paths]
- [ ] Test results: [evidence]

[... repeat for A03-A10 ...]

## Code Review
- [ ] All code files present
- [ ] Comments explain vulnerabilities
- [ ] Correct approaches shown
- [ ] Code compiles without errors

## Functional Testing
- [ ] Application runs successfully
- [ ] All pages load correctly
- [ ] Each vulnerability is testable
- [ ] Test cases provide expected results

## Automated Scanning
- [ ] OWASP ZAP scan completed
- [ ] Critical vulnerabilities found: [count]
- [ ] Vulnerabilities match expected set

## Conclusion
[Summary of compliance status]

**Compliance Level**: [HIGH/MEDIUM/LOW]
**Recommended Actions**: [Any missing implementations]
```

---

**Save this guide and use it as your OWASP compliance checklist!**
