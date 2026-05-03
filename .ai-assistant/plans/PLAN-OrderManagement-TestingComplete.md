# PLAN - Order Management - Testing Complete Order

Implementing comprehensive Unit and Integration tests for the "Admin Complete Order" functionality, including race condition verification.

## 🎯 Objective
- Verify the business logic for completing an order in `OrderService`.
- Verify the API endpoint behavior and database persistence in `OrderApiTests`.
- Ensure the system handles concurrent completion requests gracefully (Idempotency).

## 🧠 Reference Memory
- Confirmed check of `memory.md`, `rules.md`, and `architecture.md`.
- Following "Testing Standards" (Rule 15) and "Race Condition Safety" (Rule 16).
- Pattern: Using `OrderHistory` to verify state transitions.

## 🛠 File Changes

### Unit Tests
1.  **Modify** `GameTopUp.Tests/UnitTests/Services/OrderServiceTests.cs`:
    - Add `CompleteOrderAsync_ShouldSucceed_WhenValid`: Success case + History log verification.
    - Add `CompleteOrderAsync_ShouldBeIdempotent_WhenAlreadyCompleted`: Verify no exception and no duplicate history.
    - Add `CompleteOrderAsync_ShouldThrowForbidden_WhenNotAdmin`.
    - Add `CompleteOrderAsync_ShouldThrowBusiness_WhenNotProcessing`.
    - Add `CompleteOrderAsync_ShouldThrowBusiness_WhenAdminMismatch`.
    - Add `CompleteOrderAsync_ShouldHandleRaceCondition_ByCheckingFinalState`.

### Integration Tests
2.  **Modify** `GameTopUp.Tests/IntegrationTests/OrderApiTests.cs`:
    - Add `CompleteOrder_ShouldSucceed_WhenAdminCompletesAssignedOrder`: E2E success test.
    - Add `CompleteOrder_ConcurrentRequests_ShouldBeIdempotent`: Send 10 concurrent requests. All should return 200 OK, but only 1 `OrderHistory` should be created.

## ⚠️ Impact / Risk
- **Database State**: Integration tests use SQLite in-memory, ensuring isolation.
- **Race Conditions**: Specifically targeted in integration tests using `Task.WhenAll`.

## 🎓 Learning Mode
- I will implement all tests as requested. I will leave one test case (`CompleteOrderAsync_ShouldThrowBusiness_WhenAdminMismatch`) partially implemented as a `USER_TASK` for the user to practice Mock setup and assertion.

---
Approve this plan? (OK / Reject / Modify)
