using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using RecipeRecorder.Shared;

namespace RecipeRecorder.Server;

public partial class DevContext : DbContext
{
    public DevContext()
    {
    }

    public DevContext(DbContextOptions<DevContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Ingredient> Ingredients { get; set; }

    public virtual DbSet<Recipe> Recipes { get; set; }

    public virtual DbSet<RecipeIngredient> RecipeIngredients { get; set; }

    public virtual DbSet<RecipeStep> RecipeSteps { get; set; }

    public virtual DbSet<RecipeTag> RecipeTags { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Recipe>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e=>e.Id).ValueGeneratedOnAdd();

            entity.HasMany(e => e.RecipeIngredients)
                  .WithOne(e => e.Recipe)
                  .HasForeignKey(e=>e.RecipeId)
                  .IsRequired();
            entity.HasMany(e => e.RecipeTags)
                  .WithOne(e => e.Recipe)
                  .HasForeignKey(e => e.RecipeId)
                  .IsRequired();
            entity.HasMany(e => e.RecipeSteps)
                  .WithOne(e => e.Recipe)
                  .HasForeignKey(e => e.RecipeId)
                  .IsRequired();
        });

        modelBuilder.Entity<Ingredient>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
        });

        modelBuilder.Entity<RecipeIngredient>(entity =>
        {
            entity.HasKey(e => new { e.IngId, e.RecipeId });

            entity.HasOne(e => e.Recipe)
                  .WithMany(e => e.RecipeIngredients)
                  .HasForeignKey(e=>e.RecipeId)
                  .IsRequired();

            entity.HasOne(e => e.Ing)
                 .WithMany(e => e.RecipeIngredients)
                 .HasForeignKey(e => e.IngId)
                 .IsRequired();
        });

        modelBuilder.Entity<RecipeStep>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();

            entity.HasOne(e => e.Recipe)
                 .WithMany(e => e.RecipeSteps)
                 .HasForeignKey(e => e.RecipeId)
                 .IsRequired();
        });

        modelBuilder.Entity<RecipeTag>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.HasOne(e => e.Recipe)
                 .WithMany(e => e.RecipeTags)
                 .HasForeignKey(e => e.RecipeId)
                 .IsRequired();
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
