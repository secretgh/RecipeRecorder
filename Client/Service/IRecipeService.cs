using RecipeRecorder.Shared;
namespace RecipeRecorder.Client.Service
{
    public interface IRecipeService
    {
        Task<List<Recipe>> GetRecipes();
    }
}
