#!/bin/bash
# filepath: /home/poshan/projects/employee-management-system-backend/scripts/dev/dev_recreate_database.sh

# Employee Management System - Database Recreation Script
# This script drops the existing database and recreates it with all migrations using Docker sqlcmd
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
MIGRATIONS_SCRIPT="${SCRIPT_DIR}/dev_run_migrations.sh"

# Function to run sqlcmd via Docker
run_sqlcmd() {
    # Try host network first, fallback to host.docker.internal if that fails
    if docker run --rm --network host "$SQLCMD_IMAGE" /opt/mssql-tools/bin/sqlcmd "$@" 2>/dev/null; then
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
        docker run --rm "$SQLCMD_IMAGE" /opt/mssql-tools/bin/sqlcmd "${args[@]}"
    fi
}

echo "Employee Management System - Database Recreation Tool"
echo "====================================================="
echo
echo "⚠️  WARNING: This will completely destroy the existing database!"
echo "⚠️  All data in 'EmployeeDB' will be permanently lost!"
echo
echo "Connection Parameters:"
echo "Server: $SERVER"
echo "Username: $USERNAME"
echo "Password: [HIDDEN]"
echo "Timeout: $TIMEOUT seconds"
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

# Check if migration script exists
if [[ ! -f "$MIGRATIONS_SCRIPT" ]]; then
    echo "Error: Migration script not found: $MIGRATIONS_SCRIPT"
    echo "Please make sure the dev_run_migrations.sh script exists in the same directory."
    exit 1
fi

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

# Final confirmation
echo "FINAL CONFIRMATION"
echo "This action will:"
echo "  1. Drop the 'EmployeeDB' database (if it exists)"
echo "  2. Create a fresh 'EmployeeDB' database"
echo "  3. Run all migrations to set up the schema"
echo
read -p "Are you absolutely sure you want to proceed? Type 'y' to continue: " final_confirm

if [[ "$final_confirm" != "y" ]]; then
    echo "Operation cancelled by user"
    exit 0
fi

echo
echo "Starting database recreation process..."
echo

# Step 1: Check if database exists and drop it
echo "Step 1: Checking for existing database..."
database_exists_result=$(run_sqlcmd -S "$SERVER" -U "$USERNAME" -P "$PASSWORD" -Q "IF EXISTS(SELECT name FROM sys.databases WHERE name = 'EmployeeDB') SELECT 1 ELSE SELECT 0" -h -1 -W 2>/dev/null | grep -E "^[01]$")

if [[ "$database_exists_result" == "1" ]]; then
    echo "Existing 'EmployeeDB' database found - dropping it..."
    
    # Kill any active connections to the database
    echo "Terminating active connections..."
    run_sqlcmd -S "$SERVER" -U "$USERNAME" -P "$PASSWORD" -Q "
    USE master;
    ALTER DATABASE EmployeeDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE EmployeeDB;
    " -t "$TIMEOUT"
    
    if [[ $? -eq 0 ]]; then
        echo "Database dropped successfully"
    else
        echo "Error: Failed to drop database"
        exit 1
    fi
else
    echo "No existing 'EmployeeDB' database found - proceeding with creation"
fi

# Step 2: Create fresh database
echo
echo "Step 2: Creating fresh database..."
run_sqlcmd -S "$SERVER" -U "$USERNAME" -P "$PASSWORD" -Q "
CREATE DATABASE EmployeeDB
COLLATE SQL_Latin1_General_CP1_CI_AS;
" -t "$TIMEOUT"

if [[ $? -eq 0 ]]; then
    echo "Fresh database created successfully"
else
    echo "Error: Failed to create database"
    exit 1
fi

# Step 3: Run migrations
echo
echo "Step 3: Running all migrations..."
echo "Executing migration script: $MIGRATIONS_SCRIPT"
echo

# Make sure the migration script is executable
chmod +x "$MIGRATIONS_SCRIPT"

# Update the migration script's connection parameters to match ours
# Create a temporary script with updated parameters and preserve the original SCRIPT_DIR
temp_migration_script="/tmp/temp_migrations_$(date +%s).sh"
sed "s/^SERVER=.*/SERVER=\"$SERVER\"/; s/^USERNAME=.*/USERNAME=\"$USERNAME\"/; s/^PASSWORD=.*/PASSWORD=\"$PASSWORD\"/; s/^TIMEOUT=.*/TIMEOUT=$TIMEOUT/; s|^SCRIPT_DIR=.*|SCRIPT_DIR=\"$SCRIPT_DIR\"|; s|^SQLCMD_IMAGE=.*|SQLCMD_IMAGE=\"$SQLCMD_IMAGE\"|" "$MIGRATIONS_SCRIPT" > "$temp_migration_script"
chmod +x "$temp_migration_script"

# Run the migration script non-interactively by piping 'y' to it
echo "y" | "$temp_migration_script"
migration_exit_code=$?

# Clean up temporary script
rm -f "$temp_migration_script"

echo
if [[ $migration_exit_code -eq 0 ]]; then
    echo "SUCCESS! Database recreation completed successfully!"
    echo
    echo "Summary:"
    echo "Old database dropped (if existed)"
    echo "Fresh database created"
    echo "All migrations applied"
    echo
    echo "Your 'EmployeeDB' database is now ready for use with a clean schema."
else
    echo "FAILED! Migration process encountered errors."
    echo
    echo "The database was created but migrations failed."
    echo "Please check the migration output above for details."
    exit 1
fi

exit 0