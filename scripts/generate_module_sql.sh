#!/bin/bash

# Generate Module and Access SQL Scripts
# Creates SQL scripts for adding new modules and module accesses

set -e

SCRIPT_DIR="$(dirname "$0")"
SQL_DATA_DIR="$SCRIPT_DIR/sql/data"

# Color codes and print functions
GREEN='\033[0;32m'
BLUE='\033[0;34m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m'
print_info() {
    echo -e "${BLUE}[INFO]${NC} $1"
}
print_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"
}
print_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}
print_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Function to show help
show_help() {
    cat << EOF
Generate Module and Access SQL Scripts

Usage: $0 <command> [options]

Commands:
  module          Generate SQL to add a new module
  access          Generate SQL to add new module access

Module Options:
  -n, --name <name>           Module name (required)
  -c, --code <code>           Module reference code (auto-generated if not provided)
  -p, --parent <parent_code>  Parent module code (for sub-modules)
  -d, --description <desc>    Module description
  --logo <logo>              Logo name (default: module-icon)
  --page <page>              Redirect page (default: /{code})
  --order <order>            Sort order (auto-calculated if not provided)
  --hidden                   Make module hidden (not visible in menu)
  -o, --output <file>        Output file (auto-generated if not provided)

Access Options:
  -m, --module <code>         Module reference code (required)
  -n, --name <name>           Access name (required)
  -c, --code <code>           Access reference code (auto-generated if not provided)
  -d, --description <desc>    Access description
  --hidden                   Make access hidden
  -o, --output <file>        Output file (auto-generated if not provided)

Examples:
  # Add a new main module
  $0 module --name "Inventory Management" --description "Manage inventory and stock"

  # Add a sub-module
  $0 module --name "Product Categories" --parent INVENTORY --description "Manage product categories"

  # Add module access
  $0 access --module INVENTORY --name "View Inventory" --description "View inventory items"

  # Custom output location
  $0 module --name "HR Management" --output ./hr_module.sql
EOF
}

# Function to convert string to reference code
string_to_code() {
    echo "$1" | tr '[:lower:]' '[:upper:]' | sed 's/[^A-Z0-9]/_/g' | sed 's/__*/_/g' | sed 's/^_\|_$//g'
}

# Function to generate timestamp for filename
generate_timestamp() {
    date '+%Y%m%d_%H%M%S'
}

# Function to get next data file number
get_next_data_number() {
    local prefix="$1"
    local existing_files=($(find "$SQL_DATA_DIR" -name "*_${prefix}_*.sql" | sort))
    
    if [ ${#existing_files[@]} -eq 0 ]; then
        # Find the highest numbered data file
        local highest=$(find "$SQL_DATA_DIR" -name "[0-9][0-9][0-9]_*.sql" | sed 's/.*\/\([0-9][0-9][0-9]\)_.*/\1/' | sort -n | tail -n 1)
        if [ -z "$highest" ]; then
            echo "002"  # Start from 002 since 001 is initial_data
        else
            printf "%03d" $((10#$highest + 1))
        fi
    else
        # Extract numbers from existing files with same prefix
        local highest=$(echo "${existing_files[@]}" | tr ' ' '\n' | sed 's/.*\/\([0-9][0-9][0-9]\)_.*/\1/' | sort -n | tail -n 1)
        if [ -z "$highest" ]; then
            echo "002"
        else
            printf "%03d" $((10#$highest + 1))
        fi
    fi
}

# Function to generate module SQL
generate_module_sql() {
    local name="$1"
    local code="$2"
    local parent_code="$3"
    local description="$4"
    local logo="$5"
    local page="$6"
    local sort_order="$7"
    local is_visible="$8"
    local output_file="$9"
    
    # Auto-generate code if not provided
    if [ -z "$code" ]; then
        code=$(string_to_code "$name")
    fi
    
    # Default values
    if [ -z "$logo" ]; then
        logo="module-icon"
    fi
    
    if [ -z "$page" ]; then
        page="/$(echo "$code" | tr '[:upper:]' '[:lower:]')"
    fi
    
    if [ -z "$description" ]; then
        description="$name module"
    fi
    
    if [ -z "$is_visible" ]; then
        is_visible="1"
    fi
    
    # Generate output filename if not provided
    if [ -z "$output_file" ]; then
        local file_number=$(get_next_data_number "module")
        local safe_name=$(echo "$name" | tr '[:upper:]' '[:lower:]' | sed 's/[^a-z0-9]/_/g' | sed 's/__*/_/g' | sed 's/^_\|_$//g')
        output_file="$SQL_DATA_DIR/${file_number}_module_${safe_name}.sql"
    fi
    
    # Ensure output directory exists
    mkdir -p "$(dirname "$output_file")"
    
    # Generate SQL
    cat > "$output_file" << EOF
-- Add Module: $name
-- Generated: $(date '+%Y-%m-%d %H:%M:%S')
-- Description: SQL script to add new module '$name'

USE EmployeeDB;

DECLARE @ParentId INT = NULL;
DECLARE @SortOrder INT;
DECLARE @NewModuleId INT;
DECLARE @SystemUserId INT = 1; -- Default system user

EOF

    if [ -n "$parent_code" ]; then
        cat >> "$output_file" << EOF
-- Get parent module ID
SELECT @ParentId = Id FROM Modules WHERE RefCode = '$parent_code';

IF @ParentId IS NULL
BEGIN
    PRINT 'ERROR: Parent module with code ''$parent_code'' not found!';
    RETURN;
END

EOF
    fi

    if [ -z "$sort_order" ]; then
        cat >> "$output_file" << EOF
-- Calculate next sort order
IF @ParentId IS NULL
BEGIN
    -- Main module: get next order from root level modules
    SELECT @SortOrder = ISNULL(MAX(SortOrder), 0) + 1 
    FROM Modules WHERE ParentId IS NULL;
END
ELSE
BEGIN
    -- Sub-module: get next order within parent
    SELECT @SortOrder = ISNULL(MAX(SortOrder), 0) + 1 
    FROM Modules WHERE ParentId = @ParentId;
END

EOF
    else
        cat >> "$output_file" << EOF
-- Use specified sort order
SET @SortOrder = $sort_order;

EOF
    fi

    cat >> "$output_file" << EOF
-- Check if module already exists
IF EXISTS (SELECT * FROM Modules WHERE RefCode = '$code')
BEGIN
    PRINT 'WARNING: Module with code ''$code'' already exists!';
    SELECT Id, ModuleName, RefCode FROM Modules WHERE RefCode = '$code';
END
ELSE
BEGIN
    -- Insert new module
    INSERT INTO Modules (
        ModuleName, 
        ParentId, 
        RefCode, 
        IsVisible, 
        LogoName, 
        RedirectPage, 
        SortOrder, 
        Description, 
        CreatedBy
    )
    VALUES (
        '$name',
        @ParentId,
        '$code',
        $is_visible,
        '$logo',
        '$page',
        @SortOrder,
        '$description',
        @SystemUserId
    );
    
    SET @NewModuleId = SCOPE_IDENTITY();
    
    PRINT 'SUCCESS: Module ''$name'' created with ID: ' + CAST(@NewModuleId AS NVARCHAR(10));
    PRINT 'Reference Code: $code';
    PRINT 'Redirect Page: $page';
    
    -- Display the created module
    SELECT 
        Id,
        ModuleName,
        RefCode,
        RedirectPage,
        SortOrder,
        IsVisible,
        Description
    FROM Modules 
    WHERE Id = @NewModuleId;
END

-- Note: Remember to add appropriate module accesses after creating the module
-- Use: $0 access --module $code --name "Access Name" --description "Access Description"
EOF

    echo "$output_file"
}

# Function to generate access SQL
generate_access_sql() {
    local module_code="$1"
    local name="$2"
    local code="$3"
    local description="$4"
    local is_visible="$5"
    local output_file="$6"
    
    # Auto-generate code if not provided
    if [ -z "$code" ]; then
        local action=$(echo "$name" | cut -d' ' -f1 | tr '[:lower:]' '[:upper:]')
        code="${module_code}_${action}"
    fi
    
    # Default values
    if [ -z "$description" ]; then
        description="$name for module $module_code"
    fi
    
    if [ -z "$is_visible" ]; then
        is_visible="1"
    fi
    
    # Generate output filename if not provided
    if [ -z "$output_file" ]; then
        local file_number=$(get_next_data_number "access")
        local safe_name=$(echo "$name" | tr '[:upper:]' '[:lower:]' | sed 's/[^a-z0-9]/_/g' | sed 's/__*/_/g' | sed 's/^_\|_$//g')
        local safe_module=$(echo "$module_code" | tr '[:upper:]' '[:lower:]')
        output_file="$SQL_DATA_DIR/${file_number}_access_${safe_module}_${safe_name}.sql"
    fi
    
    # Ensure output directory exists
    mkdir -p "$(dirname "$output_file")"
    
    # Generate SQL
    cat > "$output_file" << EOF
-- Add Module Access: $name
-- Generated: $(date '+%Y-%m-%d %H:%M:%S')
-- Description: SQL script to add new module access '$name' for module '$module_code'

USE EmployeeDB;

DECLARE @ModuleId INT;
DECLARE @NewAccessId INT;
DECLARE @SystemUserId INT = 1; -- Default system user

-- Get module ID
SELECT @ModuleId = Id FROM Modules WHERE RefCode = '$module_code';

IF @ModuleId IS NULL
BEGIN
    PRINT 'ERROR: Module with code ''$module_code'' not found!';
    PRINT 'Available modules:';
    SELECT RefCode, ModuleName FROM Modules ORDER BY ModuleName;
    RETURN;
END

-- Check if access already exists
IF EXISTS (SELECT * FROM ModuleAccesses WHERE RefCode = '$code')
BEGIN
    PRINT 'WARNING: Module access with code ''$code'' already exists!';
    SELECT MA.Id, MA.ModuleAccessName, MA.RefCode, M.ModuleName
    FROM ModuleAccesses MA
    INNER JOIN Modules M ON MA.ModuleId = M.Id
    WHERE MA.RefCode = '$code';
END
ELSE
BEGIN
    -- Insert new module access
    INSERT INTO ModuleAccesses (
        ModuleId,
        ModuleAccessName,
        ParentId,
        RefCode,
        Description,
        IsVisible,
        CreatedBy
    )
    VALUES (
        @ModuleId,
        '$name',
        NULL,
        '$code',
        '$description',
        $is_visible,
        @SystemUserId
    );
    
    SET @NewAccessId = SCOPE_IDENTITY();
    
    PRINT 'SUCCESS: Module access ''$name'' created with ID: ' + CAST(@NewAccessId AS NVARCHAR(10));
    PRINT 'Reference Code: $code';
    PRINT 'Module: $module_code';
    
    -- Display the created access
    SELECT 
        MA.Id,
        MA.ModuleAccessName,
        MA.RefCode,
        M.ModuleName AS ModuleName,
        M.RefCode AS ModuleRefCode,
        MA.Description,
        MA.IsVisible
    FROM ModuleAccesses MA
    INNER JOIN Modules M ON MA.ModuleId = M.Id
    WHERE MA.Id = @NewAccessId;
END

-- Note: You may need to assign this access to user roles
EOF

    echo "$output_file"
}

if [ $# -eq 0 ]; then
    show_help
    exit 0
fi

COMMAND="$1"
shift

case "$COMMAND" in
    module)
        # Parse module arguments
        NAME=""
        CODE=""
        PARENT=""
        DESCRIPTION=""
        LOGO=""
        PAGE=""
        SORT_ORDER=""
        IS_VISIBLE="1"
        OUTPUT_FILE=""
        
        while [[ $# -gt 0 ]]; do
            case $1 in
                -n|--name)
                    NAME="$2"
                    shift 2
                    ;;
                -c|--code)
                    CODE="$2"
                    shift 2
                    ;;
                -p|--parent)
                    PARENT="$2"
                    shift 2
                    ;;
                -d|--description)
                    DESCRIPTION="$2"
                    shift 2
                    ;;
                --logo)
                    LOGO="$2"
                    shift 2
                    ;;
                --page)
                    PAGE="$2"
                    shift 2
                    ;;
                --order)
                    SORT_ORDER="$2"
                    shift 2
                    ;;
                --hidden)
                    IS_VISIBLE="0"
                    shift
                    ;;
                -o|--output)
                    OUTPUT_FILE="$2"
                    shift 2
                    ;;
                *)
                    print_error "Unknown option: $1"
                    show_help
                    exit 1
                    ;;
            esac
        done
        
        if [ -z "$NAME" ]; then
            print_error "Module name is required"
            show_help
            exit 1
        fi
        
        output_file=$(generate_module_sql "$NAME" "$CODE" "$PARENT" "$DESCRIPTION" "$LOGO" "$PAGE" "$SORT_ORDER" "$IS_VISIBLE" "$OUTPUT_FILE")
        print_success "Module SQL generated: $output_file"
        ;;
        
    access)
        # Parse access arguments
        MODULE_CODE=""
        NAME=""
        CODE=""
        DESCRIPTION=""
        IS_VISIBLE="1"
        OUTPUT_FILE=""
        
        while [[ $# -gt 0 ]]; do
            case $1 in
                -m|--module)
                    MODULE_CODE="$2"
                    shift 2
                    ;;
                -n|--name)
                    NAME="$2"
                    shift 2
                    ;;
                -c|--code)
                    CODE="$2"
                    shift 2
                    ;;
                -d|--description)
                    DESCRIPTION="$2"
                    shift 2
                    ;;
                --hidden)
                    IS_VISIBLE="0"
                    shift
                    ;;
                -o|--output)
                    OUTPUT_FILE="$2"
                    shift 2
                    ;;
                *)
                    print_error "Unknown option: $1"
                    show_help
                    exit 1
                    ;;
            esac
        done
        
        if [ -z "$MODULE_CODE" ]; then
            print_error "Module code is required"
            show_help
            exit 1
        fi
        
        if [ -z "$NAME" ]; then
            print_error "Access name is required"
            show_help
            exit 1
        fi
        
        output_file=$(generate_access_sql "$MODULE_CODE" "$NAME" "$CODE" "$DESCRIPTION" "$IS_VISIBLE" "$OUTPUT_FILE")
        print_success "Access SQL generated: $output_file"
        ;;
        
    --help|help)
        show_help
        ;;
        
    *)
        print_error "Unknown command: $COMMAND"
        show_help
        exit 1
        ;;
esac