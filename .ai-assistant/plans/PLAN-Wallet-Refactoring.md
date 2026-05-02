# PLAN-Wallet-Refactoring

## Objective
Refactor `WalletService` to separate wallet data from the `User` entity. Create a dedicated `Wallet` entity and `WalletRepository` to handle balance operations, adhering to the Repository pattern and separation of concerns.

## Reference Memory
- [x] `memory.md` reviewed
- [x] `rules.md` reviewed
- [x] `architecture.md` reviewed

## Proposed Changes

### 1. DAL (Data Access Layer)

#### [CREATE] `GameTopUp.DAL/Entities/Wallet.cs`
- Fields: `Id` (long), `UserId` (long), `Balance` (decimal), `CreatedAt` (DateTime), `UpdatedAt` (DateTime).
- Table name: `wallets`.

#### [MODIFY] `GameTopUp.DAL/Entities/User.cs`
- Remove `Balance` property.

#### [CREATE] `GameTopUp.DAL/Repositories/Interfaces/IWalletRepository.cs`
- `Task<Wallet?> GetByUserIdAsync(long userId);`
- `Task<long> CreateAsync(Wallet wallet);`
- `Task<int> IncreaseBalanceAsync(long userId, decimal amount);`
- `Task<int> DecreaseBalanceAsync(long userId, decimal amount);`

#### [CREATE] `GameTopUp.DAL/Repositories/WalletRepository.cs`
- Implement `IWalletRepository` using Dapper.
- `DecreaseBalanceAsync` should include `WHERE balance >= @Amount` for concurrency safety.

#### [MODIFY] `GameTopUp.DAL/Repositories/Interfaces/IUserRepository.cs`
- Remove `IncreaseBalanceAsync` and `DecreaseBalanceAsync`.

#### [MODIFY] `GameTopUp.DAL/Repositories/UserRepository.cs`
- Remove `IncreaseBalanceAsync` and `DecreaseBalanceAsync` implementations.

#### [MODIFY] `GameTopUp.DAL/DatabaseContext.cs`
- No changes needed to `DatabaseContext` itself if it uses `IDbConnection` directly, but ensure repositories are registered in DI.

### 2. BLL (Business Logic Layer)

#### [MODIFY] `GameTopUp.BLL/Services/WalletService.cs`
- Replace `IUserRepository` with `IWalletRepository` for balance operations.
- Update `DeductMoneyAsync`, `RefundMoneyAsync`, `GetBalanceAsync`, `DepositAsync`, `WithdrawAsync` to use `_walletRepo`.
- In `DeductMoneyAsync` and others, fetch balance from `_walletRepo.GetByUserIdAsync`.

### 3. API Layer & Configuration

#### [MODIFY] `GameTopUp.API/Program.cs` (or wherever DI is)
- Register `IWalletRepository` and `WalletRepository`.

### 4. Database & Tests

#### [MODIFY] `GameTopUp.Tests/IntegrationTests/CustomWebApplicationFactory.cs`
- Update SQLite schema:
    - Remove `balance` from `users` table.
    - Add `wallets` table.
- Update seed data if necessary.

#### [MODIFY] `GameTopUp.Tests/UnitTests/Services/UserServiceTests.cs` (if affected)
- Update tests to reflect `User` no longer has `Balance`.

## Impact / Risk
- **Breaking Change**: Any code relying on `User.Balance` will break.
- **Data Migration**: Existing user balances need to be moved to the new `wallets` table. (In this dev context, we'll focus on the code refactor and schema update).
- **Transactional Integrity**: Wallet operations remain transactional using `ExecuteInTransactionAsync`.

## Verification Plan
1. **Build**: Ensure the solution compiles.
2. **Unit Tests**: Run existing `UserServiceTests` and `WalletServiceTests`.
3. **Integration Tests**: Run `UserApiTests` and any wallet-related API tests.
4. **Manual Test**: Verify balance deduction and refund flow.
