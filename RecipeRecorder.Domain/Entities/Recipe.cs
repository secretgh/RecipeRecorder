namespace RecipeRecorder.Domain;

public class Recipe
{
    public int Id { get; private set; } // Id setter private - Will be set by Infrastructure layer
    public string RecipeName { get; private set; }  // Required
    public string? RecipeDesc { get; private set; }
    public List<RecipeIngredient> RecipeIngredients { get; private set; } = new();
    public List<RecipeStep> RecipeSteps { get; private set; } = new();
    public List<RecipeTag> RecipeTags { get; private set; } = new();


    // Constructor enforces required fields
    public Recipe(string recipeName, string? recipeDesc = null)
    {
        if (string.IsNullOrWhiteSpace(recipeName))
            throw new ArgumentException("Recipe name cannot be empty", nameof(recipeName));

        RecipeName = recipeName;
        RecipeDesc = recipeDesc;
    }

    // Methods to modify the entity while enforcing business rules
    public void AddIngredient(RecipeIngredient ingredient)
    {
        if (ingredient == null) throw new ArgumentNullException(nameof(ingredient));
        RecipeIngredients.Add(ingredient);
    }

    public void RemoveIngredient(RecipeIngredient ingredient)
    {
        RecipeIngredients.Remove(ingredient);
    }

    public void AddStep(RecipeStep step)
    {
        if (step == null) throw new ArgumentNullException(nameof(step));
        RecipeSteps.Add(step);
    }

    public void RemoveStep(RecipeStep step)
    {
        RecipeSteps.Remove(step);
    }

    public void AddTag(RecipeTag tag)
    {
        if (tag == null) throw new ArgumentNullException(nameof(tag));
        if (!RecipeTags.Any(t => t.Tag.Equals(tag.Tag, StringComparison.OrdinalIgnoreCase)))
            RecipeTags.Add(tag);
    }
    public void RemoveTag(RecipeTag tag)
    {
        RecipeTags.Remove(tag);
    }

    public void UpdateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Recipe name is required", nameof(name));
        RecipeName = name;
    }

    public void UpdateDescription(string? desc)
    {
        RecipeDesc = desc;
    }

    public void ClearIngredients() => RecipeIngredients.Clear();
    public void ClearSteps() => RecipeSteps.Clear();
    public void ClearTags() => RecipeTags.Clear();
}