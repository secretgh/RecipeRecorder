using Moq;
using Xunit;
using RecipeRecorder.Application.Services;
using RecipeRecorder.Domain;
using RecipeRecorder.Domain.Interfaces;
using RecipeRecorder.Shared.DTOs;

namespace RecipeRecorder.Application.Tests
{
    public class RecipeServiceTests
    {
        private readonly Mock<IRecipeRepo> _mockRepo;
        private readonly RecipeAPIService _service;

        public RecipeServiceTests()
        {
            _mockRepo = new Mock<IRecipeRepo>();
            _service = new RecipeAPIService(_mockRepo.Object);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsMappedDtos()
        {
            // Arrange
            var recipes = new List<Recipe>
            {
                new Recipe("Pancakes", "Fluffy breakfast food"),
                new Recipe("Omelette", "Egg dish")
            };
            _mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(recipes);

            // Act
            var result = await _service.GetAllAsync();

            // Assert
            Assert.Equal(2, result.Count());
            Assert.Contains(result, r => r.RecipeName == "Pancakes");
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsMappedDto_WhenFound()
        {
            var recipe = new Recipe("Soup", "Hot dish");
            _mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(recipe);

            var result = await _service.GetByIdAsync(1);

            Assert.NotNull(result);
            Assert.Equal("Soup", result!.RecipeName);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsNull_WhenNotFound()
        {
            _mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Recipe?)null);

            var result = await _service.GetByIdAsync(1);

            Assert.Null(result);
        }

        [Fact]
        public async Task CreateAsync_BuildsDomainAndReturnsDto()
        {
            var dto = new RecipeDto
            {
                RecipeName = "Salad",
                RecipeDesc = "Fresh veggies",
                Ingredients = new List<RecipeIngredientDto>
                {
                    new RecipeIngredientDto
                    {
                        Ingredient = new IngredientDto { IngredientName = "Lettuce" },
                        Quantity = 1
                    }
                },
                Steps = new List<RecipeStepDto>
                {
                    new RecipeStepDto { Instruction = "Chop lettuce" }
                },
                Tags = new List<RecipeTagDto>
                {
                    new RecipeTagDto { Tag = "Healthy" }
                }
            };

            var createdEntity = new Recipe("Salad", "Fresh veggies");
            _mockRepo.Setup(r => r.AddAsync(It.IsAny<Recipe>())).ReturnsAsync(createdEntity);

            var result = await _service.CreateAsync(dto);

            Assert.NotNull(result);
            Assert.Equal(0, result.Id);
            Assert.Equal("Salad", result.RecipeName);
        }

        [Fact]
        public async Task UpdateAsync_ReturnsTrue_WhenRecipeExists()
        {
            var existing = new Recipe("Old", "Desc");
            _mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existing);

            var dto = new RecipeDto
            {
                RecipeName = "Updated",
                RecipeDesc = "New desc",
                Ingredients = new List<RecipeIngredientDto>(),
                Steps = new List<RecipeStepDto>(),
                Tags = new List<RecipeTagDto>()
            };

            var result = await _service.UpdateAsync(1, dto);

            Assert.True(result);
            _mockRepo.Verify(r => r.UpdateAsync(existing), Times.Once);
            Assert.Equal("Updated", existing.RecipeName);
            Assert.Equal("New desc", existing.RecipeDesc);
        }

        [Fact]
        public async Task UpdateAsync_ReturnsFalse_WhenRecipeNotFound()
        {
            _mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Recipe?)null);

            var dto = new RecipeDto { RecipeName = "DoesNotMatter" };

            var result = await _service.UpdateAsync(1, dto);

            Assert.False(result);
            _mockRepo.Verify(r => r.UpdateAsync(It.IsAny<Recipe>()), Times.Never);
        }

        [Fact]
        public async Task DeleteAsync_ReturnsTrue_WhenRecipeExists()
        {
            var recipe = new Recipe("ToDelete", "Desc");
            _mockRepo.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(recipe);

            var result = await _service.DeleteAsync(10);

            Assert.True(result);
            _mockRepo.Verify(r => r.DeleteAsync(10), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_ReturnsFalse_WhenRecipeNotFound()
        {
            _mockRepo.Setup(r => r.GetByIdAsync(10)).ReturnsAsync((Recipe?)null);

            var result = await _service.DeleteAsync(10);

            Assert.False(result);
            _mockRepo.Verify(r => r.DeleteAsync(It.IsAny<int>()), Times.Never);
        }
    }
}