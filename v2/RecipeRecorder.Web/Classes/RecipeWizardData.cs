// Using your DTOs with additional properties for wizard functionality
using RecipeRecorder.Shared.DTOs;

public class RecipeWizardData
{
    public RecipeDto Recipe { get; set; } = new();
    public List<StepWizardData> WizardSteps { get; set; } = new();
    
    public List<RecipeStepDto> ConvertStepsToDto()
    {
        List<RecipeStepDto> steps = new List<RecipeStepDto>();
        foreach(StepWizardData data in WizardSteps)
        {
            steps.Add(data.ToDto(Recipe.Id));
        }
        return steps;
    }

    // Helper properties for wizard
    public string Name 
    { 
        get => Recipe.RecipeName; 
        set => Recipe.RecipeName = value; 
    }
    
    public string Description 
    { 
        get => Recipe.RecipeDesc ?? string.Empty; 
        set => Recipe.RecipeDesc = value; 
    }
}

public class StepWizardData
{
    public int StepNumber { get; set; }
    public string Text { get; set; } = string.Empty;
    public ParsedStepData? ParsedData { get; set; }
    
    // Convert to your DTO
    public RecipeStepDto ToDto(int recipeId)
    {
        return new RecipeStepDto
        {
            RecipeId = recipeId,
            Instruction = Text,
            SubText = null // Can be used for parsed time info later
        };
    }
}

public class ParsedStepData
{
    public int StepNumber { get; set; }
    public string OriginalText { get; set; } = string.Empty;
    public List<string> Ingredients { get; set; } = new();
    public List<string> Times { get; set; } = new();
}

public class ParsedIngredientData
{
    public string Name { get; set; } = string.Empty;
    public double? Quantity { get; set; }
    public string? QuantityDesc { get; set; }
    public string? IngredientNameModifier { get; set; }
    
    // Convert to your DTOs
    public RecipeIngredientDto ToDto(int recipeId, IngredientDto ingredient)
    {
        return new RecipeIngredientDto
        {
            RecipeId = recipeId,
            IngId = ingredient.Id,
            Quantity = Quantity ?? 0,
            QuantityDesc = QuantityDesc,
            IngredientNameModifier = IngredientNameModifier,
            Ingredient = ingredient
        };
    }
}