using RecipeRecorder.Application.Interfaces;
using RecipeRecorder.Shared.DTOs;
using System.Net.Http.Json;

namespace RecipeRecorder.Application.Services
{
    public class RecipeAppService : IRecipeService
    {
        private readonly HttpClient _client;
        private readonly Uri APIBaseAddress = new Uri("https://localhost:7187/");

        public RecipeAppService(HttpClient client)
        {
            _client = client;
            _client.BaseAddress = APIBaseAddress;
        }

        public async Task<List<RecipeDto>> GetAllAsync()
        {
            List<RecipeDto> recipes = new List<RecipeDto>();
            recipes = await _client.GetFromJsonAsync<List<RecipeDto>>("api/recipes") ?? recipes;
            return recipes;
        }

        public async Task<RecipeDto?> GetByIdAsync(int id)
        {
            return await _client.GetFromJsonAsync<RecipeDto>($"api/recipes/{id}");
        }

        public async Task<RecipeDto> CreateAsync(RecipeDto dto)
        {
            var response = await _client.PostAsJsonAsync("api/recipes", dto);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<RecipeDto>()
                   ?? throw new Exception("Failed to create recipe");
        }

        public async Task<bool> UpdateAsync(int id, RecipeDto dto)
        {
            var response = await _client.PutAsJsonAsync($"api/recipes/{id}", dto);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var response = await _client.DeleteAsync($"api/recipes/{id}");
            return response.IsSuccessStatusCode;
        }

        public async Task<List<IngredientDto>> SearchIngredientsAsync(string query, int limit)
        {
            return await _client.GetFromJsonAsync<List<IngredientDto>>($"api/ingredients/search?query={query}&limit={limit}") ?? new List<IngredientDto>();
        }
    }
}
