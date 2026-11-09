using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PriceBridge.Migrations
{
    /// <inheritdoc />
    public partial class AddOzonSearchDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MatchSource",
                table: "TaobaoProducts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Brand",
                table: "OzonProducts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<List<string>>(
                name: "Category",
                table: "OzonProducts",
                type: "text[]",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Characteristics",
                table: "OzonProducts",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SearchQuery",
                table: "OzonProducts",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MatchSource",
                table: "TaobaoProducts");

            migrationBuilder.DropColumn(
                name: "Brand",
                table: "OzonProducts");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "OzonProducts");

            migrationBuilder.DropColumn(
                name: "Characteristics",
                table: "OzonProducts");

            migrationBuilder.DropColumn(
                name: "SearchQuery",
                table: "OzonProducts");
        }
    }
}
