using RecipeRecorder.Shared;
using System.Net.Http.Json;

namespace RecipeRecorder.Client.Service
{
    public class RecipeService : IRecipeService
    {
        private readonly HttpClient _httpClient;

        public RecipeService(HttpClient client) { 
            _httpClient = client;
        }

        public async Task<List<Recipe>> GetRecipes()
        {
           return await _httpClient.GetFromJsonAsync<List<Recipe>>("api/recipes");
        }

        public async Task<Recipe> GetRecipe(int id)
        {
            return await _httpClient.GetFromJsonAsync<Recipe>($"api/recipes/{id}");
        }

        public async Task<List<Recipe>> CreateRecipe(Recipe r)
        {
            var result = await _httpClient.PostAsJsonAsync<Recipe>($"api/recipes", r);
            var recipes = await result.Content.ReadFromJsonAsync<List<Recipe>>();
            return recipes;
        }

        public async Task<List<Recipe>> UpdateRecipe(Recipe r)
        {
            var result = await _httpClient.PutAsJsonAsync<Recipe>($"api/recipes", r);
            var recipes = await result.Content.ReadFromJsonAsync<List<Recipe>>();
            return recipes;
        }
    }
}
