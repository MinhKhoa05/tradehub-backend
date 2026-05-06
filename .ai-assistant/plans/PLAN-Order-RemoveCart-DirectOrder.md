# PLAN: Remove Cart Module and Implement Direct Order Creation

## Objective
Simplify the checkout process by removing the Cart module and allowing users to create orders directly by specifying the `GamePackageId` and `Quantity`.

## Reference Memory
- Checked `OrderUseCase.cs`, `OrderService.cs`.
- Checked `schema.sql`, `seed.sql`.
- Follows "Multi-Agent Workflow" (Learning Mode).

## Proposed Changes

### 1. Database Schema & Seed
- **`Database/schema.sql`**: Remove `cart_items` table definition.
- **`Database/seed.sql`**: Remove `cart_items` seed data.

### 2. File Deletion (Cleanup)
- Delete `GameTopUp.DAL/Repositories/CartItemRepository.cs`
- Delete `GameTopUp.DAL/Queries/CartItemQuery.cs`
- Delete `GameTopUp.DAL/Entities/CartItem.cs`
- Delete `GameTopUp.BLL/Services/CartService.cs`
- Delete `GameTopUp.API/Controllers/CartController.cs`
- Delete `GameTopUp.BLL/DTOs/Carts` directory and its contents.

### 3. Application Updates
- **`GameTopUp.API/Program.cs`**: Remove Dependency Injection registrations for `CartItemRepository` and `CartService`.
- **`GameTopUp.BLL/ApplicationServices/OrderUseCase.cs`**:
    - Remove `CartService` dependency.
    - Replace `CheckoutAsync` with `PlaceOrderAsync(UserContext context, PlaceOrderRequestDTO request)`.
- **`GameTopUp.BLL/DTOs/Orders/PlaceOrderRequestDTO.cs`**: Create new DTO for direct order placement.

### 4. Implementation Logic (Mentorship)
- In `OrderUseCase.PlaceOrderAsync`, the flow will be:
    1. Validate inputs.
    2. Fetch `GamePackage` details (Price, Stock).
    3. Check stock availability.
    4. Start transaction:
        - Deduct money from Wallet.
        - Decrease stock in GamePackage.
        - Create Order.
        - Record Order History.

## Impact / Risk
- **Breaking Change**: Any client-side code using Cart endpoints will fail.
- **Simplified Flow**: Users can now buy items in a single step.

## Next Steps
- Approve this plan? (OK / Reject / Modify)
