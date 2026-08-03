# v1 — Shared Models

**File location:** `v1/Shared/Models/`  
**Namespace:** `RecipeRecorder.Shared`

These are plain data-transfer / EF Core entity classes shared between the Server (EF Core) and the Client (JSON deserialization). They carry no business logic — validation and behaviour live in the controller.

---

## Recipe
**File:** `Shared/Models/Recipe.cs`

Root aggregate that holds a recipe and its related collections.

| Member | Type | Description |
|---|---|---|
| `Id` | `int` | Primary key (`[Column("ID")]`) |
| `RecipeName` | `string` | Recipe display name, max 50 chars |
| `RecipeDesc` | `string` | Short description, max 255 chars |
| `RecipeIngredients` | `List<RecipeIngredient>` | Navigation — all ingredients for this recipe |
| `RecipeSteps` | `List<RecipeStep>` | Navigation — ordered steps |
| `RecipeTags` | `List<RecipeTag>` | Navigation — tags applied to this recipe |

**Used by:**
| Consumer | Usage |
|---|---|
| `DevContext` | `DbSet<Recipe> Recipes` — EF Core maps this to the `Recipes` table |
| `RecipeController.GetRecipes()` | `.Include(recipe => recipe.RecipeIngredients / RecipeSteps / RecipeTags)` |
| `RecipeController.CreateRecipe([FromBody] Recipe r)` | Receives the full recipe graph from HTTP POST |
| `RecipeController.UpdateRecipe(Recipe r)` | Receives updated recipe graph from HTTP PUT |
| `RecipeService.GetRecipes()` | Deserializes JSON response into `List<Recipe>` |
| `RecipeService.CreateRecipe(Recipe r)` | Serializes and POST's to the API |
| `RecipeService.UpdateRecipe(Recipe r)` | Serializes and PUT's to the API |

---

## Ingredient
**File:** `Shared/Models/Ingredient.cs`

Lookup table entry representing a reusable ingredient.

| Member | Type | Description |
|---|---|---|
| `Id` | `int` | Primary key (`[Column("ID")]`) |
| `IngredientName` | `string` | Name of the ingredient, max 50 chars |

**Used by:**
| Consumer | Usage |
|---|---|
| `DevContext` | `DbSet<Ingredient> Ingredients` |
| `RecipeIngredient.Ing` | Navigation property — the actual `Ingredient` entity for a recipe line |
| `RecipeController.GetAllIngredients()` | Returns all ingredients |
| `RecipeController.CreateIngredient(Ingredient i)` | Creates a new ingredient |
| `RecipeController.UpdateIngredient(int id, Ingredient updated)` | Updates ingredient name |
| `RecipeController.DeleteIngredient(int id)` | Deletes by id |
| `RecipeService.CreateIngredient(Ingredient i)` | POST to create |
| `RecipeService.SearchIngredients(string s)` | GET filtered list |
| `RecipeService.UpdateIngredientAsync(Ingredient ingredient)` | PUT update |
| `RecipeService.DeleteIngredientAsync(int id)` | DELETE by id |

---

## RecipeIngredient
**File:** `Shared/Models/RecipeIngredient.cs`

Join entity linking a `Recipe` to an `Ingredient`, with quantity and modifier details.  
Composite primary key: `(IngId, RecipeId)`.

| Member | Type | Description |
|---|---|---|
| `IngId` | `int` | FK → `Ingredient.Id` |
| `RecipeId` | `int` | FK → `Recipe.Id` |
| `IngredientNameModifier` | `string?` | Optional modifier, e.g. "diced", "finely chopped" |
| `Quantity` | `double` | Amount required |
| `QuantityDesc` | `string?` | Unit or description, e.g. "cups", "tbsp" |
| `Ing` | `Ingredient` | Navigation property to the ingredient entity |

**Used by:**
| Consumer | Usage |
|---|---|
| `DevContext` | `DbSet<RecipeIngredient> RecipeIngredients` |
| `RecipeController.CreateRecipe()` | Sets `RecipeId` and attaches the existing `Ing` entity |
| `RecipeController.UpdateRecipe()` | Clears and rebuilds the ingredient list |
| `RecipeController.ImportFromJson()` | Re-links or creates ingredients when importing |

---

## RecipeStep
**File:** `Shared/Models/RecipeStep.cs`

An ordered instruction step belonging to a recipe.

| Member | Type | Description |
|---|---|---|
| `Id` | `int` | Primary key (`[Column("ID")]`) |
| `RecipeId` | `int` | FK → `Recipe.Id` |
| `Step` | `string` | The main instruction text |
| `SubText` | `string?` | Optional supplementary detail |

**Used by:**
| Consumer | Usage |
|---|---|
| `DevContext` | `DbSet<RecipeStep> RecipeSteps` |
| `RecipeController.CreateRecipe()` | Sets `RecipeId` and inserts each step |
| `RecipeController.UpdateRecipe()` | Clears and rebuilds the steps list |

---

## RecipeTag
**File:** `Shared/Models/RecipeTag.cs`

A free-text label applied to a recipe (e.g. "vegetarian", "quick").

| Member | Type | Description |
|---|---|---|
| `Id` | `int` | Primary key (`[Column("ID")]`) |
| `RecipeId` | `int` | FK → `Recipe.Id` |
| `Tag` | `string` | Tag value, max 40 chars |

**Used by:**
| Consumer | Usage |
|---|---|
| `DevContext` | `DbSet<RecipeTag> RecipeTags` |
| `RecipeController.CreateRecipe()` | Sets `RecipeId` and inserts each tag |
| `RecipeController.UpdateRecipe()` | Clears and rebuilds the tags list |
| `RecipeController.GetRecipeTags(int rid)` | Returns tags for a specific recipe |
| `RecipeController.GetRecipeTag(int rid, int id)` | Returns a single tag |
