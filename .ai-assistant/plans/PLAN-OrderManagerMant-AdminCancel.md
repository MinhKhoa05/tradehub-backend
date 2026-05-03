# PLAN-OrderManagement-AdminCancel

## Objective
Implement the **Admin Cancel Order** feature. When an Admin cancels an order, the system must:
1. Transition the order status to `Cancelled`.
2. Refund the full order amount back to the user's wallet.
3. Write an audit entry to `order_history`.
4. All steps must be atomic and idempotent.

---

## Reference Memory

- ✅ Reviewed `memory.md`: UseCase Pattern required for cross-service flows; Race Condition safety via Atomic SQL + try-catch; Service Isolation (no cross-repo injection).
- ✅ Reviewed `rules.md`: All multi-step writes use `ExecuteInTransactionAsync`; Service orchestration via UseCase; exceptions via `NotFoundException` / `BusinessException`; API responses via `ApiResponse`.
- ✅ Reviewed `architecture.md`: Layered architecture (Controller → BLL → DAL → DB).

---

## Design Decisions

### Why UseCase (not OrderService)?
Cancellation touches **two domains**: Order (status update) and Wallet (refund). Per `rules.md Rule 5 & Service Isolation`, a service must not inject another domain's repo or call another service. A `UseCase` (orchestrator) is the only valid place to coordinate `OrderService` + `WalletService`.

### Idempotency & Validation
- **Idempotency**: If the order is already `Cancelled`, the API returns `200 OK` immediately without performing any action. This prevents errors when the same request is sent multiple times.
- **Validation**: Only orders in `Pending` status can be cancelled. If an order is in `Processing` or `Completed`, the system throws a `BusinessException`.

### Semantic Endpoint
Use `POST /api/orders/{orderId}/cancel` for the cancellation action as it represents a state transition, not a resource deletion (`DELETE`).

### Atomic Cancellation Guard
The cancel SQL in the repository will still use a conditional `WHERE status = @PendingStatus` as a safety net against race conditions.

---

## File Changes

### MODIFY
| File | Change |
|:--|:--|
| `GameTopUp.DAL/Interfaces/IOrderRepository.cs` | Add `CancelOrderAsync(long orderId)` method |
| `GameTopUp.DAL/Repositories/OrderRepository.cs` | Implement `CancelOrderAsync` (WHERE status = Pending) |
| `GameTopUp.BLL/Services/OrderService.cs` | Add `CancelOrderAsync(long orderId, UserContext adminContext)` |
| `GameTopUp.BLL/ApplicationServices/OrderUseCase.cs` | Implement `CancelOrderAsync` with Idempotency check and orchestration |
| `GameTopUp.API/Controllers/OrderController.cs` | Add `POST /{orderId}/cancel` endpoint (Admin only) |

---

## Execution Flow

```
Admin → POST /api/orders/{orderId}/cancel
  → OrderController.CancelOrderAsync(orderId)
  → OrderUseCase.CancelOrderAsync(orderId, adminContext)
      → 1. OrderService.GetByIdAsync(orderId)              
             - If null → throw NotFoundException
             - If status == Cancelled → return Success (Idempotent)
             - If status != Pending → throw BusinessException
      → 2. _database.ExecuteInTransactionAsync(...)
             a. OrderService.CancelOrderAsync(orderId, adminContext)
                  - Repo.CancelOrderAsync(orderId)         // Atomic UPDATE WHERE status = 1
                  - If 0 rows → check status → throw BusinessException (Race Condition)
                  - RepoHistory.CreateAsync(...)           // Create Audit Log
             b. WalletService.RefundMoneyAsync(orderOwner, total, note)
  ← ApiOk("Đơn hàng đã được hủy và hoàn tiền thành công.")
```

---

## Impact / Risk

| Area | Risk | Mitigation |
|:--|:--|:--|
| Idempotency | Multiple cancel requests | Early check for `Cancelled` status returns success immediately. |
| Race Condition | Order status changes during cancellation | Atomic SQL `WHERE status = 1` in Repo. |
| Service Isolation | OrderUseCase injecting Repos | UseCase ONLY injects Services. Services manage their own Repos. |
| API Semantics | Incorrect HTTP verb | Changed from `DELETE` to `POST` to match state transition semantics. |