# PLAN-OrderManagement-IntegrationTesting

## Objective
Implement comprehensive **Integration Tests** for Order management, specifically focusing on verifying system behavior under **Race Condition** scenarios.

---

## Reference Memory

- ✅ Reviewed `memory.md`: SQLite In-memory Lifecycle (TestDatabaseContext); State Verification strategy (verify DB changes); Race Condition Awareness (try-catch and atomic SQL).
- ✅ Reviewed `rules.md`: Testing standards (SQLite In-Memory, State Verification).

---

## Design Decisions

### Race Condition Testing Strategy
We will use `Task.WhenAll` to simulate concurrent API requests.
- **Goal**: Ensure that out of $N$ concurrent requests, only **one** succeeds in performing a state transition, while others fail gracefully or return idempotent success where applicable.

### Scenarios to Cover
1. **Concurrent Order Picking**:
   - $N$ Admins try to pick the same `Pending` order.
   - **Expectation**: 1 Success (200 OK), $N-1$ Failures (400 Bad Request: "Đơn hàng này đã được người khác tiếp nhận").
2. **Concurrent Order Cancellation (Idempotent)**:
   - $N$ Admins try to cancel the same `Pending` order.
   - **Expectation**: **All $N$ requests return 200 OK** (Idempotency).
   - **Internal Verification**: Verify that only **one** refund was processed and only **one** status transition was logged in history.
3. **Refund Integrity**:
   - Verify that after a successful cancellation, the user's wallet balance increases exactly by the order total.

---

## File Changes

### CREATE
| File | Description |
|:--|:--|
| `GameTopUp.Tests/IntegrationTests/OrderApiTests.cs` | New integration test suite for Order API. |

---

## Execution Flow

```
Test Runner
  → Setup: Create User, Create Wallet, Add Balance, Create Pending Order.
  → Action: Simulate Concurrent POST /api/orders/{id}/pick (or /cancel)
  → Verification:
      1. Check HTTP Status Codes:
         - **Pick Order**: Exactly 1 'Ok' (200), $N-1$ 'BadRequest' (400).
         - **Cancel Order**: All $N$ requests should be 'Ok' (200).
      2. Check Database: Status should be correctly transitioned.
      3. Check Wallet Balance: Increases exactly by `Order.Total` (no double refund).
      4. Check Order History: Only 1 transition entry exists for the target state.
```

---

## Impact / Risk

| Area | Risk | Mitigation |
|:--|:--|:--|
| Test Stability | Flaky tests due to execution timing | Use enough concurrent tasks to trigger the race condition window. |
| DB Reset | State leaking between tests | `CustomWebApplicationFactory` handles DB recreation per test class/method. |

---

> **Approve this plan? (OK / Reject / Modify)**
