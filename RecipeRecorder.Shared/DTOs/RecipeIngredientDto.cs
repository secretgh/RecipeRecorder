namespace RecipeRecorder.Shared.DTOs
{
    public class RecipeIngredientDto
    {
        public int IngId { get; set; }
        public int RecipeId { get; set; }

        public string? IngredientNameModifier { get; set; } // optional
        public double Quantity { get; set; }               // required
        public string? QuantityDesc { get; set; }          // optional
        public IngredientDto Ingredient { get; set; } = null!; // required reference
    }
}