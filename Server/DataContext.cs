using Microsoft.EntityFrameworkCore;
using RecipeRecorder.Shared;

namespace RecipeRecorder.Server
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options) {
        
        }

        public DbSet<Recipe> recipes { get; set; }
    }
}
