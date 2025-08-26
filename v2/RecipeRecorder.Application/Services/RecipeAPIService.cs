using RecipeRecorder.Application.Interfaces;
using RecipeRecorder.Domain;
using RecipeRecorder.Domain.Interfaces;
using RecipeRecorder.Shared.DTOs;

namespace RecipeRecorder.Application.Services
{
    public class RecipeAPIService : IRecipeService
    {
        private readonly IRecipeRepo _repository;
        private readonly IIngredientRepo _ingredientRepository;

        public RecipeAPIService(IRecipeRepo repository, IIngredientRepo ingredientRepo)
        {
            _repository = repository;
            _ingredientRepository = ingredientRepo;
        }

        //Recipe
        public async Task<IEnumerable<RecipeDto>> GetAllAsync()
        {
            var recipes = await _repository.GetAllAsync();

            return recipes.Select(MapToDto);
        }

        public async Task<RecipeDto?> GetByIdAsync(int id)
        {
            var recipe = await _repository.GetByIdAsync(id);
            if (recipe == null) return null;

            return MapToDto(recipe);
        }

        public async Task<RecipeDto> CreateAsync(RecipeDto dto)
        {
            var recipe = new Recipe(dto.RecipeName, dto.RecipeDesc);

            foreach (var ri in dto.Ingredients)
            {
                var ingredient = await _ingredientRepository.GetOrCreateAsync(ri.Ingredient.IngredientName);
                recipe.AddIngredient(new RecipeIngredient(ingredient, ri.Quantity, ri.QuantityDesc, ri.IngredientNameModifier));
            }

            foreach (var rs in dto.Steps)
            {
                recipe.AddStep(new RecipeStep(rs.Instruction, rs.SubText));
            }

            foreach (var rt in dto.Tags)
            {
                recipe.AddTag(new RecipeTag(rt.Tag));
            }

            var created = await _repository.AddAsync(recipe);

            return MapToDto(created);
        }

        public async Task<bool> UpdateAsync(int id, RecipeDto dto)
        {
            var existingRecipe = await _repository.GetByIdAsync(id);
            if (existingRecipe == null)
                return false;

            existingRecipe.UpdateName(dto.RecipeName);
            existingRecipe.UpdateDescription(dto.RecipeDesc);

            existingRecipe.ClearIngredients();
            foreach (var ri in dto.Ingredients)
            {
                var ingredient = new Ingredient(ri.Ingredient.IngredientName);
                existingRecipe.AddIngredient(new RecipeIngredient(ingredient, ri.Quantity, ri.QuantityDesc, ri.IngredientNameModifier));
            }

            existingRecipe.ClearSteps();
            foreach (var rs in dto.Steps)
            {
                existingRecipe.AddStep(new RecipeStep(rs.Instruction, rs.SubText));
            }

            existingRecipe.ClearTags();
            foreach (var rt in dto.Tags)
            {
                existingRecipe.AddTag(new RecipeTag(rt.Tag));
            }

            await _repository.UpdateAsync(existingRecipe);
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var existingRecipe = await _repository.GetByIdAsync(id);
            if (existingRecipe == null)
                return false;

            await _repository.DeleteAsync(id);
            return true;
        }

        //Ingredient
        public async Task<List<IngredientDto>> SearchIngredientsAsync(string query, int limit = 10)
        {
            var results = await _ingredientRepository.SearchAsync(query, limit);

            return results
                .Select(i => new IngredientDto
                {
                    Id = i.Id,
                    IngredientName = i.IngredientName
                })
                .ToList();
        }


        // 🔹 Helper to centralize mapping
        private static RecipeDto MapToDto(Recipe r)
        {
            return new RecipeDto
            {
                Id = r.Id,
                RecipeName = r.RecipeName,
                RecipeDesc = r.RecipeDesc,
                Ingredients = r.RecipeIngredients.Select(ri => new RecipeIngredientDto
                {
                    IngId = ri.IngId,
                    RecipeId = ri.RecipeId,
                    Quantity = ri.Quantity,
                    QuantityDesc = ri.QuantityDesc,
                    IngredientNameModifier = ri.IngredientNameModifier,
                    Ingredient = new IngredientDto
                    {
                        Id = ri.Ingredient.Id,
                        IngredientName = ri.Ingredient.IngredientName
                    }
                }).ToList(),
                Steps = r.RecipeSteps.Select(rs => new RecipeStepDto
                {
                    Id = rs.Id,
                    RecipeId = rs.RecipeId,
                    Instruction = rs.Instruction,
                    SubText = rs.SubText
                }).ToList(),
                Tags = r.RecipeTags.Select(rt => new RecipeTagDto
                {
                    Id = rt.Id,
                    RecipeId = rt.RecipeId,
                    Tag = rt.Tag
                }).ToList()
            };
        }
    }
}
