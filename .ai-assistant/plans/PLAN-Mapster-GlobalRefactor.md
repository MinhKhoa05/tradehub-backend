# PLAN-Mapster-GlobalRefactor

## Objective
Refactor all Update methods in the BLL Services to enforce a strict, concise logic structure using `Mapster` for property mapping. The goal is to clean up manual property assignments and if-else blocks so each Update method only contains logic to retrieve the entity, map changes via Mapster, and save.

## File Changes
1. **`GameTopUp.BLL/GameTopUp.BLL.csproj`**
   - Install the `Mapster` NuGet package.

2. **`GameTopUp.BLL/Config/MapsterConfig.cs` (Create)**
   - Configure global Mapster settings: `TypeAdapterConfig.GlobalSettings.Default.IgnoreNullValues(true);`
   - Add specific rule for `UpdateGamePackageRequest` to `GamePackage` to handle `NormalizedName` if `Name` is present.

3. **`GameTopUp.BLL/Services/GameService.cs` & `GamePackageService.cs` (Modify)**
   - Refactor `UpdateGameAsync` and `UpdatePackageAsync` to follow the concise rule:
     ```csharp
     var entity = await GetByIdAsync(id);
     request.Adapt(entity);
     await _repo.UpdateAsync(entity);
     ```

## Impact / Risk
- **Impact**: Code becomes highly readable and maintainable. Redundant and verbose manual mapping is eliminated.
- **Risk**: Low. `IgnoreNullValues(true)` prevents accidental nulling of data. `NormalizedName` logic is centralized in the Mapster configuration.
