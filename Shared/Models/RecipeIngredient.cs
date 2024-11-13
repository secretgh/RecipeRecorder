using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
namespace RecipeRecorder.Shared;

[PrimaryKey("IngId", "RecipeId")]
public partial class RecipeIngredient
{
    [Key]
    [Column("IngID")]
    public int IngId { get; set; }

    [Key]
    [Column("RecipeID")]
    public int RecipeId { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? IngredientNameModifier { get; set; }

    public double Quantity { get; set; }

    [StringLength(40)]
    [Unicode(false)]
    public string? QuantityDesc { get; set; }

    [ForeignKey("IngId")]
    [InverseProperty("RecipeIngredients")]
    public virtual Ingredient Ing { get; set; } = null!;

    [JsonIgnore]
    [ForeignKey("RecipeId")]
    [InverseProperty("RecipeIngredients")]
    public virtual Recipe? Recipe { get; set; } = null!;
}
