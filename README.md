# Employee Management System Backend - Comprehensive Guide

## Overview

This is a Employee Management System backend built with **ASP.NET Core** using **N-layered architecture**.

## Architecture Overview

The project follows **N-layered architecture**:

```
┌─────────────────┐
│   WebApi Layer  │  ← Controllers, Program.cs, Configuration
├─────────────────┤
│   Core Layer    │  ← Business Logic, Services, Mappings
├─────────────────┤
│  Models & DTOs  │  ← Data Transfer Objects, Domain Models
├─────────────────┤
│ Interfaces      │  ← Contracts & Abstractions
├─────────────────┤
│ DataAccess      │  ← EF Core, Repositories, DbContext, Entities
└─────────────────┘
```

## Database Migration Script Configuration

Edit `scripts/run_migrations.bat`:
```bat
REM SQL Server connection parameters - EDIT THESE VALUES
set "SERVER=YOUR_SQL_SERVER_INSTANCE"
set "USERNAME=YOUR_SQL_USERNAME"
set "PASSWORD=YOUR_SQL_PASSWORD"
set "TIMEOUT=30"
```

## 📜 Database Scripts Guide

### Scripts Overview

| Script | Purpose | Creates |
|--------|---------|---------|
| `run_migrations.bat` | Runs all database migrations in order | Complete database with data |
| `generate_migration.sh` | Creates new migration files | New migration templates |
| `generate_module_sql.sh` | Adds new modules/permissions | Custom modules and permissions |

### 1. Database Setup (Complete)

**Run:** `run_migrations.bat`

**What it creates in sequence:**
1. **Migration 001:** Creates `EmployeeDB` database and version tracking
2. **Migration 002:** Creates all core tables (Users, UserRoles, Modules, etc.)
3. **Migration 003:** **Inserts initial data** (admin user, roles, basic modules)

### 2. Creating New Migrations

**Standard Migration:**
```bash
./generate_migration.sh "add employee departments table"
```

**Migration from Data Files (Recommended for modules):**
```bash
# 1. First create modules/accesses (creates files in sql/data/)
./generate_module_sql.sh module --name "HR Management" --code "HR_MANAGEMENT" --description "Human Resources"
./generate_module_sql.sh access --module "HR_MANAGEMENT" --name "View Employees"
./generate_module_sql.sh access --module "HR_MANAGEMENT" --name "Manage Employees"

# 2. Then create migration from all data files
./generate_migration.sh --from-data "add HR management module"
```

**What it creates:**
- **Standard:** Template migration file for manual editing
- **From Data:** Complete migration with all data file contents, then cleans up data files
- All numbered sequentially (e.g., `004_add_hr_management_module.sql`)
- Proper structure and version tracking

### 3. Recommended Workflow for Modules

**Step 1: Create Module and Access Files**
```bash
# Creates SQL file in sql/data/ folder (not applied yet)
./generate_module_sql.sh module --name "Inventory Management" --description "Manage inventory and stock"

# Creates more SQL files in sql/data/ folder
./generate_module_sql.sh access --module "INVENTORY_MANAGEMENT" --name "View Inventory" --description "View inventory items"
./generate_module_sql.sh access --module "INVENTORY_MANAGEMENT" --name "Manage Inventory" --description "Create, edit, delete inventory"
./generate_module_sql.sh access --module "INVENTORY_MANAGEMENT" --name "Delete Inventory" --description "Remove inventory items"
```

**Step 2: Generate Migration from Data Files**
```bash
# This creates a migration containing all the data files, then deletes the data files
./generate_migration.sh --from-data "add inventory management module"
```

**Step 3: Run Migration**
```bash
# This applies the migration to the database
run_migrations.bat
```

**After running migration, database contains:**

**Modules table:**
```sql
SELECT Id, ModuleName, RefCode, ParentId, LogoName, RedirectPage FROM Modules WHERE RefCode = 'INVENTORY_MANAGEMENT';
-- 4 | Inventory Management | INVENTORY_MANAGEMENT | NULL | module-icon.svg | inventory_management
```

**ModuleAccesses table:**
```sql
SELECT Id, ModuleId, ModuleAccessName, RefCode, Description FROM ModuleAccesses WHERE ModuleId = 4;
-- 7 | 4 | View Inventory | INVENTORY_MANAGEMENT_VIEW | View inventory items
-- 8 | 4 | Manage Inventory | INVENTORY_MANAGEMENT_MANAGE | Create, edit, delete inventory
-- 9 | 4 | Delete Inventory | INVENTORY_MANAGEMENT_DELETE | Remove inventory items
```

## Implementing New Features

Follow this systematic approach for implementing new features:

### Step 1: Define Request/Response Structure

**Example: Employee Department Management**

**Request Data (Create Department):**
- Department name (required, max 100 chars)
- Department code (required, unique, max 20 chars)
- Manager ID (optional, must be valid user)
- Description (optional, max 500 chars)

**Response Data:**
- Department ID
- Department details
- Manager information
- Creation timestamp
- Success/error status

### Step 2: Create DTOs

Create data transfer objects in the `Dtos` project:

**File: `Dtos/DepartmentDtos.cs`**
```csharp
namespace Dtos;

// Request DTOs
public class CreateDepartmentRequestDto
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public int? ManagerId { get; set; }
    public string? Description { get; set; }
}

public class UpdateDepartmentRequestDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public int? ManagerId { get; set; }
    public string? Description { get; set; }
}

// Response DTOs
public class DepartmentResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public int? ManagerId { get; set; }
    public string? ManagerName { get; set; }
    public string? Description { get; set; }
    public DateTime DateCreatedUTC { get; set; }
    public int EmployeeCount { get; set; }
}

public class DepartmentListDto
{
    public List<DepartmentResponseDto> Departments { get; set; } = new();
    public int TotalCount { get; set; }
}
```

### Step 3: Create Domain Models

Create business models in the `Models` project:

**File: `Models/DepartmentModel.cs`**
```csharp
using Models.Constants;

namespace Models;

public class DepartmentModel : BaseModel
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public int? ManagerId { get; set; }
    public string? ManagerName { get; set; }
    public string? Description { get; set; }
    public int EmployeeCount { get; set; }
}

public class CreateDepartmentRequestModel : BaseRequestModel
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public int? ManagerId { get; set; }
    public string? Description { get; set; }
}
```

### Step 4: Create Entity (if new table needed)

Create database entity in the `DataAccess` project:

**File: `DataAccess/Department.cs`**
```csharp
using System.ComponentModel.DataAnnotations;

namespace DataAccess;

public class Department : BaseEntity
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string Code { get; set; } = string.Empty;

    public int? ManagerId { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    // Navigation properties
    public User? Manager { get; set; }
    public ICollection<User> Employees { get; set; } = new List<User>();
}
```

### Step 5: Update DbContext and UnitOfWork

**Add to `DataAccess/EmployeeManagementSystemDbContext.cs`:**
```csharp
public DbSet<Department> Departments { get; set; }

protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    // ... existing configurations ...
    
    // Department configuration
    modelBuilder.Entity<Department>(entity =>
    {
        entity.HasIndex(e => e.Code).IsUnique();
        entity.HasOne(d => d.Manager)
              .WithMany()
              .HasForeignKey(d => d.ManagerId)
              .OnDelete(DeleteBehavior.SetNull);
    });
}
```

**Update `DataAccess/UnitOfWork/UnitOfWork.cs`:**
```csharp
public class UnitOfWork : IUnitOfWork
{
    // ... existing repositories ...
    public IRepository<Department> Departments { get; private set; }

    public UnitOfWork(EmployeeManagementSystemDbContext context)
    {
        // ... existing initializations ...
        Departments = new Repository<Department>(_context);
    }
}
```

**Update `DataAccess/Interfaces/IUnitOfWork.cs`:**
```csharp
public interface IUnitOfWork : IDisposable
{
    // ... existing repositories ...
    IRepository<Department> Departments { get; }
    // ... existing methods ...
}
```

### Step 6: Create Business Logic Interface

**File: `Interfaces/IDepartmentLogic.cs`**
```csharp
using Models;

namespace Interfaces;

public interface IDepartmentLogic
{
    Task<ApiResponse<DepartmentModel>> CreateDepartmentAsync(CreateDepartmentRequestModel request);
    Task<ApiResponse<DepartmentModel>> GetDepartmentByIdAsync(int id);
    Task<ApiResponse<List<DepartmentModel>>> GetAllDepartmentsAsync();
    Task<ApiResponse<DepartmentModel>> UpdateDepartmentAsync(int id, CreateDepartmentRequestModel request);
    Task<ApiResponse<bool>> DeleteDepartmentAsync(int id);
}
```

### Step 7: Implement Business Logic

**File: `Core/Department/DepartmentLogic.cs`**
```csharp
using Interfaces;
using Models;
using DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Core.Department;

public class DepartmentLogic : IDepartmentLogic
{
    private readonly IUnitOfWork _unitOfWork;

    public DepartmentLogic(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<DepartmentModel>> CreateDepartmentAsync(CreateDepartmentRequestModel request)
    {
        try
        {
            // Validate business rules
            var existingDept = await _unitOfWork.Departments
                .FindAsync(d => d.Code == request.Code);
            
            if (existingDept.Any())
            {
                return ApiResponse<DepartmentModel>.ErrorResult("Department code already exists");
            }

            // Convert request model to entity
            var department = new DataAccess.Department
            {
                Name = request.Name,
                Code = request.Code,
                ManagerId = request.ManagerId,
                Description = request.Description,
                CreatedBy = request.CreatedBy,
                DateCreatedUTC = DateTime.UtcNow
            };

            // Save to database
            await _unitOfWork.Departments.AddAsync(department);
            await _unitOfWork.SaveChangesAsync();

            // Convert entity back to model for response
            var result = department.ToModel();
            
            return ApiResponse<DepartmentModel>.SuccessResult(result, "Department created successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<DepartmentModel>.ErrorResult($"Failed to create department: {ex.Message}");
        }
    }

    // Implement other methods...
}
```

### Step 8: Create Mapping Extensions

**File: `Core/Department/DepartmentMapping.cs`**
```csharp
using Models;
using Dtos;

namespace Core.Department;

public static class DepartmentMapping
{
    // Entity to Model
    public static DepartmentModel ToModel(this DataAccess.Department entity)
    {
        return new DepartmentModel
        {
            Id = entity.Id,
            Name = entity.Name,
            Code = entity.Code,
            ManagerId = entity.ManagerId,
            ManagerName = entity.Manager?.FirstName + " " + entity.Manager?.LastName,
            Description = entity.Description,
            DateCreatedUTC = entity.DateCreatedUTC,
            CreatedBy = entity.CreatedBy
        };
    }

    // Model to DTO
    public static DepartmentResponseDto ToDto(this DepartmentModel model)
    {
        return new DepartmentResponseDto
        {
            Id = model.Id,
            Name = model.Name,
            Code = model.Code,
            ManagerId = model.ManagerId,
            ManagerName = model.ManagerName,
            Description = model.Description,
            DateCreatedUTC = model.DateCreatedUTC
        };
    }

    // DTO to Model
    public static CreateDepartmentRequestModel ToModel(this CreateDepartmentRequestDto dto)
    {
        return new CreateDepartmentRequestModel
        {
            Name = dto.Name,
            Code = dto.Code,
            ManagerId = dto.ManagerId,
            Description = dto.Description
        };
    }
}
```

### Step 9: Register Services

**Update `WebApi/Program.cs`:**
```csharp
// Business services
builder.Services.AddScoped<IAuthLogic, AuthLogic>();
builder.Services.AddScoped<IModuleLogic, ModuleLogic>();
builder.Services.AddScoped<IPermissionLogic, PermissionLogic>();
builder.Services.AddScoped<IFeatureLogic, FeatureLogic>();
builder.Services.AddScoped<IDepartmentLogic, DepartmentLogic>(); // Add this line
```

### Step 10: Create Controller

**File: `WebApi/Controllers/DepartmentController.cs`**
```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Interfaces;
using Dtos;
using Core.Department;

namespace WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DepartmentController : ControllerBase
{
    private readonly IDepartmentLogic _departmentLogic;

    public DepartmentController(IDepartmentLogic departmentLogic)
    {
        _departmentLogic = departmentLogic;
    }

    /// <summary>
    /// Create a new department
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<DepartmentResponseDto>>> CreateDepartment(CreateDepartmentRequestDto request)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var requestModel = request.ToModel();
        requestModel.CreatedBy = userId;

        var response = await _departmentLogic.CreateDepartmentAsync(requestModel);
        
        if (response.Success)
            return Ok(ApiResponse<DepartmentResponseDto>.SuccessResult(response.Data!.ToDto(), response.Message));
        
        return BadRequest(ApiResponse<DepartmentResponseDto>.ErrorResult(response.Message));
    }

    /// <summary>
    /// Get all departments
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<DepartmentResponseDto>>>> GetAllDepartments()
    {
        var response = await _departmentLogic.GetAllDepartmentsAsync();
        
        if (response.Success)
        {
            var dtoList = response.Data!.Select(d => d.ToDto()).ToList();
            return Ok(ApiResponse<List<DepartmentResponseDto>>.SuccessResult(dtoList, response.Message));
        }
        
        return BadRequest(ApiResponse<List<DepartmentResponseDto>>.ErrorResult(response.Message));
    }

    /// <summary>
    /// Get department by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<DepartmentResponseDto>>> GetDepartment(int id)
    {
        var response = await _departmentLogic.GetDepartmentByIdAsync(id);
        
        if (response.Success)
            return Ok(ApiResponse<DepartmentResponseDto>.SuccessResult(response.Data!.ToDto(), response.Message));
        
        return NotFound(ApiResponse<DepartmentResponseDto>.ErrorResult(response.Message));
    }

    // Add other endpoints (PUT, DELETE) as needed...
}
```

### Step 11: Create Database Migration

Generate a new migration for the database changes:

```bash
./scripts/generate_migration.sh "add department table"
```

Edit the generated SQL file to add the Department table:

```sql
-- Migration XXX: add department table
USE EmployeeDB;

-- Create Department table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Departments' AND xtype='U')
BEGIN
    CREATE TABLE Departments (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Name NVARCHAR(100) NOT NULL,
        Code NVARCHAR(20) NOT NULL UNIQUE,
        ManagerId INT NULL,
        Description NVARCHAR(500) NULL,
        DateCreatedUTC DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        LatestDateUpdatedUTC DATETIME2 NULL,
        CreatedBy INT NOT NULL,
        LatestUpdatedBy INT NULL,
        
        CONSTRAINT FK_Departments_Manager FOREIGN KEY (ManagerId) REFERENCES Users(Id),
        CONSTRAINT FK_Departments_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES Users(Id),
        CONSTRAINT FK_Departments_UpdatedBy FOREIGN KEY (LatestUpdatedBy) REFERENCES Users(Id)
    );
    PRINT 'Departments table created successfully.';
END

-- Record this migration
IF NOT EXISTS (SELECT * FROM DbVersions WHERE Version = 'XXX')
BEGIN
    INSERT INTO DbVersions (Version, Description) 
    VALUES ('XXX', 'add department table');
END
```

## 🔧 Data Flow Summary

The complete data flow for a new feature follows this pattern:

1. **Request comes in** → `DepartmentController`
2. **DTO validation** → Convert to **Request Model**
3. **Business Logic** → `DepartmentLogic` processes with **UnitOfWork**
4. **Database Operations** → Through **Repository pattern**
5. **Entity** ↔ **Model** ↔ **DTO** conversions via **Mapping extensions**
6. **Response** → Structured **API Response** back to client
