# Plan - Order Management: Admin Pick Order

Implement the ability for Admins to claim (pick) a pending order for processing.

## Objective
- Enable Admin to transition an order from `Pending` to `Processing`.
- Prevent multiple admins from picking the same order simultaneously (Race Condition).
- Audit trail via `OrderHistory`.

## Reference Memory
- Verified `Order`, `OrderHistory` entities.
- Verified `OrderService`, `OrderRepository` structure.
- Following `workflow.md`.

## File Changes

### 1. GameTopUp.DAL
- **Modify `IOrderRepository.cs`**: Add `PickOrderAsync(long orderId, long adminId)` method.
- **Modify `OrderRepository.cs`**: Implement `PickOrderAsync` using atomic SQL `UPDATE ... WHERE id = @id AND status = 1 AND (assign_to = 0 OR assign_to IS NULL)`.

### 2. GameTopUp.BLL
- **Modify `OrderService.cs`**: Add `PickOrderAsync(long orderId, UserContext adminContext)`.
    - Validate role.
    - Call repository.
    - Create `OrderHistory` entry.

### 3. GameTopUp.API
- **Modify `OrderController.cs`**: Add `POST /api/orders/{orderId}/pick` endpoint.
    - Restrict to `Admin` role.
    - Call `OrderService.PickOrderAsync`.

## Impact / Risk
- **Database**: Adds one atomic update. Low risk.
- **Concurrency**: Handled by SQL WHERE clause.
- **Permissions**: Must ensure only Admins can call the new endpoint.

## Verification Plan
1. **Unit Test/Manual Test**: 
   - Call API as Admin on a Pending order -> Success.
   - Call API as Admin on an already Processing order -> Failure (Race condition handled).
   - Verify `OrderHistory` is created.
   - Verify `AssignTo` and `AssignAt` are set correctly.
