# v1 — Client

The Client project is a Blazor WebAssembly app. It runs entirely in the browser and communicates with the Server via HTTP through `RecipeService`.

---

## IRecipeService (interface)
**File:** `v1/Client/Service/RecipeService.cs`  
**Namespace:** `RecipeRecorder.Client.Service`

Defines the contract for all data operations available to Blazor pages and components. Registered in DI so pages depend on the abstraction, not the concrete class.

| Method Signature | Description |
|---|---|
| `Task<List<Recipe>> GetRecipes()` | Fetch all recipes |
| `Task<Recipe> GetRecipe(int id)` | Fetch a single recipe by id |
| `Task CreateRecipe(Recipe r)` | POST a new recipe |
| `Task UpdateRecipe(Recipe r)` | PUT updates to an existing recipe |
| `Task SaveRecipesToJson()` | Trigger server-side JSON export |
| `Task<List<Ingredient>> SearchIngredients(string s)` | Autocomplete ingredient search |
| `Task<Ingredient> CreateIngredient(Ingredient i)` | Create a new ingredient |
| `Task ExpandIngredientRecipes(int ingredientId)` | Toggle expanded state and lazy-load ingredient's recipes |
| `Dictionary<int, List<Recipe>> GetIngredientRecipes()` | Return cached ingredient → recipe map |
| `HashSet<int> GetExpandedIngredients()` | Return set of currently expanded ingredient ids |
| `ValueTask<ItemsProviderResult<Ingredient>> LoadIngredients(ItemsProviderRequest request)` | Virtualized paged ingredient loader |
| `Task<List<Recipe>> GetRecipesUsingIngredientAsync(int ingredientId)` | Fetch all recipes for a given ingredient |
| `Task LoadIngredientRecipes()` | Pre-load all ingredient–recipe mappings |
| `Task AddIngredientAsync(Ingredient ingredient)` | POST new ingredient |
| `Task UpdateIngredientAsync(Ingredient ingredient)` | PUT updated ingredient |
| `Task DeleteIngredientAsync(int id)` | DELETE ingredient by id |
| `Task<List<RecipeTag>> GetTags()` | Fetch all tags (not yet implemented) |
| `Task<List<Recipe>> GetFilteredRecipes(string search, List<int> tagIds)` | Filtered recipe list (not yet implemented) |
| `Task<bool> TestConnection()` | Check if the server database is available |
| `Task Test()` | Dev utility — triggers the server's test endpoint |

---

## RecipeService
**File:** `v1/Client/Service/RecipeService.cs`  
**Namespace:** `RecipeRecorder.Client.Service`  
**Implements:** `IRecipeService`  
**Dependencies:** `HttpClient` (injected via DI)

Concrete HTTP client implementation. All methods map to endpoints on `RecipeController`. The `HttpClient` base address is set to `builder.HostEnvironment.BaseAddress` (the Server origin) in `Program.cs`.

Also maintains two in-memory state dictionaries used by the Ingredient Registry page.

### State Fields

| Field | Type | Description |
|---|---|---|
| `expandedIngredients` | `HashSet<int>` | Tracks which ingredient rows are expanded in the UI |
| `ingredientRecipes` | `Dictionary<int, List<Recipe>>` | Caches fetched recipe lists keyed by ingredient id |

### Methods

#### `Test()`
POST to `api/Recipe/Test`. Dev utility only.  
**Server endpoint:** `RecipeController.TestCreate()`

#### `SaveRecipesToJson()`
GET `Api/Recipe/SaveToJson`. Triggers the server to write a JSON export file.  
**Server endpoint:** `RecipeController.SaveToJson()`

#### `TestConnection()` → `Task<bool>`
GET `/TestConnection`. Returns `true` if the database is reachable.  
**Server endpoint:** `RecipeController.IsDatabaseAvaiable()`  
**Used by:** Connection status checks in pages.

#### `GetRecipes()` → `Task<List<Recipe>>`
GET `api/Recipe`. Returns the full recipe list with ingredients, steps, and tags.  
**Server endpoint:** `RecipeController.GetRecipes()`  
**Used by:** `Index.razor`, recipe listing pages.

#### `GetRecipe(int id)` → `Task<Recipe>`
GET `api/Recipe/{id}`. Returns a single recipe with all navigation properties populated.  
**Server endpoint:** `RecipeController.GetRecipe(int id)`  
**Used by:** `RecipeView.razor`, `RecipeEdit.razor`.

#### `CreateRecipe(Recipe r)`
POST to `api/Recipe` with the full recipe object.  
**Server endpoint:** `RecipeController.CreateRecipe()`  
**Used by:** Recipe creation pages/forms.

#### `UpdateRecipe(Recipe r)`
PUT to `api/Recipe` with the full updated recipe object.  
**Server endpoint:** `RecipeController.UpdateRecipe()`  
**Used by:** `RecipeEdit.razor`.

#### `CreateIngredient(Ingredient i)` → `Task<Ingredient>`
POST to `api/Recipe/ingredients`. Returns the created ingredient (with server-assigned `Id`).  
**Server endpoint:** `RecipeController.CreateIngredient()`  
**Used by:** Ingredient creation within recipe edit forms.

#### `SearchIngredients(string s)` → `Task<List<Ingredient>>`
GET `api/Recipe/ingredients/{s}`. Returns ingredients whose name contains the search string.  
**Server endpoint:** `RecipeController.GetFilteredIngredients()`  
**Used by:** Ingredient autocomplete on recipe forms.

#### `LoadIngredients(ItemsProviderRequest request)` → `ValueTask<ItemsProviderResult<Ingredient>>`
GET `api/recipe/ingredients/page/{pageNumber}/{pageSize}`. Converts the `ItemsProviderRequest` into page number/size, fetches the paged result, and returns it for the Blazor `Virtualize` component.  
**Server endpoint:** `RecipeController.GetPagedIngredients()`  
**Used by:** `IngredientRegistry.razor` (virtualized list).

#### `GetExpandedIngredients()` → `HashSet<int>`
Returns the in-memory `expandedIngredients` set. No HTTP call.  
**Used by:** `IngredientRegistry.razor` to determine row expand/collapse state.

#### `GetIngredientRecipes()` → `Dictionary<int, List<Recipe>>`
Returns the in-memory `ingredientRecipes` cache. No HTTP call.  
**Used by:** `IngredientRegistry.razor` to render the list of recipes under each ingredient.

#### `ExpandIngredientRecipes(int ingredientId)`
Toggles the ingredient's expanded state. If expanding for the first time, fetches the ingredient's recipes and stores them in `ingredientRecipes`.  
**Server endpoint:** `RecipeController.GetRecipesUsingIngredient()`  
**Used by:** `IngredientRegistry.razor` expand/collapse button.

#### `LoadIngredientRecipes()`
Fetches all ingredients then calls `ExpandIngredientRecipes` for each (pre-loading all data), then clears `expandedIngredients` so the UI starts collapsed.  
**Server endpoints:** `RecipeController.GetAllIngredients()` + `RecipeController.GetRecipesUsingIngredient()`  
**Used by:** Bulk data pre-loading scenarios.

#### `AddIngredientAsync(Ingredient ingredient)`
POST to `api/recipe/ingredients`. Adds a new ingredient.  
**Server endpoint:** `RecipeController.AddIngredient()`  
**Used by:** Ingredient Registry add form.

#### `UpdateIngredientAsync(Ingredient ingredient)`
PUT to `api/recipe/ingredients/{ingredient.Id}`. Updates an ingredient's name.  
**Server endpoint:** `RecipeController.UpdateIngredient()`  
**Used by:** Ingredient Registry edit form.

#### `DeleteIngredientAsync(int id)`
DELETE to `api/recipe/ingredients/{id}`.  
**Server endpoint:** `RecipeController.DeleteIngredient()`  
**Used by:** Ingredient Registry delete button.

#### `GetRecipesUsingIngredientAsync(int ingredientId)` → `Task<List<Recipe>>`
GET `api/recipe/ingredients/{ingredientId}/recipes`. Direct fetch (bypasses the expand/collapse cache).  
**Server endpoint:** `RecipeController.GetRecipesUsingIngredient()`

#### `GetTags()` / `GetFilteredRecipes()`
Both throw `NotImplementedException` — placeholder methods for future filtering features.

---

## Program.cs
**File:** `v1/Client/Program.cs`

Bootstraps the Blazor WebAssembly application.

| Registration | Purpose |
|---|---|
| `Add<App>("#app")` | Mounts the root `App.razor` component into the `#app` DOM element |
| `Add<HeadOutlet>("head::after")` | Enables `<PageTitle>` and head-manipulation from Razor components |
| `AddScoped<HttpClient>` | Registers `HttpClient` with `BaseAddress = HostEnvironment.BaseAddress` (points to the Server) |
| `AddTransient<IRecipeService, RecipeService>` | Registers `RecipeService` as the implementation of `IRecipeService` — transient so each injection gets a fresh instance |
