# Employee Management System Backend - Windows Setup Guide

## Project Description

This Employee Management System backend is a RESTful API built with **ASP.NET Core** using **N-layered architecture**. The system provides comprehensive employee management functionality including authentication, authorization, and CRUD operations.

## Prerequisites

- **.NET 9.0 SDK** or later
- **SQL Server**
- **Visual Studio** or **Visual Studio Code**
- **Docker Desktop for Windows** (optional, for containerized development)
- **Windows PowerShell** or **Command Prompt**

## Setup Guide

### 1. Clone the Repository

```powershell
git clone <repository-url>
cd employee-management-system-backend
```

### 2. Development Setup (HTTP - No Docker)

For local development without Docker and HTTPS.

#### Configure Development Settings

Edit `WebApi\appsettings.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "Kestrel": {
    "Endpoints": {
      "Http": {
        "Url": "http://localhost:8080"
      }
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=EmployeeDB;User Id=sa;Password=YourPassword123!;TrustServerCertificate=true;"
  },
  "Jwt": {
    "Key": "development-key-minimum-32-characters-long-for-security",
    "Issuer": "EmployeeManagementSystem",
    "Audience": "EmployeeManagementUsers",
    "ExpireMinutes": 15
  },
  "Cors": {
    "AllowedOrigins": "http://localhost:3000,http://localhost:4200"
  }
}
```

#### Run the Application

**Using Command Prompt/PowerShell:**
```powershell
dotnet run --project WebApi --launch-profile development
```

**Using Visual Studio:**
1. Open the solution file (`.sln`) in Visual Studio 2022
2. Select "development" profile from the dropdown next to the green play button
3. Press F5 or click the "Start" button

The API will be available at: `http://localhost:8080`
Swagger UI: `http://localhost:8080/swagger`

### 3. Production Setup (HTTP - No HTTPS)

For production deployment without HTTPS.

#### Generate Secure JWT Key

**Using PowerShell:**
```powershell
# Generate a secure 64-character JWT key
$bytes = New-Object byte[] 48
(New-Object System.Security.Cryptography.RNGCryptoServiceProvider).GetBytes($bytes)
[System.Convert]::ToBase64String($bytes)

# Alternative method
[System.Web.Security.Membership]::GeneratePassword(64, 10)

# Simple GUID-based method
(New-Guid).ToString() + (New-Guid).ToString() -replace '-', ''
```

#### Configure Production HTTP Settings

Edit `WebApi\appsettings.Production.json`:


```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "Kestrel": {
    "Endpoints": {
      "Http": {
        "Url": "http://localhost:8080"
      }
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=EmployeeDB;User Id=sa;Password=YourPassword123!;TrustServerCertificate=true;"
  },
  "Jwt": {
    "Key": "replace-with-a-secure-production-key-minimum-32-characters-long",
    "Issuer": "EmployeeManagementSystem",
    "Audience": "EmployeeManagementUsers",
    "ExpireMinutes": 15
  },
  "Cors": {
    "AllowedOrigins": "http://localhost:3000,http://localhost:4200"
  }
}
```

#### Run the Application

**Using Command Prompt/PowerShell:**
```powershell
dotnet run --project WebApi --launch-profile production-http
```

**Using Visual Studio:**
1. Open the solution file (`.sln`) in Visual Studio 2022
2. Select "production-http" profile from the dropdown next to the green play button
3. Press F5 or click the "Start" button

The API will be available at: `http://localhost:8080`
Swagger UI: `http://localhost:8080/swagger`

### 4. Production Setup (HTTPS)

For production deployment with HTTPS and security configurations.

#### Generate SSL Certificate for Windows

**Option 1: Development Certificate (Testing Only)**
```powershell
# Clean existing certificates
dotnet dev-certs https --clean

# Generate and trust development certificate
dotnet dev-certs https --trust

# Export certificate for use
dotnet dev-certs https --export-path certificate.pfx --password "YourCertPassword123!"
```

**Option 2: Self-Signed Certificate with OpenSSL**
```powershell
# Generate private key
openssl genrsa -out private-key.pem 2048

# Generate certificate signing request
openssl req -new -key private-key.pem -out csr.pem

# Generate self-signed certificate
openssl x509 -req -days 365 -in csr.pem -signkey private-key.pem -out certificate.pem

# Convert to PFX format
openssl pkcs12 -export -out certificate.pfx -inkey private-key.pem -in certificate.pem -password pass:YourCertPassword123!
```

#### Generate Secure JWT Key

**Using PowerShell:**
```powershell
# Generate a secure 64-character JWT key
$bytes = New-Object byte[] 48
(New-Object System.Security.Cryptography.RNGCryptoServiceProvider).GetBytes($bytes)
[System.Convert]::ToBase64String($bytes)

# Alternative method
[System.Web.Security.Membership]::GeneratePassword(64, 10)

# Simple GUID-based method
(New-Guid).ToString() + (New-Guid).ToString() -replace '-', ''
```

#### Configure Production Settings

Configure `WebApi\appsettings.Production.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "Kestrel": {
    "Certificates": {
      "Default": {
        "Path": "C:\\certificates\\certificate.pfx",
        "Password": "YourCertPassword123!"
      }
    },
    "Endpoints": {
      "Http": {
        "Url": "http://localhost:8080"
      },
      "Https": {
        "Url": "https://localhost:8443"
      }
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=EmployeeDB;User Id=sa;Password=YourPassword123!;TrustServerCertificate=true;"
  },
  "Jwt": {
    "Key": "replace-with-a-secure-production-key-minimum-32-characters-long",
    "Issuer": "EmployeeManagementSystem",
    "Audience": "EmployeeManagementUsers",
    "ExpireMinutes": 15
  },
  "Cors": {
    "AllowedOrigins": "http://localhost:3000,http://localhost:4200"
  }
}
```

### 5. Database Migration

Before running the migration script, configure the required variables in the batch file.

#### Configure Migration Variables

Open `.\scripts\run_migrations.bat` and set the following variables at the top of the file:

```batch
set "SERVER=localhost"
set "USERNAME=sa"
set "PASSWORD=YourPassword123!"
set "TIMEOUT=30"
```

Adjust these values to match your SQL Server configuration.

#### Run Migration Script
```powershell
# Run batch file
.\scripts\run_migrations.bat
```

### 6. Docker Development Setup (Optional)

For containerized development with hot reload and automatic SQL Server setup on Windows.

#### Run with Docker

**Using PowerShell:**
```powershell
# Start all services (SQL Server + API with hot reload)
docker compose up

# Run in background
docker compose up -d

# Stop services
docker compose down

# Clean up containers and volumes
docker compose down -v
docker system prune -f
```

The API will be available at: `http://localhost:8080`



## Default Admin Account

After running migrations, you can log in with:
- **Email:** `admin@system.com`
- **Password:** `Admin@123`