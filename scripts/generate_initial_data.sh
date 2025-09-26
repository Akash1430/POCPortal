#!/bin/bash

# Generate initial data SQL script
# Creates or updates the SQL file with custom admin credentials

set -e

SCRIPT_DIR="$(dirname "$0")"
SQL_DATA_DIR="$SCRIPT_DIR/sql/data"
OUTPUT_FILE="$SQL_DATA_DIR/001_initial_data.sql"

# Color codes and print functions
GREEN='\033[0;32m'
BLUE='\033[0;34m'
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

# Function to generate BCrypt hash
generate_password_hash() {
    local password="$1"
    
    if command -v htpasswd >/dev/null 2>&1; then
        echo $(htpasswd -bnBC 12 "" "$password" | tr -d ':\n' | sed 's/^//')
        return 0
    fi

    if command -v python3 >/dev/null 2>&1; then
        local hash=$(python3 -c "
import bcrypt
import sys
try:
    password = sys.argv[1].encode('utf-8')
    salt = bcrypt.gensalt(rounds=12)
    hashed = bcrypt.hashpw(password, salt)
    print(hashed.decode('utf-8'))
except ImportError:
    print('ERROR_NO_BCRYPT_MODULE')
except Exception as e:
    print(f'ERROR_{e}')
" "$password" 2>/dev/null)
        
        if [[ "$hash" != ERROR_* ]]; then
            echo "$hash"
            return 0
        fi
    fi

    return 1
}

# Function to show help message
show_help() {
    cat << EOF
Generate Initial Data SQL Script

Usage: $0 [options]

Options:
  -u, --username <username>    Admin username (default: sysadmin)
  -p, --password <password>    Admin password (default: Admin@123)
  -e, --email <email>         Admin email (default: sysadmin@company.com)
  -f, --firstname <name>      Admin first name (default: System)
  -l, --lastname <name>       Admin last name (default: Administrator)
  -o, --output <file>         Output file (default: $OUTPUT_FILE)
  --help                     Show this help message

Examples:
  # Generate with default values
  $0

  # Generate with custom admin
  $0 --username admin --password MySecurePass123! --email admin@mycompany.com

  # Custom output location
  $0 --output ./custom_initial_data.sql

Note: This script generates BCrypt hashed passwords. Requires htpasswd.
EOF
}

# Default values
USERNAME="sysadmin"
PASSWORD="Admin@123"
EMAIL="sysadmin@company.com"
FIRSTNAME="System"
LASTNAME="Administrator"

# Parse command-line arguments
while [[ $# -gt 0 ]]; do
    case $1 in
        -u|--username)
            USERNAME="$2"
            shift 2
            ;;
        -p|--password)
            PASSWORD="$2"
            shift 2
            ;;
        -e|--email)
            EMAIL="$2"
            shift 2
            ;;
        -f|--firstname)
            FIRSTNAME="$2"
            shift 2
            ;;
        -l|--lastname)
            LASTNAME="$2"
            shift 2
            ;;
        -o|--output)
            OUTPUT_FILE="$2"
            shift 2
            ;;
        --help)
            show_help
            exit 0
            ;;
        *)
            print_error "Unknown option: $1"
            show_help
            exit 1
            ;;
    esac
done

# Generate BCrypt hash for password
print_info "Generating BCrypt hash for password..."
HASHED_PASSWORD=$(generate_password_hash "$PASSWORD")
if [ $? -ne 0 ] || [ -z "$HASHED_PASSWORD" ]; then
    print_error "Failed to generate password hash"
    exit 1
fi
print_success "Password hash generated successfully"
print_empty_line
# Create output directory if it doesn't exist
mkdir -p "$(dirname "$OUTPUT_FILE")"

# Generate SQL script
print_info "Generating SQL script at $OUTPUT_FILE..."
cat > "$OUTPUT_FILE" << EOF
-- Initial Data Script
-- Generated: $(date '+%Y-%m-%d %H:%M:%S')
-- Description: Inserts initial data for the Employee Management System

USE EmployeeDB;

-- Disable foreign key constraints temporarily for initial setup
EXEC sp_msforeachtable "ALTER TABLE ? NOCHECK CONSTRAINT all";

DECLARE @SuperAdminRoleId INT;
DECLARE @SystemUserId INT;

-- Create System User Role first
IF NOT EXISTS (SELECT * FROM UserRoles WHERE RefCode = 'SYSADMIN')
BEGIN
    INSERT INTO UserRoles (RoleName, RefCode, Description, IsVisible, CreatedBy)
    VALUES ('System Administrator', 'SYSADMIN', 'System administrator with full access', 0, 1);
    SET @SuperAdminRoleId = SCOPE_IDENTITY();
    PRINT 'System Administrator role created with ID: ' + CAST(@SuperAdminRoleId AS NVARCHAR(10));
END
ELSE
BEGIN
    SELECT @SuperAdminRoleId = Id FROM UserRoles WHERE RefCode = 'SYSADMIN';
    PRINT 'System Administrator role already exists with ID: ' + CAST(@SuperAdminRoleId AS NVARCHAR(10));
END

-- Create System User
IF NOT EXISTS (SELECT * FROM Users WHERE UserName = '$USERNAME')
BEGIN
    INSERT INTO Users (FirstName, LastName, UserName, Password, Email, UserRoleId, CreatedBy)
    VALUES ('$FIRSTNAME', '$LASTNAME', '$USERNAME', 
            '$HASHED_PASSWORD',
            '$EMAIL', @SuperAdminRoleId, 1);
    SET @SystemUserId = SCOPE_IDENTITY();
    PRINT 'System Administrator user created with ID: ' + CAST(@SystemUserId AS NVARCHAR(10));
END
ELSE
BEGIN
    SELECT @SystemUserId = Id FROM Users WHERE UserName = '$USERNAME';
    PRINT 'System Administrator user already exists with ID: ' + CAST(@SystemUserId AS NVARCHAR(10));
END

-- Re-enable all foreign key constraints
EXEC sp_msforeachtable "ALTER TABLE ? WITH CHECK CHECK CONSTRAINT all";

-- Insert additional user roles
INSERT INTO UserRoles (RoleName, RefCode, Description, IsVisible, CreatedBy)
SELECT * FROM (VALUES 
    ('User Administrator', 'USERADMIN', 'User administrator with user management access', 1, @SystemUserId),
    ('Employee', 'EMPLOYEE', 'Regular employee with basic access', 1, @SystemUserId),
    ('Manager', 'MANAGER', 'Manager with team management access', 1, @SystemUserId)
) AS NewRoles(RoleName, RefCode, Description, IsVisible, CreatedBy)
WHERE NOT EXISTS (SELECT * FROM UserRoles WHERE RefCode = NewRoles.RefCode);

-- Insert main modules
INSERT INTO Modules (ModuleName, ParentId, RefCode, IsVisible, LogoName, RedirectPage, SortOrder, Description, CreatedBy)
SELECT * FROM (VALUES 
    ('Manage Admin', NULL, 'MANAGE_ADMIN', 1, 'manage-admin-icon.svg', '/admin', 1, 'Module for managing user administrators', @SystemUserId),
    ('Manage Feature', NULL, 'MANAGE_FEATURE', 1, 'manage-feature-icon.svg', '/features', 2, 'Module for managing user roles and permissions', @SystemUserId),
    ('Manage Employee', NULL, 'MANAGE_EMPLOYEE', 1, 'manage-employee-icon.svg', '/employees', 3, 'Module for managing employees', @SystemUserId),
    ('Manage Manager', NULL, 'MANAGE_MANAGER', 1, 'manage-manager-icon.svg', '/managers', 4, 'Module for managing managers', @SystemUserId)
) AS NewModules(ModuleName, ParentId, RefCode, IsVisible, LogoName, RedirectPage, SortOrder, Description, CreatedBy)
WHERE NOT EXISTS (SELECT * FROM Modules WHERE RefCode = NewModules.RefCode);

-- Insert module accesses
INSERT INTO ModuleAccesses (ModuleId, ModuleAccessName, ParentId, RefCode, Description, IsVisible, CreatedBy)
SELECT M.Id, AccessData.AccessName, NULL, AccessData.RefCode, AccessData.Description, 1, @SystemUserId
FROM (VALUES 
    ('MANAGE_ADMIN', 'Read Admin', 'ADMIN_READ', 'Access to view admin users'),
    ('MANAGE_ADMIN', 'Create Admin', 'ADMIN_CREATE', 'Access to create new admin users'),
    ('MANAGE_ADMIN', 'Edit Admin', 'ADMIN_UPDATE', 'Access to update admin user details'),
    ('MANAGE_ADMIN', 'Delete Admin', 'ADMIN_DELETE', 'Access to delete admin users'),
    ('MANAGE_ADMIN', 'Change User Password', 'ADMIN_CHANGE_PASSWORD', 'Access to change user passwords'),

    ('MANAGE_FEATURE', 'Read Roles & Permissions', 'FEATURES_READ_ROLES', 'View roles and their assigned permissions'),
    ('MANAGE_FEATURE', 'Read All Permissions', 'FEATURES_READ_PERMISSIONS', 'View all available permissions'),
    ('MANAGE_FEATURE', 'Manage Role Permissions', 'FEATURES_UPDATE_ROLE_PERMISSIONS', 'Update permissions assigned to roles'),
    
    ('MANAGE_EMPLOYEE', 'View Employee Records', 'EMPLOYEE_READ', 'View employee records and basic profile information'),
    ('MANAGE_EMPLOYEE', 'Create Employee', 'EMPLOYEE_CREATE', 'Create new employee records'),
    ('MANAGE_EMPLOYEE', 'Edit Employee', 'EMPLOYEE_UPDATE', 'Edit existing employee records'),
    ('MANAGE_EMPLOYEE', 'Delete Employee', 'EMPLOYEE_DELETE', 'Delete employee records'),

    ('MANAGE_MANAGER', 'View Managers', 'MANAGER_READ', 'View manager records and team assignments'),
    ('MANAGE_MANAGER', 'Create Manager', 'MANAGER_CREATE', 'Add new manager records'),
    ('MANAGE_MANAGER', 'Edit Manager', 'MANAGER_UPDATE', 'Edit existing manager records and assignments'),
    ('MANAGE_MANAGER', 'Delete Manager', 'MANAGER_DELETE', 'Remove manager records'),

    ('MANAGE_FEATURE', 'View Modules', 'MODULE_READ', 'View allowed modules and navigation')
) AS AccessData(ModuleRef, AccessName, RefCode, Description)
INNER JOIN Modules M ON M.RefCode = AccessData.ModuleRef
WHERE NOT EXISTS (SELECT * FROM ModuleAccesses WHERE RefCode = AccessData.RefCode);

-- Grant only necessary access to System Administrator role (MANAGE_ADMIN and MANAGE_FEATURE modules)
DECLARE @SystemAdminRoleId INT;
SELECT @SystemAdminRoleId = Id FROM UserRoles WHERE RefCode = 'SYSADMIN';

INSERT INTO UserRoleAccesses (UserRoleId, ModuleAccessId, CreatedBy)
SELECT @SystemAdminRoleId, MA.Id, @SystemUserId
FROM ModuleAccesses MA
WHERE NOT EXISTS (
    SELECT * FROM UserRoleAccesses 
    WHERE UserRoleId = @SystemAdminRoleId AND ModuleAccessId = MA.Id
)
AND MA.RefCode IN ('ADMIN_READ', 'ADMIN_CREATE', 'ADMIN_UPDATE', 'ADMIN_DELETE', 'ADMIN_CHANGE_PASSWORD',
                   'FEATURES_READ_ROLES', 'FEATURES_READ_PERMISSIONS', 'FEATURES_UPDATE_ROLE_PERMISSIONS',
                   'MODULE_READ');

-- Grant basic access to user administrator role
DECLARE @UserAdminRoleId INT;
SELECT @UserAdminRoleId = Id FROM UserRoles WHERE RefCode = 'USERADMIN';

INSERT INTO UserRoleAccesses (UserRoleId, ModuleAccessId, CreatedBy)
SELECT @UserAdminRoleId, MA.Id, @SystemUserId
FROM ModuleAccesses MA
WHERE NOT EXISTS (
    SELECT * FROM UserRoleAccesses 
    WHERE UserRoleId = @UserAdminRoleId AND ModuleAccessId = MA.Id
)
AND MA.RefCode IN ('ADMIN_CHANGE_PASSWORD', 'MODULE_READ', 'EMPLOYEE_READ', 'EMPLOYEE_CREATE', 'EMPLOYEE_UPDATE', 
                   'EMPLOYEE_DELETE', 'MANAGER_READ', 'MANAGER_CREATE', 'MANAGER_UPDATE', 'MANAGER_DELETE');

PRINT 'Initial data setup completed successfully.';
EOF

print_success "Initial data SQL script generated: $OUTPUT_FILE"
print_empty_line
print_info "Generated Admin Credentials:"
print_info "Username: $USERNAME"
print_info "Password: $PASSWORD"