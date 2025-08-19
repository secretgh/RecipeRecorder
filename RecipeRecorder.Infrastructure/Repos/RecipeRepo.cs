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
            _context.Recipes.Add(recipe);
            await _context.SaveChangesAsync(); // EF assigns Id here
            return recipe;
        }

        public async Task<IEnumerable<Recipe>> GetAllAsync()
        {
            return await _context.Recipes
                .Include(r => r.RecipeIngredients)
                    .ThenInclude(ri => ri.Ingredient)
                .Include(r => r.RecipeSteps)
                .Include(r => r.RecipeTags)
                .AsSplitQuery()  // <- Split queries for better performance
                .ToListAsync();
        }

        public async Task<Recipe?> GetByIdAsync(int id)
        {
            return await _context.Recipes
                .Include(r => r.RecipeIngredients)
                    .ThenInclude(ri => ri.Ingredient)
                .Include(r => r.RecipeSteps)
                .Include(r => r.RecipeTags)
                .AsSplitQuery()  // <- Split queries for better performance
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task UpdateAsync(Recipe recipe)
        {
            _context.Recipes.Update(recipe);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var recipe = await _context.Recipes.FindAsync(id);
            if (recipe != null)
            {
                _context.Recipes.Remove(recipe);
                await _context.SaveChangesAsync();
            }
        }
    }
}
