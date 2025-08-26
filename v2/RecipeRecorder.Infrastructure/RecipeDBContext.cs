using Microsoft.EntityFrameworkCore;
using RecipeRecorder.Domain;

namespace RecipeRecorder.Infrastructure
{
    public class RecipeDbContext : DbContext
    {
        public RecipeDbContext(DbContextOptions<RecipeDbContext> options) : base(options) { }

        public DbSet<Recipe> Recipes => Set<Recipe>();
        public DbSet<Ingredient> Ingredients => Set<Ingredient>();
        public DbSet<RecipeIngredient> RecipeIngredients => Set<RecipeIngredient>();
        public DbSet<RecipeStep> RecipeSteps => Set<RecipeStep>();
        public DbSet<RecipeTag> RecipeTags => Set<RecipeTag>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Recipe
            modelBuilder.Entity<Recipe>(entity =>
            {
                entity.HasKey(r => r.Id);
                entity.Property(r => r.RecipeName).IsRequired();

                // Relationships
                entity.HasMany(r => r.RecipeIngredients)
                      .WithOne()
                      .HasForeignKey(ri => ri.RecipeId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(r => r.RecipeSteps)
                      .WithOne()
                      .HasForeignKey(rs => rs.RecipeId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(r => r.RecipeTags)
                      .WithOne()
                      .HasForeignKey(rt => rt.RecipeId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Ingredient
            modelBuilder.Entity<Ingredient>(entity =>
            {
                entity.HasKey(i => i.Id);
                entity.Property(i => i.NormalizedName).IsRequired();
                entity.Property(i => i.IngredientName).IsRequired();
                entity.HasIndex(i => i.NormalizedName).IsUnique();
            });

            // RecipeIngredient
            modelBuilder.Entity<RecipeIngredient>(entity =>
            {
                entity.HasKey(ri => ri.IngId);
                entity.Property(ri => ri.Quantity).IsRequired();
            });

            // RecipeStep
            modelBuilder.Entity<RecipeStep>(entity =>
            {
                entity.HasKey(rs => rs.Id);
                entity.Property(rs => rs.Instruction).IsRequired();
            });

            // RecipeTag
            modelBuilder.Entity<RecipeTag>(entity =>
            {
                entity.HasKey(rt => rt.Id);
                entity.Property(rt => rt.Tag).IsRequired();
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}
