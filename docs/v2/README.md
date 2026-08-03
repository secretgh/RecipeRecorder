# RecipeRecorder v2 — Architecture Overview

## Pattern
v2 adopts **Clean Architecture** with clear separation between Domain, Application, Infrastructure, and presentation layers. A dedicated REST API (`RecipeRecorder.API`) sits between the data store and the clients. Two client frontends exist: `RecipeRecorder.Web` (Blazor WASM) and `RecipeRecorder.Maui` (.NET MAUI Blazor Hybrid).

## Project Structure

| Project | Layer | Role |
|---|---|---|
| `v2/RecipeRecorder.Domain` | Domain | Entities with encapsulated business rules, repository interfaces |
| `v2/RecipeRecorder.Application` | Application | Use-case services, application-level interfaces |
| `v2/RecipeRecorder.Infrastructure` | Infrastructure | EF Core DbContext, repository implementations |
| `v2/RecipeRecorder.API` | Presentation (API) | REST API — thin controllers that delegate to Application services |
| `v2/RecipeRecorder.Shared` | Shared (cross-cutting) | DTOs and wizard data classes shared between API and clients |
| `v2/RecipeRecorder.Web` | Presentation (Web) | Blazor WASM browser client |
| `v2/RecipeRecorder.Maui` | Presentation (Mobile/Desktop) | .NET MAUI Blazor Hybrid mobile/desktop client |
| `v2/RecipeRecorder.Application.Tests` | Tests | Unit tests for Application services |

## Dependency Flow

```
RecipeRecorder.Web / RecipeRecorder.Maui
  └─ RecipeAppService  ──HTTP──►  RecipeRecorder.API
                                        └─ RecipesController
                                              └─ IRecipeService (Application)
                                                    └─ RecipeAPIService
                                                          ├─ IRecipeRepo  →  RecipeRepo
                                                          └─ IIngredientRepo  →  IngredientRepo
                                                                                    └─ RecipeDbContext
                                                                                          └─ SQL Server
                                    ◄──── RecipeRecorder.Shared (DTOs) ────►
```

## Key Architecture Files

| File | Layer | Description |
|---|---|---|
| `Domain/Entities/Recipe.cs` | Domain | Root aggregate entity with encapsulated mutation methods |
| `Domain/Entities/RecipeIngredient.cs` | Domain | Join entity with validation on quantity |
| `Domain/Entities/Ingredient.cs` | Domain | Ingredient with normalized name for uniqueness |
| `Domain/Entities/RecipeStep.cs` | Domain | Instruction step entity |
| `Domain/Entities/RecipeTag.cs` | Domain | Tag entity |
| `Domain/Interfaces/IRecipeRepo.cs` | Domain | Repository contract for recipes |
| `Domain/Interfaces/IIngredientRepo.cs` | Domain | Repository contract for ingredients (with search + upsert) |
| `Application/Interfaces/IRecipeService.cs` | Application | Application service contract |
| `Application/Services/RecipeAPIService.cs` | Application | Server-side service — orchestrates repos, maps domain → DTOs |
| `Application/Services/RecipeAppService.cs` | Application | Client-side service — HTTP calls to the API |
| `Infrastructure/RecipeDBContext.cs` | Infrastructure | EF Core DbContext with full relationship configuration |
| `Infrastructure/Repos/RecipeRepo.cs` | Infrastructure | EF Core implementation of `IRecipeRepo` |
| `Infrastructure/Repos/IngredientRepo.cs` | Infrastructure | EF Core implementation of `IIngredientRepo` |
| `API/Controllers/RecipesController.cs` | API | Thin REST controller — delegates everything to `IRecipeService` |
| `API/Program.cs` | API | DI registration, middleware, CORS |
| `Shared/DTOs/RecipeDto.cs` | Shared | DTO used for all recipe API traffic |
| `Shared/DTOs/RecipeIngredientDto.cs` | Shared | DTO for recipe ingredient lines |
| `Shared/Classes/RecipeWizardData.cs` | Shared | Wizard state wrapper around `RecipeDto` |

## Detailed Documentation

- [Domain — Entities & Interfaces](./domain.md)
- [Application — Services & Interfaces](./application.md)
- [Infrastructure — DbContext & Repositories](./infrastructure.md)
- [API — Controller & Program.cs](./api.md)
- [Shared — DTOs & WizardData](./shared.md)
