using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipeRecorder.Shared.DTOs
{
    public class RecipeDto
    {
        public int Id { get; set; }
        public string RecipeName { get; set; } = null!; // required
        public string? RecipeDesc { get; set; }        // optional

        public List<RecipeIngredientDto> Ingredients { get; set; } = new();
        public List<RecipeStepDto> Steps { get; set; } = new();
        public List<RecipeTagDto> Tags { get; set; } = new();

        public override string ToString()
        {
            string result = "";
            result = $"{RecipeName} | ";
            foreach(RecipeTagDto tag in Tags)
            {
                result += $"{tag.Tag} ";
            }

            return result;
        }
    }
}
