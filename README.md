# TradeHub - Peer-to-Peer Marketplace API

TradeHub is a comprehensive RESTful API for a P2P marketplace where users can sell products, manage virtual shops, and safely trade items using a secure wallet system. This project demonstrates backend engineering best practices, including clean architecture, transactional processing, and secure authentication.

## �️ Tech Stack

- **Framework:** .NET 8 (ASP.NET Core Web API)
- **Database:** MySQL
- **ORM / Data Access:** **Dapper** (Lightweight, high-performance micro-ORM)
- **Authentication:** **JWT** (JSON Web Token) with custom middleware
- **Security:** **BCrypt.Net** for advanced password hashing and salt management
- **API Documentation:** Swagger / OpenAPI
- **Serialization:** System.Text.Json (Native .NET)

## 🏗️ Architecture

The project leverages a **3-Layer Architecture** enriched with the **Usecase/Application Service pattern** to handle complex business orchestrations:

- **TradeHub.API (Presentation Layer):**
  - RESTful Controllers providing endpoints for client interaction.
  - **Global Exception Middleware**: Centralized error handling converting system exceptions into consistent API responses.
  - **Extension Methods**: Clean service registration and authentication bootstrap.
  - **Standardized Response**: Unified `ApiResponse` wrapper for all outputs.

- **TradeHub.BLL (Business Logic Layer):**
  - **Services**: Low-level business logic focused on individual entities (Product, Cart, Wallet).
  - **Usecases (Application Services)**: High-level orchestration (e.g., `OrderUsecase`) managing complex operations across multiple services within atomic transactions.
  - **Business Exceptions**: Domain-specific exceptions like `BusinessException` and `NotFoundException`.
  - **Custom Mapping**: Manual mapping logic (DTO <-> Entity) for maximum performance and clarity.

- **TradeHub.DAL (Data Access Layer):**
  - **Repository Pattern**: Abstracting raw SQL logic from business services.
  - **DatabaseContext**: Custom wrapper around `MySqlConnection` to simplify query execution and transaction lifecycle management using Dapper.

## ✨ Core Features

### 🔐 Authentication & Security
- **Secure Onboarding**: User registration with strong password validation (complexity checks).
- **JWT Authorization**: Stateless session management with bearer tokens.
- **Identity Context**: Ease of access to `CurrentUserId` through base controllers.

### 📦 Product & Inventory
- **Marketplace Listing**: Public product discovery combined with private seller management.
- **Real-time Inventory**: Secure methods for increasing/decreasing stock with concurrency-safe updates.
- **Dynamic Pricing**: Sellers can update product listings and pricing on the fly.

### 🛒 Cart & Order Management
- **Persistent Cart**: Users can manage items across sessions.
- **Transaction-Safe Checkout**: Grouping cart items by seller into separate orders, validating stock availability, and clearing carts in a single atomic database operation.
- **Order Tracking**: Order lifecycle management (Pending -> Confirmed -> Delivered).

### 💳 Wallet & Financials
- **Virtual Wallet**: Every user has an integrated balance for internal trades.
- **Transactional Consistency**: Atomic operations for deposits and withdrawals.
- **History Audit**: Complete logging of wallet transactions for transparency.

## 📂 Project Structure

```text
TradeHub/
├── TradeHub.API/          # Entry point, Middlewares, API Controllers
├── TradeHub.BLL/          # Use Cases, Business Services, DTOs, Mappings
├── TradeHub.DAL/          # Repositories, Entities, Database Context (Dapper)
└── TradeHub.sln           # Main Solution
```

## 🔌 API Modules

| Module | Key Endpoints |
| :--- | :--- |
| **Auth** | Login, Register, Profile (`/me`) |
| **Product** | CRUD Products, Manage Stock, Seller Dashboard |
| **Cart** | Add/Remove Items, Quantity Management, Summaries |
| **Order** | Atomic Checkout, Order History |
| **Wallet** | Balance Inquiries, Deposit/Withdraw, Transactions |

## � Getting Started

1. **Clone & Setup**:
   ```bash
   git clone https://github.com/MinhKhoa05/tradehub-backend.git
   ```
2. **Database Configuration**:
   - Install MySQL and create a database named `tradehub`.
   - Update `ConnectionStrings:Default` in `TradeHub.API/appsettings.json`.
3. **Environment**:
   - Ensure .NET 8 SDK is installed.
4. **Execution**:
   ```bash
   dotnet watch run --project TradeHub.API
   ```
   - Navigate to `/swagger` to explore the API.

## 📈 Roadmap & Improvements

- **Redis Caching**: Cache product details to reduce database load.
- **Unit & Integration Testing**: Achieve high code coverage using xUnit and Moq.
- **Logging**: Implement Serilog for structured logging to file/cloud.
- **Pagination**: Add `OFFSET` or `Seek` based pagination to listing endpoints.
- **Image Storage**: Integration with S3/Cloudinary for product images.
