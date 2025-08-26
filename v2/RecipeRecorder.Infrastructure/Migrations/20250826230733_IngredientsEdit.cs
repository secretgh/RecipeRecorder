using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RecipeRecorder.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class IngredientsEdit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NormalizedName",
                table: "Ingredients",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Ingredients_NormalizedName",
                table: "Ingredients",
                column: "NormalizedName",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Ingredients_NormalizedName",
                table: "Ingredients");

            migrationBuilder.DropColumn(
                name: "NormalizedName",
                table: "Ingredients");
        }
    }
}
