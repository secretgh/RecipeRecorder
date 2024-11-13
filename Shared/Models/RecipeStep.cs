using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

namespace RecipeRecorder.Shared;

public partial class RecipeStep
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column("RecipeID")]
    public int RecipeId { get; set; }

    [Unicode(false)]
    public string Step { get; set; } = null!;

    [Unicode(false)]
    public string? SubText { get; set; }

    [JsonIgnore]
    [ForeignKey("RecipeId")]
    [InverseProperty("RecipeSteps")]
    public virtual Recipe? Recipe { get; set; } = null!;
}
