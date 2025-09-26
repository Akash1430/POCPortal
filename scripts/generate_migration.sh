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

# Check if description is provided
if [ $# -eq 0 ]; then
    echo "Usage: $0 <description>"
    echo ""
    echo "Examples:"
    echo "  $0 \"add employee table\""
    echo "  $0 \"modify module table\""
    echo "  $0 \"remove obsolete columns\""
    exit 1
fi

DESCRIPTION="$1"

# Create migrations directory if it doesn't exist
mkdir -p "$MIGRATIONS_DIR"

# Get the next migration number
LAST_MIGRATION=$(ls "$MIGRATIONS_DIR"/*.sql 2>/dev/null | sort -V | tail -n 1 | sed 's/.*\/\([0-9]\{3\}\).*/\1/' || echo "000")
NEXT_NUMBER=$(printf "%03d" $((10#$LAST_MIGRATION + 1)))

# Create filename from description (replace spaces with underscores, lowercase)
FILENAME_DESC=$(echo "$DESCRIPTION" | tr '[:upper:]' '[:lower:]' | sed 's/[^a-zA-Z0-9]/_/g' | sed 's/__*/_/g' | sed 's/^_\|_$//g')
FILENAME="${NEXT_NUMBER}_${FILENAME_DESC}.sql"
FILEPATH="$MIGRATIONS_DIR/$FILENAME"

# Create the migration file with template
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

print_success "Migration file created: $FILENAME"
print_info "File path: $FILEPATH"
print_empty_line
print_info "Next steps:"
print_info "Edit the file and add your SQL commands"