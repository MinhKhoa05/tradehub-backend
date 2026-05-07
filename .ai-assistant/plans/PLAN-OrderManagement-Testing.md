# PLAN-OrderManagement-Testing

## Objective
Add deep and broad integration tests for the `PlaceOrder` functionality in `OrderApiTests.cs` to ensure business logic integrity and catch edge cases.

## Reference Memory
- Checked `workflow.md` for role-based execution.
- Checked `rules.md` for coding standards.
- Checked `learning.md` for architectural context.
- Knowledge Item: `Wallet Architecture` (for future `PayOrder` tests, though current focus is `PlaceOrder`).

## File Changes
### 1. `GameTopUp.Tests/IntegrationTests/OrderApiTests.cs`
- Add helper methods if needed (e.g., `GetStockAsync`, `SetPackageStatusAsync`).
- Implement the following test cases:
    - `PlaceOrder_HappyPath`: Successful order creation with valid data.
    - `PlaceOrder_InsufficientStock_ReturnsBadRequest`: Verify stock validation.
    - `PlaceOrder_InactivePackage_ReturnsBadRequest`: Verify availability check.
    - `PlaceOrder_OnePendingOrderLimit_ReturnsBadRequest`: Enforce the "one pending order per user" constraint.
    - `PlaceOrder_InvalidQuantity_ReturnsBadRequest`: Check for non-positive quantities.
    - `PlaceOrder_Concurrent_StockIntegrity`: (Skip for SQLite) Verify stock doesn't go negative under pressure.

## Impact / Risk
- **Impact**: Improved test coverage for the ordering flow.
- **Risk**: Low. No changes to production code. Tests might be skipped if SQLite limitations apply (e.g., `FOR UPDATE` constraints).

## Mentorship Mode (Role: DEV)
I will:
- Scaffold the test methods.
- Provide the Arrange and Assert sections.
- Leave the core `Act` and specific `Check` logic for the USER to implement as `USER_TASK` to demonstrate understanding of the ordering flow.
- Provide Socratic questions for the USER.
