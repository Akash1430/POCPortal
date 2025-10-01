@echo off
REM Generate Module and Access SQL Scripts - Windows Batch Version
REM Creates SQL scripts for adding new modules and module accesses

setlocal enabledelayedexpansion

REM Set script directory and SQL data directory
set "SCRIPT_DIR=%~dp0"
set "SQL_DATA_DIR=%SCRIPT_DIR%sql\data"

REM Function to show help
if "%1"=="" goto show_help
if /i "%1"=="-h" goto show_help
if /i "%1"=="--help" goto show_help
if /i "%1"=="help" goto show_help

set "COMMAND=%1"
shift

if /i "%COMMAND%"=="module" goto parse_module
if /i "%COMMAND%"=="access" goto parse_access
goto unknown_command

:show_help
echo Generate Module and Access SQL Scripts
echo.
echo Usage: %0 ^<command^> [options]
echo.
echo Commands:
echo   module          Generate SQL to add a new module
echo   access          Generate SQL to add new module access
echo.
echo Module Options:
echo   -n, --name ^<name^>           Module name ^(required^)
echo   -c, --code ^<code^>           Module reference code ^(auto-generated if not provided^)
echo   -p, --parent ^<parent_code^>  Parent module code ^(for sub-modules^)
echo   -d, --description ^<desc^>    Module description
echo   --logo ^<logo^>              Logo name ^(default: module-icon.svg^)
echo   --page ^<page^>              Redirect page ^(default: {code}^)
echo   --order ^<order^>            Sort order ^(auto-calculated if not provided^)
echo   --hidden                   Make module hidden ^(not visible in menu^)
echo   -o, --output ^<file^>        Output file ^(auto-generated if not provided^)
echo.
echo Access Options:
echo   -m, --module ^<code^>         Module reference code ^(required^)
echo   -n, --name ^<name^>           Access name ^(required^)
echo   -c, --code ^<code^>           Access reference code ^(auto-generated if not provided^)
echo   -d, --description ^<desc^>    Access description
echo   --hidden                   Make access hidden
echo   -o, --output ^<file^>        Output file ^(auto-generated if not provided^)
echo.
echo Examples:
echo   # Add a new main module
echo   %0 module --name "Inventory Management" --description "Manage inventory and stock"
echo.
echo   # Add a sub-module
echo   %0 module --name "Product Categories" --parent INVENTORY --description "Manage product categories"
echo.
echo   # Add module access
echo   %0 access --module INVENTORY --name "View Inventory" --description "View inventory items"
echo.
echo   # Custom output location
echo   %0 module --name "HR Management" --output ./hr_module.sql
exit /b 0

:parse_module
set "NAME="
set "CODE="
set "PARENT="
set "DESCRIPTION="
set "LOGO="
set "PAGE="
set "SORT_ORDER="
set "IS_VISIBLE=1"
set "OUTPUT_FILE="

:module_parse_loop
if "%~1"=="" goto module_validate

if /i "%~1"=="-n" (
    set "NAME=%~2"
    shift
    shift
    goto module_parse_loop
)
if /i "%~1"=="--name" (
    set "NAME=%~2"
    shift
    shift
    goto module_parse_loop
)
if /i "%~1"=="-c" (
    set "CODE=%~2"
    shift
    shift
    goto module_parse_loop
)
if /i "%~1"=="--code" (
    set "CODE=%~2"
    shift
    shift
    goto module_parse_loop
)
if /i "%~1"=="-p" (
    set "PARENT=%~2"
    shift
    shift
    goto module_parse_loop
)
if /i "%~1"=="--parent" (
    set "PARENT=%~2"
    shift
    shift
    goto module_parse_loop
)
if /i "%~1"=="-d" (
    set "DESCRIPTION=%~2"
    shift
    shift
    goto module_parse_loop
)
if /i "%~1"=="--description" (
    set "DESCRIPTION=%~2"
    shift
    shift
    goto module_parse_loop
)
if /i "%~1"=="--logo" (
    set "LOGO=%~2"
    shift
    shift
    goto module_parse_loop
)
if /i "%~1"=="--page" (
    set "PAGE=%~2"
    shift
    shift
    goto module_parse_loop
)
if /i "%~1"=="--order" (
    set "SORT_ORDER=%~2"
    shift
    shift
    goto module_parse_loop
)
if /i "%~1"=="--hidden" (
    set "IS_VISIBLE=0"
    shift
    goto module_parse_loop
)
if /i "%~1"=="-o" (
    set "OUTPUT_FILE=%~2"
    shift
    shift
    goto module_parse_loop
)
if /i "%~1"=="--output" (
    set "OUTPUT_FILE=%~2"
    shift
    shift
    goto module_parse_loop
)

echo [ERROR] Unknown option: %~1
goto show_help

:module_validate
if "%NAME%"=="" (
    echo [ERROR] Module name is required
    goto show_help
)

goto generate_module

:parse_access
set "MODULE_CODE="
set "NAME="
set "CODE="
set "DESCRIPTION="
set "IS_VISIBLE=1"
set "OUTPUT_FILE="

:access_parse_loop
if "%~1"=="" goto access_validate

if /i "%~1"=="-m" (
    set "MODULE_CODE=%~2"
    shift
    shift
    goto access_parse_loop
)
if /i "%~1"=="--module" (
    set "MODULE_CODE=%~2"
    shift
    shift
    goto access_parse_loop
)
if /i "%~1"=="-n" (
    set "NAME=%~2"
    shift
    shift
    goto access_parse_loop
)
if /i "%~1"=="--name" (
    set "NAME=%~2"
    shift
    shift
    goto access_parse_loop
)
if /i "%~1"=="-c" (
    set "CODE=%~2"
    shift
    shift
    goto access_parse_loop
)
if /i "%~1"=="--code" (
    set "CODE=%~2"
    shift
    shift
    goto access_parse_loop
)
if /i "%~1"=="-d" (
    set "DESCRIPTION=%~2"
    shift
    shift
    goto access_parse_loop
)
if /i "%~1"=="--description" (
    set "DESCRIPTION=%~2"
    shift
    shift
    goto access_parse_loop
)
if /i "%~1"=="--hidden" (
    set "IS_VISIBLE=0"
    shift
    goto access_parse_loop
)
if /i "%~1"=="-o" (
    set "OUTPUT_FILE=%~2"
    shift
    shift
    goto access_parse_loop
)
if /i "%~1"=="--output" (
    set "OUTPUT_FILE=%~2"
    shift
    shift
    goto access_parse_loop
)

echo [ERROR] Unknown option: %~1
goto show_help

:access_validate
if "%MODULE_CODE%"=="" (
    echo [ERROR] Module code is required
    goto show_help
)
if "%NAME%"=="" (
    echo [ERROR] Access name is required
    goto show_help
)

goto generate_access

:generate_module
REM Auto-generate code if not provided
if "%CODE%"=="" (
    set "CODE=%NAME%"
    REM Convert to uppercase and replace non-alphanumeric with underscores
    for %%i in (a b c d e f g h i j k l m n o p q r s t u v w x y z) do (
        call set "CODE=%%CODE:%%i=%%i%%"
    )
    for %%i in (A B C D E F G H I J K L M N O P Q R S T U V W X Y Z) do (
        call set "CODE=%%CODE:%%i=%%i%%"
    )
    set "CODE=%CODE: =_%"
    set "CODE=%CODE:,=_%"
    set "CODE=%CODE:(=_%"
    set "CODE=%CODE:)=_%"
    set "CODE=%CODE:/=_%"
    set "CODE=%CODE:\=_%"
)

REM Default values
if "%LOGO%"=="" set "LOGO=module-icon.svg"
if "%PAGE%"=="" (
    set "PAGE=%CODE%"
    REM Convert to lowercase
    for %%i in (A B C D E F G H I J K L M N O P Q R S T U V W X Y Z) do (
        call set "PAGE=%%PAGE:%%i=%%i%%"
    )
    for %%i in (a b c d e f g h i j k l m n o p q r s t u v w x y z) do (
        call set "PAGE=%%PAGE:%%i=%%i%%"
    )
)
if "%DESCRIPTION%"=="" set "DESCRIPTION=%NAME% module"

REM Generate output filename if not provided
if "%OUTPUT_FILE%"=="" (
    REM Get next data file number
    set "HIGHEST_NUM=000"
    if exist "%SQL_DATA_DIR%\*.sql" (
        for /f "tokens=*" %%f in ('dir /b "%SQL_DATA_DIR%\*_module_*.sql" 2^>nul ^| sort') do (
            set "FILENAME=%%f"
            set "NUM=!FILENAME:~0,3!"
            if !NUM! gtr !HIGHEST_NUM! set "HIGHEST_NUM=!NUM!"
        )
        if "!HIGHEST_NUM!"=="000" (
            for /f "tokens=*" %%f in ('dir /b "%SQL_DATA_DIR%\*.sql" 2^>nul ^| sort') do (
                set "FILENAME=%%f"
                set "NUM=!FILENAME:~0,3!"
                if !NUM! gtr !HIGHEST_NUM! set "HIGHEST_NUM=!NUM!"
            )
        )
    )
    
    set /a "NEXT_NUM=%HIGHEST_NUM%+1"
    set "NEXT_NUM=000%NEXT_NUM%"
    set "NEXT_NUM=%NEXT_NUM:~-3%"
    
    REM Create safe filename from name
    set "SAFE_NAME=%NAME%"
    set "SAFE_NAME=%SAFE_NAME: =_%"
    set "SAFE_NAME=%SAFE_NAME:,=_%"
    set "SAFE_NAME=%SAFE_NAME:(=_%"
    set "SAFE_NAME=%SAFE_NAME:)=_%"
    for %%i in (A B C D E F G H I J K L M N O P Q R S T U V W X Y Z) do (
        call set "SAFE_NAME=%%SAFE_NAME:%%i=%%i%%"
    )
    for %%i in (a b c d e f g h i j k l m n o p q r s t u v w x y z) do (
        call set "SAFE_NAME=%%SAFE_NAME:%%i=%%i%%"
    )
    
    set "OUTPUT_FILE=%SQL_DATA_DIR%\%NEXT_NUM%_module_%SAFE_NAME%.sql"
)

REM Ensure output directory exists
if not exist "%SQL_DATA_DIR%" mkdir "%SQL_DATA_DIR%"

REM Generate SQL file
(
    echo -- Add Module: %NAME%
    echo -- Generated: %DATE% %TIME%
    echo -- Description: SQL script to add new module '%NAME%'
    echo.
    echo USE EmployeeDB;
    echo.
    echo DECLARE @ParentId INT = NULL;
    echo DECLARE @SortOrder INT;
    echo DECLARE @NewModuleId INT;
    echo DECLARE @SystemUserId INT = 1; -- Default system user
    echo.
) > "%OUTPUT_FILE%"

if not "%PARENT%"=="" (
    (
        echo -- Get parent module ID
        echo SELECT @ParentId = Id FROM Modules WHERE RefCode = '%PARENT%';
        echo.
        echo IF @ParentId IS NULL
        echo BEGIN
        echo     PRINT 'ERROR: Parent module with code ''%PARENT%'' not found!';
        echo     RETURN;
        echo END
        echo.
    ) >> "%OUTPUT_FILE%"
)

if "%SORT_ORDER%"=="" (
    (
        echo -- Calculate next sort order
        echo IF @ParentId IS NULL
        echo BEGIN
        echo     -- Main module: get next order from root level modules
        echo     SELECT @SortOrder = ISNULL^(MAX^(SortOrder^), 0^) + 1 
        echo     FROM Modules WHERE ParentId IS NULL;
        echo END
        echo ELSE
        echo BEGIN
        echo     -- Sub-module: get next order within parent
        echo     SELECT @SortOrder = ISNULL^(MAX^(SortOrder^), 0^) + 1 
        echo     FROM Modules WHERE ParentId = @ParentId;
        echo END
        echo.
    ) >> "%OUTPUT_FILE%"
) else (
    (
        echo -- Use specified sort order
        echo SET @SortOrder = %SORT_ORDER%;
        echo.
    ) >> "%OUTPUT_FILE%"
)

(
    echo -- Check if module already exists
    echo IF EXISTS ^(SELECT * FROM Modules WHERE RefCode = '%CODE%'^)
    echo BEGIN
    echo     PRINT 'WARNING: Module with code ''%CODE%'' already exists!';
    echo     SELECT Id, ModuleName, RefCode FROM Modules WHERE RefCode = '%CODE%';
    echo END
    echo ELSE
    echo BEGIN
    echo     -- Insert new module
    echo     INSERT INTO Modules ^(
    echo         ModuleName, 
    echo         ParentId, 
    echo         RefCode, 
    echo         IsVisible, 
    echo         LogoName, 
    echo         RedirectPage, 
    echo         SortOrder, 
    echo         Description, 
    echo         CreatedBy
    echo     ^)
    echo     VALUES ^(
    echo         '%NAME%',
    echo         @ParentId,
    echo         '%CODE%',
    echo         %IS_VISIBLE%,
    echo         '%LOGO%',
    echo         '%PAGE%',
    echo         @SortOrder,
    echo         '%DESCRIPTION%',
    echo         @SystemUserId
    echo     ^);
    echo.
    echo     SET @NewModuleId = SCOPE_IDENTITY^(^);
    echo.
    echo     PRINT 'SUCCESS: Module ''%NAME%'' created with ID: ' + CAST^(@NewModuleId AS NVARCHAR^(10^)^);
    echo     PRINT 'Reference Code: %CODE%';
    echo     PRINT 'Redirect Page: %PAGE%';
    echo.
    echo     -- Display the created module
    echo     SELECT 
    echo         Id,
    echo         ModuleName,
    echo         RefCode,
    echo         RedirectPage,
    echo         SortOrder,
    echo         IsVisible,
    echo         Description
    echo     FROM Modules 
    echo     WHERE Id = @NewModuleId;
    echo END
    echo.
    echo -- Note: Remember to add appropriate module accesses after creating the module
    echo -- Use: %0 access --module %CODE% --name "Access Name" --description "Access Description"
) >> "%OUTPUT_FILE%"

echo [SUCCESS] Module SQL generated: %OUTPUT_FILE%
goto end

:generate_access
REM Auto-generate code if not provided
if "%CODE%"=="" (
    REM Get first word from name as action
    for /f "tokens=1" %%i in ("%NAME%") do set "ACTION=%%i"
    REM Convert to uppercase
    for %%i in (a b c d e f g h i j k l m n o p q r s t u v w x y z) do (
        call set "ACTION=%%ACTION:%%i=%%i%%"
    )
    for %%i in (A B C D E F G H I J K L M N O P Q R S T U V W X Y Z) do (
        call set "ACTION=%%ACTION:%%i=%%i%%"
    )
    set "CODE=%MODULE_CODE%_%ACTION%"
)

REM Default values
if "%DESCRIPTION%"=="" set "DESCRIPTION=%NAME% for module %MODULE_CODE%"

REM Generate output filename if not provided
if "%OUTPUT_FILE%"=="" (
    REM Get next data file number
    set "HIGHEST_NUM=000"
    if exist "%SQL_DATA_DIR%\*.sql" (
        for /f "tokens=*" %%f in ('dir /b "%SQL_DATA_DIR%\*_access_*.sql" 2^>nul ^| sort') do (
            set "FILENAME=%%f"
            set "NUM=!FILENAME:~0,3!"
            if !NUM! gtr !HIGHEST_NUM! set "HIGHEST_NUM=!NUM!"
        )
        if "!HIGHEST_NUM!"=="000" (
            for /f "tokens=*" %%f in ('dir /b "%SQL_DATA_DIR%\*.sql" 2^>nul ^| sort') do (
                set "FILENAME=%%f"
                set "NUM=!FILENAME:~0,3!"
                if !NUM! gtr !HIGHEST_NUM! set "HIGHEST_NUM=!NUM!"
            )
        )
    )
    
    set /a "NEXT_NUM=%HIGHEST_NUM%+1"
    set "NEXT_NUM=000%NEXT_NUM%"
    set "NEXT_NUM=%NEXT_NUM:~-3%"
    
    REM Create safe filename from name and module
    set "SAFE_NAME=%NAME%"
    set "SAFE_NAME=%SAFE_NAME: =_%"
    set "SAFE_NAME=%SAFE_NAME:,=_%"
    set "SAFE_NAME=%SAFE_NAME:(=_%"
    set "SAFE_NAME=%SAFE_NAME:)=_%"
    for %%i in (A B C D E F G H I J K L M N O P Q R S T U V W X Y Z) do (
        call set "SAFE_NAME=%%SAFE_NAME:%%i=%%i%%"
    )
    for %%i in (a b c d e f g h i j k l m n o p q r s t u v w x y z) do (
        call set "SAFE_NAME=%%SAFE_NAME:%%i=%%i%%"
    )
    
    set "SAFE_MODULE=%MODULE_CODE%"
    for %%i in (A B C D E F G H I J K L M N O P Q R S T U V W X Y Z) do (
        call set "SAFE_MODULE=%%SAFE_MODULE:%%i=%%i%%"
    )
    for %%i in (a b c d e f g h i j k l m n o p q r s t u v w x y z) do (
        call set "SAFE_MODULE=%%SAFE_MODULE:%%i=%%i%%"
    )
    
    set "OUTPUT_FILE=%SQL_DATA_DIR%\%NEXT_NUM%_access_%SAFE_MODULE%_%SAFE_NAME%.sql"
)

REM Ensure output directory exists
if not exist "%SQL_DATA_DIR%" mkdir "%SQL_DATA_DIR%"

REM Generate SQL file
(
    echo -- Add Module Access: %NAME%
    echo -- Generated: %DATE% %TIME%
    echo -- Description: SQL script to add new module access '%NAME%' for module '%MODULE_CODE%'
    echo.
    echo USE EmployeeDB;
    echo.
    echo DECLARE @ModuleId INT;
    echo DECLARE @NewAccessId INT;
    echo DECLARE @SystemUserId INT = 1; -- Default system user
    echo.
    echo -- Get module ID
    echo SELECT @ModuleId = Id FROM Modules WHERE RefCode = '%MODULE_CODE%';
    echo.
    echo IF @ModuleId IS NULL
    echo BEGIN
    echo     PRINT 'ERROR: Module with code ''%MODULE_CODE%'' not found!';
    echo     PRINT 'Available modules:';
    echo     SELECT RefCode, ModuleName FROM Modules ORDER BY ModuleName;
    echo     RETURN;
    echo END
    echo.
    echo -- Check if access already exists
    echo IF EXISTS ^(SELECT * FROM ModuleAccesses WHERE RefCode = '%CODE%'^)
    echo BEGIN
    echo     PRINT 'WARNING: Module access with code ''%CODE%'' already exists!';
    echo     SELECT MA.Id, MA.ModuleAccessName, MA.RefCode, M.ModuleName
    echo     FROM ModuleAccesses MA
    echo     INNER JOIN Modules M ON MA.ModuleId = M.Id
    echo     WHERE MA.RefCode = '%CODE%';
    echo END
    echo ELSE
    echo BEGIN
    echo     -- Insert new module access
    echo     INSERT INTO ModuleAccesses ^(
    echo         ModuleId,
    echo         ModuleAccessName,
    echo         ParentId,
    echo         RefCode,
    echo         Description,
    echo         IsVisible,
    echo         CreatedBy
    echo     ^)
    echo     VALUES ^(
    echo         @ModuleId,
    echo         '%NAME%',
    echo         NULL,
    echo         '%CODE%',
    echo         '%DESCRIPTION%',
    echo         %IS_VISIBLE%,
    echo         @SystemUserId
    echo     ^);
    echo.
    echo     SET @NewAccessId = SCOPE_IDENTITY^(^);
    echo.
    echo     PRINT 'SUCCESS: Module access ''%NAME%'' created with ID: ' + CAST^(@NewAccessId AS NVARCHAR^(10^)^);
    echo     PRINT 'Reference Code: %CODE%';
    echo     PRINT 'Module: %MODULE_CODE%';
    echo.
    echo     -- Display the created access
    echo     SELECT 
    echo         MA.Id,
    echo         MA.ModuleAccessName,
    echo         MA.RefCode,
    echo         M.ModuleName AS ModuleName,
    echo         M.RefCode AS ModuleRefCode,
    echo         MA.Description,
    echo         MA.IsVisible
    echo     FROM ModuleAccesses MA
    echo     INNER JOIN Modules M ON MA.ModuleId = M.Id
    echo     WHERE MA.Id = @NewAccessId;
    echo END
    echo.
    echo -- Note: You may need to assign this access to user roles
) > "%OUTPUT_FILE%"

echo [SUCCESS] Access SQL generated: %OUTPUT_FILE%
goto end

:unknown_command
echo [ERROR] Unknown command: %COMMAND%
goto show_help

:end
endlocal