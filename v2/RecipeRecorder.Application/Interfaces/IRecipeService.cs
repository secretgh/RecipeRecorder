using RecipeRecorder.Shared.DTOs;

namespace RecipeRecorder.Application.Interfaces
{
    public interface IRecipeService
    {
        Task<IEnumerable<RecipeDto>> GetAllAsync();
        Task<RecipeDto?> GetByIdAsync(int id);
        Task<RecipeDto> CreateAsync(RecipeDto dto);
        Task<bool> UpdateAsync(int id, RecipeDto dto);
        Task<bool> DeleteAsync(int id);

        Task<List<IngredientDto>> SearchIngredientsAsync(string query, int limit);
    }
}
