# TradeHub Backend Architecture

## 🎯 Overview
Backend system for game top-up, orders, wallet payments, and user management.

- Stack: .NET 8, Dapper/Dommel, MySQL, JWT
- Architecture: Layered + Clean principles

---

## 🧱 Layers

### API Layer (TradeHub.API)
- Controllers: handle requests
- Middlewares: auth, error handling
- Filters: validation
- Program.cs: DI + config

### BLL (TradeHub.BLL)
- Services: business logic
- UseCases: complex flows
- DTOs: data transfer
- Common: ServiceResult, UserContext

### DAL (TradeHub.DAL)
- Entities: DB mapping (snake_case ↔ PascalCase)
- Repositories: Dapper queries
- DatabaseContext: connection + transaction (Unit of Work)

---

## 🔄 Request Flow
Client → API → Middleware → Controller → BLL → Repository → DB → Response

- Business errors → BusinessException
- Global errors → middleware handler
- Response standard: ApiResponse

---

## 🗄 Database
- MySQL
- Mapping: snake_case ↔ PascalCase
- Dapper: MatchNamesWithUnderscores = true

---

## 💳 Transaction Model
- DatabaseContext = Scoped UoW
- ExecuteInTransactionAsync handles commit/rollback
- All multi-table writes MUST use transaction

---

## 🧩 Patterns
- Repository Pattern
- UseCase Pattern
- Exception-driven flow
- Standard ApiResponse
- SOLID principles

---

## ⚙️ DI
- Scoped: DatabaseContext, Services, Repos
- Central registration in Program.cs

---

## 🔧 Config
- appsettings.json
- JWT config
- CORS config

---

## 📌 Rules for Agents
- BLL = business logic only
- DAL = data access only
- API = request handling only
- Always use UserContext
- Use transaction for multi-step writes

---

## 🧠 Core Principle
Separation of concerns + predictable data flow