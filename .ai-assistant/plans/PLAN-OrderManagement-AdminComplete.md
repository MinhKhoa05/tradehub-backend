# PLAN - Order Management - Admin Complete Order

Implementing the functionality for an admin to mark an order as "Completed" (Success). This action is restricted to the admin who is currently assigned to the order and when the order is in the "Processing" state.

## 🎯 Objective
- Allow Admins to complete an order.
- Enforce business rules:
    - Only the assigned Admin can complete the order.
    - The order must be in `Processing` status.
- **Idempotency**: If the order is already `Completed`, return success without performing any action.
- Maintain audit trail via `OrderHistory`.
- Ensure race condition safety using atomic DB updates.

## 🧠 Reference Memory
- Confirmed check of `memory.md`, `rules.md`, and `architecture.md`.
- Following "Fail Fast", "KISS", and "Atomic DB Updates" principles.

## 🛠 File Changes

### DAL Layer
1.  **Modify** `GameTopUp.DAL/Interfaces/IOrderRepository.cs`:
    - Add `Task<int> CompleteOrderAsync(long orderId, long adminId);`
2.  **Modify** `GameTopUp.DAL/Repositories/OrderRepository.cs`:
    - Implement `CompleteOrderAsync` with atomic SQL: 
      ```sql
      UPDATE orders 
      SET status = @CompletedStatus, updated_at = CURRENT_TIMESTAMP 
      WHERE id = @OrderId AND status = @ProcessingStatus AND assign_to = @AdminId
      ```

### BLL Layer
3.  **Modify** `GameTopUp.BLL/Services/OrderService.cs`:
    - Add `CompleteOrderAsync(long orderId, UserContext adminContext)`.
    - **Logic**:
        1. Fetch current order status.
        2. **Idempotency Check**: If `Status == Completed`, return immediately.
        3. **Validation**: If `Status != Processing` or `AssignTo != adminId`, throw `BusinessException`.
        4. Call `_orderRepo.CompleteOrderAsync`.
        5. If rows affected == 0, re-verify state (Race Condition check) and throw exception if necessary.
        6. Create `OrderHistory`.

### API Layer
4.  **Modify** `GameTopUp.API/Controllers/OrderController.cs`:
    - Add `[HttpPost("{orderId}/complete")]` endpoint (Admin only).

## ⚠️ Impact / Risk
- **Race Condition**: Handled by atomic SQL update and post-update state verification.
- **Unauthorized Access**: Handled by `[Authorize(Roles = "Admin")]` and `assign_to` check in SQL/BLL.

## 🎓 Learning Mode
- I will leave the `OrderHistory` creation logic in `OrderService.CompleteOrderAsync` as a `USER_TASK` for the user to practice following the pattern established in `PickOrderAsync` and `CancelOrderAsync`.

---
Approve this plan? (OK / Reject / Modify)
