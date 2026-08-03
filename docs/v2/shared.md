# v2 — Shared Layer

**Project:** `v2/RecipeRecorder.Shared`  
**Namespaces:** `RecipeRecorder.Shared.DTOs`, `RecipeRecorder.Shared.Classes`

The Shared project contains DTOs and helper classes that are referenced by both the API and client projects. It has no dependency on Domain, Application, or Infrastructure — it exists purely to carry data across process boundaries.

---

## DTOs

DTOs (Data Transfer Objects) are the shape of data sent over HTTP between the API and its clients. They are mapped to/from domain entities inside `RecipeAPIService`.

### RecipeDto
**File:** `Shared/DTOs/RecipeDto.cs`

The primary DTO for a complete recipe. Used in all API request and response bodies.

| Member | Type | Required | Description |
|---|---|---|---|
| `Id` | `int` | — | Recipe id (0 on create, server-assigned on response) |
| `RecipeName` | `string` | Yes | Display name |
| `RecipeDesc` | `string?` | No | Optional description |
| `Ingredients` | `List<RecipeIngredientDto>` | — | Ingredient lines (default empty list) |
| `Steps` | `List<RecipeStepDto>` | — | Instruction steps (default empty list) |
| `Tags` | `List<RecipeTagDto>` | — | Tags (default empty list) |

#### `ToString()` _(override)_
Returns a formatted string: `"{RecipeName} | tag1 tag2 …"`.  
**Used by:** Debug output, list display in `RecipeList.razor`.

**Used by:**
| Consumer | Usage |
|---|---|
| `RecipesController.GetAll()` | Response body |
| `RecipesController.Get(int id)` | Response body |
| `RecipesController.Create(RecipeDto dto)` | Request body and response body |
| `RecipesController.Update(int id, RecipeDto dto)` | Request body |
| `RecipeAPIService.GetAllAsync()` / `GetByIdAsync()` | Returned after `MapToDto()` |
| `RecipeAPIService.CreateAsync()` / `UpdateAsync()` | Input DTO consumed to build/update domain entities |
| `RecipeAppService` (all methods) | Serialized to/from JSON in HTTP calls |
| `RecipeList.razor` | Bound to `@foreach(RecipeDto recipe in recipes)` |
| `RecipeWizardData.Recipe` | Wrapped for wizard use |

---

### RecipeIngredientDto
**File:** `Shared/DTOs/RecipeIngredientDto.cs`

DTO for a single ingredient line within a recipe.

| Member | Type | Required | Description |
|---|---|---|---|
| `IngId` | `int` | — | Line-item id (0 on create) |
| `RecipeId` | `int` | — | Parent recipe id |
| `IngredientNameModifier` | `string?` | No | Context modifier, e.g. "finely chopped" |
| `Quantity` | `double` | Yes | Amount |
| `QuantityDesc` | `string?` | No | Unit, e.g. "cups" |
| `Ingredient` | `IngredientDto` | Yes | The ingredient reference |

**Used by:**
| Consumer | Usage |
|---|---|
| `RecipeDto.Ingredients` | Nested inside a `RecipeDto` |
| `RecipeAPIService.CreateAsync()` | Reads `ri.Ingredient.IngredientName` to upsert the ingredient |
| `RecipeAPIService.UpdateAsync()` | Reads fields to rebuild the ingredient collection |
| `RecipeAPIService.MapToDto()` | Mapped from `RecipeIngredient` domain entity |
| `ParsedIngredientData.ToDto()` | Converted from wizard-parsed ingredient data |

---

### IngredientDto
**File:** `Shared/DTOs/IngredientDto.cs`

Minimal DTO for an ingredient reference.

| Member | Type | Description |
|---|---|---|
| `Id` | `int` | Ingredient id |
| `IngredientName` | `string` | Display name |

**Used by:**
| Consumer | Usage |
|---|---|
| `RecipeIngredientDto.Ingredient` | Nested inside an ingredient line |
| `RecipeAPIService.SearchIngredientsAsync()` | Maps search results to this DTO |
| `RecipesController.SearchIngredients()` | Response body |
| `RecipeAppService.SearchIngredientsAsync()` | Deserialized from search response |

---

### RecipeStepDto
**File:** `Shared/DTOs/RecipeStepDto.cs`

DTO for a single instruction step.

| Member | Type | Description |
|---|---|---|
| `Id` | `int` | Step id |
| `RecipeId` | `int` | Parent recipe id |
| `Instruction` | `string` | Main instruction text |
| `SubText` | `string?` | Optional supplementary detail |

**Used by:**
| Consumer | Usage |
|---|---|
| `RecipeDto.Steps` | Nested inside a `RecipeDto` |
| `RecipeAPIService.CreateAsync()` / `UpdateAsync()` | Reads `rs.Instruction` / `rs.SubText` to construct `RecipeStep` domain entities |
| `RecipeAPIService.MapToDto()` | Mapped from `RecipeStep` domain entity |
| `StepWizardData.ToDto(int recipeId)` | Converted from wizard step data |
| `RecipeWizardData.ConvertStepsToDto()` | Collection of these returned from wizard |

---

### RecipeTagDto
**File:** `Shared/DTOs/RecipeTagDto.cs`

DTO for a single tag.

| Member | Type | Description |
|---|---|---|
| `Id` | `int` | Tag id |
| `RecipeId` | `int` | Parent recipe id |
| `Tag` | `string` | Tag value |

**Used by:**
| Consumer | Usage |
|---|---|
| `RecipeDto.Tags` | Nested inside a `RecipeDto` |
| `RecipeAPIService.CreateAsync()` / `UpdateAsync()` | Reads `rt.Tag` to construct `RecipeTag` domain entities |
| `RecipeAPIService.MapToDto()` | Mapped from `RecipeTag` domain entity |
| `RecipeDto.ToString()` | Tag values are joined and appended to the recipe string |

---

## Wizard Classes

### RecipeWizardData
**File:** `Shared/Classes/RecipeWizardData.cs`  
**Namespace:** `RecipeRecorder.Shared.Classes`

A state wrapper around `RecipeDto` designed for multi-step recipe creation wizards. Holds the recipe being built and the step entries before they are converted to DTOs.

| Member | Type | Description |
|---|---|---|
| `Recipe` | `RecipeDto` | The recipe being built |
| `WizardSteps` | `List<StepWizardData>` | Step entries collected during wizard flow |
| `Name` | `string` | Proxy for `Recipe.RecipeName` — reads/writes `Recipe.RecipeName` directly |
| `Description` | `string` | Proxy for `Recipe.RecipeDesc` — reads/writes `Recipe.RecipeDesc` directly |

#### `ConvertStepsToDto()` → `List<RecipeStepDto>`
Iterates `WizardSteps` and calls `StepWizardData.ToDto(Recipe.Id)` on each, returning the converted list.  
**Used by:** `SyncToRecipe()` internally.

#### `SyncToRecipe()`
**Must be called before `OnRecipeSaved` is invoked.** Flushes all wizard state into `Recipe` so the DTO is complete and ready to send to the API:

1. Re-assigns `Recipe.RecipeName` from `Name` and `Recipe.RecipeDesc` from `Description` (explicit safety flush — the proxy setters write live, but this ensures correctness even if future refactoring changes that).
2. Calls `ConvertStepsToDto()` and assigns the result to `Recipe.Steps`. **This is the critical step** — `Recipe.Steps` was previously never populated from `WizardSteps`, causing an empty steps list to be sent to the API.
3. `Recipe.Ingredients` and `Recipe.Tags` are already written directly onto `Recipe` by `IngredientSectionEditor` and `DescriptionSectionEditor` via their event callbacks, so no additional work is needed for those.

**Called by:** `RecipeWizardRoot.SaveRecipe()` immediately before `OnRecipeSaved.InvokeAsync(recipeData)`.

---

### StepWizardData
**File:** `Shared/Classes/RecipeWizardData.cs`

Represents a single step as entered in the wizard, with optional parsed metadata.

| Member | Type | Description |
|---|---|---|
| `StepNumber` | `int` | Display order |
| `Text` | `string` | Raw instruction text entered by the user |
| `ParsedData` | `ParsedStepData?` | Optional AI/regex-parsed info (ingredients, times) |

#### `ToDto(int recipeId)` → `RecipeStepDto`
Converts this wizard entry into a `RecipeStepDto`. Maps `Text` → `Instruction`; `SubText` is null (reserved for future parsed time info).  
**Called by:** `RecipeWizardData.ConvertStepsToDto()`

---

### ParsedStepData
**File:** `Shared/Classes/RecipeWizardData.cs`

Holds the result of parsing a step's text to extract ingredient names and time references.

| Member | Type | Description |
|---|---|---|
| `StepNumber` | `int` | The step this data belongs to |
| `OriginalText` | `string` | The raw text that was parsed |
| `Ingredients` | `List<string>` | Ingredient names identified in the step text |
| `Times` | `List<string>` | Time references identified in the step text (e.g. "30 minutes") |

**Used by:** `StepWizardData.ParsedData` — attached after parsing; not yet used in any conversion flow.

---

### ParsedIngredientData
**File:** `Shared/Classes/RecipeWizardData.cs`

Represents a parsed ingredient entry with quantity and modifier details, before it is converted to a DTO.

| Member | Type | Description |
|---|---|---|
| `Name` | `string` | Ingredient name |
| `Quantity` | `double?` | Parsed quantity (null if not found) |
| `QuantityDesc` | `string?` | Unit description |
| `IngredientNameModifier` | `string?` | Modifier, e.g. "chopped" |

#### `ToDto(int recipeId, IngredientDto ingredient)` → `RecipeIngredientDto`
Converts this parsed entry into a `RecipeIngredientDto`, combining the parsed fields with the resolved `IngredientDto`.  
`Quantity` defaults to `0` if null.  
**Used by:** Wizard ingredient assembly step — after an `IngredientDto` has been resolved via the search API.
