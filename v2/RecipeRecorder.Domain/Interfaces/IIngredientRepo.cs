using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipeRecorder.Domain.Interfaces
{
    public interface IIngredientRepo
    {
        Task<Ingredient?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<Ingredient?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
        Task<List<Ingredient>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<Ingredient> AddAsync(Ingredient ingredient, CancellationToken cancellationToken = default);
        Task<Ingredient> UpdateAsync(Ingredient ingredient, CancellationToken cancellationToken = default);
        Task DeleteAsync(int id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Ensures an ingredient exists. If not found by normalized name, creates it.
        /// </summary>
        Task<Ingredient> GetOrCreateAsync(string name, CancellationToken cancellationToken = default);

        /// <summary>
        /// Autocomplete search for ingredients by partial name.
        /// </summary>
        Task<List<Ingredient>> SearchAsync(string searchTerm, int maxResults = 10, CancellationToken cancellationToken = default);
    }
}
