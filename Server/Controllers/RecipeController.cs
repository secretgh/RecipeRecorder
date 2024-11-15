using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using RecipeRecorder.Shared;
using Microsoft.EntityFrameworkCore;
using Azure;

namespace RecipeRecorder.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RecipeController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly DevContext _context;
        private readonly string databaseString = "local";

        public RecipeController(IConfiguration config, DevContext context)
        {
            this._config = config;
            this._context = context;
        }

        [HttpGet("/TestConnection")]
        public bool IsDatabaseAvaiable()
        {
            SqlConnection con = new SqlConnection(_config.GetConnectionString(databaseString));
            try{
                con.Open();
                con.Close();
                return true;
            }
            catch(Exception ex) {
                return false;
            }
        }

        [HttpPost("Test")]
        public async Task<IActionResult> TestCreate()
        {
            /*            RecipeIngredient i = new RecipeIngredient();
                        i.RecipeId = 3;
                        i.IngId = 1;
                        _context.RecipeIngredients.Add(i);
                        _context.SaveChanges();*/
            return Ok();
        }

        [HttpGet("ingredients/{search}")]
        public async Task<IActionResult> GetFilteredIngredients(string search)
        {
            List<Ingredient> searchedIngredients = await _context.Ingredients.Where(i => i.IngredientName.Contains(search)).ToListAsync();
            return Ok(searchedIngredients);
        }

        #region "Object CRUD"

        [HttpGet]
        public async Task<IActionResult> GetRecipes()
        {
            List<Recipe> recipes = await _context.Recipes
                .Include(recipe => recipe.RecipeIngredients)
                .Include(recipe => recipe.RecipeSteps)
                .Include(recipe => recipe.RecipeTags)
                .ToListAsync();

           return Ok(recipes);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetRecipe(int id)
        {
            Recipe? recipe = await _context.Recipes
                .Include(recipe => recipe.RecipeIngredients)
                .Include(recipe => recipe.RecipeSteps)
                .Include(recipe => recipe.RecipeTags)
                .FirstOrDefaultAsync(r => r.Id == id);
            if (recipe == null)
            {
                return NotFound("Recipe was not found");
            }

            foreach(RecipeIngredient ir in recipe.RecipeIngredients)
            {
                ir.Ing = await _context.Ingredients.FirstAsync(i => i.Id == ir.IngId);
            }

            return Ok((Recipe)recipe);
        }

        [HttpGet("ingredient/{id}")]
        public async Task<IActionResult> GetIngredient(int id)
        {
            return Ok(await _context.Ingredients.FirstAsync(r => r.Id == id));
        }

        [HttpPost]
        public async Task<IActionResult> CreateRecipe([FromBody] Recipe r)
        {
            Console.WriteLine("Create Recipe End Point Hit!");

            using (SqlConnection con = new SqlConnection(_config.GetConnectionString("local")))
            {
                con.Open();
                SqlCommand createRecipe = new SqlCommand("CreateRecipe", con);
                SqlCommand createStep = new SqlCommand("CreateStep", con);
                SqlCommand createIngredient = new SqlCommand("CreateIngredient", con);
                SqlCommand createTag = new SqlCommand("CreateTag", con);
                createRecipe.CommandType = System.Data.CommandType.StoredProcedure;
                createIngredient.CommandType = System.Data.CommandType.StoredProcedure;
                createStep.CommandType = System.Data.CommandType.StoredProcedure;
                createTag.CommandType = System.Data.CommandType.StoredProcedure;
                try
                {
                    //recipe table
                    createRecipe.Parameters.AddRange(new SqlParameter[]{
                        new SqlParameter("@name", r.RecipeName),
                        new SqlParameter("@desc", r.RecipeDesc)
                    });

                    r.Id = int.Parse(createRecipe.ExecuteScalar().ToString());
                    con.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return BadRequest();
                }
            }

            foreach (RecipeTag t in r.RecipeTags)
            {
                t.RecipeId = r.Id;
                await _context.RecipeTags.AddAsync(t);
            }

            //step table
            foreach (RecipeStep s in r.RecipeSteps)
            {
                s.RecipeId = r.Id;
                await _context.RecipeSteps.AddAsync(s);
            }

            //RecipeIngredient table
            foreach (RecipeIngredient i in r.RecipeIngredients)
            {

                i.RecipeId = r.Id;
                await _context.RecipeIngredients.AddAsync(i);
            }

            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPut]
        public async Task<IActionResult> UpdateRecipe(Recipe r)
        {

            if(r == null)
            {
                return BadRequest(r);
            }
            else
            {
                _context.Recipes.Entry(r).State = EntityState.Modified;
            }
            foreach (RecipeTag t in r.RecipeTags)
            {
                if (!_context.RecipeTags.Any(tag => tag.Id == t.Id))
                { 
                    await _context.RecipeTags.AddAsync(t);            
                }
                else
                {
                    _context.RecipeTags.Entry(t).State = EntityState.Modified;
                }
            }

            //step table
            foreach (RecipeStep s in r.RecipeSteps)
            {
                if (!_context.RecipeSteps.Any(step => step.Id == s.Id))
                { 
                    await _context.RecipeSteps.AddAsync(s);
                }   
                else
                {
                    _context.RecipeSteps.Entry(s).State = EntityState.Modified;
                }
            }

            //RecipeIngredient table
            foreach (RecipeIngredient i in r.RecipeIngredients)
            {
                if (!_context.RecipeIngredients.Any(ri => ri.IngId == i.IngId && ri.RecipeId == r.Id)) { 
                    await _context.RecipeIngredients.AddAsync(i);                
                }
                else
                {
                    _context.RecipeIngredients.Entry(i).State = EntityState.Modified;
                }
            }

            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpGet("{rid}/tags")]
        public async Task<IActionResult> GetRecipeTags(int rid) {
            var recipe = await _context.Recipes.FirstOrDefaultAsync(h => h.Id == rid);

            if (recipe == null)
            {
                return NotFound("Recipe was not found");
            }

            return Ok(recipe.RecipeTags);
        }

        [HttpGet("{rid}/tags/{id}")]
        public async Task<IActionResult> GetRecipeTag(int rid, int id)
        {
            var recipe = await _context.Recipes.FirstOrDefaultAsync(r => r.Id == rid);


            if (recipe == null)
            {
                return NotFound("Recipe was not found");
            }

            var tag = recipe.RecipeTags.FirstOrDefault(t => t.Id == id);

            if (tag == null)
            {
                return NotFound("tag was not found");
            }

            return Ok(tag);
        }

        [HttpGet("ingredients")]
        public async Task<IActionResult> GetAllIngredients()
        {
            List<Ingredient> ingredients = await _context.Ingredients.ToListAsync();
            return Ok(ingredients);
        }

        [HttpPost("ingredients")]
        public async Task<Ingredient> CreateIngredient([FromBody] Ingredient i)
        {
            _context.Ingredients.Add(i);
            await _context.SaveChangesAsync();
            return i;
        }

    }
    #endregion
}
