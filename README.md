<div align="center">
  <h1>TradeHub Backend</h1>
  <p><strong>Backend system for a game top-up intermediary platform built with ASP.NET Core 8 & Dapper</strong></p>
</div>

![.NET 8](https://img.shields.io/badge/.NET-8.0-512bd4?style=flat-square&logo=dotnet)
![MySQL](https://img.shields.io/badge/MySQL-4479A1?style=flat-square&logo=mysql&logoColor=white)
![SQLite](https://img.shields.io/badge/SQLite-003B57?style=flat-square&logo=sqlite&logoColor=white)
![Dapper](https://img.shields.io/badge/ORM-Dapper-6d429c?style=flat-square)
![License](https://img.shields.io/badge/License-MIT-green?style=flat-square)

---

## 🚀 Project Overview

**TradeHub** is a backend system for a game top-up intermediary platform. It handles order processing for game packages and manages an internal digital wallet for payments and deposits.

Key features:
- **Order Processing**: Checkout workflows for game packages and account info handling.
- **Transaction Tracking**: Detailed history for orders and wallet transactions.
- **Internal Wallet**: Secure digital wallet for user balances with transaction atomicity.
- **Data Integrity**: Database-level concurrency controls and atomic transactions to prevent race conditions during updates.
- **Standardized Responses**: All API endpoints use an `ApiResponse` wrapper for consistent data structure.

## 🛠 Tech Stack

- **Language/Framework**: C# (ASP.NET Core 8)
- **Database**: MySQL (Production), Microsoft.Data.Sqlite (Integration Testing)
- **Data Access**: Dapper and Dommel
- **Mapping**: Mapster (configured to ignore nulls for partial updates)
- **Security**: JWT Authentication and BCrypt for password hashing
- **Testing**: xUnit, Moq, and FluentAssertions
- **Infrastructure**: Docker and Docker Compose

## 🤖 AI Workflow

The project uses a multi-agent system to manage development tasks and ensure code quality.

- **Role Isolation**: Tasks are split between agents for Business Analysis, Architecture, Development, Testing, and Review.
- **Scope Locking**: Development only starts after a plan file (`PLAN-{topic}-{subtopic}.md`) is approved.
- **Knowledge Base**: Centralized documentation in `rules.md` and `memory.md` ensures consistency with architectural decisions.

## 🏗 Architecture Overview

The application follows a layered architecture to maintain clear boundaries:

* **Presentation Layer (`TradeHub.API`)**: Handles HTTP endpoints, authorization, and uses global middleware for exception handling and response wrapping.
* **Business Logic Layer (`TradeHub.BLL`)**: Implements business rules and service coordination. It handles data mapping and business validation.
* **Data Access Layer (`TradeHub.DAL`)**: Manages MySQL operations and provides a `DatabaseContext` for transaction atomicity across repositories.

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

### Concurrency Control
- **Context**: Financial operations are vulnerable to race conditions when multiple requests update the same balance.
- **Implementation**: Uses database-level conditional updates:
  `UPDATE users SET balance = balance - @Amount WHERE id = @UserId AND balance >= @Amount`
- **Result**: Ensures balance integrity without the overhead of application-level locking.

## 🧪 Testing Strategy

The project uses a dual testing approach to ensure reliability:
- **Unit Tests**: Focus on the BLL layer by mocking repositories with Moq.
- **Integration Tests**: Verify full API flows using `Microsoft.AspNetCore.Mvc.Testing`. 
  - Uses an **In-Memory SQLite** database for data persistence during the test session.
  - Uses `ApiResponseTestWrapper<T>` to deserialize and verify the standardized API responses.

## 🏁 How to Run

1. **Configure Environment**: Create a `.env` file in the root directory with database credentials and JWT secrets.
2. **Start Services**:
   ```bash
   docker-compose up -d
   ```
3. **Run Tests**:
   ```bash
   dotnet test
   ```

## 📁 Project Structure

```text
TradeHub.sln
├── .ai-assistant/             # AI Workflow and project rules
│   ├── agents/                # Agent role prompts
│   ├── context/               # Architecture, rules, and memory
│   └── plans/                 # Approved development plans
├── TradeHub.API/              # Controllers, Middlewares, and API setup
├── TradeHub.BLL/              # Services, DTOs, and Business Logic
├── TradeHub.DAL/              # Entities and Repositories
├── TradeHub.Tests/            # Unit and Integration test projects
└── Database/                  # SQL scripts and migrations
```