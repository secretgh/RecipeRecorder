# v2 — Application Layer

**Project:** `v2/RecipeRecorder.Application`  
**Namespace:** `RecipeRecorder.Application`

The Application layer contains the use-case logic. It depends on the Domain (entities and interfaces) and on `RecipeRecorder.Shared` (DTOs), but never on Infrastructure or any HTTP framework directly.

Two concrete implementations of `IRecipeService` exist — one runs **server-side** (orchestrates repositories), the other runs **client-side** (makes HTTP calls to the API).

---

## IRecipeService
**File:** `Application/Interfaces/IRecipeService.cs`  
**Namespace:** `RecipeRecorder.Application.Interfaces`

The single application-layer contract shared by both the API and the browser clients.

| Method | Returns | Description |
|---|---|---|
| `GetAllAsync()` | `Task<List<RecipeDto>>` | Fetch all recipes as DTOs |
| `GetByIdAsync(int id)` | `Task<RecipeDto?>` | Fetch a single recipe DTO by id, null if not found |
| `CreateAsync(RecipeDto dto)` | `Task<RecipeDto>` | Create a new recipe, return the created DTO (with id) |
| `UpdateAsync(int id, RecipeDto dto)` | `Task<bool>` | Update an existing recipe; returns `false` if not found |
| `DeleteAsync(int id)` | `Task<bool>` | Delete a recipe by id; returns `false` if not found |
| `SearchIngredientsAsync(string query, int limit)` | `Task<List<IngredientDto>>` | Autocomplete ingredient search |

**Implemented by:**
- `RecipeAPIService` — server-side (registered in `API/Program.cs`)
- `RecipeAppService` — client-side (registered in `Web/Program.cs`)

**Used by:**
- `RecipesController` (API layer)
- `RecipeList.razor`, and other Blazor pages via `@inject IRecipeService`

---

## RecipeAPIService _(server-side)_
**File:** `Application/Services/RecipeAPIService.cs`  
**Namespace:** `RecipeRecorder.Application.Services`  
**Implements:** `IRecipeService`  
**Dependencies:** `IRecipeRepo`, `IIngredientRepo`

Runs inside `RecipeRecorder.API`. Orchestrates repository operations, maps between domain entities and DTOs, and enforces application-level rules (e.g. ingredient upsert on create).

### Constructor: `RecipeAPIService(IRecipeRepo repository, IIngredientRepo ingredientRepo)`
Receives both repositories via DI.  
**Registered by:** `API/Program.cs` → `builder.Services.AddScoped<IRecipeService, RecipeAPIService>()`

---

### Methods

#### `GetAllAsync()` → `Task<List<RecipeDto>>`
Calls `IRecipeRepo.GetAllAsync()`, maps each `Recipe` to a `RecipeDto` via `MapToDto()`.  
**Called by:** `RecipesController.GetAll()`

#### `GetByIdAsync(int id)` → `Task<RecipeDto?>`
Calls `IRecipeRepo.GetByIdAsync(id)`. Returns null if not found, otherwise maps with `MapToDto()`.  
**Called by:** `RecipesController.Get(int id)`

#### `CreateAsync(RecipeDto dto)` → `Task<RecipeDto>`
Full create flow:
1. Constructs a new `Recipe` domain entity from the DTO name/desc.
2. For each ingredient DTO → calls `IIngredientRepo.GetOrCreateAsync()` to upsert the ingredient, then `Recipe.AddIngredient(new RecipeIngredient(...))`.
3. For each step DTO → `Recipe.AddStep(new RecipeStep(...))`.
4. For each tag DTO → `Recipe.AddTag(new RecipeTag(...))`.
5. Calls `IRecipeRepo.AddAsync(recipe)`.
6. Maps the persisted entity back to a DTO and returns it.

The DTO arriving here must have `Steps`, `Ingredients`, and `Tags` already populated. The client ensures this by calling `RecipeWizardData.SyncToRecipe()` before invoking the API (see `RecipeWizardRoot.SaveRecipe()`).

**Called by:** `RecipesController.Create(RecipeDto dto)`

#### `UpdateAsync(int id, RecipeDto dto)` → `Task<bool>`
1. Loads the existing recipe via `IRecipeRepo.GetByIdAsync(id)`. Returns `false` if not found.
2. Updates name and description via domain methods.
3. Clears and rebuilds ingredients (creates transient `Ingredient` objects — does not upsert), steps, and tags.
4. Calls `IRecipeRepo.UpdateAsync(existingRecipe)`.
5. Returns `true`.

**Called by:** `RecipesController.Update(int id, RecipeDto dto)`

#### `DeleteAsync(int id)` → `Task<bool>`
Checks existence via `IRecipeRepo.GetByIdAsync(id)`, returns `false` if missing. Calls `IRecipeRepo.DeleteAsync(id)` and returns `true`.  
**Called by:** `RecipesController.Delete(int id)`

#### `SearchIngredientsAsync(string query, int limit)` → `Task<List<IngredientDto>>`
Calls `IIngredientRepo.SearchAsync(query, limit)`, maps results to `IngredientDto` list.  
**Called by:** `RecipesController.SearchIngredients(string query, int limit)`

#### `MapToDto(Recipe r)` → `RecipeDto` _(private static)_
Central mapping helper. Converts a fully-loaded `Recipe` entity (including all navigation properties) to a `RecipeDto`, recursively mapping `RecipeIngredient`, `RecipeStep`, and `RecipeTag` collections.  
**Called by:** `GetAllAsync()`, `GetByIdAsync()`, `CreateAsync()`

---

## RecipeAppService _(client-side)_
**File:** `Application/Services/RecipeAppService.cs`  
**Namespace:** `RecipeRecorder.Application.Services`  
**Implements:** `IRecipeService`  
**Dependencies:** `HttpClient`

Runs inside `RecipeRecorder.Web` (Blazor WASM) and `RecipeRecorder.Maui`. Makes HTTP calls to `RecipeRecorder.API`. The `HttpClient` base address is hard-coded to `https://localhost:7187/` (the API's development URL).

### Constructor: `RecipeAppService(HttpClient client)`
Sets the `HttpClient.BaseAddress`.  
**Registered by:** `Web/Program.cs` → `builder.Services.AddScoped<IRecipeService, RecipeAppService>()`

---

### Methods

#### `GetAllAsync()` → `Task<List<RecipeDto>>`
GET `api/recipes`. Returns deserialized list of recipe DTOs.  
**API endpoint:** `RecipesController.GetAll()`  
**Called by:** `RecipeList.razor` on `OnInitialized`

#### `GetByIdAsync(int id)` → `Task<RecipeDto?>`
GET `api/recipes/{id}`. Returns the recipe DTO or null if not found.  
**API endpoint:** `RecipesController.Get(int id)`

#### `CreateAsync(RecipeDto dto)` → `Task<RecipeDto>`
POST `api/recipes` with the DTO body. Returns the created DTO (with server-assigned id).  
**API endpoint:** `RecipesController.Create(RecipeDto dto)`

#### `UpdateAsync(int id, RecipeDto dto)` → `Task<bool>`
PUT `api/recipes/{id}` with the DTO body. Returns `true` if the response is successful.  
**API endpoint:** `RecipesController.Update(int id, RecipeDto dto)`

#### `DeleteAsync(int id)` → `Task<bool>`
DELETE `api/recipes/{id}`. Returns `true` if successful.  
**API endpoint:** `RecipesController.Delete(int id)`

#### `SearchIngredientsAsync(string query, int limit)` → `Task<List<IngredientDto>>`
GET `api/ingredients/search?query={query}&limit={limit}`. Returns matching ingredient DTOs.  
**API endpoint:** `RecipesController.SearchIngredients()`
