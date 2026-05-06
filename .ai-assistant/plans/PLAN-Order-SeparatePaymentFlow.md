# PLAN: Separate Order Creation and Payment Flow

## Objective
Refactor the ordering process to support "Order Now, Pay Later" logic. Implement a constraint where each user can only have one `Pending` order at a time.

## Reference Memory
- Checked `OrderUseCase.cs`, `OrderService.cs`.
- Checked `schema.sql`.
- Follows "Multi-Agent Workflow" (Learning Mode).

## Proposed Changes

### 1. Database Schema (`Database/schema.sql`)
- **`orders` table**: 
    - Remove `wallet_transaction_id`.
    - Add a **Unique Functional Index**: Ensure a user can only have one order with `status = 1` (Pending). 
    - *Syntax*: `UNIQUE INDEX idx_one_pending_per_user (user_id, (CASE WHEN status = 1 THEN 1 ELSE NULL END))`
- **`wallet_transactions` table**: Add `order_id BIGINT SIGNED NULL`, and add a foreign key constraint to `orders(id)`.

### 2. DAL Layer updates
- **`IOrderRepository` & `OrderRepository`**:
    - Add `Task<bool> HasPendingOrderAsync(long userId)`: Checks if any order exists for the user with `Status = Pending`.

### 3. BLL Service updates (`OrderService.cs`)
- Add `HasPendingOrderAsync(long userId)` method.
- Update `CreateOrderAsync`: Ensure the constraint is checked before creation.

### 4. Application Services (`OrderUseCase.cs`)
- **`PlaceOrderAsync`**:
    - Transaction Flow:
        1. Check `HasPendingOrderAsync` (Service-level check).
        2. Fetch `GamePackage` to get current price and check stock.
        3. **Decrease stock** via `GamePackageService`.
        4. **Create Order**: Status: `Pending`, UnitPrice: `package.SalePrice`.
        5. Return Order ID.
- **`PayOrderAsync(UserContext context, long orderId)`**:
    - New method to handle the actual payment.
    - Transaction Flow:
        1. Lock Order for update.
        2. Validate Order belongs to user and is in `Pending` state.
        3. **Deduct money** from `WalletService` (passing `orderId`).
        4. Update Order: `Status = Processing`.
        5. Record Order History.

### 5. DTO Updates
- Update `PlaceOrderResponseDTO` (or `CheckoutResponseDTO`) to reflect that it's now just a created order, not necessarily paid yet.

## Impact / Risk
- **Stock Reservation**: Since stock is only deducted at payment, a user might place an order but find it "Out of stock" when they try to pay later. (We should discuss if we want to "reserve" stock at placement).
- **Concurrency**: `HasPendingOrderAsync` check needs to be robust against race conditions.

## Next Steps
- Approve this plan? (OK / Reject / Modify)
