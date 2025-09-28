#!/bin/bash

# Migration Generator Script
# Creates a new migration file with the correct naming convention and template

set -e

SCRIPT_DIR="$(dirname "$0")"
MIGRATIONS_DIR="$SCRIPT_DIR/sql/migrations"

# Color codes and print functions
BLUE='\033[0;34m'
GREEN='\033[0;32m'
RED='\033[0;31m'
NC='\033[0m'
print_info() {
    echo -e "${BLUE}[INFO]${NC} $1"
}
print_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"
}
print_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}
print_empty_line() {
    echo ""
}

# Parse command line arguments
DESCRIPTION=""
FROM_DATA=false

while [[ $# -gt 0 ]]; do
    case $1 in
        --from-data)
            FROM_DATA=true
            shift
            ;;
        -h|--help)
            echo "Usage: $0 [options] <description>"
            echo ""
            echo "Options:"
            echo "  --from-data    Create migration from files in sql/data folder"
            echo "  -h, --help     Show this help message"
            echo ""
            echo "Examples:"
            echo "  $0 \"add employee table\""
            echo "  $0 --from-data \"add modules and permissions\""
            exit 0
            ;;
        *)
            if [ -z "$DESCRIPTION" ]; then
                DESCRIPTION="$1"
            else
                print_error "Multiple descriptions provided. Use quotes for descriptions with spaces."
                exit 1
            fi
            shift
            ;;
    esac
done

# Check if description is provided (unless using --from-data)
if [ -z "$DESCRIPTION" ]; then
    if [ "$FROM_DATA" = false ]; then
        print_error "Description is required"
        echo ""
        echo "Usage: $0 [options] <description>"
        echo "Use --help for more information"
        exit 1
    else
        DESCRIPTION="add modules and permissions from data files"
    fi
fi

# Create migrations directory if it doesn't exist
mkdir -p "$MIGRATIONS_DIR"

# Create data directory if it doesn't exist
DATA_DIR="$SCRIPT_DIR/sql/data"
mkdir -p "$DATA_DIR"

# Get the next migration number
LAST_MIGRATION=$(ls "$MIGRATIONS_DIR"/*.sql 2>/dev/null | sort -V | tail -n 1 | sed 's/.*\/\([0-9]\{3\}\).*/\1/' || echo "000")
NEXT_NUMBER=$(printf "%03d" $((10#$LAST_MIGRATION + 1)))

# Create filename from description (replace spaces with underscores, lowercase)
FILENAME_DESC=$(echo "$DESCRIPTION" | tr '[:upper:]' '[:lower:]' | sed 's/[^a-zA-Z0-9]/_/g' | sed 's/__*/_/g' | sed 's/^_\|_$//g')
FILENAME="${NEXT_NUMBER}_${FILENAME_DESC}.sql"
FILEPATH="$MIGRATIONS_DIR/$FILENAME"

if [ "$FROM_DATA" = true ]; then
    # Check if data directory has files
    DATA_FILES=($(find "$DATA_DIR" -name "*.sql" 2>/dev/null | sort))
    
    if [ ${#DATA_FILES[@]} -eq 0 ]; then
        print_error "No SQL files found in $DATA_DIR"
        print_info "Use generate_module_sql.sh to create module/access files first"
        exit 1
    fi
    
    print_info "Found ${#DATA_FILES[@]} data file(s) to process:"
    for file in "${DATA_FILES[@]}"; do
        echo "  - $(basename "$file")"
    done
    print_empty_line
    
    # Create migration with data files content
    cat > "$FILEPATH" << EOF
-- Migration $NEXT_NUMBER: $DESCRIPTION
-- Created: $(date +%Y-%m-%d)
-- Description: $DESCRIPTION
-- Generated from data files in sql/data/

USE EmployeeDB;

EOF

    # Process each data file
    for data_file in "${DATA_FILES[@]}"; do
        filename=$(basename "$data_file")
        print_info "Processing: $filename"
        
        cat >> "$FILEPATH" << EOF
-- ============================================================================
-- Content from: $filename
-- ============================================================================

EOF
        
        # Extract the SQL content (skip comments and USE statements)
        grep -v "^--" "$data_file" | grep -v "^USE " | grep -v "^$" >> "$FILEPATH"
        
        cat >> "$FILEPATH" << EOF
GO

EOF
    done
    
    # Add migration tracking
    cat >> "$FILEPATH" << EOF

-- Record this migration
IF NOT EXISTS (SELECT * FROM DbVersions WHERE Version = '$NEXT_NUMBER')
BEGIN
    INSERT INTO DbVersions (Version, Description) 
    VALUES ('$NEXT_NUMBER', '$DESCRIPTION');
END

PRINT 'Migration $NEXT_NUMBER completed successfully.';
EOF

    # Delete processed data files
    print_info "Cleaning up data files..."
    for data_file in "${DATA_FILES[@]}"; do
        rm "$data_file"
        print_success "Deleted: $(basename "$data_file")"
    done
    
else
    # Create standard migration template
    cat > "$FILEPATH" << EOF
-- Migration $NEXT_NUMBER: $DESCRIPTION
-- Created: $(date +%Y-%m-%d)
-- Description: $DESCRIPTION

USE EmployeeDB;

-- Add your SQL commands here
-- Example:
-- ALTER TABLE Users ADD NewColumn NVARCHAR(100) NULL;
-- CREATE TABLE NewTable (
--     Id INT IDENTITY(1,1) PRIMARY KEY,
--     Name NVARCHAR(100) NOT NULL
-- );

-- Record this migration
IF NOT EXISTS (SELECT * FROM DbVersions WHERE Version = '$NEXT_NUMBER')
BEGIN
    INSERT INTO DbVersions (Version, Description) 
    VALUES ('$NEXT_NUMBER', '$DESCRIPTION');
END

PRINT 'Migration $NEXT_NUMBER completed successfully.';
EOF
fi

print_success "Migration file created: $FILENAME"
print_info "File path: $FILEPATH"
print_empty_line

if [ "$FROM_DATA" = true ]; then
    print_info "Migration created from ${#DATA_FILES[@]} data file(s)"
    print_info "Data files have been cleaned up"
    print_info "Next steps:"
    print_info "1. Review the migration file: $FILEPATH"
    print_info "2. Run migrations with: run_migrations.bat"
else
    print_info "Next steps:"
    print_info "1. Edit the file and add your SQL commands"
    print_info "2. Run migrations with: run_migrations.bat"
fi