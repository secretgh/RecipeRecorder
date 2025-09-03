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
            Ingredient? ing = null;
            if (await ConnectionChecker.IsDBUp(_context))
                ing = await _context.Ingredients.FindAsync(new object?[] { id }, cancellationToken);
            return ing;
        }

        public async Task<Ingredient?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
        {
            Ingredient? ing = null;
            var normalized = NormalizeName(name);
            if (await ConnectionChecker.IsDBUp(_context))
                ing = await _context.Ingredients.FirstOrDefaultAsync(i => i.NormalizedName == normalized, cancellationToken);
            return ing;
        }

        public async Task<List<Ingredient>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            List<Ingredient> ing = new List<Ingredient>();
            if (await ConnectionChecker.IsDBUp(_context)) { 
                ing = await _context.Ingredients
                .OrderBy(i => i.IngredientName)
                .ToListAsync(cancellationToken);            
            }
            return ing;
        }

        public async Task<Ingredient> AddAsync(Ingredient ingredient, CancellationToken cancellationToken = default)
        {
            ingredient.NormalizedName = NormalizeName(ingredient.IngredientName);
            if (await ConnectionChecker.IsDBUp(_context)) { 
                _context.Ingredients.Add(ingredient);
                await _context.SaveChangesAsync(cancellationToken);
            }
            return ingredient;
        }

        public async Task<Ingredient> UpdateAsync(Ingredient ingredient, CancellationToken cancellationToken = default)
        {
            ingredient.NormalizedName = NormalizeName(ingredient.IngredientName);
            if (await ConnectionChecker.IsDBUp(_context)) { 
                _context.Ingredients.Update(ingredient);
                await _context.SaveChangesAsync(cancellationToken);
            }
            return ingredient;
        }

        public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            if (await ConnectionChecker.IsDBUp(_context)) { 
                var ingredient = await GetByIdAsync(id, cancellationToken);
                if (ingredient != null)
                {
                    _context.Ingredients.Remove(ingredient);
                    await _context.SaveChangesAsync(cancellationToken);
                }
            }
        }

        public async Task<Ingredient> GetOrCreateAsync(string name, CancellationToken cancellationToken = default)
        {
            var normalized = NormalizeName(name);
            var ingredient = new Ingredient();
            if (await ConnectionChecker.IsDBUp(_context)) { 
                var existing = await _context.Ingredients
                    .FirstOrDefaultAsync(i => i.NormalizedName == normalized, cancellationToken);

                if (existing != null)
                    return existing;

                ingredient = new Ingredient(name.Trim());

                _context.Ingredients.Add(ingredient);
                await _context.SaveChangesAsync(cancellationToken);
            
            }

            return ingredient;
        }

        public async Task<List<Ingredient>> SearchAsync(string searchTerm, int maxResults = 10, CancellationToken cancellationToken = default)
        {
            List<Ingredient> ing = new List<Ingredient>();
            var normalized = NormalizeName(searchTerm);
            if (await ConnectionChecker.IsDBUp(_context)) {             
                ing = await _context.Ingredients
                .Where(i => i.NormalizedName.Contains(normalized))
                .OrderBy(i => i.IngredientName)
                .Take(maxResults)
                .ToListAsync(cancellationToken);
            }
            return ing;
        }

        private static string NormalizeName(string name)
        {
            return name.Trim().ToLowerInvariant();
        }
    }
}
