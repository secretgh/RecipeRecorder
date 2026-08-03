# RecipeRecorder v1 — Architecture Overview

## Pattern
v1 is a **Blazor WebAssembly + ASP.NET Core hosted** application. The client (WASM) runs in the browser and communicates with the server via HTTP. Shared models are defined once in the `Shared` project and used by both sides.

## Project Structure

| Project | Type | Role |
|---|---|---|
| `v1/Shared` | Class Library | Shared data models used by Server and Client |
| `v1/Server` | ASP.NET Core Web API | REST API, EF Core DbContext, SQL Server |
| `v1/Client` | Blazor WebAssembly | Browser-side UI, calls Server via HTTP |

## Dependency Flow

```
Client (Blazor WASM)
  └─ RecipeService  ──HTTP──► RecipeController (Server)
                                    └─ DevContext (EF Core)
                                          └─ SQL Server
                    ◄── Shared Models (Recipe, Ingredient, …) ──►
```

## Key Architecture Files

| File | Project | Description |
|---|---|---|
| `Shared/Models/Recipe.cs` | Shared | Root aggregate model |
| `Shared/Models/RecipeIngredient.cs` | Shared | Join model linking Recipe ↔ Ingredient |
| `Shared/Models/Ingredient.cs` | Shared | Ingredient lookup table model |
| `Shared/Models/RecipeStep.cs` | Shared | Ordered step belonging to a Recipe |
| `Shared/Models/RecipeTag.cs` | Shared | Tag belonging to a Recipe |
| `Server/DevContext.cs` | Server | EF Core DbContext — maps models to SQL Server |
| `Server/Controllers/RecipeController.cs` | Server | REST API — all recipe and ingredient endpoints |
| `Server/Program.cs` | Server | App bootstrap, service registration, middleware |
| `Client/Service/RecipeService.cs` | Client | HTTP client wrapper — all API calls |
| `Client/Program.cs` | Client | Blazor WASM bootstrap, DI registration |

## Detailed Documentation

- [Shared Models](./shared-models.md)
- [Server — DevContext & RecipeController](./server.md)
- [Client — RecipeService & Program.cs](./client.md)
