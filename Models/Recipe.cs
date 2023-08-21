namespace RecipeRecorder.Models
{
    public class Recipe
    {
        public int? id { get; set; }
        public string? name { get; set; }
        public string? description { get; set; }
        public List<Tag>? tags { get; set; }
        public List<Ingredient>? ingredients { get; set; }
        public List<Step>? steps { get; set; }

        public Recipe(int id = 0, string name = "", string description = "", 
            List<Tag> tags = null, List<Ingredient> ingredients = null, List<Step> steps = null) {
            this.id = id;
            this.name = name;
            this.description = description;
            this.tags = tags == null ? new List<Tag>() : tags;
            this.ingredients = ingredients == null ? new List<Ingredient>() : ingredients;
            this.steps = steps == null ? new List<Step>() : steps;
        }
    }

    public class Ingredient
    {
        //public int id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public float amount { get; set; }
        public string amountDescription { get; set; }

        public Ingredient(string name="", string description = "", float amount=0, string amountDesc = "")
        {
            this.name = name;
            this.description = description;
            this.amount = amount;
            this.amountDescription = amountDesc;
        }
    }

    public class Step
    {
        //public int id;
        public string description;
    }

    public class Tag
    {
        //public int id;
        public string description;

        public Tag(string tag)
        {
            description = tag;
        }
    }
}
