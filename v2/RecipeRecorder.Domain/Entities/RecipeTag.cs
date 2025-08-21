namespace RecipeRecorder.Domain;

public class RecipeTag
{
    public int Id { get; private set; }         // DB Id
    public int RecipeId { get; private set; }   // Reference to parent Recipe

    private string _tag = null!;
    public string Tag => _tag;

    // EF constructor
    private RecipeTag() { }

    // Constructor enforces required fields
    public RecipeTag(string tag)
    {
        if (string.IsNullOrWhiteSpace(tag))
            throw new ArgumentException("Tag cannot be empty.", nameof(tag));

        _tag = tag;
    }

    // Method to update tag value while enforcing rules
    public void UpdateTag(string newTag)
    {
        if (string.IsNullOrWhiteSpace(newTag))
            throw new ArgumentException("Tag cannot be empty.", nameof(newTag));
        _tag = newTag;
    }
}
