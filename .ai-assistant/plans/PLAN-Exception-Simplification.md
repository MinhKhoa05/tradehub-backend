# PLAN-Exception-Simplification

## Objective
Simplify `NotFoundException` and other business exceptions to accept a single `string message` instead of custom structured parameters.

## Reference Memory
- Checked `rules.md`: Confirmed exception handling rules (Section 1).
- **Codebase Analysis**: Identified usages of `NotFoundException(item, field, value)` in `UserService`, `GameService`, and `GamePackageService`.

## File Changes

### 📁 BLL Layer (TradeHub.BLL)
- **Modify** `Exceptions/NotFoundException.cs`: Change constructor to `public NotFoundException(string message)`.
- **Modify** `Services/UserService.cs`: Update calls to `new NotFoundException("Người dùng không tồn tại.")`.
- **Modify** `Services/GameService.cs`: Update calls to `new NotFoundException("Game không tồn tại.")`.
- **Modify** `Services/GamePackageService.cs`: Update calls to `new NotFoundException("Gói nạp không tồn tại.")`.

### 📁 Tests Layer (TradeHub.Tests)
- **Modify** `UnitTests/Services/UserServiceTests.cs`: Update test cases checking for `NotFoundException`.
- **Modify** `UnitTests/Services/GameServiceTests.cs`: Update test cases checking for `NotFoundException`.
- **Modify** `UnitTests/Services/GamePackageServiceTests.cs`: Update test cases checking for `NotFoundException`.

## Verification
- Run `dotnet build` to ensure no compilation errors.
- Run `dotnet test` to confirm all tests pass with the new message format.

## Definition of Done
- `NotFoundException` has a simple constructor.
- All service methods use the simplified exception format.
- All tests pass.
