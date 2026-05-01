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

**TradeHub** is a backend system for a game top-up intermediary platform. It handles order processing and supports wallet-based transactions.

Key features:
- **Order Processing**: Checkout workflows for game packages.
- **Transaction Tracking**: Transaction history and order state management.
- **Internal Wallet**: Digital wallet for user deposits, withdrawals, and payments.
- **Data Integrity**: Database-level concurrency controls and atomic transactions to prevent race conditions during balance updates.

## 🛠 Tech Stack

- **Language**: C# (ASP.NET Core 8)
- **Database**: MySQL
- **ORM**: Dapper, Dommel
- **Security**: JWT Authentication, BCrypt
- **Infrastructure**: Docker & Docker Compose

## 🤖 AI Workflow

This project uses a multi-agent system to control development execution.

- **Role Isolation**: Separate agents for Business Analysis, Architecture, Development, Testing, and Code Review.
- **Approval Gate**: Execution requires an approved `PLAN.md`. All changes must follow the approved scope.
- **Context Usage**: Agents use centralized documentation (`rules.md`, `architecture.md`, `memory.md`) to ensure consistency.

## 🏗 Architecture Overview

The application follows a layered architecture to separate concerns:

* **Presentation Layer (`TradeHub.API`)**: HTTP endpoints, JWT authorization, and global exception handling.
* **Business Logic Layer (`TradeHub.BLL`)**: Domain rules and service coordination. Domain services handle single-entity logic, while application services coordinate cross-domain workflows.
* **Data Access Layer (`TradeHub.DAL`)**: MySQL database operations using Dapper. A custom `DatabaseContext` manages connections and transactions.

## ⚙️ Key Engineering Decisions

### Transaction Management
- **Context**: Multi-step database operations require atomic execution to maintain data consistency.
- **Implementation**: A custom `DatabaseContext` provides an `ExecuteInTransactionAsync()` wrapper. This shares a single connection and transaction across multiple repository calls.
- **Result**: Guarantees atomicity for workflows like order checkout and wallet deduction. Connection management is abstracted away from the business logic.

### Concurrency Control
- **Context**: Financial operations are vulnerable to race conditions under concurrent load.
- **Implementation**: Database-level conditional SQL updates are used instead of application-level locks:
  `UPDATE users SET balance = balance - @Amount WHERE id = @UserId AND balance >= @Amount`
- **Result**: Prevents negative balances and handles concurrent requests at the database layer, avoiding the overhead of distributed locking.

## 🔌 Example API Flows

### 1. Deposit to Wallet
`POST /api/wallet/transactions/deposit`
```json
500000
```
**Function**: Increases user balance and records the transaction history in a single transaction.

### 2. Pay for Orders
`POST /api/wallet/pay`
```json
{
  "orderIds": [101, 102],
  "totalAmount": 250000
}
```
**Function**: Deducts funds with database-level concurrency checks to prevent negative balances.

### 3. Checkout a Game Package
`POST /api/orders/checkout`
```json
{
  "gameAccountInfo": "Player123_ServerAsia"
}
```
**Function**: Creates an order linked to game account details for automated processing.

## 🏁 How to Run

The application runs in Docker containers. Local .NET or MySQL installations are not required.

1. **Configure Environment**: Create a `.env` file in the root directory with database credentials and JWT secrets.
2. **Start Services**:
   ```bash
   docker-compose up -d
   ```

*Database schema initialization runs automatically on startup.*

## 📁 Project Structure

```text
TradeHub.sln
├── .ai-assistant/             # Multi-agent workflow configuration
│   ├── agents/                # Agent prompts and rules
│   ├── context/               # System architecture and coding guidelines
│   ├── plans/                 # Approved execution plans
│   └── workflows.md           # Pipeline definitions
├── docker-compose.yml         # Container configuration
├── TradeHub.API/              # HTTP API, middleware, DI setup
│   ├── Controllers/
│   ├── Middlewares/           
│   └── Program.cs
├── TradeHub.BLL/              # Business logic layer
│   ├── ApplicationServices/   # Cross-domain workflows
│   ├── Services/              # Domain services
│   ├── DTOs/
│   └── Exceptions/            
├── TradeHub.DAL/              # Data access layer
│   ├── Entities/              # Database models
│   ├── Repositories/          # Dapper queries
│   ├── DatabaseContext.cs     # Connection and transaction management
│   └── DatabaseContextExtension.cs # Dapper extensions
└── TradeHub.Tests/            # Test suite
```