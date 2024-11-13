using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using RecipeRecorder.Shared;
using Microsoft.EntityFrameworkCore;

namespace RecipeRecorder.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RecipeController : ControllerBase
    {
        static List<Recipe> recipes = new List<Recipe>();

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

        #region "Recipe Object CRUD"

        [HttpGet]
        public async Task<IActionResult> GetRecipes()
        {
            recipes = await _context.Recipes
                .Include(recipe => recipe.RecipeIngredients).ThenInclude(i => i.Ing)
                .Include(recipe => recipe.RecipeSteps)
                .Include(recipe => recipe.RecipeTags)
                .ToListAsync();
                            
           return Ok(recipes);
        }

        [HttpGet("{id}")]
        public Task<IActionResult> GetRecipe(int id)
        {
            var recipe = recipes.FirstOrDefault(r => r.Id == id);
            if (recipe == null)
            {
                return Task.FromResult<IActionResult>(NotFound("Recipe was not found"));
            }

            return Task.FromResult<IActionResult>(Ok(recipe));
        }

        [HttpPost]
        public async Task<IActionResult> CreateRecipe([FromBody] Recipe r)
        {
            Console.WriteLine("Create Recipe End Point Hit!");
            await _context.Recipes.AddAsync(r);
/*            foreach (RecipeTag t in r.RecipeTags)
            {
                t.RecipeId = r.Id;
                _context.RecipeTags.Add(t);
            }

            //step table
            foreach (RecipeStep s in r.RecipeSteps)
            {
                s.RecipeId = r.Id;
                _context.RecipeSteps.Add(s);
            }

            //RecipeIngredient table
            foreach (RecipeIngredient i in r.RecipeIngredients)
            {

                i.RecipeId = r.Id;
                _context.RecipeIngredients.Add(i);
            }*/

            await _context.SaveChangesAsync();
            await GetRecipes();

            return Ok();
        }

        [HttpPut]
        public async Task<IActionResult> UpdateRecipe(Recipe r)
        {
            var recipe = await _context.Recipes.FindAsync(r.Id);

            if(recipe == null)
            {
                return NotFound(null);
            }

            foreach (RecipeTag t in r.RecipeTags)
            {
                if(!_context.RecipeTags.Contains(t))
                    _context.RecipeTags.Add(t);
            }

            //step table
            foreach (RecipeStep s in r.RecipeSteps)
            {
                if(!_context.RecipeSteps.Contains(s))
                    _context.RecipeSteps.Add(s);
            }

            //RecipeIngredient table
            foreach (RecipeIngredient i in r.RecipeIngredients)
            {
                if(!_context.RecipeIngredients.Contains(i))
                    _context.RecipeIngredients.Add(i);
            }

            _context.SaveChanges();
            recipes = await _context.Recipes.ToListAsync();
            return Ok(recipes);
        }
        #endregion

        #region "Tag Object CRUD"

        [HttpGet("{rid}/tags")]
        public async Task<IActionResult> GetRecipeTags(int rid) {
            var recipe = recipes.FirstOrDefault(h => h.Id == rid);

            if (recipe == null)
            {
                return NotFound("Recipe was not found");
            }

            return Ok(recipe.RecipeTags);
        }

        [HttpGet("{rid}/tags/{id}")]
        public async Task<IActionResult> GetRecipeTag(int rid, int id)
        {
            var recipe = recipes.FirstOrDefault(r => r.Id == rid);


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

        [HttpPut("{rid}/tags/{id}")]
        public async Task<IActionResult> UpdateTag(int rid, int id, RecipeTag tag)
        {
            //List<Tag> tags = recipes[recipes.FindIndex(r => r.id == rid)].tags;

            //using (SqlConnection con = new SqlConnection(_config.GetConnectionString(databaseString)))
            //{
            //    con.Open();
            //    SqlCommand cmd = new SqlCommand("UpdateTag", con);
            //    cmd.CommandType = CommandType.StoredProcedure;
            //    try
            //    {
            //        cmd.Parameters.AddRange(new SqlParameter[] {
            //            new SqlParameter("@ID", tag.id),
            //            new SqlParameter("@Tag", tag.description)
            //        });
            //        await cmd.ExecuteNonQueryAsync();
            //        con.Close();
            //    }
            //    catch (Exception ex)
            //    {
            //        Console.WriteLine(ex.Message);
            //    }
            //}

            //tags[tags.FindIndex(t => t.id == id)] = tag;
            //return Ok(tag);
            var recipe = await _context.Recipes.FirstOrDefaultAsync(r => r.Id == rid);
            if(recipe == null)
            {
                return NotFound(null);
            }

            if (!_context.RecipeTags.Contains(tag))
            {
                _context.RecipeTags.Add(tag);
            }

            _context.SaveChanges();

            return Ok(tag);
        }

        [HttpDelete("{rid}/tags/{id}")]
        public async Task<IActionResult> DeleteTag(int rid, int id)
        {
            //List<Tag> tags = recipes[recipes.FindIndex(r => r.id == rid)].tags;
            //using (SqlConnection con = new SqlConnection(_config.GetConnectionString(databaseString)))
            //{
            //    con.Open();
            //    SqlCommand cmd = new SqlCommand("DeleteTag", con);
            //    cmd.CommandType = CommandType.StoredProcedure;
            //    try
            //    {
            //        cmd.Parameters.AddRange(new SqlParameter[] {
            //            new SqlParameter("@id", id)
            //        });
            //        await cmd.ExecuteNonQueryAsync();
            //        con.Close();
            //    }
            //    catch (Exception ex)
            //    {
            //        Console.WriteLine(ex.Message);
            //    }
            //}
            //tags.Remove(tags[tags.FindIndex(t => t.id == id)]);
            //return Ok();
            return Ok(null);
        }

        private async Task<List<RecipeTag>> GetTags(int id)
        {
            //using (SqlConnection con = new SqlConnection(_config.GetConnectionString(databaseString)))
            //{
            //    con.Open();
            //    SqlCommand cmd = new SqlCommand("GetTags", con);
            //    cmd.CommandType = CommandType.StoredProcedure;
            //    cmd.Parameters.AddWithValue("recipeID", id);
            //    SqlDataReader reader = await cmd.ExecuteReaderAsync();
            //    var tags = new List<Tag>();
            //    while (reader.Read())
            //    {
            //        Tag t = new Tag
            //        {
            //            id = int.Parse(reader["ID"].ToString()),
            //            description = reader["Tag"].ToString()
            //        };
            //        tags.Add(t);
            //    }

            //    con.Close();
            //    return tags;
            //}
            
            return null;
        }




        #endregion


        [HttpGet("{rid}/recipeingredients")]
        public async Task<List<RecipeIngredient>> GetRecipeIngredients(int rid)
        {
            Recipe r = await _context.Recipes.Where(r => r.Id == rid).FirstAsync();

            if (r == null)
            {
                List<RecipeIngredient> recipeIngredients = new List<RecipeIngredient>();
                return recipeIngredients;
            }

            return r.RecipeIngredients;
        }

        [HttpPost("{rid}/recipeingredients")]
        public async Task<IActionResult> CreateRecipeIngredient(int rid, [FromBody] RecipeIngredient recipeIngredient)
        {
            Recipe r = await _context.Recipes.Where(r => r.Id == rid).FirstAsync();
            if (r == null)
            {
                return NotFound("Recipe does not exist.");
            }

            await _context.RecipeIngredients.AddRangeAsync(recipeIngredient);
            _context.SaveChanges();
            return Ok();
        }

        [HttpGet("ingredients")]
        public async Task<IActionResult> GetAllIngredients()
        {
            List<Ingredient> ingredients = await _context.Ingredients.ToListAsync();
            return Ok(ingredients);
        }
        [HttpGet("ingredients/{search}")]
        public async Task<IActionResult> GetFilteredIngredients(string search)
        {
            List<Ingredient> searchedIngredients = await _context.Ingredients.Where(i => i.IngredientName.Contains(search)).ToListAsync();
            return Ok(searchedIngredients);
        }
        [HttpPost("ingredients")]
        public async Task<Ingredient> CreateIngredient([FromBody] Ingredient i)
        {
            _context.Ingredients.Add(i);
            await _context.SaveChangesAsync();
            return i;
        }


        [HttpGet("{rid}/steps")]
        public async Task<List<RecipeStep>> GetSteps(int rid)
        {
           Recipe r = await _context.Recipes.Where(r => r.Id == rid).FirstAsync();
           if (r == null) {
            return new List<RecipeStep>();
           }

           return r.RecipeSteps;
        }

    }
}
