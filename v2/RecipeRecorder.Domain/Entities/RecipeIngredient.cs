namespace RecipeRecorder.Domain;

public class RecipeIngredient
{
    public int IngId { get; private set; }        // DB Id
    public int RecipeId { get; private set; }     // Reference to parent Recipe

    public string? IngredientNameModifier { get; private set; }

    private double _quantity;
    public double Quantity => _quantity;

    public string? QuantityDesc { get; private set; }
    public Ingredient Ingredient { get; private set; } = null!; // Reference to Ingredient entity

    // EF Core requires a parameterless constructor
    public RecipeIngredient() { }

    // Domain constructor for creating new entities
    public RecipeIngredient(Ingredient ingredient, double quantity, string? quantityDesc = null, string? nameModifier = null)
    {
        Ingredient = ingredient ?? throw new ArgumentNullException(nameof(ingredient));
        _quantity = quantity;
        QuantityDesc = quantityDesc;
        IngredientNameModifier = nameModifier;
    }
    // Methods to update properties while enforcing rules
    public void UpdateQuantity(double newQuantity)
    {
        if (newQuantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero.", nameof(newQuantity));
        _quantity = newQuantity;
    }

    public void UpdateQuantityDesc(string? newDesc)
    {
        QuantityDesc = newDesc;
    }

    public void UpdateNameModifier(string? newModifier)
    {
        IngredientNameModifier = newModifier;
    }
}
