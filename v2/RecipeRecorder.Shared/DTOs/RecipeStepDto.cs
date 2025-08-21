namespace RecipeRecorder.Shared.DTOs
{
    public class RecipeStepDto
    {
        public int Id { get; set; }
        public int RecipeId { get; set; }
        public string Instruction { get; set; } = null!; // required
        public string? SubText { get; set; }             // optional
    }
}