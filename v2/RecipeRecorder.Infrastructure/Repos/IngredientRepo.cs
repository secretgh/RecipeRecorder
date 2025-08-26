using Microsoft.EntityFrameworkCore;
using RecipeRecorder.Domain;
using RecipeRecorder.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipeRecorder.Infrastructure.Repos
{
    public class IngredientRepo : IIngredientRepo
    {
        private readonly RecipeDbContext _context;

        public IngredientRepo(RecipeDbContext context)
        {
            _context = context;
        }

        public async Task<Ingredient?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.Ingredients.FindAsync(new object?[] { id }, cancellationToken);
        }

        public async Task<Ingredient?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
        {
            var normalized = NormalizeName(name);
            return await _context.Ingredients
                .FirstOrDefaultAsync(i => i.NormalizedName == normalized, cancellationToken);
        }

        public async Task<List<Ingredient>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Ingredients
                .OrderBy(i => i.IngredientName)
                .ToListAsync(cancellationToken);
        }

        public async Task<Ingredient> AddAsync(Ingredient ingredient, CancellationToken cancellationToken = default)
        {
            ingredient.NormalizedName = NormalizeName(ingredient.IngredientName);
            _context.Ingredients.Add(ingredient);
            await _context.SaveChangesAsync(cancellationToken);
            return ingredient;
        }

        public async Task<Ingredient> UpdateAsync(Ingredient ingredient, CancellationToken cancellationToken = default)
        {
            ingredient.NormalizedName = NormalizeName(ingredient.IngredientName);
            _context.Ingredients.Update(ingredient);
            await _context.SaveChangesAsync(cancellationToken);
            return ingredient;
        }

        public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var ingredient = await GetByIdAsync(id, cancellationToken);
            if (ingredient != null)
            {
                _context.Ingredients.Remove(ingredient);
                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task<Ingredient> GetOrCreateAsync(string name, CancellationToken cancellationToken = default)
        {
            var normalized = NormalizeName(name);

            var existing = await _context.Ingredients
                .FirstOrDefaultAsync(i => i.NormalizedName == normalized, cancellationToken);

            if (existing != null)
                return existing;

            var ingredient = new Ingredient(name.Trim());

            _context.Ingredients.Add(ingredient);
            await _context.SaveChangesAsync(cancellationToken);

            return ingredient;
        }

        public async Task<List<Ingredient>> SearchAsync(string searchTerm, int maxResults = 10, CancellationToken cancellationToken = default)
        {
            var normalized = NormalizeName(searchTerm);
            return await _context.Ingredients
                .Where(i => i.NormalizedName.Contains(normalized))
                .OrderBy(i => i.IngredientName)
                .Take(maxResults)
                .ToListAsync(cancellationToken);
        }

        private static string NormalizeName(string name)
        {
            return name.Trim().ToLowerInvariant();
        }
    }
}
