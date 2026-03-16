<div align="center">
  <h1>TradeHub Backend</h1>
  <p><strong>Backend system for a peer-to-peer marketplace built with ASP.NET Core 8 & Dapper</strong></p>
</div>

![.NET 8](https://img.shields.io/badge/.NET-8.0-512bd4?style=flat-square&logo=dotnet)
![MySQL](https://img.shields.io/badge/MySQL-4479A1?style=flat-square&logo=mysql&logoColor=white)
![Dapper](https://img.shields.io/badge/ORM-Dapper-6d429c?style=flat-square)
![License](https://img.shields.io/badge/License-MIT-green?style=flat-square)

---

## 🚀 Project Overview

**TradeHub** is a backend application for a peer-to-peer e-commerce marketplace built with ASP.NET Core 8 and MySQL.

The system provides REST APIs for managing users, products, shopping carts, orders, and a digital wallet for payments.

The primary technical focus of the project is maintaining data consistency and preventing race conditions during multi-step business transactions such as checkout and wallet deductions.

The project uses Dapper for explicit SQL control while maintaining a clean layered architecture.

## 🛠 Tech Stack

* **Language**: C# (ASP.NET Core 8+)
* **Micro-ORM**: Dapper
* **Database**: MySQL (`MySqlConnector`)
* **Security & Auth**: JWT (JSON Web Tokens), BCrypt password hashing
* **Architecture**: Layered Architecture (API → Application Services → Domain Services → Repositories)

## 🏗 Architecture Overview

The application is structured into three primary layers to establish strict separation of concerns:

* **Presentation Layer (`TradeHub.API`)**: Exposes API endpoints. It handles HTTP request parsing, JWT authorization, and global exception mapping via middleware.

* **Business Logic Layer (`TradeHub.BLL`)**: Contains domain rules and workflow orchestrators. Domain services (e.g., `WalletService`, `ProductService`) handle logic specific to a single entity. Application services (e.g., `OrderUsecase`) orchestrate workflows that require coordination across multiple domain services.

* **Data Access Layer (`TradeHub.DAL`)**: Manages MySQL database interactions. It implements the Repository pattern to encapsulate SQL queries using Dapper. A custom `DatabaseContext` manages database connections and transaction lifecycles.

---

## ⚙️ Key Engineering Decisions

Some notable backend design choices in this project:

* **Custom Transaction Wrapper**  
  Implemented `ExecuteInTransactionAsync()` in `DatabaseContext` to simplify transaction management. It wraps multi-step operations inside a single transaction, allowing use cases to execute multiple repository calls atomically.

* **Concurrency-Safe Updates**  
  Financial operations (such as wallet balance updates) use conditional SQL statements like  
  `UPDATE wallets SET balance = balance - @Amount WHERE user_id = @UserId AND balance >= @Amount`  
  to prevent race conditions during concurrent requests.

* **Custom DatabaseContext**  
  A lightweight wrapper around `MySqlConnection` that provides:
  - lazy connection opening
  - shared transaction handling
  - async resource cleanup via `IAsyncDisposable`
  - simple async helper methods for common Dapper queries

* **Usecase Orchestration**  
  Complex workflows (for example `OrderUsecase`) coordinate multiple services such as product validation, order creation, and cart updates while keeping individual domain services focused on their own responsibilities.

* **Centralized Error Handling**  
  `GlobalExceptionMiddleware` captures domain exceptions (`BusinessException`, `NotFoundException`) and converts them into consistent HTTP responses.

## 📦 Core Modules

**Auth & Security**  
Implements JWT-based authentication, identity claims management, and secure password storage using BCrypt hashing.

**Product Catalog**  
Manages product listings, inventory data, and stock validation during the checkout process.

**Cart Management**  
Handles temporary cart items associated with a user before checkout.

**Order Orchestration**  
Coordinates the checkout workflow and splits a single cart into multiple orders grouped by `SellerId`.

**Wallet System**  
Manages user balances, deposits, and payment deductions. Order payments are executed within database transactions to ensure atomic balance updates.

## 🔄 Example System Workflow

### Checkout Flow

The checkout process is coordinated by the `OrderUsecase` and executed within a single database transaction to ensure data consistency.

1. The user's cart items are retrieved.
2. Product data is fetched in bulk to validate availability and pricing.
3. A database transaction is started via `DatabaseContext.ExecuteInTransactionAsync`.
4. Product stock is validated and decremented.
5. Cart items are grouped by `SellerId`.
6. Separate `Order` records are created for each seller.
7. Corresponding `OrderItem` and initial `OrderHistory` records are inserted.
8. The user's cart is cleared.
9. The transaction is committed.

If any step fails (for example, due to insufficient stock or a database error), the transaction is rolled back to maintain system consistency.

## 📁 Project Structure

```text
TradeHub.sln
├── TradeHub.API/              # Controllers, Middlewares, DI Setup
│   ├── Controllers/
│   ├── Middlewares/           # GlobalExceptionMiddleware
│   └── Program.cs
├── TradeHub.BLL/              # Business Logic & Orchestration
│   ├── ApplicationServices/   # Cross-domain Usecases (Auth, Order, CartView)
│   ├── Services/              # Domain Rules (Token, Wallet, Product, User)
│   ├── DTOs/
│   └── Exceptions/            # Custom BusinessExceptions
└── TradeHub.DAL/              # Data Access
    ├── Entities/              # POCO classes mapping to tables
    ├── Repositories/          # Dapper queries mapped by domain
    └── DatabaseContext.cs     # IAsyncDisposable MySql Wrapper
```

## 🌐 Example API Endpoints

Sample endpoints available in the system:

* `POST /api/auth/login` - Authenticate and retrieve JWT payload
* `GET /api/products` - Fetch active product catalog
* `POST /api/cart` - Add an item to the shopping cart
* `POST /api/orders/checkout` - Create multi-seller orders from the current cart
* `POST /api/wallet/deposit` - Top up wallet balance

## 🏁 Getting Started

1. **Clone the repository**:
   ```bash
   git clone https://github.com/MinhKhoa05/tradehub-backend.git
   ```
2. **Configure Database**:
   - Ensure a MySQL server is running.
   - Execute the provided SQL schema scripts to create the required tables.
   - Update `appsettings.json` in `TradeHub.API` with your MySQL `Default` connection string.
3. **Configure JWT settings** in `appsettings.json` (Issuer, Audience, Secret Key).
4. **Run the API**:
   ```bash
   cd TradeHub.API
   dotnet run
   ```
5. Navigate to `https://localhost:<port>/swagger` to view the interactive API documentation.

## Possible Improvements

* **Distributed Caching**  
  Introduce Redis to cache frequently accessed data such as the product catalog and user carts, reducing read load on the MySQL database.

* **Asynchronous Order Processing**  
  Move non-critical tasks (e.g., notifications or heavy order-processing logic) to background workers using a message broker such as RabbitMQ.

* **Database Schema Migrations**  
  Introduce a migration tool (e.g., DbUp or EF Core Migrations) to version-control database schema changes and simplify deployments.

## Technical Notes

This project explores several backend engineering concepts:

- **Database-level concurrency control**  
  Wallet deductions rely on conditional SQL updates (`balance >= @Amount`) to prevent race conditions during concurrent checkout requests.

- **Micro-ORM tradeoffs**  
  Using Dapper provides explicit control over SQL execution but requires careful management of connections and transactions, addressed through the custom `DatabaseContext`.

- **Centralized exception handling**  
  `GlobalExceptionMiddleware` removes repetitive try-catch logic from controllers and ensures consistent HTTP error responses.