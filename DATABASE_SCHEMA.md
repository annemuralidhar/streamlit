# Employee Details Form - Database Schema

## Overview
This document defines the database schema for the Employee Details Form PowerApps application.

---

## Employees Table

### Table Name: `Employees`

| Column Name | Data Type | Required | Description | Example |
|------------|-----------|----------|-------------|---------|
| EmployeeID | Text | Yes | Unique identifier for employee | "EMP001" |
| FirstName | Text | Yes | Employee's first name | "John" |
| LastName | Text | Yes | Employee's last name | "Doe" |
| Email | Email | Yes | Work email address | "john.doe@company.com" |
| Phone | Text | No | Contact phone number | "(555) 123-4567" |
| Department | Lookup (Department) | Yes | Reference to Department table | "IT" |
| Position | Text | Yes | Job title/position | "Senior Developer" |
| StartDate | Date | Yes | Employment start date | "2022-01-15" |
| EmploymentType | Text | Yes | Type of employment | "Full-Time" |
| Salary | Number | No | Annual salary (HR only) | 85000.00 |
| Address | Text | No | Street address | "123 Main Street" |
| City | Text | No | City of residence | "New York" |
| State | Text | No | State/Province | "NY" |
| PostalCode | Text | No | Postal/ZIP code | "10001" |
| Status | Text | No | Employment status | "Active" |
| IsActive | Boolean | No | Active flag | true |
| CreatedDate | DateTime | Yes | Record creation timestamp | "2024-01-15 10:30:00" |
| CreatedBy | Text | Yes | User who created record | "admin@company.com" |
| ModifiedDate | DateTime | No | Last modification timestamp | "2024-03-10 14:45:00" |
| ModifiedBy | Text | No | User who last modified | "hr@company.com" |

### Primary Key
- **EmployeeID** (Unique)

### Foreign Keys
- **DepartmentID** → References Departments table

### Indexes
- EmployeeID (Primary, Unique)
- Email (Unique)
- Department
- StartDate
- Status

### Constraints
- EmployeeID: Cannot be blank, must be unique
- Email: Must be valid email format, must be unique
- FirstName: Cannot be blank, max 50 characters
- LastName: Cannot be blank, max 50 characters
- Salary: Must be >= 0 if populated
- StartDate: Cannot be in future

---

## Departments Table

### Table Name: `Departments`

| Column Name | Data Type | Required | Description | Example |
|------------|-----------|----------|-------------|---------|
| DepartmentID | Text | Yes | Unique department identifier | "DEPT001" |
| Name | Text | Yes | Department name | "Information Technology" |
| Code | Text | Yes | Department code | "IT" |
| Manager | Lookup (Employee) | No | Department manager | "EMP001" |
| Budget | Number | No | Annual budget | 500000.00 |
| IsActive | Boolean | No | Active flag | true |
| CreatedDate | DateTime | Yes | Record creation timestamp | "2023-01-01 09:00:00" |

### Primary Key
- **DepartmentID** (Unique)

---

## Employment Types Reference Table

### Supported Values:
- **Full-Time**: Standard full-time employment
- **Part-Time**: Part-time employment (less than 40 hours/week)
- **Contract**: Contract-based employment with defined end date
- **Intern**: Internship position (typically temporary)
- **Freelance**: Freelance or independent contractor
- **Temporary**: Temporary assignment

---

## Employment Status Reference Table

### Supported Values:
- **Active**: Currently employed
- **Inactive**: Not currently employed
- **On Leave**: Currently on leave (vacation, sabbatical, etc.)
- **Terminated**: Employment ended
- **Retired**: Employee retired
- **Suspended**: Temporarily suspended

---

## Sample SQL Create Statements

### Employees Table (SQL Server)
```sql
CREATE TABLE Employees (
    EmployeeID NVARCHAR(20) PRIMARY KEY NOT NULL,
    FirstName NVARCHAR(50) NOT NULL,
    LastName NVARCHAR(50) NOT NULL,
    Email NVARCHAR(100) UNIQUE NOT NULL,
    Phone NVARCHAR(20),
    DepartmentID NVARCHAR(20) NOT NULL,
    [Position] NVARCHAR(100) NOT NULL,
    StartDate DATE NOT NULL,
    EmploymentType NVARCHAR(20) NOT NULL,
    Salary DECIMAL(10, 2),
    [Address] NVARCHAR(200),
    City NVARCHAR(50),
    [State] NVARCHAR(50),
    PostalCode NVARCHAR(20),
    [Status] NVARCHAR(20),
    IsActive BIT DEFAULT 1,
    CreatedDate DATETIME NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy NVARCHAR(100) NOT NULL,
    ModifiedDate DATETIME,
    ModifiedBy NVARCHAR(100),
    FOREIGN KEY (DepartmentID) REFERENCES Departments(DepartmentID)
);

CREATE INDEX idx_Email ON Employees(Email);
CREATE INDEX idx_Department ON Employees(DepartmentID);
CREATE INDEX idx_Status ON Employees([Status]);
CREATE INDEX idx_StartDate ON Employees(StartDate);
```

### Departments Table (SQL Server)
```sql
CREATE TABLE Departments (
    DepartmentID NVARCHAR(20) PRIMARY KEY NOT NULL,
    [Name] NVARCHAR(100) NOT NULL,
    Code NVARCHAR(10) UNIQUE NOT NULL,
    ManagerID NVARCHAR(20),
    Budget DECIMAL(15, 2),
    IsActive BIT DEFAULT 1,
    CreatedDate DATETIME NOT NULL DEFAULT GETUTCDATE(),
    FOREIGN KEY (ManagerID) REFERENCES Employees(EmployeeID)
);

CREATE INDEX idx_Code ON Departments(Code);
CREATE INDEX idx_Active ON Departments(IsActive);
```

---

## Sample Data

### Sample Employees Records
```sql
INSERT INTO Departments (DepartmentID, Name, Code, IsActive, CreatedDate)
VALUES
    ('DEPT001', 'Information Technology', 'IT', 1, GETUTCDATE()),
    ('DEPT002', 'Human Resources', 'HR', 1, GETUTCDATE()),
    ('DEPT003', 'Sales', 'SALES', 1, GETUTCDATE()),
    ('DEPT004', 'Finance', 'FIN', 1, GETUTCDATE());

INSERT INTO Employees
(EmployeeID, FirstName, LastName, Email, Phone, DepartmentID, Position,
 StartDate, EmploymentType, Salary, Address, City, State, PostalCode,
 Status, IsActive, CreatedDate, CreatedBy)
VALUES
    ('EMP001', 'John', 'Doe', 'john.doe@company.com', '(555) 123-4567', 'DEPT001',
     'Senior Developer', '2022-01-15', 'Full-Time', 85000.00, '123 Main Street',
     'New York', 'NY', '10001', 'Active', 1, GETUTCDATE(), 'admin@company.com'),

    ('EMP002', 'Jane', 'Smith', 'jane.smith@company.com', '(555) 234-5678', 'DEPT002',
     'HR Manager', '2021-06-01', 'Full-Time', 75000.00, '456 Oak Avenue',
     'Boston', 'MA', '02101', 'Active', 1, GETUTCDATE(), 'admin@company.com'),

    ('EMP003', 'Michael', 'Johnson', 'michael.johnson@company.com', '(555) 345-6789', 'DEPT003',
     'Sales Manager', '2020-03-10', 'Full-Time', 70000.00, '789 Elm Street',
     'Chicago', 'IL', '60601', 'Active', 1, GETUTCDATE(), 'admin@company.com');
```

---

## Data Security & Compliance

### Field-Level Security
- **Salary**: Visible only to HR managers and above
- **Email**: Visible to all authenticated users
- **Phone**: Visible to department managers and HR
- **Address**: Visible to HR and employee's direct manager

### Audit Trail
All create/modify operations should be tracked:
- CreatedDate: Automatically set on creation
- CreatedBy: Set by system with current user
- ModifiedDate: Updated on every change
- ModifiedBy: Updated with current user on every change

### Compliance Considerations
- GDPR: Include data retention policies
- CCPA: Support data export/deletion requests
- HIPAA: If applicable, encrypt sensitive fields
- SOC 2: Maintain audit logs for access

---

## Maintenance & Performance

### Recommended Maintenance Tasks
- Regular backups (daily)
- Index defragmentation (weekly)
- Statistics update (weekly)
- Archive inactive employee records (annually)
- Clean up old audit logs (retention: 7 years)

### Performance Optimization
- Use indexed lookups for EmployeeID, Email
- Filter by IsActive in list views
- Implement pagination for large result sets
- Cache department list for dropdown values
- Use batch operations for bulk updates

---

## Data Validation Rules

### At Database Level (SQL Constraints)
```sql
-- Email format validation (SQL Server)
ALTER TABLE Employees
ADD CONSTRAINT ck_Email
CHECK (Email LIKE '%_@_%._%');

-- Salary must be non-negative
ALTER TABLE Employees
ADD CONSTRAINT ck_Salary
CHECK (Salary IS NULL OR Salary >= 0);

-- Start date cannot be in future
ALTER TABLE Employees
ADD CONSTRAINT ck_StartDate
CHECK (StartDate <= CAST(GETDATE() AS DATE));

-- Valid employment type
ALTER TABLE Employees
ADD CONSTRAINT ck_EmploymentType
CHECK (EmploymentType IN ('Full-Time', 'Part-Time', 'Contract', 'Intern', 'Freelance', 'Temporary'));
```

---

## Backup & Recovery Strategy

### Backup Schedule
- **Full Backup**: Daily (off-hours)
- **Differential Backup**: Every 6 hours
- **Transaction Log Backup**: Every 15 minutes

### Recovery Point Objective (RPO): 15 minutes
### Recovery Time Objective (RTO): 1 hour

### Test Recovery Process
- Monthly full restore test
- Document recovery procedures
- Train team on recovery steps
