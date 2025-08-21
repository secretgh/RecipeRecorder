namespace RecipeRecorder.Domain;

public class Ingredient
{
    public int Id { get; private set; }  // DB Id

    private string _ingredientName = null!;
    public string IngredientName => _ingredientName;

    // EF constructor
    private Ingredient() { }

    // Constructor enforces required fields
    public Ingredient(string ingredientName)
    {
        if (string.IsNullOrWhiteSpace(ingredientName))
            throw new ArgumentException("Ingredient name cannot be empty.", nameof(ingredientName));

        _ingredientName = ingredientName;
    }

    // Method to update ingredient name while enforcing rules
    public void UpdateName(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            throw new ArgumentException("Ingredient name cannot be empty.", nameof(newName));

        _ingredientName = newName;
    }
}