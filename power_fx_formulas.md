# PowerApps Employee Details Form - Power Fx Formulas & Logic

## Overview
This document contains all Power Fx formulas and business logic used in the Employee Details Form application.

---

## Form Initialization & Display

### Load Employee Record for Editing
```powerapps
Set(
    selectedEmployee,
    LookUp(
        EmployeeDatabase,
        EmployeeID = selectedEmployeeId
    )
)
```

### Reset Form to New Record
```powerapps
ResetForm(FormContainer);
Set(isEditMode, false);
ClearCollect(formValidation, {isValid: true, errors: []})
```

---

## Validation Formulas

### Email Validation
```powerapps
If(
    IsBlank(EmailInput.Value),
    true,
    If(
        IsMatch(
            EmailInput.Value,
            Email
        ),
        true,
        false
    )
)
```

### Phone Number Validation
```powerapps
If(
    IsBlank(PhoneInput.Value),
    true,
    If(
        IsMatch(
            PhoneInput.Value,
            "^[0-9\-\+\(\)\ ]{7,}$"
        ),
        true,
        false
    )
)
```

### Postal Code Validation (North American)
```powerapps
If(
    IsBlank(PostalCodeInput.Value),
    true,
    If(
        Or(
            IsMatch(PostalCodeInput.Value, "[0-9]{5}(-[0-9]{4})?"),  // US Format: 12345 or 12345-6789
            IsMatch(PostalCodeInput.Value, "[A-Z][0-9][A-Z]\ ?[0-9][A-Z][0-9]")  // Canada Format: A1A 1A1
        ),
        true,
        false
    )
)
```

### Salary Validation
```powerapps
If(
    IsBlank(SalaryInput.Value),
    true,
    If(
        And(
            IsNumeric(SalaryInput.Value),
            Value(SalaryInput.Value) >= 0,
            Value(SalaryInput.Value) <= 10000000  // Max salary limit
        ),
        true,
        false
    )
)
```

### Complete Form Validation
```powerapps
And(
    !IsBlank(EmployeeIDInput.Value),
    !IsBlank(FirstNameInput.Value),
    !IsBlank(LastNameInput.Value),
    IsMatch(EmailInput.Value, Email),
    !IsBlank(DepartmentDropdown.Value),
    !IsBlank(PositionInput.Value),
    !IsBlank(StartDatePicker.Value),
    !IsBlank(EmploymentTypeDropdown.Value),
    If(IsBlank(PhoneInput.Value), true, IsMatch(PhoneInput.Value, "^[0-9\-\+\(\)\ ]{7,}$")),
    If(IsBlank(PostalCodeInput.Value), true, IsMatch(PostalCodeInput.Value, "[0-9]{5}(-[0-9]{4})?|[A-Z][0-9][A-Z]\ ?[0-9][A-Z][0-9]"))
)
```

---

## Data Operations

### Save Employee (Create New)
```powerapps
Collect(
    EmployeeDatabase,
    {
        EmployeeID: EmployeeIDInput.Value,
        FirstName: FirstNameInput.Value,
        LastName: LastNameInput.Value,
        Email: EmailInput.Value,
        Phone: PhoneInput.Value,
        Department: DepartmentDropdown.Value,
        Position: PositionInput.Value,
        StartDate: StartDatePicker.Value,
        EmploymentType: EmploymentTypeDropdown.Value,
        Salary: If(IsBlank(SalaryInput.Value), 0, Value(SalaryInput.Value)),
        Address: AddressInput.Value,
        City: CityInput.Value,
        State: StateInput.Value,
        PostalCode: PostalCodeInput.Value,
        CreatedDate: Now(),
        ModifiedDate: Now()
    }
)
```

### Update Employee (Edit Mode)
```powerapps
Patch(
    EmployeeDatabase,
    LookUp(EmployeeDatabase, EmployeeID = selectedEmployeeId),
    {
        FirstName: FirstNameInput.Value,
        LastName: LastNameInput.Value,
        Email: EmailInput.Value,
        Phone: PhoneInput.Value,
        Department: DepartmentDropdown.Value,
        Position: PositionInput.Value,
        StartDate: StartDatePicker.Value,
        EmploymentType: EmploymentTypeDropdown.Value,
        Salary: If(IsBlank(SalaryInput.Value), 0, Value(SalaryInput.Value)),
        Address: AddressInput.Value,
        City: CityInput.Value,
        State: StateInput.Value,
        PostalCode: PostalCodeInput.Value,
        ModifiedDate: Now()
    }
)
```

### Delete Employee Record
```powerapps
If(
    Confirm("Are you sure you want to delete this employee record? This action cannot be undone."),
    Remove(
        EmployeeDatabase,
        LookUp(EmployeeDatabase, EmployeeID = selectedEmployeeId)
    );
    Notify("Employee record deleted successfully.", NotificationType.Success);
    Set(isEditMode, false);
    ResetForm(FormContainer),
    Notify("Delete operation cancelled.", NotificationType.Information)
)
```

---

## Conditional Logic

### Show Required Field Indicator
```powerapps
If(
    Or(
        FirstNameInput.SubmitAttempted,
        FirstNameInput.Value = ""
    ),
    "✱ Required",
    ""
)
```

### Enable/Disable Save Button
```powerapps
If(
    And(
        !IsBlank(EmployeeIDInput.Value),
        !IsBlank(FirstNameInput.Value),
        !IsBlank(LastNameInput.Value),
        IsMatch(EmailInput.Value, Email),
        !IsBlank(DepartmentDropdown.Value),
        !IsBlank(PositionInput.Value)
    ),
    true,
    false
)
```

### Calculate Employment Duration
```powerapps
If(
    IsBlank(StartDatePicker.Value),
    "",
    Let(
        today, Today(),
        startDate, StartDatePicker.Value,
        years, RoundDown((DateDiff(today, startDate, TimeUnit.Days) / 365.25), 0),
        months, RoundDown(Mod(DateDiff(today, startDate, TimeUnit.Days), 365.25) / 30.44, 0),
        Concatenate(
            If(years > 0, Concatenate(years, " years "), ""),
            If(months > 0, Concatenate(months, " months"), "")
        )
    )
)
```

### Show Salary Only if User Has Permission
```powerapps
If(
    Or(
        User().Email = "hr@company.com",
        User().Email = "manager@company.com"
    ),
    SalaryInput.Value,
    "[Confidential]"
)
```

---

## Search & Filter

### Search Employees by Name
```powerapps
Filter(
    EmployeeDatabase,
    Or(
        StartsWith(FirstName, SearchInput.Value),
        StartsWith(LastName, SearchInput.Value),
        StartsWith(Email, SearchInput.Value)
    )
)
```

### Filter Employees by Department
```powerapps
Filter(
    EmployeeDatabase,
    Department = DepartmentDropdown.Value
)
```

### Get Employees by Employment Type
```powerapps
Filter(
    EmployeeDatabase,
    EmploymentType = EmploymentTypeDropdown.Value
)
```

---

## UI Display Formulas

### Format Full Name
```powerapps
Concatenate(FirstNameInput.Value, " ", LastNameInput.Value)
```

### Format Phone Number Display
```powerapps
If(
    IsBlank(PhoneInput.Value),
    "-",
    Concatenate(
        Left(PhoneInput.Value, 3),
        "-",
        Mid(PhoneInput.Value, 4, 3),
        "-",
        Right(PhoneInput.Value, 4)
    )
)
```

### Format Salary Display
```powerapps
If(
    IsBlank(SalaryInput.Value),
    "-",
    Concatenate("$", Text(Value(SalaryInput.Value), "0,0.00"))
)
```

### Full Address Display
```powerapps
Concatenate(
    If(IsBlank(AddressInput.Value), "", Concatenate(AddressInput.Value, ", ")),
    If(IsBlank(CityInput.Value), "", Concatenate(CityInput.Value, ", ")),
    If(IsBlank(StateInput.Value), "", Concatenate(StateInput.Value, " ")),
    PostalCodeInput.Value
)
```

---

## Error Handling

### Try-Catch Pattern for Save Operation
```powerapps
IfError(
    {
        result: Patch(
            EmployeeDatabase,
            LookUp(EmployeeDatabase, EmployeeID = selectedEmployeeId),
            {FirstName: FirstNameInput.Value}
        )
    },
    Notify(
        Concatenate("Error saving record: ", FirstError.Message),
        NotificationType.Error
    ),
    Notify("Record saved successfully!", NotificationType.Success)
)
```

---

## Notifications

### Success Notification
```powerapps
Notify(
    "Employee record saved successfully!",
    NotificationType.Success
)
```

### Error Notification
```powerapps
Notify(
    "Please fill in all required fields (marked with *).",
    NotificationType.Error
)
```

### Information Notification
```powerapps
Notify(
    "Changes have been discarded.",
    NotificationType.Information
)
```

### Warning Notification
```powerapps
Notify(
    "This employee record is marked as inactive.",
    NotificationType.Warning
)
```

---

## Export & Reporting

### Export Employee List to Excel
```powerapps
ForAll(
    Filter(EmployeeDatabase, Department = DepartmentDropdown.Value),
    {
        "Employee ID": EmployeeID,
        "Name": Concatenate(FirstName, " ", LastName),
        "Email": Email,
        "Phone": Phone,
        "Position": Position,
        "Start Date": Text(StartDate, "mm/dd/yyyy"),
        "Employment Type": EmploymentType
    }
)
```

---

## Key Functions Used

| Function | Purpose |
|----------|---------|
| `Collect()` | Add new records to a data source |
| `Patch()` | Update existing records |
| `Remove()` | Delete records |
| `LookUp()` | Find a single record matching criteria |
| `Filter()` | Return records matching conditions |
| `IsBlank()` | Check if value is empty |
| `IsMatch()` | Validate text against pattern |
| `IsNumeric()` | Check if value is a number |
| `Concatenate()` | Combine text values |
| `DateDiff()` | Calculate difference between dates |
| `Now()` | Get current date and time |
| `Today()` | Get current date |
| `Notify()` | Display message to user |
| `Set()` | Store variable value |
| `IfError()` | Handle errors gracefully |
