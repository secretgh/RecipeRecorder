# v2 — Infrastructure Layer

**Project:** `v2/RecipeRecorder.Infrastructure`  
**Namespace:** `RecipeRecorder.Infrastructure`

The Infrastructure layer implements the repository interfaces defined in the Domain, using Entity Framework Core against a SQL Server database. It also contains a connectivity guard (`ConnectionChecker`) that all repos use before executing any database call.

---

## RecipeDbContext
**File:** `Infrastructure/RecipeDBContext.cs`  
**Namespace:** `RecipeRecorder.Infrastructure`  
**Base class:** `DbContext`

The EF Core database context. Configures all entity relationships, constraints, and indexes. Registered as scoped in `API/Program.cs`.

### Constructor: `RecipeDbContext(DbContextOptions<RecipeDbContext> options)`
Accepts EF Core options. Connection string is supplied via `API/Program.cs`.  
**Used by:** `API/Program.cs` → `builder.Services.AddDbContext<RecipeDbContext>(...)`, injected into `RecipeRepo` and `IngredientRepo`.

### DbSet Properties

| Property | Entity | Notes |
|---|---|---|
| `Recipes` | `Recipe` | Via `Set<Recipe>()` |
| `Ingredients` | `Ingredient` | Via `Set<Ingredient>()` |
| `RecipeIngredients` | `RecipeIngredient` | Via `Set<RecipeIngredient>()` |
| `RecipeSteps` | `RecipeStep` | Via `Set<RecipeStep>()` |
| `RecipeTags` | `RecipeTag` | Via `Set<RecipeTag>()` |

### `OnModelCreating(ModelBuilder modelBuilder)`
Configures all entity shapes and relationships:

| Entity | Configuration |
|---|---|
| `Recipe` | PK on `Id`; required `RecipeName`; cascade-delete for `RecipeIngredients`, `RecipeSteps`, `RecipeTags` (FK = `RecipeId`) |
| `Ingredient` | PK on `Id`; required `NormalizedName` and `IngredientName`; unique index on `NormalizedName` |
| `RecipeIngredient` | PK on `IngId`; required `Quantity` |
| `RecipeStep` | PK on `Id`; required `Instruction` |
| `RecipeTag` | PK on `Id`; required `Tag` |

**Used by:** Called internally by EF Core during model initialization.

---

## ConnectionChecker
**File:** `Infrastructure/Repos/ConnectionChecker.cs`

Static utility called by all repository methods before touching the database. Guards against running queries when SQL Server is unreachable.

#### `IsDBUp(RecipeDbContext context)` → `Task<bool>` _(static)_
Attempts to open a connection using `context.Database.CanConnectAsync()`.  
Returns `true` if reachable, `false` otherwise.  
**Used by:** Every method in `RecipeRepo` and `IngredientRepo`.

---

## RecipeRepo
**File:** `Infrastructure/Repos/RecipeRepo.cs`  
**Namespace:** `RecipeRecorder.Infrastructure.Repos`  
**Implements:** `IRecipeRepo`  
**Dependencies:** `RecipeDbContext`

EF Core implementation of the recipe repository. All methods check `ConnectionChecker.IsDBUp()` before executing. Methods degrade gracefully (return empty/null) when the DB is down rather than throwing.

### Constructor: `RecipeRepo(RecipeDbContext context)`
**Used by:** DI — registered in `API/Program.cs` → `builder.Services.AddScoped<IRecipeRepo, RecipeRepo>()`

### Methods

#### `AddAsync(Recipe recipe)` → `Task<Recipe>`
Adds the recipe to the context and calls `SaveChangesAsync()` — EF assigns the `Id` at this point.  
Returns the recipe (with populated `Id`) regardless of DB state.  
**Called by:** `RecipeAPIService.CreateAsync()`

#### `GetAllAsync()` → `Task<List<Recipe>>`
Fetches all recipes with full includes:
- `RecipeIngredients` → `ThenInclude(ri => ri.Ingredient)`
- `RecipeSteps`
- `RecipeTags`

Uses `.AsSplitQuery()` for better performance on multiple collection includes.  
Returns empty list if DB is down.  
**Called by:** `RecipeAPIService.GetAllAsync()`

#### `GetByIdAsync(int id)` → `Task<Recipe?>`
Fetches a single recipe with the same full includes as `GetAllAsync`, filtered by `r.Id == id`.  
Uses `.AsSplitQuery()`.  
Returns null if not found or DB is down.  
**Called by:** `RecipeAPIService.GetByIdAsync()`, `RecipeAPIService.UpdateAsync()`, `RecipeAPIService.DeleteAsync()`

#### `UpdateAsync(Recipe recipe)`
Calls `_context.Recipes.Update(recipe)` and `SaveChangesAsync()`.  
**Called by:** `RecipeAPIService.UpdateAsync()`

#### `DeleteAsync(int id)`
Finds the recipe by id, removes it, and calls `SaveChangesAsync()`. No-ops if the recipe doesn't exist or the DB is down.  
**Called by:** `RecipeAPIService.DeleteAsync()`

---

## IngredientRepo
**File:** `Infrastructure/Repos/IngredientRepo.cs`  
**Namespace:** `RecipeRecorder.Infrastructure.Repos`  
**Implements:** `IIngredientRepo`  
**Dependencies:** `RecipeDbContext`

EF Core implementation of the ingredient repository. All methods use `ConnectionChecker.IsDBUp()` before executing. Has a private `NormalizeName` helper that mirrors the domain entity's logic to ensure consistent lookups.

### Constructor: `IngredientRepo(RecipeDbContext context)`
**Used by:** DI — registered in `API/Program.cs` → `builder.Services.AddScoped<IIngredientRepo, IngredientRepo>()`

### Methods

#### `GetByIdAsync(int id, CancellationToken)` → `Task<Ingredient?>`
Finds an ingredient by PK using `FindAsync`. Returns null if not found or DB is down.  
**Called by:** `IngredientRepo.DeleteAsync()` (internally), and available for direct use.

#### `GetByNameAsync(string name, CancellationToken)` → `Task<Ingredient?>`
Normalizes the name and queries `NormalizedName == normalized`. Returns null if not found.  
**Called by:** (available for future use)

#### `GetAllAsync(CancellationToken)` → `Task<List<Ingredient>>`
Fetches all ingredients ordered by `IngredientName`. Returns empty list if DB is down.  
**Called by:** (available for direct use or future endpoints)

#### `AddAsync(Ingredient ingredient, CancellationToken)` → `Task<Ingredient>`
Sets `NormalizedName` before inserting, then calls `SaveChangesAsync()`.  
**Called by:** (available; prefer `GetOrCreateAsync` when uniqueness matters)

#### `UpdateAsync(Ingredient ingredient, CancellationToken)` → `Task<Ingredient>`
Sets `NormalizedName` and calls `_context.Ingredients.Update()` + `SaveChangesAsync()`.  
**Called by:** (available for ingredient edit flows)

#### `DeleteAsync(int id, CancellationToken)`
Looks up the ingredient via `GetByIdAsync`, removes it if found, and calls `SaveChangesAsync()`.  
**Called by:** `RecipeAPIService` (future delete-ingredient endpoint)

#### `GetOrCreateAsync(string name, CancellationToken)` → `Task<Ingredient>`
Upsert operation:
1. Normalizes the name.
2. Queries for an existing ingredient by `NormalizedName`.
3. If found, returns it immediately.
4. If not found, creates a new `Ingredient(name.Trim())`, adds it, and saves.

Returns an empty `Ingredient()` object if the DB is down (safe fallback).  
**Called by:** `RecipeAPIService.CreateAsync()` — ensures ingredients exist before linking them to a recipe.

#### `SearchAsync(string searchTerm, int maxResults, CancellationToken)` → `Task<List<Ingredient>>`
Normalizes `searchTerm`, queries `NormalizedName.Contains(normalized)`, orders by `IngredientName`, and takes up to `maxResults`.  
Returns empty list if DB is down.  
**Called by:** `RecipeAPIService.SearchIngredientsAsync()` → `RecipesController.SearchIngredients()`

#### `NormalizeName(string name)` → `string` _(private static)_
Trims and lowercases the name. Used internally before any name-based query or insert.
