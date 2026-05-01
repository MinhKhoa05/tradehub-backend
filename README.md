<div align="center">
  <h1>TradeHub Backend</h1>
  <p><strong>Backend system for a game top-up intermediary platform built with ASP.NET Core 8 & Dapper</strong></p>
</div>

![.NET 8](https://img.shields.io/badge/.NET-8.0-512bd4?style=flat-square&logo=dotnet)
![MySQL](https://img.shields.io/badge/MySQL-4479A1?style=flat-square&logo=mysql&logoColor=white)
![Dapper](https://img.shields.io/badge/ORM-Dapper-6d429c?style=flat-square)
![License](https://img.shields.io/badge/License-MIT-green?style=flat-square)

---

## 🚀 Project Overview

**TradeHub** is a backend system designed to automate and secure the game top-up process. Evolving from a generic P2P marketplace, TradeHub now operates as a specialized **game top-up intermediary**. 

It was built to replace manual, chat-based top-up workflows that often led to missed orders, delayed processing, and financial discrepancies. By centralizing operations, the system solves these operational bottlenecks through:
- **Automated Order Handling**: Streamlines checkout workflows for game packages, eliminating the need for manual chat confirmations.
- **Real-Time Status Tracking**: Provides transparent order states and comprehensive transaction histories.
- **Wallet-Based Transactions**: Uses an internal digital wallet for deposits, withdrawals, and instant payments.
- **Transaction Integrity**: Enforces strict database-level concurrency controls and atomic transactions to prevent race conditions and ensure accurate balance management.

## 🛠 Tech Stack

- **Language**: C# (.NET 8)
- **Database**: MySQL
- **ORM**: Dapper, Dommel
- **Security**: JWT Authentication, BCrypt
- **Infrastructure**: Docker & Docker Compose

## 🏗 Architecture Overview

The application is structured into three primary layers to establish strict separation of concerns:

* **Presentation Layer (`TradeHub.API`)**: Exposes API endpoints. It handles HTTP request parsing, JWT authorization, and global exception mapping via middleware.
* **Business Logic Layer (`TradeHub.BLL`)**: Contains domain rules and workflow orchestrators. Domain services (e.g., `WalletService`, `GamePackageService`) handle logic specific to a single entity. Application workflows coordinate multiple domain services for complex tasks like checking out game packages.
* **Data Access Layer (`TradeHub.DAL`)**: Manages MySQL database interactions. It implements the Repository pattern to encapsulate SQL queries using Dapper. A custom `DatabaseContext` manages database connections and transaction lifecycles.

## ⚙️ Key Engineering Decisions

### Custom Transaction Wrapper
- **Problem**: Managing multi-step database operations (like wallet deductions combined with order creations and transaction logging) across various repositories often leads to leaked connections or inconsistent data states if a single step fails.
- **Approach**: Implemented `ExecuteInTransactionAsync()` inside a custom `DatabaseContext`. This wrapper shares a single database connection and transaction lifecycle across multiple repository calls.
- **Why it matters**: Guarantees atomicity for complex workflows. If any repository operation fails (e.g., due to insufficient funds), the entire transaction rolls back, ensuring system data integrity without cluttering business logic with connection management.

### Concurrency-Safe Updates
- **Problem**: Financial operations (e.g., wallet deductions) are prone to race conditions when concurrent requests attempt to modify the same user balance simultaneously.
- **Approach**: Utilized database-level conditional SQL updates instead of application-level locking. 
  `UPDATE users SET balance = balance - @Amount WHERE id = @UserId AND balance >= @Amount`
- **Why it matters**: Eliminates race conditions efficiently at the database layer. Prevents negative balances and ensures safe concurrent request handling without the performance overhead of distributed locks.

## 🔌 Example API Flows

### 1. Deposit to Wallet
`POST /api/wallet/transactions/deposit`
```json
500000
```
**Why it matters**: Directly tops up the authenticated user's wallet and automatically logs a transaction history record within a single ACID-compliant database transaction.

### 2. Pay for Orders
`POST /api/wallet/pay`
```json
{
  "orderIds": [101, 102],
  "totalAmount": 250000
}
```
**Why it matters**: Deducts money securely from the user's wallet. It uses database-level concurrency checks (`balance >= @Amount`) to prevent negative balances during simultaneous payment requests.

### 3. Checkout a Game Package
`POST /api/orders/checkout`
```json
{
  "gameAccountInfo": "Player123_ServerAsia"
}
```
**Why it matters**: Automates the order creation process, linking it with the provided game account details, replacing the manual chat-based order intake.

## 🤖 AI-Assisted Workflow

This project utilizes **Antigravity AI** as an active development tool:
- **Optimization**: Streamlining Dockerfile setups for containerized deployment.
- **Refactoring**: Enforcing strict architectural standards and transitioning the project to a modular feature-based structure.
- **Value**: Speeds up the development lifecycle, allowing engineers to focus primarily on core business logic and system design rather than boilerplate tasks.

## 🏁 How to Run

The application is fully containerized. No local .NET or MySQL installation is required.

**Step 1: Configure Environment**  
Create a `.env` file in the root directory to configure database credentials and JWT secrets.

**Step 2: Start Services**  
```bash
docker-compose up -d
```

*The database schema is initialized automatically on startup. No additional manual setup is required.*

## 📁 Project Structure

```text
TradeHub.sln
├── .ai-assistant/             # AI workflows, memory, and architectural guidelines
├── docker-compose.yml         # Container orchestration configuration
├── TradeHub.API/              # HTTP entry points: Controllers, Middlewares, DI Setup
│   ├── Controllers/
│   ├── Middlewares/           # GlobalExceptionMiddleware
│   └── Program.cs
├── TradeHub.BLL/              # Business Logic & Orchestration
│   ├── ApplicationServices/   # Cross-domain workflows
│   ├── Services/              # Domain Rules (Wallet, Game, GamePackage, User)
│   ├── DTOs/
│   └── Exceptions/            # Custom BusinessExceptions
├── TradeHub.DAL/              # Data Access
│   ├── Entities/              # POCO classes mapping to tables
│   ├── Repositories/          # Dapper queries mapped by domain
│   ├── DatabaseContext.cs     # IAsyncDisposable wrapper for connections and transactions
│   └── DatabaseContextExtension.cs # Custom Dapper extensions for dynamic query building
└── TradeHub.Tests/            # Unit tests and integration tests
```