using Microsoft.AspNetCore.Mvc;
using RecipeRecorder.Application.Interfaces;
using RecipeRecorder.Domain;
using RecipeRecorder.Domain.Interfaces;
using RecipeRecorder.Shared.DTOs;

namespace RecipeRecorder.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RecipesController : ControllerBase
    {
        private readonly IRecipeService _service;

        public RecipesController(IRecipeService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IEnumerable<RecipeDto>> GetAll()
            => await _service.GetAllAsync();

        [HttpGet("{id}")]
        public async Task<ActionResult<RecipeDto>> Get(int id)
        {
            var recipe = await _service.GetByIdAsync(id);
            return recipe is null ? NotFound() : Ok(recipe);
        }

        [HttpPost]
        public async Task<ActionResult<RecipeDto>> Create(RecipeDto dto)
        {
            var created = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Update(int id, RecipeDto dto)
        {
            var success = await _service.UpdateAsync(id, dto);
            return success ? NoContent() : NotFound();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            var success = await _service.DeleteAsync(id);
            return success ? NoContent() : NotFound();
        }


        [HttpGet("/api/ingredients/search")]
        public async Task<IEnumerable<IngredientDto>> SearchIngredients(string query, int limit=10)
        {
            var ingredients = await _service.SearchIngredientsAsync(query, limit);
            return ingredients; 
        }
    }
}
