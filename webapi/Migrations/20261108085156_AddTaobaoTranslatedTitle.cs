using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PriceBridge.Migrations
{
    /// <inheritdoc />
    public partial class AddTaobaoTranslatedTitle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TranslatedTitle",
                table: "TaobaoProducts",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TranslatedTitle",
                table: "TaobaoProducts");
        }
    }
}
