using Microsoft.EntityFrameworkCore;
using RecipeRecorder.Domain;
using RecipeRecorder.Domain.Interfaces;


namespace RecipeRecorder.Infrastructure.Repos
{
    public class RecipeRepo : IRecipeRepo
    {
        private readonly RecipeDbContext _context;

        public RecipeRepo(RecipeDbContext context)
        {
            _context = context;
        }

        public async Task<Recipe> AddAsync(Recipe recipe)
        {
            if (await ConnectionChecker.IsDBUp(_context))
            {
                _context.Recipes.Add(recipe);
                await _context.SaveChangesAsync(); // EF assigns Id here
            }
            return recipe;
        }

        public async Task<List<Recipe>> GetAllAsync()
        {
            List<Recipe> recipes = new List<Recipe>();
            if (await ConnectionChecker.IsDBUp(_context))
            {
                recipes = await _context.Recipes
                .Include(r => r.RecipeIngredients)
                    .ThenInclude(ri => ri.Ingredient)
                .Include(r => r.RecipeSteps)
                .Include(r => r.RecipeTags)
                .AsSplitQuery()  // <- Split queries for better performance
                .ToListAsync();
            }
            return recipes;
        }

        public async Task<Recipe?> GetByIdAsync(int id)
        {
            Recipe? recipe = null;
            if (await ConnectionChecker.IsDBUp(_context))
            {
                recipe = await _context.Recipes
                .Include(r => r.RecipeIngredients)
                    .ThenInclude(ri => ri.Ingredient)
                .Include(r => r.RecipeSteps)
                .Include(r => r.RecipeTags)
                .AsSplitQuery()  // <- Split queries for better performance
                .FirstOrDefaultAsync(r => r.Id == id);
            }
            return recipe;
        }

        public async Task UpdateAsync(Recipe recipe)
        {
            if (await ConnectionChecker.IsDBUp(_context)) { 
                _context.Recipes.Update(recipe);
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteAsync(int id)
        {
            if (await ConnectionChecker.IsDBUp(_context)) {             
                var recipe = await _context.Recipes.FindAsync(id);
                if (recipe != null)
                {
                    _context.Recipes.Remove(recipe);
                    await _context.SaveChangesAsync();
                }
            }
        }
    }
}
