# SMS (Store Management System) API Documentation

## Overview
This API documentation covers all endpoints for the SMS (Store Management System) application, which is designed for managing gold/jewelry pawn shop operations including customers, items, transactions, invoices, loans, pricing, and reporting.

## Base Configuration
- **Base URL**: `https://localhost:7217` (configurable via appConfig.json)
- **Authentication**: JWT Bearer Token
- **Content-Type**: `application/json` (except for file uploads which use `multipart/form-data`)

## Authentication

### 1. Login
**Endpoint**: `POST /api/account/login`
**Description**: Authenticate user and receive JWT token
**Request Body**:
```json
{
  "username": "string",
  "password": "string"
}
```
**Response**:
```json
{
  "token": "string",
  "expiration": "datetime"
}
```

### 2. Register User
**Endpoint**: `POST /api/account/register`
**Description**: Register a new user (requires admin token)
**Query Parameters**: `token` (admin JWT token)
**Request Body**:
```json
{
  "username": "string",
  "email": "string",
  "password": "string",
  "roles": ["string"]
}
```

## Customer Management

### 1. Create Customer
**Endpoint**: `POST /api/customers`
**Description**: Create a new customer with optional NIC photo
**Content-Type**: `multipart/form-data`
**Request Body**:
```
customerNIC: string
customerName: string
customerAddress: string
customerContactNo: string
nicPhoto: file (optional)
```

### 2. Get All Customers
**Endpoint**: `GET /api/customers`
**Description**: Retrieve customers within date range with pagination and search
**Query Parameters**:
- `From`: datetime (ISO string)
- `To`: datetime (ISO string)
- `Page`: number (default: 1) - Page number for pagination
- `PageSize`: number (default: 10) - Number of items per page
- `Search`: string (optional) - Search term for filtering customers
- `SortBy`: string (optional) - Field to sort by (e.g., "customerName", "createdAt")
- `SortOrder`: string (optional) - Sort direction ("asc" or "desc")

### 3. Get Customer by ID
**Endpoint**: `GET /api/customers/{customerId}/customer`
**Description**: Retrieve specific customer details

### 4. Update Customer
**Endpoint**: `PUT /api/customers/{customerId}/customer`
**Description**: Update customer information
**Content-Type**: `multipart/form-data`
**Request Body**:
```
customerNIC: string
customerName: string
customerAddress: string
customerContactNo: string
nicPhoto: file (optional)
```

### 5. Delete Customer
**Endpoint**: `DELETE /api/customers/{customerId}/customer`
**Description**: Delete a specific customer

### 6. Delete Multiple Customers
**Endpoint**: `DELETE /api/customers/delete-multiple`
**Description**: Delete multiple customers
**Request Body**:
```json
[1, 2, 3]
```

### 7. Get Customers by IDs
**Endpoint**: `POST /api/customers/byIds`
**Description**: Retrieve multiple customers by their IDs
**Request Body**:
```json
[1, 2, 3]
```

### 8. Get Customer by NIC
**Endpoint**: `POST /api/customers/byNIC`
**Description**: Retrieve customer by National Identity Card number
**Request Body**:
```json
"NIC_NUMBER"
```

## Item Management

### 1. Create Item
**Endpoint**: `POST /api/items`
**Description**: Create a new item/jewelry piece
**Request Body**:
```json
{
  "customerId": "number",
  "itemDescription": "string",
  "itemCaratage": "decimal",
  "itemGoldWeight": "decimal",
  "itemValue": "decimal",
  "itemWeight": "decimal",
  "itemRemarks": "string"
}
```

### 2. Get All Items
**Endpoint**: `GET /api/items`
**Description**: Retrieve items within date range with pagination and search
**Query Parameters**:
- `From`: datetime (ISO string)
- `To`: datetime (ISO string)
- `Page`: number (default: 1) - Page number for pagination
- `PageSize`: number (default: 10) - Number of items per page
- `Search`: string (optional) - Search term for filtering items by description, remarks, or customer info
- `SortBy`: string (optional) - Field to sort by (e.g., "itemDescription", "createdAt", "itemValue")
- `SortOrder`: string (optional) - Sort direction ("asc" or "desc")
- `CustomerNIC`: string (optional) - Filter items by specific customer NIC

### 3. Get Item by ID
**Endpoint**: `GET /api/items/{itemId}/item`
**Description**: Retrieve specific item details

### 4. Update Item
**Endpoint**: `PUT /api/items/{itemId}/item`
**Description**: Update item information
**Request Body**:
```json
{
  "customerId": "number",
  "itemDescription": "string",
  "itemCaratage": "decimal",
  "itemGoldWeight": "decimal",
  "itemValue": "decimal",
  "itemWeight": "decimal",
  "itemRemarks": "string"
}
```

### 5. Delete Item
**Endpoint**: `DELETE /api/items/{itemId}/item`
**Description**: Delete a specific item

### 6. Delete Multiple Items
**Endpoint**: `DELETE /api/items/delete-multiple`
**Description**: Delete multiple items
**Request Body**:
```json
[1, 2, 3]
```

### 7. Get Items by Customer NIC
**Endpoint**: `GET /api/items/customer/{nic}`
**Description**: Retrieve all items belonging to a specific customer

## Transaction Management

### 1. Create Transaction
**Endpoint**: `POST /api/transactions`
**Description**: Create a new transaction
**Request Body**:
```json
{
  "customerId": "number",
  "subTotal": "decimal",
  "interestRate": "decimal",
  "totalAmount": "decimal",
  "transactionType": "number",
  "interestAmount": "decimal",
  "items": [
    {
      "itemId": "number",
      "pawnValue": "decimal"
    }
  ]
}
```

### 2. Get All Transactions
**Endpoint**: `GET /api/transactions`
**Description**: Retrieve transactions within date range with pagination and search
**Query Parameters**:
- `From`: datetime (ISO string)
- `To`: datetime (ISO string)
- `Page`: number (default: 1) - Page number for pagination
- `PageSize`: number (default: 10) - Number of items per page
- `Search`: string (optional) - Search term for filtering transactions by customer info or transaction details
- `SortBy`: string (optional) - Field to sort by (e.g., "createdAt", "totalAmount", "customer.customerName")
- `SortOrder`: string (optional) - Sort direction ("asc" or "desc")
- `CustomerNIC`: string (optional) - Filter transactions by specific customer NIC
- `TransactionType`: number (optional) - Filter by transaction type
- `MinAmount`: decimal (optional) - Filter by minimum transaction amount
- `MaxAmount`: decimal (optional) - Filter by maximum transaction amount

### 3. Get Transaction by ID
**Endpoint**: `GET /api/transactions/{transactionId}`
**Description**: Retrieve specific transaction details

### 4. Get Transactions by IDs
**Endpoint**: `POST /api/transactions/byIds`
**Description**: Retrieve multiple transactions by their IDs
**Request Body**:
```json
[1, 2, 3]
```

### 5. Delete Transaction
**Endpoint**: `DELETE /api/transactions/{transactionId}`
**Description**: Delete a specific transaction

### 6. Delete Multiple Transactions
**Endpoint**: `DELETE /api/transactions/delete-multiple`
**Description**: Delete multiple transactions
**Request Body**:
```json
[1, 2, 3]
```

### 7. Get Transactions by Customer NIC
**Endpoint**: `GET /api/transactions/customer/{nic}`
**Description**: Retrieve all transactions for a specific customer

## Invoice Management

### 1. Create Invoice
**Endpoint**: `POST /api/invoices/{initialInvoiceNumber}/{installmentNumber}`
**Description**: Create a new invoice for a transaction
**Request Body**:
```json
{
  "transactionId": "number",
  "invoiceTypeId": "number",
  "status": "number"
}
```

### 2. Get All Invoices
**Endpoint**: `GET /api/invoices`
**Description**: Retrieve invoices within date range with pagination and search
**Query Parameters**:
- `From`: datetime (ISO string)
- `To`: datetime (ISO string)
- `Page`: number (default: 1) - Page number for pagination
- `PageSize`: number (default: 10) - Number of items per page
- `Search`: string (optional) - Search term for filtering invoices by invoice number or customer info
- `SortBy`: string (optional) - Field to sort by (e.g., "dateGenerated", "invoiceNo", "status")
- `SortOrder`: string (optional) - Sort direction ("asc" or "desc")
- `CustomerNIC`: string (optional) - Filter invoices by specific customer NIC
- `Status`: number (optional) - Filter by invoice status
- `InvoiceTypeId`: number (optional) - Filter by invoice type

### 3. Get Invoice by ID
**Endpoint**: `GET /api/invoices/{invoiceId}`
**Description**: Retrieve specific invoice details

### 4. Update Invoice
**Endpoint**: `PUT /api/invoices/{invoiceId}`
**Description**: Update invoice information
**Request Body**:
```json
{
  "transactionId": "number",
  "invoiceTypeId": "number",
  "status": "number"
}
```

### 5. Delete Invoice
**Endpoint**: `DELETE /api/invoices/{invoiceId}`
**Description**: Delete a specific invoice

### 6. Delete Multiple Invoices
**Endpoint**: `DELETE /api/invoices/delete-multiple`
**Description**: Delete multiple invoices
**Request Body**:
```json
[1, 2, 3]
```

### 7. Get Invoices by Customer NIC
**Endpoint**: `GET /api/invoices/customer/{nic}`
**Description**: Retrieve all invoices for a specific customer

### 8. Get Invoice by Invoice Number
**Endpoint**: `GET /api/invoices/invoiceNo/{invoiceNo}`
**Description**: Retrieve invoice by its invoice number

### 9. Get Loan Info by Initial Invoice Number
**Endpoint**: `GET /api/invoices/InitialInvoice/{invoiceNo}`
**Description**: Retrieve loan information using initial invoice number

## User Management

### 1. Get All Users
**Endpoint**: `GET /api/account/users`
**Description**: Retrieve all system users with pagination and search
**Query Parameters**:
- `Page`: number (default: 1) - Page number for pagination
- `PageSize`: number (default: 10) - Number of items per page
- `Search`: string (optional) - Search term for filtering users by username or email
- `SortBy`: string (optional) - Field to sort by (e.g., "userName", "email")
- `SortOrder`: string (optional) - Sort direction ("asc" or "desc")
- `Role`: string (optional) - Filter users by role

### 2. Update User
**Endpoint**: `PUT /api/account/user/{userId}`
**Description**: Update user information
**Request Body**:
```json
{
  "username": "string",
  "email": "string",
  "roles": ["string"]
}
```

### 3. Delete Multiple Users
**Endpoint**: `DELETE /api/account/users/delete-multiple`
**Description**: Delete multiple users
**Request Body**:
```json
["userId1", "userId2", "userId3"]
```

## Karatage Management

### 1. Get All Karats
**Endpoint**: `GET /api/karatage/karats`
**Description**: Retrieve all available karat values with pagination and search
**Query Parameters**:
- `Page`: number (default: 1) - Page number for pagination
- `PageSize`: number (default: 10) - Number of items per page
- `Search`: string (optional) - Search term for filtering by karat value
- `SortBy`: string (optional) - Field to sort by (e.g., "karatValue")
- `SortOrder`: string (optional) - Sort direction ("asc" or "desc")

### 2. Get Karat by ID
**Endpoint**: `GET /api/karatage/karats/{karatId}`
**Description**: Retrieve specific karat details

### 3. Create Karat
**Endpoint**: `POST /api/karatage/karats`
**Description**: Create a new karat value
**Request Body**:
```json
{
  "karatValue": "number"
}
```

### 4. Update Karat
**Endpoint**: `PUT /api/karatage/karats/{karatId}`
**Description**: Update karat information
**Request Body**:
```json
{
  "karatValue": "number"
}
```

### 5. Delete Karat
**Endpoint**: `DELETE /api/karatage/karats/{karatId}`
**Description**: Delete a specific karat

## Loan Period Management

### 1. Get All Loan Periods
**Endpoint**: `GET /api/karatage/loanperiods`
**Description**: Retrieve all available loan periods with pagination and search
**Query Parameters**:
- `Page`: number (default: 1) - Page number for pagination
- `PageSize`: number (default: 10) - Number of items per page
- `Search`: string (optional) - Search term for filtering by period value
- `SortBy`: string (optional) - Field to sort by (e.g., "period")
- `SortOrder`: string (optional) - Sort direction ("asc" or "desc")

### 2. Get Loan Period by ID
**Endpoint**: `GET /api/karatage/loanperiods/{loanPeriodId}`
**Description**: Retrieve specific loan period details

### 3. Create Loan Period
**Endpoint**: `POST /api/karatage/loanperiods`
**Description**: Create a new loan period
**Request Body**:
```json
{
  "period": "number"
}
```

### 4. Update Loan Period
**Endpoint**: `PUT /api/karatage/loanperiods/{loanPeriodId}`
**Description**: Update loan period information
**Request Body**:
```json
{
  "period": "number"
}
```

### 5. Delete Loan Period
**Endpoint**: `DELETE /api/karatage/loanperiods/{loanPeriodId}`
**Description**: Delete a specific loan period

## Pricing Management

### 1. Get All Pricings
**Endpoint**: `GET /api/karatage/pricings`
**Description**: Retrieve all pricing configurations with pagination and search
**Query Parameters**:
- `Page`: number (default: 1) - Page number for pagination
- `PageSize`: number (default: 10) - Number of items per page
- `Search`: string (optional) - Search term for filtering by karat value or loan period
- `SortBy`: string (optional) - Field to sort by (e.g., "price", "karat.karatValue", "loanPeriod.period")
- `SortOrder`: string (optional) - Sort direction ("asc" or "desc")
- `KaratId`: number (optional) - Filter by specific karat ID
- `LoanPeriodId`: number (optional) - Filter by specific loan period ID
- `MinPrice`: decimal (optional) - Filter by minimum price
- `MaxPrice`: decimal (optional) - Filter by maximum price

### 2. Get Pricing by ID
**Endpoint**: `GET /api/karatage/pricings/{pricingId}`
**Description**: Retrieve specific pricing details

### 3. Create Pricing
**Endpoint**: `POST /api/karatage/pricings`
**Description**: Create a new pricing configuration
**Request Body**:
```json
{
  "price": "decimal",
  "karatId": "number",
  "loanPeriodId": "number"
}
```

### 4. Create Pricing Batch
**Endpoint**: `POST /api/karatage/pricings/batch`
**Description**: Create multiple pricing configurations
**Request Body**:
```json
[
  {
    "price": "decimal",
    "karatId": "number",
    "loanPeriodId": "number"
  }
]
```

### 5. Update Pricing
**Endpoint**: `PUT /api/karatage/pricings/{pricingId}`
**Description**: Update pricing information
**Request Body**:
```json
{
  "price": "decimal"
}
```

### 6. Delete Pricing
**Endpoint**: `DELETE /api/karatage/pricings/{pricingId}`
**Description**: Delete a specific pricing configuration

### 7. Get Pricings by Karat and Loan Period
**Endpoint**: `GET /api/karatage/pricings/karat/{karatId}/loanperiod/{loanPeriodId}`
**Description**: Retrieve pricing for specific karat and loan period combination

## Reports

### 1. Get Customer Report
**Endpoint**: `GET /api/reports/customer/{customerNIC}`
**Description**: Generate detailed report for a specific customer

### 2. Get Overview Report
**Endpoint**: `GET /api/reports/overview`
**Description**: Get system overview with statistics and summaries

## Response Status Codes

- **200 OK**: Successful operation
- **201 Created**: Resource created successfully
- **400 Bad Request**: Invalid request data
- **401 Unauthorized**: Authentication required or invalid token
- **403 Forbidden**: Insufficient permissions
- **404 Not Found**: Resource not found
- **500 Internal Server Error**: Server error

## Pagination Response Format
All paginated endpoints return data in the following format:
```json
{
  "data": [...], // Array of items for current page
  "pagination": {
    "currentPage": 1,
    "pageSize": 10,
    "totalItems": 150,
    "totalPages": 15,
    "hasNextPage": true,
    "hasPreviousPage": false
  },
  "filters": {
    "search": "search term",
    "sortBy": "fieldName",
    "sortOrder": "asc",
    "appliedFilters": {...}
  }
}
```

## Search and Filter Guidelines
- **Text Search**: Searches across multiple relevant fields for each entity
- **Case Insensitive**: All text searches are case-insensitive
- **Partial Match**: Search terms support partial matching
- **Date Filters**: Support both single date and date range filtering
- **Numeric Filters**: Support range filtering (min/max values)
- **Combined Filters**: Multiple filters can be applied simultaneously

## Error Response Format
```json
{
  "message": "Error description",
  "details": "Additional error details (optional)"
}
```

## Authentication Headers
All protected endpoints require the following header:
```
Authorization: Bearer {JWT_TOKEN}
```

## Database Schema Overview

### Main Tables:
- **Customers**: Customer information and NIC photos
- **Items**: Jewelry/gold items with specifications
- **Transactions**: Financial transactions between customers and store
- **TransactionItems**: Junction table linking transactions and items
- **Invoices**: Generated invoices for transactions
- **Loans**: Loan information with periods and settlement status
- **Installments**: Payment installments for loans
- **Karats**: Available gold karat values
- **LoanPeriods**: Available loan duration options
- **Pricings**: Price configurations based on karat and loan period
- **AspNet*** Tables**: Identity framework for user management

### Business Logic:
1. Customers can pawn multiple items in a single transaction
2. Each transaction can generate invoices
3. Loans are created for transactions with installment options
4. Pricing is determined by gold karat value and loan period
5. The system supports both initial pawn transactions and installment payments
6. Reports provide insights into customer activities and business overview
