#!/bin/bash

# SMS API Environment Variables Verification Script
# This script checks if all required environment variables are properly set

# Color codes
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
CYAN='\033[0;36m'
GRAY='\033[0;37m'
NC='\033[0m' # No Color

# Required environment variables
REQUIRED_VARS=(
    "ASPNETCORE_ENVIRONMENT"
    "SMS_ConnectionStrings__MasterDatabase"
    "SMS_Jwt__Key"
    "SMS_Jwt__Issuer"
    "SMS_Jwt__Audience"
    "SMS_FileSettings__NicUploadFolderPath"
)

echo ""
echo -e "${CYAN}==================================================================${NC}"
echo -e "${CYAN}SMS API Environment Variables Verification${NC}"
echo -e "${CYAN}==================================================================${NC}"
echo ""

missing_count=0

echo -e "${YELLOW}Variable Status:${NC}"
echo ""

# Check each required variable
for var in "${REQUIRED_VARS[@]}"; do
    value="${!var}"
    
    if [ -z "$value" ]; then
        echo -e "  ${RED}? $var${NC}"
        echo -e "    ${RED}Value: NOT SET${NC}"
        ((missing_count++))
    else
        echo -e "  ${GREEN}? $var${NC}"
        
        # Mask sensitive values
        if [[ "$var" == *"Password"* ]] || [[ "$var" == *"Key"* ]] || [[ "$var" == *"ConnectionString"* ]]; then
            echo -e "    ${GRAY}Value: [SET - ${#value} characters]${NC}"
        else
            echo -e "    ${GRAY}Value: $value${NC}"
        fi
    fi
    echo ""
done

echo -e "${CYAN}==================================================================${NC}"

if [ $missing_count -eq 0 ]; then
    echo -e "${GREEN}? All required environment variables are set!${NC}"
    echo ""
    echo -e "${YELLOW}Next steps:${NC}"
    echo "1. If you just set these variables, run: source ~/.bashrc (or ~/.zshrc)"
    echo "2. Start the SMS API application"
    echo "3. Check the logs for: 'Database configuration: Configuration loaded'"
    echo "4. Test the health endpoint: GET /api/health/database"
else
    echo -e "${RED}? $missing_count variable(s) missing!${NC}"
    echo ""
    echo -e "${YELLOW}To set missing variables, run:${NC}"
    echo "  ./setup-environment.sh \\"
    echo "    --connection-string \"...\" \\"
    echo "    --jwt-key \"...\""
    echo ""
    echo -e "${YELLOW}Or set them manually by adding to ~/.bashrc (or ~/.zshrc):${NC}"
    echo "  export VARIABLE_NAME=\"value\""
    echo "Then run: source ~/.bashrc"
fi

echo -e "${CYAN}==================================================================${NC}"
echo ""

# Additional checks
echo -e "${YELLOW}Additional Checks:${NC}"
echo ""

# Check if upload path exists
if [ -n "$SMS_FileSettings__NicUploadFolderPath" ]; then
    if [ -d "$SMS_FileSettings__NicUploadFolderPath" ]; then
        echo -e "  ${GREEN}? Upload directory exists: $SMS_FileSettings__NicUploadFolderPath${NC}"
        
        # Check permissions
        if [ -w "$SMS_FileSettings__NicUploadFolderPath" ]; then
            echo -e "    ${GREEN}? Directory is writable${NC}"
        else
            echo -e "    ${RED}? Directory is not writable${NC}"
            echo -e "    ${YELLOW}Fix with: chmod 755 $SMS_FileSettings__NicUploadFolderPath${NC}"
        fi
    else
        echo -e "  ${RED}? Upload directory does not exist: $SMS_FileSettings__NicUploadFolderPath${NC}"
        echo -e "    ${YELLOW}Create it with: mkdir -p $SMS_FileSettings__NicUploadFolderPath${NC}"
    fi
else
    echo -e "  ${GRAY}- Upload directory path not set${NC}"
fi

echo ""

# Check JWT key length
if [ -n "$SMS_Jwt__Key" ]; then
    # Decode base64 and get length
    key_bytes=$(echo -n "$SMS_Jwt__Key" | base64 -d 2>/dev/null | wc -c)
    key_bits=$((key_bytes * 8))
    
    if [ $key_bits -ge 256 ]; then
        echo -e "  ${GREEN}? JWT Key length: $key_bits bits (Secure)${NC}"
    elif [ $key_bits -eq 0 ]; then
        echo -e "  ${RED}? JWT Key is not valid Base64${NC}"
    else
        echo -e "  ${RED}? JWT Key length: $key_bits bits (Too short, should be at least 256 bits)${NC}"
    fi
else
    echo -e "  ${GRAY}- JWT Key not set${NC}"
fi

echo ""

# Check for shell config file
echo -e "${YELLOW}Shell Configuration:${NC}"
echo ""

if [ -n "$BASH_VERSION" ]; then
    if [ -f "$HOME/.bash_profile" ]; then
        CONFIG_FILE="$HOME/.bash_profile"
    else
        CONFIG_FILE="$HOME/.bashrc"
    fi
    echo -e "  Shell: Bash"
    echo -e "  Config file: $CONFIG_FILE"
elif [ -n "$ZSH_VERSION" ]; then
    CONFIG_FILE="$HOME/.zshrc"
    echo -e "  Shell: Zsh"
    echo -e "  Config file: $CONFIG_FILE"
else
    CONFIG_FILE="Unknown"
    echo -e "  Shell: Unknown"
fi

if [ "$CONFIG_FILE" != "Unknown" ] && [ -f "$CONFIG_FILE" ]; then
    if grep -q "SMS_" "$CONFIG_FILE" 2>/dev/null; then
        echo -e "  ${GREEN}? SMS environment variables found in $CONFIG_FILE${NC}"
    else
        echo -e "  ${YELLOW}! SMS environment variables not found in $CONFIG_FILE${NC}"
        echo -e "    ${YELLOW}Run the setup script to add them automatically${NC}"
    fi
fi

echo ""
echo -e "${CYAN}==================================================================${NC}"
echo ""

# Exit with error code if variables are missing
exit $missing_count
