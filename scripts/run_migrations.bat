@echo off
REM Employee Management System - Database Migration Runner
REM This batch file connects to SQL Server and runs migration scripts in order
REM 
REM CONFIGURATION: Edit the variables below to match your setup
REM ============================================================

setlocal enabledelayedexpansion

REM SQL Server connection parameters - EDIT THESE VALUES
set "SERVER=localhost"
set "USERNAME=sa"
set "PASSWORD=YourPassword123!"
set "TIMEOUT=30"

REM Script directory - resolve full path to work from any directory
set "SCRIPT_DIR=%~dp0"
if "%SCRIPT_DIR:~-1%"=="\" set "SCRIPT_DIR=%SCRIPT_DIR:~0,-1%"
set "MIGRATIONS_DIR=%SCRIPT_DIR%\sql\migrations"

echo Employee Management System - Migration Runner
echo.
REM Validate required parameters
if "%SERVER%"=="" (
    echo Error: Server name is required
    echo Please edit the SERVER variable in this script
    exit /b 1
)
if "%USERNAME%"=="" (
    echo Error: Username is required
    echo Please edit the USERNAME variable in this script
    exit /b 1
)
if "%PASSWORD%"=="" (
    echo Error: Password is required
    echo Please edit the PASSWORD variable in this script
    exit /b 1
)

REM Check if migrations directory exists
if not exist "%MIGRATIONS_DIR%" (
    echo Error: Migrations directory not found: %MIGRATIONS_DIR%
    echo Please make sure you're running this script from the correct location.
    exit /b 1
)

echo Connection Parameters:
echo Server: %SERVER%
echo Username: %USERNAME%
echo Password: [HIDDEN]
echo Timeout: %TIMEOUT% seconds
echo Migrations Directory: %MIGRATIONS_DIR%
echo.

REM Test connection
echo Testing SQL Server connection...
sqlcmd -S %SERVER% -U %USERNAME% -P %PASSWORD% -Q "SELECT 1 as ConnectionTest" -t %TIMEOUT% >nul 2>&1
if errorlevel 1 (
    echo Error: Failed to connect to SQL Server
    echo Please check your connection parameters and ensure SQL Server is running.
    exit /b 1
)
echo Connection successful!
echo.

REM Get list of migration files sorted by name
echo Scanning for migration files...
set "migration_count=0"
for /f "tokens=*" %%f in ('dir /b /o:n "%MIGRATIONS_DIR%\*.sql" 2^>nul') do (
    set /a migration_count+=1
    set "migration_!migration_count!=%%f"
)

if %migration_count%==0 (
    echo No migration files found in %MIGRATIONS_DIR%
    exit /b 0
)

echo Found %migration_count% migration file(s):
for /l %%i in (1,1,%migration_count%) do (
    echo   %%i. !migration_%%i!
)
echo.

REM Check if target database exists
echo Checking database existence...
set "database_exists=false"
for /f "tokens=*" %%d in ('sqlcmd -S %SERVER% -U %USERNAME% -P %PASSWORD% -Q "IF EXISTS(SELECT name FROM sys.databases WHERE name = 'EmployeeDB') SELECT 1 ELSE SELECT 0" -h -1 -W 2^>nul ^| findstr /r "^[01]$"') do (
    if "%%d"=="1" set "database_exists=true"
)

if "!database_exists!"=="false" (
    echo Target database 'EmployeeDB' does not exist - will run all migrations
    REM If database doesn't exist, all migrations are pending
    set "pending_count=%migration_count%"
    for /l %%i in (1,1,%migration_count%) do (
        set "pending_%%i=!migration_%%i!"
        set "pending_version_%%i=!migration_%%i:~0,3!"
    )
) else (
    echo Database 'EmployeeDB' exists - checking migration status...
    
    REM Build list of pending migrations by checking database directly
    set "pending_count=0"
    for /l %%i in (1,1,%migration_count%) do (
        set "current_file=!migration_%%i!"
        
        REM Extract version number from filename (first 3 characters)
        set "version_num=!current_file:~0,3!"
        
        REM Check if this version already exists in database
        set "already_run=false"
        
        REM Query database for this specific version
        for /f "tokens=*" %%r in ('sqlcmd -S %SERVER% -U %USERNAME% -P %PASSWORD% -Q "IF OBJECT_ID('EmployeeDB.dbo.DbVersions', 'U') IS NOT NULL AND EXISTS(SELECT 1 FROM EmployeeDB.dbo.DbVersions WHERE Version='!version_num!') SELECT 1 ELSE SELECT 0" -h -1 -W 2^>nul ^| findstr /r "^[01]$"') do (
            if "%%r"=="1" set "already_run=true"
        )
        
        if "!already_run!"=="false" (
            set /a pending_count+=1
            set "pending_!pending_count!=!current_file!"
            set "pending_version_!pending_count!=!version_num!"
        ) else (
            echo Skipping !current_file! - already applied
        )
    )
)

echo.
echo Migration Status Summary:
set /a "applied_count=%migration_count%-%pending_count%"
echo   Total migrations found: %migration_count%
echo   Already applied: %applied_count%
echo   Pending to run: %pending_count%
echo.

if %pending_count%==0 (
    echo All migrations are already up to date!
    echo No pending migrations to run.
    exit /b 0
)

echo Found %pending_count% pending migration(s) to run:
for /l %%i in (1,1,%pending_count%) do (
    echo   %%i. !pending_%%i!
)
echo.

REM Confirm execution of pending migrations
set /p "confirm=Do you want to run these %pending_count% pending migrations? (y/N): "
if /i not "!confirm!"=="y" (
    echo Migration cancelled by user
    exit /b 0
)
echo.

REM Execute pending migrations only
echo Starting migration execution...
echo.

set "success_count=0"
set "error_count=0"

for /l %%i in (1,1,%pending_count%) do (
    set "current_file=!pending_%%i!"
    set "file_path=%MIGRATIONS_DIR%\!current_file!"
    
    echo Running migration %%i of %pending_count%: !current_file!
    
    REM Execute the migration
    sqlcmd -S %SERVER% -U %USERNAME% -P %PASSWORD% -i "!file_path!" -t %TIMEOUT%
    
    if errorlevel 1 (
        echo Error: Migration !current_file! failed
        set /a error_count+=1
        
        REM Ask if user wants to continue
        set /p "continue=Continue with remaining migrations? (y/N): "
        if /i not "!continue!"=="y" (
            echo Migration process stopped by user
            goto :summary
        )
    ) else (
        echo Migration !current_file! completed successfully
        set /a success_count+=1
    )
    echo.
)

:summary
echo.
echo Migration Summary:
echo   Successful: %success_count%
echo   Failed: %error_count%
echo   Pending: %pending_count%
echo   Total Available: %migration_count%

if %error_count% gtr 0 (
    echo.
    echo Some migrations failed. Please check the error messages above.
    exit /b 1
) else (
    echo.
    echo All migrations completed successfully!
    echo Your database is now ready for use.
)

exit /b 0