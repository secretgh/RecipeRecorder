using Microsoft.EntityFrameworkCore;
using RecipeRecorder.Application.Interfaces;
using RecipeRecorder.Application.Services;
using RecipeRecorder.Domain.Interfaces;
using RecipeRecorder.Infrastructure;
using RecipeRecorder.Infrastructure.Repos;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddDbContext<RecipeDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("local")));

// Register custom interfaces
builder.Services.AddScoped<IRecipeRepo, RecipeRepo>();
builder.Services.AddScoped<IIngredientRepo, IngredientRepo>();
builder.Services.AddScoped<IRecipeService, RecipeAPIService>();

builder.Services.AddControllers();

builder.Services.AddCors(
    options =>
    {
        options.AddPolicy(
            name: "AllowedOrigins",
            policy =>
            {
                policy.WithOrigins(
                    "https://localhost:7187",
                    "https://localhost:7063"
                ).AllowAnyHeader().AllowAnyMethod();
            }
        );
    }    
);

var app = builder.Build();

app.UseCors("AllowedOrigins");

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();