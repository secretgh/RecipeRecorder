using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace RecipeRecorder.Shared;

public partial class Recipe
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string RecipeName { get; set; } = null!;

    [StringLength(255)]
    [Unicode(false)]
    public string? RecipeDesc { get; set; }

    public virtual List<RecipeIngredient> RecipeIngredients { get; set; } = new List<RecipeIngredient>();

    public virtual List<RecipeStep> RecipeSteps { get; set; } = new List<RecipeStep>();

    public virtual List<RecipeTag> RecipeTags { get; set; } = new List<RecipeTag>();
}
