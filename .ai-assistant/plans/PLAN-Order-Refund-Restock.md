# PLAN: Order Cancellation with Refund and Restock

## Objective
Implement a robust cancellation flow that handles both stock restoration and wallet refunds. Introduce a `Paid` status to separate payment from admin processing.

## Proposed Changes

### 1. Entity Update (`Order.cs`)
- Update `OrderStatus` enum:
    - `Pending = 1`
    - `Paid = 2` (New: Payment successful, waiting for admin)
    - `Processing = 3` (Admin picked)
    - `Completed = 4`
    - `Cancelled = 5`

### 2. BLL Service Updates (`OrderService.cs`)
- **`PayOrderAsync`**: Update status to `Paid`.
- **`PickOrderAsync`**: Ensure it only allows picking orders in `Paid` status. Transition to `Processing`.
- **`CancelOrderAsync`**: 
    - Change return type to `OrderStatus?` (returns the original status before cancellation if successful, or null if already cancelled).
    - Allow cancellation from `Pending`, `Paid`, or `Processing`.

### 3. Application Services (`OrderUseCase.cs`)
- **`CancelOrderAsync`**:
    - Transaction Flow:
        1. Lock Order.
        2. Call `_orderService.CancelOrderAsync` -> get `oldStatus`.
        3. If `oldStatus` is not null:
            - **Restock**: Call `_packageService.IncreaseStockAsync`.
            - **Refund**: If `oldStatus` was `Paid` or `Processing`, call `_walletService.RefundMoneyAsync`.
            - Record log.

### 4. Database Schema (`schema.sql`)
- Update status comments in `orders` table definition.

## Impact / Risk
- **State Machine**: Need to ensure all transitions are updated to account for the new `Paid` status.
- **Atomic Operations**: Refund and Restock must be part of the same transaction as the status update.

## Next Steps
- Approve this plan? (OK / Reject / Modify)
