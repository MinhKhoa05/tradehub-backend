# PLAN: Refactor Game Package Stock Management to Normal Quantity

## Objective
Replace the budget-based stock management (`PackageBudget`, `SpentAmount`) with a standard discrete quantity-based approach (`StockQuantity`). This simplifies the logic to match common product management patterns as requested.

## Reference Memory
- Checked `GamePackage.cs`, `GamePackageRepository.cs`, `IGamePackageRepository.cs`.
- Checked `schema.sql`.
- Follows "Multi-Agent Workflow" (Learning Mode).

## Proposed Changes

### 1. Database Schema (`Database/schema.sql`)
- Update `game_packages` table:
    - Add `stock_quantity INT DEFAULT 0`.
    - Remove `package_budget` and `spent_amount`.
- Update `Database/seed.sql` to provide sample stock quantities.

### 2. DAL Entities (`GameTopUp.DAL/Entities/GamePackage.cs`)
- Add `StockQuantity` property.
- Remove `PackageBudget` and `SpentAmount` properties.
- Remove `CurrentStock` calculated property (as it becomes redundant).

### 3. DAL Interfaces (`GameTopUp.DAL/Interfaces/IGamePackageRepository.cs`)
- Remove `UpdateStockBudgetAsync`.
- Add `IncreaseStockAsync(long id, int quantity)`.
- Add `DecreaseStockAsync(long id, int quantity)`.

### 4. DAL Repositories (`GameTopUp.DAL/Repositories/GamePackageRepository.cs`)
- Implement `IncreaseStockAsync` and `DecreaseStockAsync` using atomic SQL updates.
- Update `UpdateAsync` and `CreateAsync` to include `stock_quantity`.

### 5. BLL Layer (DTOs & Services)
- Update `CreateGamePackageRequest` and `UpdateGamePackageRequest` to include `StockQuantity`.
- Update `GamePackageService.cs`:
    - Map the new `StockQuantity` property.
    - Add `IncreaseStockAsync(long id, int quantity)` to handle stock addition.
    - Add `DecreaseStockAsync(long id, int quantity)` to handle stock deduction.

## Impact / Risk
- **Breaking Change**: Logic previously relying on budget calculation will need to be updated.
- **Simplification**: Moves from "amount-based" to "unit-based" stock, which is more intuitive for "1 sản phẩm".

## Next Steps
- Approve this plan? (OK / Reject / Modify)
