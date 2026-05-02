# PLAN-UserManagement-API

## 1. Objective
Establish the User Management API suite including CRUD operations, DTOs, and service logic following strict Mapster update patterns and architecture rules.

## 2. Reference Memory
- Checked `memory.md`: Standardized DTO naming (`ResponseDTO`).
- Checked `rules.md`: Mapster 3-line update pattern, `IgnoreNullValues` config, `ApiResponse` wrapper, and soft delete/disable preference.

## 3. File Changes

### 📁 DAL Layer (TradeHub.DAL)
- **Modify** `Repositories/UserRepository.cs`:
    - Add `GetAllAsync(int page, int pageSize)` for pagination.
    - Add `DeleteAsync(long id)` for disabling (soft delete).
    - Update `UpdateAsync(User user)` to handle `updated_at`.

### 📁 BLL Layer (TradeHub.BLL)
- **Create** `DTOs/Users/UserResponseDTO.cs`: Output DTO.
- **Create** `DTOs/Users/UpdateUserRequest.cs`: Input DTO for updates.
- **Delete** `DTOs/Users/UserResponse.cs`: Deprecated file.
- **Modify** `Services/UserService.cs`:
    - Implement `GetAllAsync`, `GetByIdAsync`, `UpdateProfileAsync`, `DeleteAsync`.
    - Apply Mapster 3-line pattern for `UpdateProfileAsync`.

### 📁 API Layer (TradeHub.API)
- **Create** `Controllers/UserController.cs`:
    - `GET /api/users` (Query: page, pageSize)
    - `GET /api/users/{id}`
    - `PUT /api/users/{id}`
    - `DELETE /api/users/{id}`

## 4. Impact / Risk
- **Impact**: Provides admin/management capabilities for users.
- **Risk**: Low. Soft delete (setting `is_active = 0`) is used to maintain data integrity.

## 5. Definition of Done
- All CRUD endpoints functional.
- Mapster used for mapping.
- Responses wrapped in `ApiResponse`.
- Proper error handling via `BusinessException`.
