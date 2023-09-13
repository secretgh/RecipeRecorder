using Microsoft.AspNetCore.Mvc;
using RecipeRecorder.Shared;
using Microsoft.Data.SqlClient;

namespace RecipeRecorder.Server.Controllers
{
    [Route("api/recipes")]
    [ApiController]
    public class RecipeController : ControllerBase
    {
        static List<Recipe> recipes = new List<Recipe>();

        private readonly IConfiguration _config;

        public RecipeController(IConfiguration config)
        {
            _config = config;
        }

        [HttpGet]
        public async Task<IActionResult> GetRecipes()
        {
            using (SqlConnection con = new SqlConnection(_config.GetConnectionString("defaultConnection")))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand("GetRecipes", con);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                try { 
                    SqlDataReader reader = await cmd.ExecuteReaderAsync();
                    var temp = new List<Recipe>();
                    while (reader.Read())
                    {
                        Recipe r = new Recipe
                        {
                            id = int.Parse(reader["ID"].ToString()),
                            name = reader["RecipeName"].ToString(),
                            description = reader["RecipeDesc"].ToString(),
                        };
                        r.tags = GetTags(r.id).Result;
                        r.ingredients = GetIngredients(r.id).Result;
                        r.steps = GetSteps(r.id).Result;

                        temp.Add(r);
                    }
                    con.Close();

                    recipes = temp;
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            return Ok(recipes);
        }

        private async Task<List<Tag>> GetTags(int id)
        {
            using (SqlConnection con = new SqlConnection(_config.GetConnectionString("defaultConnection")))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand("GetTags", con);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("recipeID", id);
                SqlDataReader reader = await cmd.ExecuteReaderAsync();
                var tags = new List<Tag>();
                while (reader.Read())
                {
                    Tag t = new Tag
                    {
                        id = int.Parse(reader["ID"].ToString()),
                        description = reader["Tag"].ToString()
                    };
                    tags.Add(t);
                }

                con.Close();
                return tags;
            }
        }

        private async Task<List<Ingredient>> GetIngredients(int id)
        {
            using (SqlConnection con = new SqlConnection(_config.GetConnectionString("defaultConnection")))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand("GetIngredients", con);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("recipeID", id);
                SqlDataReader reader = await cmd.ExecuteReaderAsync();
                var ingredients = new List<Ingredient>();
                while (reader.Read())
                {
                    Ingredient i = new Ingredient
                    {
                        id = int.Parse(reader["ID"].ToString()),
                        name = reader["IngredientDesc"].ToString(),
                        amount = float.Parse(reader["Quantity"].ToString()),
                        amountDescription = reader["QuantityDesc"].ToString()
                    };
                    ingredients.Add(i);
                }

                con.Close();
                return ingredients;
            }
        }

        private async Task<List<Step>> GetSteps(int id)
        {
            using (SqlConnection con = new SqlConnection(_config.GetConnectionString("defaultConnection")))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand("GetSteps", con);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("recipeID", id);
                SqlDataReader reader = await cmd.ExecuteReaderAsync();
                var steps = new List<Step>();
                while (reader.Read())
                {
                    Step s = new Step
                    {
                        id = int.Parse(reader["ID"].ToString()),
                        description = reader["Step"].ToString()
                    };
                    steps.Add(s);
                }

                con.Close();
                return steps;
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetRecipe(int id)
        {
            var recipe = recipes.FirstOrDefault(h => h.id == id);
            if (recipe == null)
            {
                return NotFound("Recipe was not found");
            }

            return Ok(recipe);
        }

        [HttpPost]
        public async Task<IActionResult> CreateRecipe(Recipe r)
        {
            using (SqlConnection con = new SqlConnection(_config.GetConnectionString("defaultConnection")))
            {
                con.Open();
                SqlCommand createRecipe = new SqlCommand("CreateRecipe", con);
                SqlCommand createStep = new SqlCommand("CreateStep", con);
                SqlCommand createIngredient = new SqlCommand("CreateIngredient", con);
                SqlCommand createTag= new SqlCommand("CreateTag", con);
                createRecipe.CommandType = System.Data.CommandType.StoredProcedure;
                createIngredient.CommandType = System.Data.CommandType.StoredProcedure;
                createStep.CommandType = System.Data.CommandType.StoredProcedure;
                createTag.CommandType = System.Data.CommandType.StoredProcedure;
                try
                {
                    //recipe table
                    createRecipe.Parameters.AddRange(new SqlParameter[]{ 
                        new SqlParameter("@name", r.name),
                        new SqlParameter("@desc", r.description)
                    });
                    
                    r.id = int.Parse(createRecipe.ExecuteScalar().ToString());

                    //tag table
                    foreach (Tag t in r.tags)
                    {
                        createTag.Parameters.Clear();
                        createTag.Parameters.AddRange(new SqlParameter[]
                        {
                            new SqlParameter("@recipeID", r.id),
                            new SqlParameter("@tag", t.description)
                        });
                        t.id = int.Parse(createTag.ExecuteScalar().ToString());
                    }

                    //step table
                    foreach (Step s in r.steps)
                    {
                        createStep.Parameters.Clear();
                        createStep.Parameters.AddRange(new SqlParameter[]
                        {
                            new SqlParameter("@recipeID", r.id),
                            new SqlParameter("@Step", s.description)
                        });
                        s.id = int.Parse(createStep.ExecuteScalar().ToString());
                    }

                    //ingredient table
                    foreach (Ingredient i in r.ingredients)
                    {
                        createIngredient.Parameters.Clear();
                        createIngredient.Parameters.AddRange(new SqlParameter[]
                        {
                            new SqlParameter("@recipeID", r.id),
                            new SqlParameter("@name", i.name),
                            new SqlParameter("@amount", i.amount),
                            new SqlParameter("@amountDesc", i.amountDescription)
                        });
                        i.id = int.Parse(createIngredient.ExecuteScalar().ToString());
                    }
                    con.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return BadRequest(recipes);
                }
            }

            recipes.Add(r);
            return Ok(recipes);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateRecipe(Recipe r)
        {
            //TODO: Change how to update recipe
            recipes[r.id] = r;
            return Ok(recipes);
        }
    }
}
