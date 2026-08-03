# v2 — API Layer

**Project:** `v2/RecipeRecorder.API`  
**Namespace:** `RecipeRecorder.API`

The API project is a thin ASP.NET Core Web API. Controllers do nothing but validate routing and delegate every operation to `IRecipeService`. All business logic lives in the Application layer.

---

## RecipesController
**File:** `API/Controllers/RecipesController.cs`  
**Route:** `api/recipes`  
**Dependencies:** `IRecipeService`

### Constructor: `RecipesController(IRecipeService service)`
`IRecipeService` is injected. The concrete implementation (`RecipeAPIService`) is registered in `Program.cs`.  
**Used by:** ASP.NET Core DI — resolved on every controller instantiation.

---

### Endpoints

#### `GET api/recipes` → `Task<ActionResult<List<RecipeDto>>> GetAll()`
Returns all recipes as a list of DTOs.  
**Delegates to:** `IRecipeService.GetAllAsync()`  
**Called by:** `RecipeAppService.GetAllAsync()` (client)

#### `GET api/recipes/{id}` → `Task<ActionResult<RecipeDto>> Get(int id)`
Returns a single recipe DTO. Returns `404 Not Found` if the recipe doesn't exist.  
**Delegates to:** `IRecipeService.GetByIdAsync(id)`  
**Called by:** `RecipeAppService.GetByIdAsync(int id)` (client)

#### `POST api/recipes` → `Task<ActionResult<RecipeDto>> Create(RecipeDto dto)`
Creates a new recipe. Returns `201 Created` with a `Location` header pointing to `GET api/recipes/{id}` and the created DTO in the body.  
**Delegates to:** `IRecipeService.CreateAsync(dto)`  
**Called by:** `RecipeAppService.CreateAsync(RecipeDto dto)` (client)

#### `PUT api/recipes/{id}` → `Task<ActionResult> Update(int id, RecipeDto dto)`
Updates an existing recipe. Returns `204 No Content` on success, `404 Not Found` if the recipe doesn't exist.  
**Delegates to:** `IRecipeService.UpdateAsync(id, dto)`  
**Called by:** `RecipeAppService.UpdateAsync(int id, RecipeDto dto)` (client)

#### `DELETE api/recipes/{id}` → `Task<ActionResult> Delete(int id)`
Deletes a recipe. Returns `204 No Content` on success, `404 Not Found` if not found.  
**Delegates to:** `IRecipeService.DeleteAsync(id)`  
**Called by:** `RecipeAppService.DeleteAsync(int id)` (client)

#### `GET /api/ingredients/search` → `Task<IEnumerable<IngredientDto>> SearchIngredients(string query, int limit = 10)`
Route is absolute (`/api/ingredients/search`), not prefixed with `api/recipes`. Returns up to `limit` ingredients whose name contains `query`.  
**Delegates to:** `IRecipeService.SearchIngredientsAsync(query, limit)`  
**Called by:** `RecipeAppService.SearchIngredientsAsync(string query, int limit)` (client)

---

## Program.cs
**File:** `API/Program.cs`

Bootstraps the API and wires up the full dependency chain from Infrastructure through Application to the controller.

### Service Registrations

| Registration | Interface | Implementation | Lifetime |
|---|---|---|---|
| `AddDbContext<RecipeDbContext>` | — | EF Core, SQL Server with connection string `"local"` | Scoped |
| `AddScoped<IRecipeRepo, RecipeRepo>` | `IRecipeRepo` | `RecipeRepo` | Scoped |
| `AddScoped<IIngredientRepo, IngredientRepo>` | `IIngredientRepo` | `IngredientRepo` | Scoped |
| `AddScoped<IRecipeService, RecipeAPIService>` | `IRecipeService` | `RecipeAPIService` | Scoped |
| `AddControllers` | — | MVC controller pipeline | — |

### CORS Policy
Policy name `"AllowedOrigins"` — permits requests from:
- `https://localhost:7187` (the API itself, for same-origin tooling)
- `https://localhost:7063` (the Blazor Web client dev URL)

Any header and any method are allowed within these origins.  
**Applied by:** `app.UseCors("AllowedOrigins")`

### Middleware Pipeline

| Middleware | Purpose |
|---|---|
| `UseCors("AllowedOrigins")` | Apply CORS policy before any routing |
| `UseHttpsRedirection` | Redirect HTTP to HTTPS |
| `UseAuthorization` | Authorization middleware (no auth policies configured yet) |
| `MapControllers` | Route HTTP requests to controller actions |
