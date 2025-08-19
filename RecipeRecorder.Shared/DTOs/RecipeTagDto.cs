namespace RecipeRecorder.Shared.DTOs
{
    public class RecipeTagDto
    {
        public int Id { get; set; }
        public int RecipeId { get; set; }
        public string Tag { get; set; } = null!; // required
    }
}