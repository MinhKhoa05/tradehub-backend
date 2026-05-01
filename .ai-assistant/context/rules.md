# 🚀 TradeHub Backend Rules (Final + Complete)

Rules for all AI Agents working on TradeHub Backend. Mandatory.

## 1. Coding Style
**Naming**
- Classes/Interfaces/Methods: PascalCase
- Private fields: _camelCase
- Variables: camelCase
- Constants: PascalCase

**Async**
- All I/O must use async/await
- Method suffix: Async
- Never use .Result / .Wait()

**Null Safety**
- Use nullable types where needed
- Always null-check DB results

**Errors**
- Service/Repo: throw BusinessException (or derived)
- API: handled by GlobalExceptionMiddleware
- All success responses use ApiResponse

**Comments**
- Explain WHY, not WHAT
- No redundant code description

## 2. Data Access (Dapper/Dommel)
**Usage**
- Dommel: simple CRUD
- Dapper: complex queries

**Mapping**
- snake_case DB ↔ PascalCase C#
- Auto mapping enabled

**SQL Rules**
- Always parameterized (@Param)
- Inline SQL allowed in Repository only

**Connection**
- Managed by DatabaseContext (Scoped)
- Never manually open/close connection

## 3. Repository Rules
- Data access only
- No business logic
- Return Entity or primitive types only
- No cross-service calls inside repository

## 4. Transaction (Unit of Work)
- Use ExecuteInTransactionAsync for multi-step writes
- Typically in UseCase layer
- Auto commit/rollback via DatabaseContext

## 5. Service / BLL Rules (NEW)
- Service = business orchestration layer
- Can call multiple repositories
- Must NOT contain raw data access logic
- Avoid deep service chaining (no service calling service chain > 2 levels)

## 6. Validation Rules (NEW)
- Input validation must be done in Service/BLL layer before execution
- Controller must NOT validate business rules
- Repository must NOT validate input

## 7. DTO Rules
**Mandatory**
- All write operations MUST return DTO
- No anonymous / dynamic / object return
- Mapping done in Service/UseCase only
- Controllers never map DTOs

**Direction Rule (NEW)**
- DTO = API boundary only (input/output)
- Entities = internal DAL usage only

**Naming**
- Use ResponseDTO / ResultDTO

**Exception**
- Primitive return allowed only for simple checks (Exists/IsValid)

## 8. Response Rules (NEW)
- All API responses MUST be wrapped in ApiResponse
- No raw DTO return from controller
- Consistent response format across entire system

## 9. Transaction (Unit of Work)
- Use ExecuteInTransactionAsync for multi-step writes
- Typically in UseCase layer
- Auto commit/rollback via DatabaseContext

## 10. Forbidden
- No EF Core / ORMs other than Dapper/Dommel
- No external patterns (CQRS, MediatR, Generic Repo)
- No logic in Controller
- No new dependencies without approval
- No implicit user identity (must use UserContext/userId)
- No duplicate logic or rule repetition in responses
- No logging in Repository layer

## 11. Logging Rules (NEW)
- Logging allowed only in Service/BLL layer
- Used for debugging or business tracking only
- Never log inside Repository

## 12. Architecture Flow

`Client → API → Middleware → Controller → BLL → DAL → DB`

- Controller: input/output only
- BLL: business logic + validation + orchestration
- DAL: data access only

## 13. Core Principle

> Strict separation of concerns, clear layer boundaries, predictable execution flow.