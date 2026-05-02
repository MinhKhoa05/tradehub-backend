# PLAN-Database-IndexOptimization

## Objective
Establish database indexes to optimize query performance based on actual DAL repository usage and Selectivity principles, keeping the existing schema structure without adding unauthorized columns.

## Reference Memory
- **DAL Analysis**: Reviewed all Repositories to extract `WHERE` and `ORDER BY` patterns.
- **Selectivity Evaluation**: Prioritize High Selectivity columns (`user_id`, `email`, `game_id`) and use Composite Indexes for sorting and filtering.

## Index Validation Rules
- **Evidence-Based**: Every index must map to a real query pattern identified in the DAL.
- **Order of Columns**: Composite indexes must follow the `WHERE` (filters) + `ORDER BY` (sorting) sequence.
- **Selectivity Balance**: Low selectivity columns are allowed inside composite indexes.
- **No Assumptions**: Do not add or remove columns arbitrarily. Only optimize existing structure.

## File Changes

### 📁 Database Layer
- **Modify** `Database/schema.sql`: Apply optimized indexes to existing columns.
- **Modify** `TradeHub.Tests/IntegrationTests/CustomWebApplicationFactory.cs`: Sync test schema with production indexes.

## Optimized Index Plan (Selectivity-Driven)

### 1. Table `users`
- Index: `idx_users_username` on `username`.
- Index: `idx_users_created` on `created_at`.

### 2. Table `games`
- *Decision*: Keep original structure. No single-column index on `is_active` due to low selectivity.

### 3. Table `game_packages`
- Index: `idx_packages_lookup` on `(game_id, is_active)`.
- Index: `idx_packages_normalized` on `normalized_name` (Existing column).

### 4. Table `game_accounts`
- Index: `idx_accounts_user_sort` on `(user_id, is_default, created_at)`.

### 5. Table `cart_items`
- Index: `idx_cart_lookup` on `(user_id, game_package_id)`.

### 6. Table `wallet_transactions`
- Index: `idx_tx_user_sort` on `(user_id, created_at)`.

### 7. Table `orders`
- Index: `idx_orders_user_sort` on `(user_id, created_at)`.
- Index: `idx_orders_status` on `status`.

### 8. Table `order_history`
- Index: `idx_history_order_sort` on `(order_id, created_at)`.

## Impact / Risk
- **Impact**: Improved query performance for current repository logic.
- **Risk**: Minimal. No changes to data types or constraints.

## Verification
- Run `dotnet build`.
- Run `dotnet test`.
