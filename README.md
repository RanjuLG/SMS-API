# 🏪 SMS API - Store Management System

A comprehensive **RESTful API** for managing gold/jewelry pawn shop operations. Built with **.NET 8**, this system provides robust functionality for customer management, item tracking, transactions, loans, invoices, and real-time health monitoring.

![.NET](https://img.shields.io/badge/.NET-8.0-purple?style=flat-square&logo=dotnet)
![SQL Server](https://img.shields.io/badge/SQL%20Server-Database-CC2927?style=flat-square&logo=microsoftsqlserver)
![JWT](https://img.shields.io/badge/JWT-Authentication-000000?style=flat-square&logo=jsonwebtokens)
![Swagger](https://img.shields.io/badge/Swagger-API%20Docs-85EA2D?style=flat-square&logo=swagger)

---

## ✨ Features

### Core Business Operations

- **👥 Customer Management** - Complete CRUD with NIC photo uploads
- **💎 Item Management** - Track jewelry/gold items with specifications (weight, karatage, value)
- **💰 Transaction Processing** - Handle pawn transactions, installments, and settlements
- **📄 Invoice Generation** - Automated invoice creation with multiple types
- **🏦 Loan Management** - Track loans with customizable periods and interest rates
- **💵 Installment Tracking** - Manage payment schedules and due dates

### Advanced Features

- **📊 Reporting** - Customer and business overview reports
- **🔐 Role-Based Access Control** - JWT authentication with role management
- **📈 Health Monitoring** - Real-time system, database, and service health checks
- **💾 Backup Management** - Automated backup tracking and alerts
- **📝 Comprehensive Logging** - Serilog integration for detailed logging

---

## 🛠️ Tech Stack

| Technology                  | Purpose                        |
| --------------------------- | ------------------------------ |
| **.NET 8**                  | Core framework                 |
| **Entity Framework Core 8** | ORM & database migrations      |
| **SQL Server**              | Database                       |
| **ASP.NET Core Identity**   | Authentication & authorization |
| **JWT Bearer Tokens**       | API authentication             |
| **AutoMapper**              | Object mapping                 |
| **Serilog**                 | Structured logging             |
| **Swashbuckle**             | Swagger/OpenAPI documentation  |

---

## 🚀 Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) (LocalDB, Express, or Full)
- IDE: Visual Studio 2022 / VS Code / Rider

### Installation

1. **Clone the repository**

   ```bash
   git clone https://github.com/yourusername/SMS-API.git
   cd SMS-API
   ```

2. **Update the connection string** in `appsettings.json`

   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=YOUR_SERVER;Database=SMS;Trusted_Connection=True;TrustServerCertificate=True"
     }
   }
   ```

3. **Apply database migrations**

   ```bash
   dotnet ef database update
   ```

4. **Run the application**

   ```bash
   dotnet run
   ```

5. **Access the API**
   - Swagger UI: `https://localhost:7217/swagger`
   - API Base URL: `https://localhost:7217/api`

---

## 📚 API Documentation

### Authentication

All protected endpoints require a JWT token in the Authorization header:

```
Authorization: Bearer {your_token}
```

**Login to obtain token:**

```http
POST /api/account/login
Content-Type: application/json

{
  "username": "your_username",
  "password": "your_password"
}
```

### Main Endpoints

| Module           | Endpoint                          | Description              |
| ---------------- | --------------------------------- | ------------------------ |
| **Auth**         | `POST /api/account/login`         | User login               |
| **Auth**         | `POST /api/account/register`      | Register new user        |
| **Customers**    | `GET/POST /api/customers`         | List/Create customers    |
| **Items**        | `GET/POST /api/items`             | List/Create items        |
| **Transactions** | `GET/POST /api/transactions`      | List/Create transactions |
| **Invoices**     | `GET/POST /api/invoices`          | List/Create invoices     |
| **Installments** | `GET /api/installments`           | List installments        |
| **Karatage**     | `GET/POST /api/karatage/karats`   | Manage karat values      |
| **Pricing**      | `GET/POST /api/karatage/pricings` | Manage pricing configs   |
| **Reports**      | `GET /api/reports/overview`       | Business overview        |
| **Health**       | `GET /api/health`                 | System health status     |

> 📖 For complete API documentation, see [API_DOCUMENTATION.md](./Documents/API_DOCUMENTATION.md)

---

## 📁 Project Structure

```
SMS-API/
├── Controllers/          # API controllers
│   ├── AccountController.cs
│   ├── CustomerController.cs
│   ├── ItemController.cs
│   ├── InvoiceController.cs
│   ├── TransactionController.cs
│   └── ...
├── Models/               # Entity models & DTOs
│   ├── Customer.cs
│   ├── Item.cs
│   ├── Transaction.cs
│   └── DTO/
├── Services/             # Business logic services
├── Interfaces/           # Service interfaces
├── Repositories/         # Data access layer
├── Business/             # Core business logic
├── DBContext/            # EF Core DbContext
├── Migrations/           # Database migrations
├── Middleware/           # Custom middleware
├── Documents/            # API documentation
├── SMS.Tests/            # Unit tests (xUnit)
└── appsettings.json      # Configuration
```

---

## 🧪 Running Tests

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

---

## 📊 Health Monitoring

The API includes comprehensive health monitoring:

- **System Health** - Overall application status
- **Database Health** - SQL Server connectivity
- **Storage Health** - Disk space monitoring
- **CPU/Memory Metrics** - Resource usage tracking
- **Backup Status** - Backup health and history

Access health endpoints at `/api/health`

---

## 🔒 Security

- **JWT Authentication** - Secure token-based auth
- **Role-Based Authorization** - Admin, Manager, User roles
- **Password Hashing** - ASP.NET Identity security
- **HTTPS Enforcement** - SSL/TLS encryption

---

## 📄 Additional Documentation

| Document                                                  | Description                  |
| --------------------------------------------------------- | ---------------------------- |
| [API Documentation](./Documents/API_DOCUMENTATION.md)     | Complete API reference       |
| [Health Endpoints](./Documents/API_HEALTH_ENDPOINTS.md)   | Health monitoring guide      |
| [Role System](./Documents/ROLE_SYSTEM.md)                 | Role-based access control    |
| [Background Services](./Documents/BACKGROUND_SERVICES.md) | Background job documentation |
| [Status Summary](./Documents/Status-Summary.md)           | All status values reference  |

---

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

---

## 📝 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

## 👨‍💻 Author

**Ranju**

---

<p align="center">
  Made with ❤️ for jewelry pawn shop management
</p>
