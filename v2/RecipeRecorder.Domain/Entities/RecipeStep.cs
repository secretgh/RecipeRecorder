namespace RecipeRecorder.Domain;

public class RecipeStep
{
    public int Id { get; private set; }        // DB Id
    public int RecipeId { get; private set; }  // Reference to parent Recipe

    private string _instruction = null!;
    public string Instruction => _instruction;

    private string? _subText;
    public string? SubText => _subText;

    // EF Core parameterless constructor
    public RecipeStep() { }

    // Constructor enforces required fields
    public RecipeStep(string instruction, string? subText = null)
    {
        if (string.IsNullOrWhiteSpace(instruction))
            throw new ArgumentException("Instruction cannot be empty.", nameof(instruction));

        _instruction = instruction;
        _subText = subText;
    }

    // Methods to update properties while enforcing rules
    public void UpdateInstruction(string newInstruction)
    {
        if (string.IsNullOrWhiteSpace(newInstruction))
            throw new ArgumentException("Instruction cannot be empty.", nameof(newInstruction));
        _instruction = newInstruction;
    }

    public void UpdateSubText(string? newSubText)
    {
        _subText = newSubText;
    }
}