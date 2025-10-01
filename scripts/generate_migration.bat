@echo off
REM Migration Generator Script - Windows Batch Version
REM Creates a new migration file with the correct naming convention and template

setlocal enabledelayedexpansion

REM Set script directory and migrations directory
set "SCRIPT_DIR=%~dp0"
set "MIGRATIONS_DIR=%SCRIPT_DIR%sql\migrations"

REM Initialize variables
set "DESCRIPTION="
set "FROM_DATA=false"

REM Parse command line arguments
:parse_args
if "%~1"=="" goto check_args
if /i "%~1"=="--from-data" (
    set "FROM_DATA=true"
    shift
    goto parse_args
)
if /i "%~1"=="-h" goto show_help
if /i "%~1"=="--help" goto show_help
if "%DESCRIPTION%"=="" (
    set "DESCRIPTION=%~1"
    shift
    goto parse_args
) else (
    echo [ERROR] Multiple descriptions provided. Use quotes for descriptions with spaces.
    exit /b 1
)

:show_help
echo Usage: %0 [options] ^<description^>
echo.
echo Options:
echo   --from-data    Create migration from files in sql/data folder
echo   -h, --help     Show this help message
echo.
echo Examples:
echo   %0 "add employee table"
echo   %0 --from-data "add modules and permissions"
exit /b 0

:check_args
REM Check if description is provided (unless using --from-data)
if "%DESCRIPTION%"=="" (
    if "%FROM_DATA%"=="false" (
        echo [ERROR] Description is required
        echo.
        echo Usage: %0 [options] ^<description^>
        echo Use --help for more information
        exit /b 1
    ) else (
        set "DESCRIPTION=add modules and permissions from data files"
    )
)

REM Create migrations directory if it doesn't exist
if not exist "%MIGRATIONS_DIR%" mkdir "%MIGRATIONS_DIR%"

REM Create data directory if it doesn't exist
set "DATA_DIR=%SCRIPT_DIR%sql\data"
if not exist "%DATA_DIR%" mkdir "%DATA_DIR%"

REM Get the next migration number
set "LAST_MIGRATION=000"
for /f "tokens=*" %%f in ('dir /b "%MIGRATIONS_DIR%\*.sql" 2^>nul ^| sort') do (
    set "FILENAME=%%f"
    set "NUMBER=!FILENAME:~0,3!"
    if !NUMBER! gtr !LAST_MIGRATION! set "LAST_MIGRATION=!NUMBER!"
)

set /a "NEXT_NUMBER=%LAST_MIGRATION%+1"
set "NEXT_NUMBER=000%NEXT_NUMBER%"
set "NEXT_NUMBER=%NEXT_NUMBER:~-3%"

REM Create filename from description (replace spaces with underscores, lowercase)
set "FILENAME_DESC=%DESCRIPTION%"
set "FILENAME_DESC=%FILENAME_DESC: =_%"
set "FILENAME_DESC=%FILENAME_DESC:,=_%"
set "FILENAME_DESC=%FILENAME_DESC:(=_%"
set "FILENAME_DESC=%FILENAME_DESC:)=_%"
set "FILENAME_DESC=%FILENAME_DESC:/=_%"
set "FILENAME_DESC=%FILENAME_DESC:\=_%"

REM Convert to lowercase (simple method for basic chars)
for %%i in (A B C D E F G H I J K L M N O P Q R S T U V W X Y Z) do (
    call set "FILENAME_DESC=%%FILENAME_DESC:%%i=%%i%%"
)
for %%i in (a b c d e f g h i j k l m n o p q r s t u v w x y z) do (
    call set "FILENAME_DESC=%%FILENAME_DESC:%%i=%%i%%"
)

set "FILENAME=%NEXT_NUMBER%_%FILENAME_DESC%.sql"
set "FILEPATH=%MIGRATIONS_DIR%\%FILENAME%"

if "%FROM_DATA%"=="true" (
    REM Check if data directory has files
    set "DATA_FILE_COUNT=0"
    if exist "%DATA_DIR%\*.sql" (
        for %%f in ("%DATA_DIR%\*.sql") do (
            set /a "DATA_FILE_COUNT+=1"
        )
    )
    
    if !DATA_FILE_COUNT! equ 0 (
        echo [ERROR] No SQL files found in %DATA_DIR%
        echo [INFO] Use generate_module_sql.bat to create module/access files first
        exit /b 1
    )
    
    echo [INFO] Found !DATA_FILE_COUNT! data file^(s^) to process:
    for %%f in ("%DATA_DIR%\*.sql") do (
        echo   - %%~nxf
    )
    echo.
    
    REM Create migration with data files content
    (
        echo -- Migration %NEXT_NUMBER%: %DESCRIPTION%
        echo -- Created: %DATE% %TIME%
        echo -- Description: %DESCRIPTION%
        echo -- Generated from data files in sql/data/
        echo.
        echo USE EmployeeDB;
        echo.
    ) > "%FILEPATH%"
    
    REM Process each data file
    for %%f in ("%DATA_DIR%\*.sql") do (
        echo [INFO] Processing: %%~nxf
        
        (
            echo -- ============================================================================
            echo -- Content from: %%~nxf
            echo -- ============================================================================
            echo.
        ) >> "%FILEPATH%"
        
        REM Extract the SQL content (skip comments and USE statements)
        for /f "usebackq delims=" %%l in ("%%f") do (
            set "line=%%l"
            if not "!line:~0,2!"=="--" (
                if not "!line:~0,4!"=="USE " (
                    if not "!line!"=="" (
                        echo !line! >> "%FILEPATH%"
                    )
                )
            )
        )
        
        echo GO >> "%FILEPATH%"
        echo. >> "%FILEPATH%"
    )
    
    REM Add migration tracking
    (
        echo.
        echo -- Record this migration
        echo IF NOT EXISTS ^(SELECT * FROM DbVersions WHERE Version = '%NEXT_NUMBER%'^)
        echo BEGIN
        echo     INSERT INTO DbVersions ^(Version, Description^) 
        echo     VALUES ^('%NEXT_NUMBER%', '%DESCRIPTION%'^);
        echo END
        echo.
        echo PRINT 'Migration %NEXT_NUMBER% completed successfully.';
    ) >> "%FILEPATH%"
    
    REM Delete processed data files
    echo [INFO] Cleaning up data files...
    for %%f in ("%DATA_DIR%\*.sql") do (
        del "%%f"
        echo [SUCCESS] Deleted: %%~nxf
    )
    
) else (
    REM Create standard migration template
    (
        echo -- Migration %NEXT_NUMBER%: %DESCRIPTION%
        echo -- Created: %DATE% %TIME%
        echo -- Description: %DESCRIPTION%
        echo.
        echo USE EmployeeDB;
        echo.
        echo -- Add your SQL commands here
        echo -- Example:
        echo -- ALTER TABLE Users ADD NewColumn NVARCHAR^(100^) NULL;
        echo -- CREATE TABLE NewTable ^(
        echo --     Id INT IDENTITY^(1,1^) PRIMARY KEY,
        echo --     Name NVARCHAR^(100^) NOT NULL
        echo -- ^);
        echo.
        echo -- Record this migration
        echo IF NOT EXISTS ^(SELECT * FROM DbVersions WHERE Version = '%NEXT_NUMBER%'^)
        echo BEGIN
        echo     INSERT INTO DbVersions ^(Version, Description^) 
        echo     VALUES ^('%NEXT_NUMBER%', '%DESCRIPTION%'^);
        echo END
        echo.
        echo PRINT 'Migration %NEXT_NUMBER% completed successfully.';
    ) > "%FILEPATH%"
)

echo [SUCCESS] Migration file created: %FILENAME%
echo [INFO] File path: %FILEPATH%
echo.

if "%FROM_DATA%"=="true" (
    echo [INFO] Migration created from %DATA_FILE_COUNT% data file^(s^)
    echo [INFO] Data files have been cleaned up
    echo [INFO] Next steps:
    echo [INFO] 1. Review the migration file: %FILEPATH%
    echo [INFO] 2. Run migrations with: run_migrations.bat
) else (
    echo [INFO] Next steps:
    echo [INFO] 1. Edit the file and add your SQL commands
    echo [INFO] 2. Run migrations with: run_migrations.bat
)

endlocal