using Microsoft.AspNetCore.Components.Web.Virtualization;
using RecipeRecorder.Shared;
using System.Drawing.Printing;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using static System.Net.WebRequestMethods;

namespace RecipeRecorder.Client.Service
{
    public interface IRecipeService
    {
        Task<List<Recipe>> GetRecipes();
        Task<Recipe> GetRecipe(int id);
        Task CreateRecipe(Recipe r);
        Task UpdateRecipe(Recipe r);
        Task SaveRecipesToJson();
        Task<List<Ingredient>> SearchIngredients(string s);
        Task<Ingredient> CreateIngredient(Ingredient i);
        Task ExpandIngredientRecipes(int ingredientId);
        Dictionary<int, List<Recipe>> GetIngredientRecipes();
        HashSet<int> GetExpandedIngredients();
        ValueTask<ItemsProviderResult<Ingredient>> LoadIngredients(ItemsProviderRequest request);
        Task<List<Recipe>> GetRecipesUsingIngredientAsync(int ingredientId);
        Task LoadIngredientRecipes();
        Task AddIngredientAsync(Ingredient ingredient);
        Task UpdateIngredientAsync(Ingredient ingredient);
        Task DeleteIngredientAsync(int id);
        Task<List<RecipeTag>> GetTags();
        Task<List<Recipe>> GetFilteredRecipes(string search, List<int> tagIds);
        Task<bool> TestConnection();
        Task Test();
    }

    public class RecipeService : IRecipeService
    {
        private readonly HttpClient _httpClient;
        private HashSet<int> expandedIngredients;
        private Dictionary<int, List<Recipe>> ingredientRecipes;

        public RecipeService(HttpClient client) { 
            _httpClient = client;
            expandedIngredients = new();
            ingredientRecipes = new();
        }

        public async Task Test()
        {
            await _httpClient.PostAsJsonAsync<string>("api/Recipe/Test", "");
        }
        
        public async Task SaveRecipesToJson()
        {
            await _httpClient.GetAsync("Api/Recipe/SaveToJson");
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

        public async Task UpdateRecipe(Recipe r)
        {
            var result = await _httpClient.PutAsJsonAsync($"api/Recipe", r);
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

        public async ValueTask<ItemsProviderResult<Ingredient>> LoadIngredients(ItemsProviderRequest request)
        {
            int pageNumber = (request.StartIndex / request.Count) + 1;
            int pageSize = request.Count;

            //var response = await _httpClient.GetFromJsonAsync<PagedResult<Ingredient>>(
            //    $"api/recipe/ingredients/page/{pageNumber}/{pageSize}"
            //);

            //return new ItemsProviderResult<Ingredient>(
            //    response!.Items, response.TotalCount
            //);

            var response = await _httpClient.GetAsync($"api/recipe/ingredients/page/{pageNumber}/{pageSize}");
            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Server returned {response.StatusCode}: {body}");
                throw new Exception(body);
            }

            var result = JsonSerializer.Deserialize<PagedResult<Ingredient>>(body,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return new ItemsProviderResult<Ingredient>(result!.Items, result.TotalCount);
        }

        public HashSet<int> GetExpandedIngredients()
        {
            return expandedIngredients;
        }

        public Dictionary<int, List<Recipe>> GetIngredientRecipes()
        {
            return ingredientRecipes;
        }

        public async Task ExpandIngredientRecipes(int ingredientId)
        {
            if (expandedIngredients.Contains(ingredientId))
            {
                expandedIngredients.Remove(ingredientId);
            }
            else
            {
                expandedIngredients.Add(ingredientId);

                if (!ingredientRecipes.ContainsKey(ingredientId))
                {
                    var recipes = await _httpClient.GetFromJsonAsync<List<Recipe>>(
                        $"api/recipe/ingredients/{ingredientId}/recipes"
                    );

                    if (recipes != null)
                        ingredientRecipes[ingredientId] = recipes;
                }
            }
        }

        public async Task LoadIngredientRecipes()
        {
            List<Ingredient> ings = await _httpClient.GetFromJsonAsync<List<Ingredient>>($"api/recipe/ingredients");
            if(ings != null)
            {
                foreach(Ingredient i in ings)
                {
                    await ExpandIngredientRecipes(i.Id);
                }
                expandedIngredients.Clear();
            }
        }

        public async Task AddIngredientAsync(Ingredient ingredient)
        {
            await _httpClient.PostAsJsonAsync("api/recipe/ingredients", ingredient);
        }

        public async Task UpdateIngredientAsync(Ingredient ingredient)
        {
            await _httpClient.PutAsJsonAsync($"api/recipe/ingredients/{ingredient.Id}", ingredient);
        }

        public async Task DeleteIngredientAsync(int id)
        {
            await _httpClient.DeleteAsync($"api/recipe/ingredients/{id}");
        }

        public async Task<List<Recipe>> GetRecipesUsingIngredientAsync(int ingredientId)
        {
            return await _httpClient.GetFromJsonAsync<List<Recipe>>($"api/recipe/ingredients/{ingredientId}/recipes");
        }

        public Task<List<RecipeTag>> GetTags()
        {
            throw new NotImplementedException();
        }

        public Task<List<Recipe>> GetFilteredRecipes(string search, List<int> tagIds)
        {
            throw new NotImplementedException();
        }

        public class PagedResult<T>
        {
            public List<T> Items { get; set; } = [];
            public int TotalCount { get; set; }
        }
    }
}
