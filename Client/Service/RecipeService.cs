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
           return await _httpClient.GetFromJsonAsync<List<Recipe>>("api/Recipe");
        }
    }
}
