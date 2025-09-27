#!/bin/bash

# Generate SQL script to change password of users (default: sysadmin)

set -e

SCRIPT_DIR="$(dirname "$0")"
SQL_PASSWORD_DIR="$SCRIPT_DIR/sql/passwords"

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

# Default values
USERNAME="sysadmin"
PASSWORD="Admin@123"
OUTPUT_FILE="$SQL_PASSWORD_DIR/sysadmin_password.sql"

# Function to show help message
show_help() {
    cat << EOF
Generate SQL script to change password of users (default: sysadmin)
Useful if sysadmin password is forgotten but can also be used to reset password of any user.

Usage: $0 [options]

Options:
  -u, --username <username>    username (default: sysadmin)
  -p, --password <password>    password (default: Admin@123)
  -o, --output <file>         Output file (default: ./scripts/sql/passwords/[username]_password.sql)
  --help                     Show this help message

Examples:
  # Generate with default values
  $0

  # Generate with custom username and password
  $0 --username admin --password MySecurePass123!

  # Custom output location
  $0 --output ./custom_pass_loc.sql

Note: This script generates BCrypt hashed passwords. Requires htpasswd.
EOF
}

# Parse command-line arguments
while [[ $# -gt 0 ]]; do
    case $1 in
        -u|--username)
            USERNAME="$2"
            OUTPUT_FILE="$SQL_PASSWORD_DIR/${USERNAME}_password.sql"
            shift 2
            ;;
        -p|--password)
            PASSWORD="$2"
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
-- Script to change password for user '$USERNAME'
-- Generated: $(date '+%Y-%m-%d %H:%M:%S')
-- Description: Changes password for user '$USERNAME' to a new BCrypt hashed password

USE EmployeeDB;

-- Update password for the specified user
IF EXISTS (SELECT 1 FROM Users WHERE UserName = '$USERNAME')
BEGIN
    UPDATE Users
    SET Password = '$HASHED_PASSWORD',
        PasswordChangedUTC = NULL,
        LatestDateUpdatedUTC = GETUTCDATE(),
        LatestUpdatedBy = 1
    WHERE UserName = '$USERNAME';
    PRINT 'Password for user $USERNAME has been updated successfully.';
END
ELSE
BEGIN
    PRINT 'User $USERNAME does not exist. No changes made.';
END

PRINT 'Script execution completed.';
EOF

print_success "SQL script generated successfully at $OUTPUT_FILE"
print_empty_line
print_info "Details:"
print_info "Username: $USERNAME"
print_info "Password: $PASSWORD"