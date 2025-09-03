namespace RecipeRecorder.Domain.Interfaces
{
    public interface IRecipeRepo
    {
        Task<Recipe?> GetByIdAsync(int id);
        Task<List<Recipe>> GetAllAsync();
        Task<Recipe> AddAsync(Recipe recipe);
        Task UpdateAsync(Recipe recipe);
        Task DeleteAsync(int id);
    }
}