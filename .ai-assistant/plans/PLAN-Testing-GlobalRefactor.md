# PLAN-Testing-GlobalRefactor

## Objective
Review and refactor the entire test suite of the GameTopUp project to ensure robust validation, realistic mocking, and alignment with architectural standards.

## Reference Memory
- Checked `memory.md`: Confirmed recent fixes for GameApi tests and lessons learned about SQLite schema and ApiResponse wrappers.
- Checked `rules.md`: Confirmed coding standards and layering rules.

## Scope
Modules with existing tests:
1. **User Module**: `UserServiceTests.cs`, `UserApiTests.cs`
2. **Game Module**: `GameServiceTests.cs`, `GameApiTests.cs`
3. **GamePackage Module**: `GamePackageServiceTests.cs`, `GamePackageApiTests.cs`

*Note: Order, Wallet, and Cart modules currently do not have tests and will be skipped per instructions.*

## Methodology (Per Module)
1. **Understand Implementation**: Read Service and Controller code.
2. **Review Current Tests**: Identify weak assertions or missing edge cases.
3. **Refactor**:
   - Enhance Mock data to be realistic (realistic IDs, dates, names).
   - Strengthen Assertions (verify state changes, multiple properties).
   - Explicitly test Soft Delete behavior (`is_active = 0`) where applicable.
   - Ensure Integration tests handle `ApiResponse` wrapper correctly.
4. **Validation**: Run `dotnet test` for the specific module and confirm pass.

## Schedule

### Phase 1: User Module
- Refactor `UserServiceTests.cs` (Unit)
- Refactor `UserApiTests.cs` (Integration)
- Validation: `dotnet test --filter User`

### Phase 2: Game Module
- Refactor `GameServiceTests.cs` (Unit)
- Refactor `GameApiTests.cs` (Integration) - *Review previous fixes*
- Validation: `dotnet test --filter GameApiTests|GameServiceTests`

### Phase 3: GamePackage Module
- Refactor `GamePackageServiceTests.cs` (Unit)
- Refactor `GamePackageApiTests.cs` (Integration)
- Validation: `dotnet test --filter GamePackage`

## Impact / Risk
- **Impact**: Higher code quality and confidence in the system's behavior.
- **Risk**: Low, as these are changes to tests. Refactoring Service code is not planned unless absolutely necessary for testability.

## Final Report
A summary of all passing tests will be provided after completion.
