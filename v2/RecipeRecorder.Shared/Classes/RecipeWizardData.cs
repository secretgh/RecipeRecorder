// Using your DTOs with additional properties for wizard functionality
using RecipeRecorder.Shared.DTOs;
namespace RecipeRecorder.Shared.Classes
{
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

        /// <summary>
        /// Flushes all wizard state into the child Recipe DTO so it is
        /// complete and ready to be sent to the API.
        /// Call this immediately before invoking OnRecipeSaved.
        /// </summary>
        public void SyncToRecipe()
        {
            // Name and Description are already live-synced via the proxy
            // properties, but we re-assign here to be explicit and safe.
            Recipe.RecipeName = Name;
            Recipe.RecipeDesc = string.IsNullOrWhiteSpace(Description) ? null : Description;

            // Convert wizard steps → DTOs and write them onto the Recipe.
            // This is the step that was missing: Recipe.Steps was never
            // populated from WizardSteps when the user went straight to save.
            Recipe.Steps = ConvertStepsToDto();

            // Ingredients and Tags are already written onto Recipe directly
            // by IngredientSectionEditor and DescriptionSectionEditor via
            // their respective callbacks, so no additional work is needed here.
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
}