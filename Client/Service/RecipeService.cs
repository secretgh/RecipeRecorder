using RecipeRecorder.Shared;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace RecipeRecorder.Client.Service
{
    public interface IRecipeService
    {
        Task<List<Recipe>> GetRecipes();
        Task<Recipe> GetRecipe(int id);
        Task CreateRecipe(Recipe r);
        Task<Recipe> UpdateRecipe(Recipe r);
        Task<List<Ingredient>> SearchIngredients(string s);
        Task<Ingredient> CreateIngredient(Ingredient i);
        Task DeleteTag(int rid, int id);
        Task<bool> TestConnection();
    }

    public class RecipeService : IRecipeService
    {
        private readonly HttpClient _httpClient;

        public RecipeService(HttpClient client) { 
            _httpClient = client;
        }

        public async Task<bool> TestConnection() {
            return await _httpClient.GetFromJsonAsync<bool>("/TestConnection");
        }

        public async Task<List<Recipe>> GetRecipes()
        {
           return await _httpClient.GetFromJsonAsync<List<Recipe>>("api/Recipe");
        }

        public async Task<Recipe> GetRecipe(int id)
        {
            return await _httpClient.GetFromJsonAsync<Recipe>($"api/Recipe/{id}");
        }

        public async Task CreateRecipe(Recipe r)
        {
            Console.WriteLine("Service calling create Recipe.");
            JsonContent content = JsonContent.Create(r);
            HttpResponseMessage result = await _httpClient.PostAsJsonAsync("api/Recipe", r);
        }

        public async Task<Recipe> UpdateRecipe(Recipe r)
        {
            var result = await _httpClient.PutAsJsonAsync($"api/Recipe", r);
            var Recipe = await result.Content.ReadFromJsonAsync<Recipe>();
            return Recipe;
        }

        public async Task CreateRecipeIngredients(int rid, RecipeIngredient i)
        {
            await _httpClient.PostAsJsonAsync<RecipeIngredient>($"api/Recipe/{rid}/Recipeingredients", i);
        }


        public async Task<List<RecipeIngredient>> GetRecipeIngredients(int rid)
        {
            var results = await _httpClient.GetFromJsonAsync<List<RecipeIngredient>>($"api/Recipe/{rid}/Recipeingredients");
            return results;
        }        

        public async Task<Ingredient> CreateIngredient(Ingredient i)
        {
            var result = await _httpClient.PostAsJsonAsync<Ingredient>($"api/Recipe/ingredients", i);
            var ingredient = await result.Content.ReadFromJsonAsync<Ingredient>();

            return ingredient;
        }

        public async Task<List<Ingredient>> SearchIngredients(string s)
        {
            var ingredients = await _httpClient.GetFromJsonAsync<List<Ingredient>>($"api/Recipe/ingredients/{s}");
            return ingredients;
        }

        public async Task DeleteTag(int rid, int id)
        {
            await _httpClient.DeleteAsync($"api/Recipe/{rid}/tags/{id}");
        }
    }
}
