using RecipeRecorder.Shared;
namespace RecipeRecorder.Client.Service
{
    public interface IRecipeService
    {
        Task<List<Recipe>> GetRecipes();
        Task<Recipe> GetRecipe(int id);
        Task<List<Recipe>> CreateRecipe(Recipe r);
        Task<List<Recipe>> UpdateRecipe(Recipe r);
    }
}
