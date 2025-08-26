namespace RecipeRecorder.Domain;

public class Ingredient
{
    public int Id { get; private set; }  // DB Id

    private string _ingredientName = null!;
    public string IngredientName => _ingredientName;

    // Used internally to enforce uniqueness
    public string NormalizedName { get; set; } = string.Empty;

    // EF constructor
    public Ingredient() { }

    // Constructor enforces required fields
    public Ingredient(string ingredientName)
    {
        if (string.IsNullOrWhiteSpace(ingredientName))
            throw new ArgumentException("Ingredient name cannot be empty.", nameof(ingredientName));

        _ingredientName = ingredientName;
        NormalizedName = NormalizeString(ingredientName);
    }

    // Method to update ingredient name while enforcing rules
    public void UpdateName(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            throw new ArgumentException("Ingredient name cannot be empty.", nameof(newName));

        _ingredientName = newName;
    }

    private static string NormalizeString(string name)
    {
        return name.Trim().ToLowerInvariant();
    }
}