<div align="center">
  <h1>GameTopUp Backend</h1>
  <p><strong>Structured backend system for game top-up intermediary operations</strong></p>
</div>

![.NET 8](https://img.shields.io/badge/.NET-8.0-512bd4?style=flat-square&logo=dotnet)
![MySQL](https://img.shields.io/badge/MySQL-4479A1?style=flat-square&logo=mysql&logoColor=white)
![SQLite](https://img.shields.io/badge/SQLite-003B57?style=flat-square&logo=sqlite&logoColor=white)
![Dapper](https://img.shields.io/badge/ORM-Dapper-6d429c?style=flat-square)

---

## 🚀 Project Overview

**GameTopUp** is a specialized backend platform designed to transform manual, chat-based game top-up workflows into a structured, automated intermediary system. It handles high-concurrency order orchestration, secure wallet management, and precise commission/discount tracking.

📖 **[Read the System Motivation & Goals](./MOTIVATION.md)**

Key features:
- **Order Orchestration**: Decoupled "Place Order" (Stock Reservation) and "Payment" flows.
- **Transaction Tracking**: Comprehensive audit trail for wallet transactions using `BalanceBefore` and `BalanceAfter`.
- **Internal Wallet**: Secure internal wallet using pessimistic locking to ensure consistency under concurrent operations.
- **Data Integrity**: Database-level constraints and locking mechanisms to enforce business rules such as "One Pending Order per User".
- **Standardized Responses**: All API endpoints use an `ApiResponse` wrapper for consistent data structure.

## 🛠 Tech Stack

- **Language/Framework**: ASP.NET Core 8 (C#)
- **Database**: MySQL (Production), Sqlite (Integration Testing)
- **Data Access**: Dapper, Dommel
- **Mapping**: Mapster
- **Security**: JWT Authentication, BCrypt
- **Testing**: xUnit, Moq, FluentAssertions
- **Infrastructure**: Docker, Docker Compose

## 🤖 AI Workflow

The project follows a role-based AI-assisted workflow to improve development consistency and code quality:

- **Role separation**: Defined roles for analysis, design, implementation, testing, and review
- **Plan-driven development**: Execution starts only after an approved `PLAN.md`
- **Shared knowledge base**: `rules.md` and `learning.md` maintain architectural consistency and accumulated development insights

## 🏗 Architecture Overview

The application follows a layered architecture:

* **Presentation Layer (`GameTopUp.API`)**: Handles HTTP endpoints, authentication/authorization, and global middleware (exception handling, response wrapping).
* **Application Services Layer (`GameTopUp.BLL/ApplicationServices`)**: Orchestrates business workflows and ensures transactional consistency across services.
* **Business Service Layer (`GameTopUp.BLL/Services`)**: Encapsulates business rules, validations, and entity workflows while interacting with repositories for data operations.
* **Data Access Layer (`GameTopUp.DAL`)**: Handles database operations and data persistence using repositories (MySQL via Dapper), and supports transaction management through `DatabaseContext`.

### Exception to HTTP Mapping
| Exception Type | HTTP Status | Use Case |
| :--- | :--- | :--- |
| `NotFoundException` | 404 | When a resource like a User or Game is not found |
| `BusinessException` | 400 | For business rule violations like insufficient balance |
| `UnauthorizedException`| 401 | When authentication fails or tokens expire |
| `ForbiddenException` | 403 | When a user lacks permission for a resource |

## ⚙️ Key Engineering Decisions

### Transaction Management
- **Context**: Multi-step database operations (e.g., checkout) require atomicity to prevent partial data updates.
- **Implementation**: `DatabaseContext` provides an `ExecuteInTransactionAsync()` wrapper that shares a single connection and transaction across multiple repository calls.

### Concurrency Control (Pessimistic Locking)
- **Problem**: Financial operations (Wallet) and Inventory (Stock) are vulnerable to race conditions.
- **Implementation**: Uses database-level **Pessimistic Locking** (`SELECT ... FOR UPDATE`).
  - Wallet: Locked during credit/debit to ensure balance integrity.
  - Orders: Locked during payment/cancellation to prevent state conflicts.
- **Result**: Guarantees absolute data consistency even under high concurrent load.

### Decoupled Order Workflow
- **Flow**: `Place Order` (Stock Reservation) → `Payment` (Wallet Debit & Mark Paid).
- **Rationale**: Immediate inventory reservation prevents overselling, while separate payment allows for flexible checkout experiences.
- **Integrity**: Automatic inventory restoration and wallet refunds on order cancellation, handled via atomic UseCase transactions.

### Audit Trail
- **Wallet**: Every transaction records `BalanceBefore` and `BalanceAfter`.
- **Traceability**: All financial movements are linked to specific `OrderId` or transaction reasons for transparent accounting.

## 🧪 Testing Strategy

The project uses a dual testing approach to ensure reliability:
- **Unit Tests**: Focus on the BLL layer by mocking repositories with Moq.
- **Integration Tests**: Verify full API flows using `Microsoft.AspNetCore.Mvc.Testing`. 
  - Uses an **In-Memory SQLite** database for fast and isolated test execution.
  - Implements customized `AuthenticationHandler` to simulate multi-user scenarios and role-based access.
  - Uses `ApiResponseTestWrapper<T>` for strongly-typed verification of standardized responses.

## 🏁 How to Run

### 1. Environment Setup

Create a `.env` file in the root directory based on the provided example configuration.

It includes:
- **Database**: MySQL via Docker
- **API Settings**: Ports, environment
- **Security**: JWT authentication settings
- **CORS**: Domain configuration

Refer to `.env.example` for full details.

---

### 2. Start Services

Run the application using Docker Compose:
   ```bash
   docker-compose up -d
   ```

- MySQL: Container starts automatically
- Initialization: Database schema and seed data are initialized on startup.
- Connectivity: API connects to the database using the configured connection string.

### 3. Run Tests
Integration tests use an isolated SQLite database and mocked authentication to ensure environment consistency.
   ```bash
   dotnet test
   ```

## 📁 Project Structure

```text
GameTopUp.sln
├── .ai-assistant/             # AI Workflow and project rules
│   ├── agents/                # Agent role prompts
│   ├── context/               # Architecture, rules, and memory
│   └── plans/                 # Approved development plans
├── GameTopUp.API/             # Controllers, Middlewares, and API setup
├── GameTopUp.BLL/             # Logic Layer
│   ├── ApplicationServices/   # UseCases (Transaction Orchestration)
│   ├── Services/              # Domain Services (Business Rules)
│   └── DTOs/                  # Data Transfer Objects
├── GameTopUp.DAL/             # Data Access Layer
│   ├── Repositories/          # Dapper-based data operations
│   └── Entities/              # Database models
├── GameTopUp.Tests/           # Unit and Integration test projects
└── Database/                  # SQL scripts and seed data
```