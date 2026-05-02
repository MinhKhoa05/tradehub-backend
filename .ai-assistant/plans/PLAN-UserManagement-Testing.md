# PLAN-UserManagement-Testing

## 1. Objective
Implement unit tests for User Management (UserService and UserController) to ensure logic correctness and maintain high code quality.

## 2. Reference Memory
- Checked `memory.md`: Standard testing patterns used (Moq, xUnit, FluentAssertions).
- Checked `rules.md`: Business logic should be in BLL; Controllers are for input/output.

## 3. File Changes

### 📁 DAL Layer (TradeHub.DAL)
- **Create** `Repositories/Interfaces/IUserRepository.cs`: Extract interface from `UserRepository`.
- **Modify** `Repositories/UserRepository.cs`: Implement `IUserRepository`.

### 📁 BLL Layer (TradeHub.BLL)
- **Modify** `Services/UserService.cs`: Inject `IUserRepository` instead of the concrete class.

### 📁 API Layer (TradeHub.API)
- **Modify** `Program.cs`: Update DI registration for `IUserRepository`.

### 📁 Tests Layer (TradeHub.Tests)
- **Create** `UnitTests/Services/UserServiceTests.cs`: 
    - `GetAllAsync_ShouldReturnMappedDTOs`
    - `GetByIdAsync_ShouldReturnDTO_WhenExists`
    - `GetByIdAsync_ShouldThrow_WhenNotExists`
    - `UpdateProfileAsync_ShouldCallUpdate_WhenExists`
    - `DeleteAsync_ShouldCallDelete_WhenExists`
- **Create** `UnitTests/Controllers/UserControllerTests.cs`:
    - Test endpoints and response wrapping.

## 4. Impact / Risk
- **Impact**: Improves reliability and prevents regressions.
- **Risk**: None. Refactoring to interface is a standard non-breaking change.

## 5. Definition of Done
- `UserService` fully covered by unit tests.
- `UserController` verified for proper response wrapping.
- All tests passing.
