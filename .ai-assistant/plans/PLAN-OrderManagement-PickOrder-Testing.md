# Plan - Testing: Order Management Pick Order

Implement unit tests for the Admin Pick Order feature.

## Objective
- Verify `OrderService.PickOrderAsync` behavior.
- Cover success, race condition, not found, and forbidden scenarios.

## Reference Memory
- Verified `OrderService` dependencies: `IOrderRepository`, `IOrderHistoryRepository`, `DatabaseContext`.
- Using `Xunit`, `Moq`, `FluentAssertions`.

## File Changes

### 1. GameTopUp.Tests
- **Create `UnitTests/Services/OrderServiceTests.cs`**:
    - `PickOrderAsync_ShouldSucceed_WhenOrderIsPendingAndAdminPicks`: Verify success flow and history creation.
    - `PickOrderAsync_ShouldThrowBusinessException_WhenRaceConditionOccurs`: Verify handling of `affectedRows == 0`.
    - `PickOrderAsync_ShouldThrowNotFoundException_WhenOrderDoesNotExist`: Verify check for non-existent order.
    - `PickOrderAsync_ShouldThrowForbiddenException_WhenUserIsNotAdmin`: Verify role-based security.

## Impact / Risk
- **No production code changes**: Purely adding tests.
- **Improved Reliability**: Ensures race condition logic works as intended.

## Verification Plan
- Run `dotnet test` and ensure all tests in `OrderServiceTests.cs` pass.
