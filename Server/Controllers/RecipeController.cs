using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecipeRecorder.Shared;

namespace RecipeRecorder.Server.Controllers
{
    [Route("api/recipes")]
    [ApiController]
    public class RecipeController : ControllerBase
    {
        static List<Recipe> testSet = new List<Recipe> {
            new Recipe{ id=0, name="Mashed Potato", description = "Creamy potato", tags=(new List<Tag>{ new Tag {description="Simple" }, new Tag {description="Side Dish" } }) },
            new Recipe{ id=1, name="Hashed Potato", description = "Crunchy potato"},
            new Recipe{ id=2, name="Baked Potato", description = "Soft potato"}
        };

        private readonly DataContext _context;

        //RecipeController(DataContext context) {
        //    _context = context;
        //}

        [HttpGet]
        public async Task<IActionResult> GetRecipes()
        {
            //TODO: Add database call for recipes
            //return Ok(await _context.recipes.ToListAsync());
            return Ok(testSet);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetRecipe(int id)
        {
            var recipe = testSet.FirstOrDefault(h => h.id == id);
            if (recipe == null)
            {
                return NotFound("Recipe was not found");
            }

            return Ok(recipe);
        }

        [HttpPost]
        public async Task<IActionResult> CreateRecipe(Recipe r)
        {
           //TODO: Change r.id to be from the database
           testSet.Add(r);
           r.id = testSet.FindIndex(recipe => recipe == r);
           return Ok(testSet);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateRecipe(Recipe r)
        {
            //TODO: Change how to update recipe
            testSet[r.id] = r;
            return Ok(testSet);
        }
    }
}
