using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RecipeRecorder.Shared;

namespace RecipeRecorder.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RecipeController : ControllerBase
    {
        List<Recipe> testSet = new List<Recipe> {
            new Recipe{ id=0, name="Mashed Potato", description = "Creamy potato", tags=(new List<Tag>{ new Tag {description="Simple" }, new Tag {description="Side Dish" } }) },
            new Recipe{ id=1, name="Hashed Potato", description = "Crunchy potato"},
            new Recipe{ id=2, name="Baked Potato", description = "Soft potato"}
        };
        
        public async Task<IActionResult> GetRecipes()
        {
            //TODO: Add database call for recipes
            return Ok(testSet);
        }
    }
}
