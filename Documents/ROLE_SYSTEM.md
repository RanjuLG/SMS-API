# SMS API Role-Based Access Control System

## Overview
The SMS API implements a comprehensive role-based access control (RBAC) system with three main roles and multiple authorization policies.

## Roles

### 1. SuperAdmin
- **Highest level of access** - can perform all operations in the system
- **Default User**: 
  - Username: `superadmin`
  - Email: `superadmin@sms.com`
  - Password: `SuperAdmin@123!`
- **Capabilities**:
  - Full system management
  - User management (create, read, update, delete users)
  - Role management (create, read, delete roles)
  - All business operations (customers, items, transactions, etc.)
  - System configuration and maintenance

### 2. Admin
- **Administrative access** - can manage users and most business operations
- **Default User**: 
  - Username: `admin`
  - Email: `admin@example.com`
  - Password: `Admin@123`
- **Capabilities**:
  - User management (create, read, update, delete users)
  - All business operations (customers, items, transactions, etc.)
  - Cannot manage roles (reserved for SuperAdmin)

### 3. Cashier
- **Operational access** - can perform day-to-day business operations
- **Default User**: 
  - Username: `cashier`
  - Email: `cashier@example.com`
  - Password: `Cashier@123`
- **Capabilities**:
  - Customer management
  - Item management
  - Transaction processing
  - Invoice generation
  - Report viewing
  - Cannot manage users or system settings

## Authorization Policies

### SuperAdminPolicy
- **Roles**: SuperAdmin only
- **Usage**: System-critical operations, role management
- **Controllers**: RolesController

### UserManagementPolicy
- **Roles**: SuperAdmin, Admin
- **Usage**: User account management operations
- **Controllers**: AccountController (user CRUD operations)

### AdminPolicy
- **Roles**: SuperAdmin, Admin
- **Usage**: Administrative operations
- **Controllers**: General administrative functions

### CashierPolicy
- **Roles**: SuperAdmin, Admin, Cashier
- **Usage**: Standard business operations
- **Controllers**: CustomerController, ItemController, TransactionController, etc.

### SystemManagementPolicy
- **Roles**: SuperAdmin only
- **Usage**: System configuration and maintenance
- **Controllers**: Future system management controllers

## Controller Access Levels

### Public Access (No Authentication Required)
- `POST /api/account/login`
- `POST /api/account/register` (requires valid admin token)

### SuperAdmin Only
- `GET /api/roles` - View all roles
- `POST /api/roles` - Create new role
- `DELETE /api/roles/{roleName}` - Delete role (except system roles)

### SuperAdmin + Admin
- `GET /api/account/users` - View all users
- `PUT /api/account/user/{userId}` - Update user
- `DELETE /api/account/users/delete-multiple` - Delete users

### SuperAdmin + Admin + Cashier
- All business operation endpoints:
  - `/api/customers/*`
  - `/api/items/*`
  - `/api/transactions/*`
  - `/api/invoices/*`
  - `/api/reports/*`
  - `/api/installments/*`
  - `/api/karatage/*`

## Security Features

### Role Hierarchy
- SuperAdmin inherits all Admin capabilities
- Admin inherits all Cashier capabilities
- Explicit role checks ensure proper access control

### Protected System Roles
- SuperAdmin, Admin, and Cashier roles cannot be deleted
- Prevents accidental system lockout

### Automatic Upgrades
- Existing admin users are automatically upgraded to SuperAdmin on system startup
- Ensures backward compatibility

## User Management

### Registration
- Only SuperAdmin and Admin users can register new users
- Requires valid JWT token from authorized user
- New users can be assigned any available role

### Authentication
- JWT token-based authentication
- Tokens expire after 1 hour
- Role information included in token claims

### Default Users Creation
- System automatically creates default users on first startup
- Console output shows creation status and credentials

## Migration from Previous System

### Automatic Upgrade
- Existing admin users are automatically assigned SuperAdmin role
- No manual intervention required
- Backward compatibility maintained

### Legacy Support
- Original admin credentials continue to work
- Existing authorization attributes remain functional
- Gradual migration to new policy system

## API Usage Examples

### Login as SuperAdmin
```http
POST /api/account/login
Content-Type: application/json

{
  "username": "superadmin",
  "password": "SuperAdmin@123!"
}
```

### Create New Role (SuperAdmin only)
```http
POST /api/roles
Authorization: Bearer {superadmin_token}
Content-Type: application/json

"Manager"
```

### Register New User (SuperAdmin/Admin)
```http
POST /api/account/register?token={admin_token}
Content-Type: application/json

{
  "username": "newuser",
  "email": "newuser@company.com",
  "password": "NewUser@123",
  "roles": ["Cashier"]
}
```

## Best Practices

1. **Use Specific Policies**: Apply the most restrictive policy that meets requirements
2. **Regular Audits**: Review user roles and permissions regularly
3. **Principle of Least Privilege**: Assign minimum required roles to users
4. **Secure Credentials**: Change default passwords immediately after deployment
5. **Monitor Access**: Log and monitor administrative actions

## Future Enhancements

- Claims-based authorization for fine-grained permissions
- Role-based UI elements hiding/showing
- Audit logging for administrative actions
- Time-based role assignments
- Multi-factor authentication for SuperAdmin accounts
