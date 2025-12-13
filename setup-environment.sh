#!/bin/bash

# SMS API Environment Setup Script for Linux/macOS
# This script helps set up environment variables for the SMS API

set -e  # Exit on error

# Color codes for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
CYAN='\033[0;36m'
NC='\033[0m' # No Color

# Default values
ENVIRONMENT="${ENVIRONMENT:-Production}"
SHELL_CONFIG="$HOME/.bashrc"

# Detect shell and set appropriate config file
if [ -n "$ZSH_VERSION" ]; then
    SHELL_CONFIG="$HOME/.zshrc"
elif [ -n "$BASH_VERSION" ]; then
    if [ -f "$HOME/.bash_profile" ]; then
        SHELL_CONFIG="$HOME/.bash_profile"
    else
        SHELL_CONFIG="$HOME/.bashrc"
    fi
fi

# Function to display usage
usage() {
    echo -e "${CYAN}==================================================================${NC}"
    echo -e "${CYAN}SMS API Environment Setup for Linux/macOS${NC}"
    echo -e "${CYAN}==================================================================${NC}"
    echo ""
    echo "Usage: $0 [OPTIONS]"
    echo ""
    echo "Options:"
    echo "  -c, --connection-string  Database connection string (required)"
    echo "  -k, --jwt-key           JWT secret key in base64 (required)"
    echo "  -e, --environment       Environment name (default: Production)"
    echo "  -p, --upload-path       File upload path (default: /var/www/sms/uploads/nic)"
    echo "  -s, --shell-config      Shell config file (auto-detected: $SHELL_CONFIG)"
    echo "  -h, --help              Show this help message"
    echo ""
    echo "Example:"
    echo "  $0 \\"
    echo "    --connection-string \"Server=localhost;Database=SMS;User Id=sa;Password=Pass123\" \\"
    echo "    --jwt-key \"GzFzP7f2gT83KnCTHtJ1Vk3a7q2rjE2F7opqP3kpMj8=\" \\"
    echo "    --environment Production \\"
    echo "    --upload-path /var/www/sms/uploads/nic"
    echo ""
    exit 1
}

# Parse command line arguments
CONNECTION_STRING=""
JWT_KEY=""
UPLOAD_PATH="/var/www/sms/uploads/nic"

while [[ $# -gt 0 ]]; do
    case $1 in
        -c|--connection-string)
            CONNECTION_STRING="$2"
            shift 2
            ;;
        -k|--jwt-key)
            JWT_KEY="$2"
            shift 2
            ;;
        -e|--environment)
            ENVIRONMENT="$2"
            shift 2
            ;;
        -p|--upload-path)
            UPLOAD_PATH="$2"
            shift 2
            ;;
        -s|--shell-config)
            SHELL_CONFIG="$2"
            shift 2
            ;;
        -h|--help)
            usage
            ;;
        *)
            echo -e "${RED}Unknown option: $1${NC}"
            usage
            ;;
    esac
done

# Validate required parameters
if [ -z "$CONNECTION_STRING" ] || [ -z "$JWT_KEY" ]; then
    echo -e "${RED}ERROR: Connection string and JWT key are required.${NC}"
    echo ""
    usage
fi

# Display configuration
echo -e "${CYAN}==================================================================${NC}"
echo -e "${CYAN}SMS API Environment Setup${NC}"
echo -e "${CYAN}==================================================================${NC}"
echo ""
echo -e "${YELLOW}Configuration:${NC}"
echo "  Environment: $ENVIRONMENT"
echo "  Connection String: ${CONNECTION_STRING:0:50}..."
echo "  JWT Key: ${JWT_KEY:0:20}..."
echo "  Upload Path: $UPLOAD_PATH"
echo "  Shell Config: $SHELL_CONFIG"
echo ""

# Confirm before proceeding
read -p "Do you want to proceed? (y/N) " -n 1 -r
echo ""
if [[ ! $REPLY =~ ^[Yy]$ ]]; then
    echo -e "${YELLOW}Operation cancelled.${NC}"
    exit 0
fi

echo ""
echo -e "${GREEN}Adding environment variables to $SHELL_CONFIG...${NC}"

# Backup existing config file
BACKUP_FILE="${SHELL_CONFIG}.backup.$(date +%Y%m%d_%H%M%S)"
cp "$SHELL_CONFIG" "$BACKUP_FILE"
echo -e "${GREEN}? Backup created: $BACKUP_FILE${NC}"

# Remove old SMS_ environment variables if they exist
sed -i.tmp '/^export SMS_/d' "$SHELL_CONFIG" 2>/dev/null || true
rm -f "${SHELL_CONFIG}.tmp"

# Add new environment variables
cat >> "$SHELL_CONFIG" << EOF

# SMS API Environment Variables (Added $(date))
export ASPNETCORE_ENVIRONMENT="$ENVIRONMENT"
export SMS_ConnectionStrings__MasterDatabase="$CONNECTION_STRING"
export SMS_Jwt__Key="$JWT_KEY"
export SMS_Jwt__Issuer="SMS"
export SMS_Jwt__Audience="SMS"
export SMS_FileSettings__NicUploadFolderPath="$UPLOAD_PATH"
EOF

echo -e "${GREEN}? Environment variables added to $SHELL_CONFIG${NC}"

# Export variables for current session
export ASPNETCORE_ENVIRONMENT="$ENVIRONMENT"
export SMS_ConnectionStrings__MasterDatabase="$CONNECTION_STRING"
export SMS_Jwt__Key="$JWT_KEY"
export SMS_Jwt__Issuer="SMS"
export SMS_Jwt__Audience="SMS"
export SMS_FileSettings__NicUploadFolderPath="$UPLOAD_PATH"

echo -e "${GREEN}? Environment variables set for current session${NC}"

# Create upload directory if it doesn't exist
if [ ! -d "$UPLOAD_PATH" ]; then
    echo ""
    echo -e "${YELLOW}Upload directory does not exist: $UPLOAD_PATH${NC}"
    read -p "Do you want to create it? (y/N) " -n 1 -r
    echo ""
    if [[ $REPLY =~ ^[Yy]$ ]]; then
        mkdir -p "$UPLOAD_PATH"
        chmod 755 "$UPLOAD_PATH"
        echo -e "${GREEN}? Upload directory created: $UPLOAD_PATH${NC}"
    fi
fi

echo ""
echo -e "${CYAN}==================================================================${NC}"
echo -e "${GREEN}Environment variables configured successfully!${NC}"
echo -e "${CYAN}==================================================================${NC}"
echo ""
echo -e "${YELLOW}IMPORTANT: Next steps:${NC}"
echo "1. Run: source $SHELL_CONFIG"
echo "   (or close and reopen your terminal)"
echo "2. Verify variables are set: echo \$SMS_Jwt__Issuer"
echo "3. Start the SMS API application"
echo "4. Check the logs to verify configuration loaded correctly"
echo ""
echo -e "${YELLOW}For systemd service:${NC}"
echo "Add these environment variables to your service file:"
echo "/etc/systemd/system/sms-api.service"
echo ""
echo -e "${YELLOW}Backup:${NC}"
echo "Your previous config was backed up to: $BACKUP_FILE"
echo ""

# Verification
echo -e "${CYAN}==================================================================${NC}"
echo -e "${CYAN}Current Session Verification:${NC}"
echo -e "${CYAN}==================================================================${NC}"
echo "ASPNETCORE_ENVIRONMENT: $ASPNETCORE_ENVIRONMENT"
echo "SMS_Jwt__Issuer: $SMS_Jwt__Issuer"
echo "SMS_Jwt__Audience: $SMS_Jwt__Audience"
echo "SMS_FileSettings__NicUploadFolderPath: $SMS_FileSettings__NicUploadFolderPath"
echo "SMS_ConnectionStrings__MasterDatabase: [SET]"
echo "SMS_Jwt__Key: [SET]"
echo ""

echo -e "${GREEN}Setup complete!${NC}"
