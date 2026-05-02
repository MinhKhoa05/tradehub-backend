# PLAN-Wallet-LazyCreation

## Objective
Implement explicit wallet creation (manual lazy creation) in `WalletService` and remove `IWalletRepository` dependency from `UserService`. If a user attempts to use wallet features before creation, a `NotFoundException` will be thrown.

## Reference Memory
- [x] `memory.md` reviewed
- [x] `rules.md` reviewed
- [x] `architecture.md` reviewed

## Proposed Changes

### 1. BLL (Business Logic Layer)

#### [MODIFY] `GameTopUp.BLL/Services/UserService.cs`
- Remove `IWalletRepository` and `DatabaseContext` from the constructor.
- Revert `RegisterAsync` to a simple user creation (remove transaction and wallet creation logic).

#### [MODIFY] `GameTopUp.BLL/Services/WalletService.cs`
- Add `Task<long> CreateWalletAsync(UserContext context)`:
    - Create a new wallet for the user with 0 balance.
    - Check if wallet already exists to avoid duplicates.
- Update all public methods (`DeductMoneyAsync`, `RefundMoneyAsync`, `GetBalanceAsync`, `DepositAsync`, `WithdrawAsync`):
    - If `_walletRepo.GetByUserIdAsync(userId)` returns `null`, throw `NotFoundException("Ví của bạn chưa được kích hoạt. Vui lòng kích hoạt ví để sử dụng.")`.

### 2. API Layer

#### [MODIFY] `GameTopUp.API/Controllers/WalletController.cs`
- Add `[HttpPost]` method to call `_wallet.CreateWalletAsync(CurrentUser)`.

### 3. DAL & Database

#### [MODIFY] `Database/schema.sql`
- Remove `balance` from `users` table.
- Add `wallets` table with `user_id` UNIQUE constraint and FOREIGN KEY to `users`.
- Add index on `wallets(user_id)`.

#### [MODIFY] `Database/seed.sql`
- Update `users` data to remove `balance`.
- Add initial `wallets` data for seeded users.

#### [MODIFY] `GameTopUp.Tests/IntegrationTests/CustomWebApplicationFactory.cs`
- Sync schema changes with `schema.sql`.

### 4. Tests

#### [MODIFY] `GameTopUp.Tests/UnitTests/Services/UserServiceTests.cs`
- Revert constructor and mocks to remove `IWalletRepository` and `DatabaseContext`.
- Update `RegisterAsync_ShouldCreateUser_WhenEmailIsUnique` to remove verification of wallet creation.

## Impact / Risk
- **UI/UX**: The frontend must handle 404 errors from wallet endpoints and prompt the user to "Activate Wallet".
- **Design**: Strict separation between user registration and financial account setup.

## Verification Plan
1. **Build**: Ensure the solution compiles.
2. **Unit Tests**: Run `UserServiceTests`.
3. **Manual Test**: 
    - Register a user.
    - Call `GET /api/wallet` -> Should return 404.
    - Call `POST /api/wallet` -> Should return 200 (Success).
    - Call `GET /api/wallet` -> Should return 200 (Balance: 0).
