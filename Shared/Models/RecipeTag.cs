using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace RecipeRecorder.Shared;

public partial class RecipeTag
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column("RecipeID")]
    public int RecipeId { get; set; }

    [StringLength(40)]
    [Unicode(false)]
    public string Tag { get; set; } = null!;
}
