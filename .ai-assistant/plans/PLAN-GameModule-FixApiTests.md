# PLAN-GameModule-FixApiTests

## Objective
Fix failing integration tests in `GameApiTests.cs` and ensure they align with the project's architectural standards (ApiResponse wrapper, standardized exceptions).

## Reference Memory
- Checked `memory.md`: Confirmed previous issues with SQLite schema and connection persistence.
- Checked `rules.md`: Confirmed standards for ApiResponse (Rule 87) and Mapping (Rule 124).

## File Changes

### 1. TradeHub.Tests (Integration Tests)
- **Modify** `CustomWebApplicationFactory.cs`: 
    - Update `CREATE TABLE games` to include `created_at` and `updated_at` columns.
- **Modify** `GameApiTests.cs`:
    - Update deserialization logic to read from `ApiResponse.Data`.
    - Update `GetGameById_ShouldReturnNotFound` assertion to expect `404 NotFound` (instead of currently failing with 400).
    - Fix `DeleteGame_ShouldDeleteCascadePackages` to properly extract the ID from the created game response.

### 2. TradeHub.BLL (Business Logic)
- **Modify** `Services/GameService.cs`:
    - Change `BusinessException` to `NotFoundException` in `GetGameByIdAsync`.

## Impact / Risk
- **Impact**: All integration tests for Games will pass, ensuring reliability of the API.
- **Risk**: Low. These are targeted changes to the test infrastructure and a minor fix in the service layer.

## Validation Plan
1. Run `dotnet test TradeHub.Tests --filter GameApiTests` to verify all 6 tests pass.
2. Run all integration tests to ensure no regressions.
