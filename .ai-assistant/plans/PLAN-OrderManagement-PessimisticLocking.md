# PLAN - Order Management - Pessimistic Locking Refactor

## Objective
Refactor the order cancellation workflow to use pessimistic locking (`SELECT ... FOR UPDATE`) to simplify concurrency handling and improve code readability.

## Reference Memory
- [x] `memory.md` checked (No specific conflicts found)
- [x] `rules.md` checked (Adhering to "Fail Fast" and "KISS")

## File changes

### GameTopUp.DAL
- **Modify** `Interfaces/IOrderRepository.cs`: 
    - Add `GetByIdForUpdateAsync(long orderId)`.
    - Add `UpdateAsync(Order order)`: General update for Order entity.
    - (Cleanup) Eventually remove specialized `PickOrderAsync`, `CompleteOrderAsync`, `CancelOrderAsync` once refactor is complete.
- **Modify** `Repositories/OrderRepository.cs`: 
    - Implement `GetByIdForUpdateAsync` using `SELECT * FROM orders WHERE id = @Id FOR UPDATE`.
    - Implement `UpdateAsync` to update status, assign_to, assign_at, updated_at, etc.

### GameTopUp.BLL
- **Modify** `Services/OrderService.cs`:
    - Add `GetByIdForUpdateAsync(long orderId)`.
    - Refactor `PickOrderAsync`, `CancelOrderAsync`, `CompleteOrderAsync` to use the `FOR UPDATE` + `UpdateAsync` pattern.
    - Ensure `OrderHistory` is still recorded for each transition.
- **Modify** `ApplicationServices/OrderUseCase.cs`:
    - Refactor `CancelOrderAsync` to use the new service methods.

## Impact / Risk
- **Impact**: Unified pattern for all order transitions. BLL code becomes much cleaner and more descriptive.
- **Risk**: Potential for deadlocks if multiple rows are locked in different orders, but here we usually lock a single order ID, so risk is low.

## Proposed Logic Pattern (Service Level)

```csharp
public async Task PickOrderAsync(long orderId, UserContext adminContext)
{
    await _database.ExecuteInTransactionAsync(async () =>
    {
        // 1. Lock
        var order = await _orderRepo.GetByIdForUpdateAsync(orderId)
            ?? throw new NotFoundException($"Không tìm thấy đơn hàng #{orderId}");

        // 2. Validate state
        if (order.Status == OrderStatus.Processing && order.AssignTo == adminContext.UserId) return; // Idempotent
        if (order.Status != OrderStatus.Pending || order.AssignTo != 0) 
            throw new BusinessException("Đơn hàng không ở trạng thái chờ hoặc đã có người nhận.");

        // 3. Update
        order.Status = OrderStatus.Processing;
        order.AssignTo = adminContext.UserId;
        order.AssignAt = DateTime.UtcNow;
        order.UpdatedAt = DateTime.UtcNow;
        await _orderRepo.UpdateAsync(order);

        // 4. History
        await _orderHistoryRepo.CreateAsync(new OrderHistory { ... });
    });
}
```

Approve this modified plan? (OK / Reject / Modify)

Approve this plan? (OK / Reject / Modify)
