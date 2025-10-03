using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using RecipeRecorder.Shared;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

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
            string json = (string)((OkObjectResult)await SaveToJson()).Value;


            Console.WriteLine(json);
            return Ok();
        }

        [HttpGet("SaveToJson")]
        public async Task<IActionResult> SaveToJson()
        {
            List<Recipe> recipes = new List<Recipe>();
            OkObjectResult task = (OkObjectResult)await GetRecipes();
            recipes = (List<Recipe>)task.Value;
            string json = JsonSerializer.Serialize(recipes);
            string path = "C:\\Users\\USER_NAME\\Downloads".Replace("USER_NAME", Environment.UserName);
            System.IO.File.WriteAllText($"{path}/ExportedRecipes.json", json);
            Console.WriteLine("Successfully wrote ExportedRecipes.json");
            return Ok(json);
        }

        [HttpGet("ImportFromJson")]
        public async Task<IActionResult> ImportFromJson([FromHeader] List<Recipe> recipes)
        {
            //Check if DB is up.
            if (IsDatabaseAvaiable()) return BadRequest();
            
            List<Recipe> CurrentRecipes = (List<Recipe>)await GetRecipes();
            int numberOfRecipesAdded = 0;
            //Loop through recipes.
            foreach (Recipe recipe in recipes)
            {
                //Check if recipe name matches a name from database. Skip if that is the case
                int index = CurrentRecipes.FindIndex(r => r.RecipeName.ToLower().Trim().Equals(recipe.RecipeName.ToLower().Trim()));
                if(index == -1) continue;

                //Loop through recipe ingredients, if ingredient does not exist, create it, and set ingredient id to recipe ingredient.
                foreach(RecipeIngredient RI in recipe.RecipeIngredients)
                {
                    string ingName = RI.Ing.IngredientName.ToLower();
                    List<Ingredient> closeIngs = await _context.Ingredients.Where(ing => ing.IngredientName.Equals(ingName)).ToListAsync();
                    if(closeIngs.Count == 0)
                    {
                        Ingredient i = await CreateIngredient(RI.Ing);
                        RI.Ing = i;
                        RI.IngId = i.Id;
                    }
                    else
                    {
                        Ingredient i = closeIngs[0];
                        RI.Ing = i;
                        RI.IngId = i.Id;
                    }
                }

                //Create Recipe.    
                await CreateRecipe(recipe);
                numberOfRecipesAdded++;
            }

            Console.WriteLine($"Added {numberOfRecipesAdded} recipes to DB");
            return Ok();
        }

        [HttpGet("ingredients/{search}")]
        public async Task<IActionResult> GetFilteredIngredients(string search)
        {
            List<Ingredient> searchedIngredients = await _context.Ingredients.Where(i => i.IngredientName.Contains(search)).ToListAsync();
            return Ok(searchedIngredients);
        }


        [HttpGet("ingredients/{id}/recipes")]
        public async Task<IActionResult> GetRecipesUsingIngredient(int id)
        {
            var recipes = await _context.Recipes
                .Include(r => r.RecipeIngredients)
                .Include(r => r.RecipeSteps)
                .Include(r => r.RecipeTags)
                .Where(r => r.RecipeIngredients.Any(ri => ri.IngId == id))
                .ToListAsync();

            return Ok(recipes);
        }

        [HttpPut("ingredients/{id}")]
        public async Task<IActionResult> UpdateIngredient(int id, [FromBody] Ingredient updated)
        {
            var ingredient = await _context.Ingredients.FindAsync(id);
            if (ingredient == null) return NotFound();

            ingredient.IngredientName = updated.IngredientName;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("ingredients/{id}")]
        public async Task<IActionResult> DeleteIngredient(int id)
        {
            var ingredient = await _context.Ingredients.FindAsync(id);
            if (ingredient == null) return NotFound();

            _context.Ingredients.Remove(ingredient);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        public class PagedResult<T>
        {
            public List<T> Items { get; set; } = [];
            public int TotalCount { get; set; }
        }

        [HttpGet("ingredients/page/{page}/{pageSize}")]
        public async Task<ActionResult<PagedResult<Ingredient>>> GetPagedIngredients(int page = 1, int pageSize = 50)
        {
            var query = _context.Ingredients.OrderBy(i => i.IngredientName);
            var total = await query.CountAsync();

            List<Ingredient> items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<Ingredient>
            {
                Items = items,
                TotalCount = total
            };
        }

        [HttpPost("ingredients/page")]
        public async Task<IActionResult> AddIngredient([FromBody] Ingredient ingredient)
        {
            _context.Ingredients.Add(ingredient);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetPagedIngredients), new { id = ingredient.Id }, ingredient);
        }

        public async Task<List<Recipe>> GetFilteredRecipesAsync(string search, List<int> tagIds)
        {
            var query = _context.Recipes
                .Include(r => r.RecipeTags)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(r => r.RecipeName.Contains(search));

            if (tagIds?.Any() == true)
                query = query.Where(r => r.RecipeTags.Any(t => tagIds.Contains(t.Id)));

            return await query.ToListAsync();
        }
        public async Task<List<RecipeTag>> GetAllTagsAsync()
        {
            return await _context.RecipeTags.ToListAsync();
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

            await _context.Recipes.AddAsync(r);

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
                _context.Entry(i.Ing).State = EntityState.Unchanged;
                await _context.RecipeIngredients.AddAsync(i);
            }

            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPut]
        public async Task<IActionResult> UpdateRecipe(Recipe r)
        {

            var existingRecipe = await _context.Recipes
                                        .Include(rec => rec.RecipeTags)
                                        .Include(rec => rec.RecipeIngredients)
                                        .ThenInclude(ri => ri.Ing)
                                        .Include(rec => rec.RecipeSteps)
                                        .FirstOrDefaultAsync(rec => rec.Id == r.Id);

            if (existingRecipe is null)
                return NotFound();

            // Update scalar properties
            _context.Entry(existingRecipe).CurrentValues.SetValues(r);

            // ---------------------------
            // Update Ingredients
            // ---------------------------
            existingRecipe.RecipeIngredients.Clear();

            foreach (var ri in r.RecipeIngredients)
            {
                var newRi = new RecipeIngredient
                {
                    RecipeId = existingRecipe.Id,
                    IngId = ri.IngId,  // <-- IMPORTANT: use FK, not the Ing object
                    IngredientNameModifier = ri.IngredientNameModifier,
                    Quantity = ri.Quantity,
                    QuantityDesc = ri.QuantityDesc
                };

                // Only attach Ingredient if needed
                if (ri.Ing != null && ri.Ing.Id != 0)
                {
                    // Tell EF it's an existing Ingredient
                    _context.Attach(ri.Ing);
                    newRi.Ing = ri.Ing;
                }

                existingRecipe.RecipeIngredients.Add(newRi);
            }

            // ---------------------------
            // Update Tags
            // ---------------------------
            existingRecipe.RecipeTags.Clear();
            foreach (var tag in r.RecipeTags)
            {
                existingRecipe.RecipeTags.Add(new RecipeTag
                {
                    Id = tag.Id,
                    RecipeId = existingRecipe.Id,
                    Tag = tag.Tag
                });
            }

            // ---------------------------
            // Update Steps
            // ---------------------------
            existingRecipe.RecipeSteps.Clear();
            foreach (var step in r.RecipeSteps)
            {
                existingRecipe.RecipeSteps.Add(new RecipeStep
                {
                    Id = step.Id,
                    RecipeId = existingRecipe.Id,
                    Step = step.Step,
                    SubText = step.SubText
                });
            }

            await _context.SaveChangesAsync();
            return Ok(existingRecipe);
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
