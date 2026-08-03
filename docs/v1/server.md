# v1 — Server

The Server project is an ASP.NET Core Web API that hosts the Blazor WASM static files and exposes all data through a REST API backed by SQL Server via EF Core.

---

## DevContext
**File:** `v1/Server/DevContext.cs`  
**Namespace:** `RecipeRecorder.Server`  
**Base class:** `DbContext` (Entity Framework Core)

The EF Core database context. It maps the shared models to SQL Server tables and configures primary keys, identity columns, and the `RecipeIngredient` composite key.

### DbSets

| Property | Entity | Table |
|---|---|---|
| `Ingredients` | `Ingredient` | `Ingredient` |
| `Recipes` | `Recipe` | `Recipes` |
| `RecipeIngredients` | `RecipeIngredient` | `RecipeIngredients` |
| `RecipeSteps` | `RecipeStep` | `RecipeSteps` |
| `RecipeTags` | `RecipeTag` | `RecipeTags` |

### Methods

#### `DevContext(DbContextOptions<DevContext> options)`
Constructor. Accepts EF Core options — connection string is supplied via DI in `Program.cs`.  
**Used by:** `Program.cs` → `builder.Services.AddDbContext<DevContext>(...)`, injected into `RecipeController`.

#### `OnModelCreating(ModelBuilder modelBuilder)`
Configures entity shapes that can't be expressed purely through data annotations:
- `Recipe.Id` → auto-incremented PK
- `Ingredient.Id` → auto-incremented PK
- `RecipeIngredient` → composite PK `(IngId, RecipeId)`, FK from `IngId` to `Ingredient.Id`
- `RecipeStep.Id` / `RecipeTag.Id` → auto-incremented PKs

**Used by:** Called internally by EF Core when building the model.

#### `OnModelCreatingPartial(ModelBuilder modelBuilder)`
Partial method hook — empty by default, allows generated code to be extended without modifying the main file.

---

## RecipeController
**File:** `v1/Server/Controllers/RecipeController.cs`  
**Namespace:** `RecipeRecorder.Server.Controllers`  
**Route:** `api/Recipe`  
**Dependencies:** `IConfiguration`, `DevContext`

The single controller that handles all recipe and ingredient operations. Accessed by the Client via `RecipeService`.

### Constructor

#### `RecipeController(IConfiguration config, DevContext context)`
Injects configuration (for the connection string) and the EF Core context.  
**Used by:** ASP.NET Core DI — resolved when any endpoint is hit.

---

### Endpoints

#### `GET /TestConnection` → `bool IsDatabaseAvaiable()`
Tries to open a raw `SqlConnection` and returns `true` if successful, `false` otherwise.  
**Called by:** `RecipeService.TestConnection()`

#### `POST api/Recipe/Test` → `Task<IActionResult> TestCreate()`
Development utility — calls `SaveToJson` internally and logs the result to console. Not used in production flow.

#### `GET api/Recipe/SaveToJson` → `Task<IActionResult> SaveToJson()`
Serializes all recipes to JSON and writes the file to the current user's Downloads folder.  
Returns the JSON string as the response body.  
**Called by:** `RecipeService.SaveRecipesToJson()`

#### `GET api/Recipe/ImportFromJson` → `Task<IActionResult> ImportFromJson([FromHeader] List<Recipe> recipes)`
Imports a list of recipes from the request header. Skips recipes whose name already exists in the database. For each new recipe, creates any missing ingredients and then calls `CreateRecipe`.  
**Internal calls:** `IsDatabaseAvaiable()`, `CreateIngredient()`, `CreateRecipe()`

#### `GET api/Recipe/ingredients/{search}` → `Task<IActionResult> GetFilteredIngredients(string search)`
Returns all ingredients whose name contains the `search` string (case-sensitive DB query).  
**Called by:** `RecipeService.SearchIngredients(string s)`

#### `GET api/Recipe/ingredients/{id}/recipes` → `Task<IActionResult> GetRecipesUsingIngredient(int id)`
Returns all recipes that contain the specified ingredient, with full includes (ingredients, steps, tags).  
**Called by:** `RecipeService.ExpandIngredientRecipes(int ingredientId)`, `RecipeService.GetRecipesUsingIngredientAsync(int ingredientId)`

#### `PUT api/Recipe/ingredients/{id}` → `Task<IActionResult> UpdateIngredient(int id, [FromBody] Ingredient updated)`
Finds the ingredient by id and updates its name. Returns `NoContent` on success, `NotFound` if missing.  
**Called by:** `RecipeService.UpdateIngredientAsync(Ingredient ingredient)`

#### `DELETE api/Recipe/ingredients/{id}` → `Task<IActionResult> DeleteIngredient(int id)`
Deletes an ingredient by id. Returns `NoContent` on success, `NotFound` if missing.  
**Called by:** `RecipeService.DeleteIngredientAsync(int id)`

#### `GET api/Recipe/ingredients/page/{page}/{pageSize}` → `Task<ActionResult<PagedResult<Ingredient>>> GetPagedIngredients(int page, int pageSize)`
Returns a paged list of ingredients ordered alphabetically, plus a total count.  
**Called by:** `RecipeService.LoadIngredients(ItemsProviderRequest request)` (used by Virtualize component)

#### `POST api/Recipe/ingredients/page` → `Task<IActionResult> AddIngredient([FromBody] Ingredient ingredient)`
Adds a single ingredient via the paged endpoint (effectively the same as `CreateIngredient` but returns a `CreatedAtAction` response).  
**Called by:** `RecipeService.AddIngredientAsync(Ingredient ingredient)`

#### `GET api/Recipe` → `Task<IActionResult> GetRecipes()`
Returns all recipes with full includes (ingredients, steps, tags).  
**Called by:** `RecipeService.GetRecipes()`  
**Internal use:** `SaveToJson()`, `ImportFromJson()`

#### `GET api/Recipe/{id}` → `Task<IActionResult> GetRecipe(int id)`
Returns a single recipe by id. Also resolves each `RecipeIngredient.Ing` navigation property by querying `Ingredients` directly.  
Returns `NotFound` if the recipe doesn't exist.  
**Called by:** `RecipeService.GetRecipe(int id)`

#### `GET api/Recipe/ingredient/{id}` → `Task<IActionResult> GetIngredient(int id)`
Returns a single ingredient by id.

#### `POST api/Recipe` → `Task<IActionResult> CreateRecipe([FromBody] Recipe r)`
Creates a new recipe along with its tags, steps, and ingredients. Sets `RecipeId` on each child and marks the `Ingredient` navigation as `Unchanged` to avoid duplicate inserts.  
**Called by:** `RecipeService.CreateRecipe(Recipe r)`  
**Internal use:** `ImportFromJson()`

#### `PUT api/Recipe` → `Task<IActionResult> UpdateRecipe(Recipe r)`
Loads the existing recipe with all includes, then replaces its tags, steps, and ingredients with the values from the request body. Attaches existing `Ingredient` entities when provided.  
Returns `NotFound` if the recipe doesn't exist.  
**Called by:** `RecipeService.UpdateRecipe(Recipe r)`

#### `GET api/Recipe/{rid}/tags` → `Task<IActionResult> GetRecipeTags(int rid)`
Returns all tags for a specific recipe.

#### `GET api/Recipe/{rid}/tags/{id}` → `Task<IActionResult> GetRecipeTag(int rid, int id)`
Returns a single tag by id within a specific recipe.

#### `GET api/Recipe/ingredients` → `Task<IActionResult> GetAllIngredients()`
Returns all ingredients (unfiltered).  
**Called by:** `RecipeService.LoadIngredientRecipes()` (to pre-load all ingredient–recipe mappings)

#### `POST api/Recipe/ingredients` → `Task<Ingredient> CreateIngredient([FromBody] Ingredient i)`
Creates a new ingredient. Returns the created ingredient (with assigned `Id`).  
**Called by:** `RecipeService.CreateIngredient(Ingredient i)`  
**Internal use:** `ImportFromJson()`

---

### Non-endpoint Helpers

#### `GetFilteredRecipesAsync(string search, List<int> tagIds)`
Not an HTTP endpoint — a reusable async helper that filters recipes by name substring and/or tag ids.  
**Called by:** (internal use — available for future controller methods)

#### `GetAllTagsAsync()`
Returns all tags from the database.  
**Called by:** (internal use — not yet exposed as an endpoint)

---

## Program.cs
**File:** `v1/Server/Program.cs`

Bootstraps the ASP.NET Core app and wires up all services.

| Registration | Purpose |
|---|---|
| `AddDbContext<DevContext>` | Registers EF Core with the SQL Server connection string `"local"` from `appsettings.json` |
| `AddControllersWithViews` + JSON options | Registers MVC/API with `ReferenceHandler.IgnoreCycles` to handle circular object graphs |
| `AddRazorPages` | Required for the error page |
| `AddSwaggerGen` | Enables Swagger UI in Development |
| `AddCors` | Allows requests from `localhost:7100`, `localhost:5071`, `localhost:11951` (Blazor WASM origins) |
| `UseBlazorFrameworkFiles` | Serves the compiled Blazor WASM bundle |
| `MapFallbackToFile("index.html")` | Returns `index.html` for all non-API routes (SPA routing) |
