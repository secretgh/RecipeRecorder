# v2 — Domain Layer

**Project:** `v2/RecipeRecorder.Domain`  
**Namespace:** `RecipeRecorder.Domain`

The Domain layer contains entities with encapsulated business rules and repository interfaces. It has **no dependencies** on any other project — it defines the core concepts that everything else builds on.

---

## Entities

### Recipe
**File:** `Domain/Entities/Recipe.cs`

Root aggregate. All child collections (ingredients, steps, tags) are owned by and mutated through `Recipe`. Private setters enforce that state changes go through the domain methods.

| Member | Type | Description |
|---|---|---|
| `Id` | `int` | DB primary key — set by Infrastructure (private setter) |
| `RecipeName` | `string` | Required display name |
| `RecipeDesc` | `string?` | Optional description |
| `RecipeIngredients` | `List<RecipeIngredient>` | Collection of recipe–ingredient lines |
| `RecipeSteps` | `List<RecipeStep>` | Ordered instruction steps |
| `RecipeTags` | `List<RecipeTag>` | Tags applied to the recipe |

#### Constructor: `Recipe(string recipeName, string? recipeDesc = null)`
Enforces that `recipeName` is non-empty. Throws `ArgumentException` if blank.  
**Used by:** `RecipeAPIService.CreateAsync(RecipeDto dto)` — creates a new domain entity from a DTO.

#### `AddIngredient(RecipeIngredient ingredient)`
Appends an ingredient to `RecipeIngredients`. Null-guards the argument.  
**Used by:** `RecipeAPIService.CreateAsync()`, `RecipeAPIService.UpdateAsync()`

#### `RemoveIngredient(RecipeIngredient ingredient)`
Removes a specific ingredient line from the collection.

#### `AddStep(RecipeStep step)`
Appends a step to `RecipeSteps`. Null-guards the argument.  
**Used by:** `RecipeAPIService.CreateAsync()`, `RecipeAPIService.UpdateAsync()`

#### `RemoveStep(RecipeStep step)`
Removes a specific step from the collection.

#### `AddTag(RecipeTag tag)`
Appends a tag only if the same tag value (case-insensitive) does not already exist — prevents duplicates.  
**Used by:** `RecipeAPIService.CreateAsync()`, `RecipeAPIService.UpdateAsync()`

#### `RemoveTag(RecipeTag tag)`
Removes a specific tag from the collection.

#### `UpdateName(string name)`
Updates `RecipeName`. Throws `ArgumentException` if the new name is blank.  
**Used by:** `RecipeAPIService.UpdateAsync()`

#### `UpdateDescription(string? desc)`
Updates `RecipeDesc`. Accepts null.  
**Used by:** `RecipeAPIService.UpdateAsync()`

#### `ClearIngredients()` / `ClearSteps()` / `ClearTags()`
Bulk-clears the respective child collections. Used before rebuilding them during an update.  
**Used by:** `RecipeAPIService.UpdateAsync()`

---

### Ingredient
**File:** `Domain/Entities/Ingredient.cs`

Shared lookup entity representing a reusable ingredient. Uses a `NormalizedName` (lowercase, trimmed) to enforce uniqueness at the application level.

| Member | Type | Description |
|---|---|---|
| `Id` | `int` | DB primary key (private setter) |
| `IngredientName` | `string` | Display name (backing field `_ingredientName`) |
| `NormalizedName` | `string` | Lowercased, trimmed version — used for uniqueness checks |

#### Constructor: `Ingredient(string ingredientName)`
Sets `_ingredientName` and computes `NormalizedName`. Throws if blank.  
**Used by:** `RecipeAPIService.UpdateAsync()` (creates transient ingredient objects for update), `IngredientRepo.GetOrCreateAsync()` (creates persisted ingredients).

#### `UpdateName(string newName)`
Updates the ingredient name. Throws if blank.  
**Used by:** (available for future update flows)

#### `NormalizeString(string name)` _(private static)_
Trims and lowercases the input. Used internally to set `NormalizedName`.  
**Used by:** constructor, implicitly by `IngredientRepo` which calls `NormalizeName` independently.

---

### RecipeIngredient
**File:** `Domain/Entities/RecipeIngredient.cs`

Join entity linking a `Recipe` to an `Ingredient`, carrying quantity metadata.

| Member | Type | Description |
|---|---|---|
| `IngId` | `int` | DB primary key |
| `RecipeId` | `int` | FK → `Recipe.Id` |
| `IngredientNameModifier` | `string?` | Context modifier, e.g. "diced" |
| `Quantity` | `double` | Amount (backing field `_quantity`) |
| `QuantityDesc` | `string?` | Unit label, e.g. "cups" |
| `Ingredient` | `Ingredient` | Navigation property (non-null at runtime) |

#### Constructor: `RecipeIngredient(Ingredient ingredient, double quantity, string? quantityDesc, string? nameModifier)`
Null-guards `ingredient`. Sets all fields.  
**Used by:** `RecipeAPIService.CreateAsync()`, `RecipeAPIService.UpdateAsync()`

#### `UpdateQuantity(double newQuantity)`
Updates quantity. Throws `ArgumentException` if `newQuantity <= 0`.

#### `UpdateQuantityDesc(string? newDesc)`
Updates the unit description.

#### `UpdateNameModifier(string? newModifier)`
Updates the name modifier.

---

### RecipeStep
**File:** `Domain/Entities/RecipeStep.cs`

An instruction step belonging to a recipe.

| Member | Type | Description |
|---|---|---|
| `Id` | `int` | DB primary key |
| `RecipeId` | `int` | FK → `Recipe.Id` |
| `Instruction` | `string` | Step instruction text (backing field `_instruction`) |
| `SubText` | `string?` | Optional supplementary detail |

#### Constructor: `RecipeStep(string instruction, string? subText = null)`
Throws `ArgumentException` if `instruction` is blank.  
**Used by:** `RecipeAPIService.CreateAsync()`, `RecipeAPIService.UpdateAsync()`

#### `UpdateInstruction(string newInstruction)`
Updates instruction. Throws if blank.

#### `UpdateSubText(string? newSubText)`
Updates sub-text.

---

### RecipeTag
**File:** `Domain/Entities/RecipeTag.cs`

A label applied to a recipe.

| Member | Type | Description |
|---|---|---|
| `Id` | `int` | DB primary key |
| `RecipeId` | `int` | FK → `Recipe.Id` |
| `Tag` | `string` | Tag value (backing field `_tag`) |

#### Constructor: `RecipeTag(string tag)`
Throws `ArgumentException` if `tag` is blank.  
**Used by:** `RecipeAPIService.CreateAsync()`, `RecipeAPIService.UpdateAsync()`. Also called by `Recipe.AddTag()` to check for duplicates.

#### `UpdateTag(string newTag)`
Updates the tag value. Throws if blank.

---

## Interfaces

### IRecipeRepo
**File:** `Domain/Interfaces/IRecipeRepo.cs`  
**Namespace:** `RecipeRecorder.Domain.Interfaces`

Defines the persistence contract for `Recipe`. Implemented by `RecipeRepo` in the Infrastructure layer.

| Method | Returns | Description |
|---|---|---|
| `GetByIdAsync(int id)` | `Task<Recipe?>` | Fetch a single recipe by id, returns null if not found |
| `GetAllAsync()` | `Task<List<Recipe>>` | Fetch all recipes |
| `AddAsync(Recipe recipe)` | `Task<Recipe>` | Persist a new recipe, return it with assigned Id |
| `UpdateAsync(Recipe recipe)` | `Task` | Persist changes to an existing recipe |
| `DeleteAsync(int id)` | `Task` | Remove a recipe by id |

**Implemented by:** `RecipeRepo` (Infrastructure)  
**Used by:** `RecipeAPIService` (Application)

---

### IIngredientRepo
**File:** `Domain/Interfaces/IIngredientRepo.cs`  
**Namespace:** `RecipeRecorder.Domain.Interfaces`

Defines the persistence contract for `Ingredient`, including upsert and search capabilities.

| Method | Returns | Description |
|---|---|---|
| `GetByIdAsync(int id, CancellationToken)` | `Task<Ingredient?>` | Fetch by id |
| `GetByNameAsync(string name, CancellationToken)` | `Task<Ingredient?>` | Fetch by normalized name |
| `GetAllAsync(CancellationToken)` | `Task<List<Ingredient>>` | Fetch all, ordered by name |
| `AddAsync(Ingredient, CancellationToken)` | `Task<Ingredient>` | Persist new ingredient |
| `UpdateAsync(Ingredient, CancellationToken)` | `Task<Ingredient>` | Persist updated ingredient |
| `DeleteAsync(int id, CancellationToken)` | `Task` | Remove by id |
| `GetOrCreateAsync(string name, CancellationToken)` | `Task<Ingredient>` | Upsert — returns existing or creates new by normalized name |
| `SearchAsync(string searchTerm, int maxResults, CancellationToken)` | `Task<List<Ingredient>>` | Autocomplete search by partial normalized name |

**Implemented by:** `IngredientRepo` (Infrastructure)  
**Used by:** `RecipeAPIService` (Application)
