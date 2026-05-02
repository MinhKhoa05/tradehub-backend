# 🚀 GameTopUp Backend Rules (Elite Edition)

Rules for all AI Agents working on GameTopUp Backend. Mandatory.

## 1. Coding Style
**Naming**
- Classes/Interfaces/Methods: PascalCase
- Private fields: _camelCase
- Variables: camelCase
- Constants: PascalCase

**Async**
- All I/O must use async/await. Method suffix: `Async`.
- Never use `.Result` or `.Wait()`.

**Null Safety**
- Use nullable types where needed. Always null-check DB results.

**Errors & Exceptions**
- Service/Repo: throw `BusinessException` (or derived).
- API: handled by `GlobalExceptionMiddleware`. All success responses use `ApiResponse`.

| Exception Type | HTTP Status | Description |
| :--- | :--- | :--- |
| `NotFoundException` | 404 Not Found | Khi không tìm thấy tài nguyên (ID, Email, v.v.) |
| `BusinessException` | 400 Bad Request | Lỗi nghiệp vụ (Email tồn tại, không đủ số dư, v.v.) |
| `UnauthorizedException` | 401 Unauthorized | Lỗi xác thực hoặc Token hết hạn |
| `ForbiddenException` | 403 Forbidden | Không có quyền truy cập tài nguyên |
| `Internal Exception` | 500 Error | Lỗi hệ thống không xác định |

**Comments**
- Explain WHY (Rationale), not WHAT.
- Use explicit code blocks `{ ... }` for business logic; avoid expression-bodied `=>` for complex methods.

## 2. Data Access (Dapper/Dommel)
- **Usage**: Dommel for simple CRUD, Dapper for complex queries.
- **Mapping**: snake_case DB ↔ PascalCase C#. Auto mapping enabled via `MatchNamesWithUnderscores`.
- **SQL Rules**: Always parameterized (@Param). Inline SQL allowed in Repository only.
- **Search Optimization**: Always query on `normalized_name` columns. Never use SQL `LOWER()`/`UPPER()` in `WHERE` clauses (prevents Index usage).
- **Connection**: Managed by `DatabaseContext` (Scoped). Never manually open/close connection.

## 3. Repository Rules
- Data access only. No business logic.
- Return Entity or primitive types only. No cross-service calls.
- **Soft Delete**: Use `is_active = 0` for entities with transactional history (e.g., User) to preserve data integrity.

## 4. Transaction (Unit of Work)
- Use `ExecuteInTransactionAsync` for multi-step writes.
- Typically implemented in Service or UseCase layer.
- Auto commit/rollback managed by `DatabaseContext`.

## 5. Service / BLL Rules
- Service = business orchestration layer.
- Must NOT contain raw data access logic.
- Avoid deep service chaining (max 2 levels).
- **Service Isolation (Data Ownership)**: Each Service may ONLY inject and access its own Repository. Absolutely NO Cross-Repo (injecting a Repository belonging to another domain). See `memory.md [2026-05-02]`.
- **UseCase Pattern**: When a business flow requires interaction between 2 or more Services, a dedicated `UseCase` or `ApplicationService` MUST be created to orchestrate. Services must NOT call other Services directly.

## 6. Validation Rules
- **Controller (Request Validation)**: Only validate syntax and mandatory fields using `DataAnnotations` (e.g., `[Required]`, `[EmailAddress]`).
- **Service/BLL (Business Validation)**: Responsible for all logic-based validation (e.g., checking uniqueness in DB, checking balance).
- **Repository**: Must NOT perform any validation.

## 7. DTO Rules
- **Mandatory**: All write operations MUST return DTO. No anonymous/dynamic/object returns.
- **Mapping Boundary**: Mapping done in Service/UseCase only. Controllers never map DTOs.
- **Direction**: DTO = API boundary (input/output). Entities = internal DAL usage only.
- **Naming**: Use `ResponseDTO` or `ResultDTO` suffixes.

## 8. Response Rules
- All API responses MUST be wrapped in `ApiResponse`.
- No raw DTO return from controller.
- Consistent response format across the entire system.

## 9. Forbidden Rules
- No EF Core / ORMs other than Dapper/Dommel.
- No external patterns (CQRS, MediatR, Generic Repo).
- No logic in Controller.
- No logging in Repository layer.
- No implicit user identity (must use `UserContext`/`userId`).

## 10. Logging Rules
- Logging allowed only in Service/BLL layer.
- Used for debugging or business tracking only.
- Never log inside Repository.

## 11. Architecture Flow
`Client → API → Middleware → Controller → BLL → DAL → DB`

- **Controller**: Input/Output handling only.
- **BLL**: Business logic, validation, and orchestration.
- **DAL**: Raw data access and persistence.

## 12. Core Principle
> Strict separation of concerns, clear layer boundaries, predictable execution flow.

## 13. Mapping Rules (Mapster)
- **Standard**: Mapster is the required library for all object-to-object mapping.
- **Auto-Mapping Scope**: Use `request.Adapt(entity)` for simple property-to-property updates.
- **Explicit-Mapping Scope**: Complex logic (conditional mapping, flattening) MUST be defined in `MapsterConfig.cs`.
- **Global Config**: `IgnoreNullValues(true)` is mandatory to prevent partial updates from overwriting with null.

## 14. Planning & Knowledge Management
- **Strict Naming**: All tasks must have a dedicated PLAN file using the format: `PLAN-{topic}-{subtopic}.md`. (e.g., `PLAN-UserManagement-API.md`).
- **Context Awareness**: Every PLAN must have a `Reference Memory` section confirming review of `memory.md` and `rules.md`.
- **Update Memory**: After finishing a task, important lessons must be recorded back into `memory.md`.

## 15. Testing Standards
- **SQLite In-Memory**: Connection persistence must be managed via `TestDatabaseContext` (overriding `Dispose`) to prevent schema loss.
- **Source of Truth**: `CustomWebApplicationFactory` must maintain an up-to-date schema script including all audit and normalization columns.
- **API Deserialization**: Integration tests must use `ApiResponseTestWrapper<T>` to handle wrapped responses.
- **State Verification**: Tests must verify actual database changes (e.g., checking `IsActive` for soft delete) rather than just asserting HTTP status codes.
- **Consistency**: All Test Classes must call `MapsterConfig.RegisterMappings()` in their constructor.

## 16. Race Condition Safety
- **Assume Concurrency**: Always assume Race Conditions can occur at any step, even after a logic check (`if exists`).
- **DB Constraint as Last Line of Defense**: For any resource creation with a `UNIQUE` constraint, a `try-catch` block MUST be present to handle database-level violations.
- **Friendly Error**: `UNIQUE`/`Duplicate` exceptions from the database MUST be caught and re-thrown as a `BusinessException` with a user-friendly message before reaching the client.

```csharp
// Standard Pattern: Logic Check + DB Constraint Safety
try
{
    return await _repo.CreateAsync(entity);
}
catch (Exception ex) when (ex.Message.Contains("Duplicate", StringComparison.OrdinalIgnoreCase) ||
                           ex.Message.Contains("UNIQUE", StringComparison.OrdinalIgnoreCase))
{
    // Rationale: Race condition safety - DB constraint catches concurrent duplicate creation.
    throw new BusinessException("This resource already exists or is being processed.");
}
```