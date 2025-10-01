#!/bin/bash
# filepath: /home/poshan/projects/employee-management-system-backend/scripts/dev/dev_run_migrations.sh

# Employee Management System - Database Migration Runner
# This bash script connects to SQL Server and runs migration scripts in order using Docker sqlcmd
# 
# CONFIGURATION: Edit the variables below to match your setup
# ============================================================

# SQL Server connection parameters - EDIT THESE VALUES
SERVER="127.0.0.1"
USERNAME="sa"
PASSWORD="YourPassword123!"
TIMEOUT=30

# Docker image for SQL Server tools
SQLCMD_IMAGE="mcr.microsoft.com/mssql-tools"

# Script directory - resolve full path to work from any directory
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
MIGRATIONS_DIR="${SCRIPT_DIR}/../sql/migrations"

# Function to run sqlcmd via Docker
run_sqlcmd() {
    # Try host network first, fallback to host.docker.internal if that fails
    if docker run --rm --network host -v "$MIGRATIONS_DIR:/migrations" "$SQLCMD_IMAGE" /opt/mssql-tools/bin/sqlcmd "$@" 2>/dev/null; then
        return 0
    else
        # Fallback: replace 127.0.0.1/localhost with host.docker.internal
        local args=("$@")
        for i in "${!args[@]}"; do
            if [[ "${args[i]}" == "-S" && "${args[i+1]}" =~ ^(127\.0\.0\.1|localhost) ]]; then
                args[i+1]="host.docker.internal"
                break
            fi
        done
        docker run --rm -v "$MIGRATIONS_DIR:/migrations" "$SQLCMD_IMAGE" /opt/mssql-tools/bin/sqlcmd "${args[@]}"
    fi
}

echo "Employee Management System - Migration Runner"
echo

# Validate required parameters
if [[ -z "$SERVER" ]]; then
    echo "Error: Server name is required"
    echo "Please edit the SERVER variable in this script"
    exit 1
fi

if [[ -z "$USERNAME" ]]; then
    echo "Error: Username is required"
    echo "Please edit the USERNAME variable in this script"
    exit 1
fi

if [[ -z "$PASSWORD" ]]; then
    echo "Error: Password is required"
    echo "Please edit the PASSWORD variable in this script"
    exit 1
fi

# Check if migrations directory exists
if [[ ! -d "$MIGRATIONS_DIR" ]]; then
    echo "Error: Migrations directory not found: $MIGRATIONS_DIR"
    echo "Please make sure you're running this script from the correct location."
    exit 1
fi

echo "Connection Parameters:"
echo "Server: $SERVER"
echo "Username: $USERNAME"
echo "Password: [HIDDEN]"
echo "Timeout: $TIMEOUT seconds"
echo "Migrations Directory: $MIGRATIONS_DIR"
echo

# Test connection
echo "Testing SQL Server connection..."
echo "Pulling Docker image if not available..."
docker pull "$SQLCMD_IMAGE" >/dev/null 2>&1

if ! run_sqlcmd -S "$SERVER" -U "$USERNAME" -P "$PASSWORD" -Q "SELECT 1 as ConnectionTest" -t "$TIMEOUT" >/dev/null 2>&1; then
    echo "Error: Failed to connect to SQL Server"
    echo "Please check your connection parameters and ensure SQL Server is running."
    echo "Also ensure Docker is running and can access the host network."
    exit 1
fi
echo "Connection successful!"
echo

# Get list of migration files sorted by name
echo "Scanning for migration files..."
migration_files=()
if [[ -d "$MIGRATIONS_DIR" ]]; then
    while IFS= read -r -d '' file; do
        migration_files+=("$(basename "$file")")
    done < <(find "$MIGRATIONS_DIR" -name "*.sql" -type f -print0 | sort -z)
fi

migration_count=${#migration_files[@]}

if [[ $migration_count -eq 0 ]]; then
    echo "No migration files found in $MIGRATIONS_DIR"
    exit 0
fi

echo "Found $migration_count migration file(s):"
for i in "${!migration_files[@]}"; do
    echo "  $((i+1)). ${migration_files[i]}"
done
echo

# Check if target database exists
echo "Checking database existence..."
database_exists_result=$(run_sqlcmd -S "$SERVER" -U "$USERNAME" -P "$PASSWORD" -Q "IF EXISTS(SELECT name FROM sys.databases WHERE name = 'EmployeeDB') SELECT 1 ELSE SELECT 0" -h -1 -W 2>/dev/null | grep -E "^[01]$")

if [[ "$database_exists_result" != "1" ]]; then
    echo "Target database 'EmployeeDB' does not exist - will run all migrations"
    # If database doesn't exist, all migrations are pending
    pending_migrations=("${migration_files[@]}")
else
    echo "Database 'EmployeeDB' exists - checking migration status..."
    
    # Build list of pending migrations by checking database directly
    pending_migrations=()
    for file in "${migration_files[@]}"; do
        # Extract version number from filename (first 3 characters)
        version_num="${file:0:3}"
        
        # Check if this version already exists in database
        already_run_result=$(run_sqlcmd -S "$SERVER" -U "$USERNAME" -P "$PASSWORD" -Q "IF OBJECT_ID('EmployeeDB.dbo.DbVersions', 'U') IS NOT NULL AND EXISTS(SELECT 1 FROM EmployeeDB.dbo.DbVersions WHERE Version='$version_num') SELECT 1 ELSE SELECT 0" -h -1 -W 2>/dev/null | grep -E "^[01]$")
        
        if [[ "$already_run_result" != "1" ]]; then
            pending_migrations+=("$file")
        else
            echo "Skipping $file - already applied"
        fi
    done
fi

pending_count=${#pending_migrations[@]}
applied_count=$((migration_count - pending_count))

echo
echo "Migration Status Summary:"
echo "  Total migrations found: $migration_count"
echo "  Already applied: $applied_count"
echo "  Pending to run: $pending_count"
echo

if [[ $pending_count -eq 0 ]]; then
    echo "All migrations are already up to date!"
    echo "No pending migrations to run."
    exit 0
fi

echo "Found $pending_count pending migration(s) to run:"
for i in "${!pending_migrations[@]}"; do
    echo "  $((i+1)). ${pending_migrations[i]}"
done
echo

# Confirm execution of pending migrations
read -p "Do you want to run these $pending_count pending migrations? (y/N): " confirm
if [[ ! "$confirm" =~ ^[Yy]$ ]]; then
    echo "Migration cancelled by user"
    exit 0
fi
echo

# Execute pending migrations only
echo "Starting migration execution..."
echo

success_count=0
error_count=0

for i in "${!pending_migrations[@]}"; do
    current_file="${pending_migrations[i]}"
    # Use Docker volume path for the migration file
    docker_file_path="/migrations/$current_file"
    
    echo "Running migration $((i+1)) of $pending_count: $current_file"
    
    # Execute the migration using Docker volume
    if run_sqlcmd -S "$SERVER" -U "$USERNAME" -P "$PASSWORD" -i "$docker_file_path" -t "$TIMEOUT"; then
        echo "Migration $current_file completed successfully"
        ((success_count++))
    else
        echo "Error: Migration $current_file failed"
        ((error_count++))
        
        # Ask if user wants to continue
        read -p "Continue with remaining migrations? (y/N): " continue_choice
        if [[ ! "$continue_choice" =~ ^[Yy]$ ]]; then
            echo "Migration process stopped by user"
            break
        fi
    fi
    echo
done

echo
echo "Migration Summary:"
echo "  Successful: $success_count"
echo "  Failed: $error_count"
echo "  Pending: $pending_count"
echo "  Total Available: $migration_count"

if [[ $error_count -gt 0 ]]; then
    echo
    echo "Some migrations failed. Please check the error messages above."
    exit 1
else
    echo
    echo "All migrations completed successfully!"
    echo "Your database is now ready for use."
fi

exit 0