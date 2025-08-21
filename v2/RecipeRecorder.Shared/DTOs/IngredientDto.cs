namespace RecipeRecorder.Shared.DTOs
{
    public class IngredientDto
    {
        public int Id { get; set; }
        public string IngredientName { get; set; } = null!; // required
    }
}