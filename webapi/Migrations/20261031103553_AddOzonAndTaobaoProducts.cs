using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PriceBridge.Migrations
{
    /// <inheritdoc />
    public partial class AddOzonAndTaobaoProducts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OzonProducts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Sku = table.Column<string>(type: "text", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: true),
                    Price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    OriginalPrice = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    ImageUrl = table.Column<string>(type: "text", nullable: true),
                    Url = table.Column<string>(type: "text", nullable: false),
                    TaobaoFetchedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OzonProducts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TaobaoProducts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ItemId = table.Column<string>(type: "text", nullable: true),
                    Title = table.Column<string>(type: "text", nullable: true),
                    Price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    ImageUrl = table.Column<string>(type: "text", nullable: true),
                    ItemUrl = table.Column<string>(type: "text", nullable: false),
                    OzonProductId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaobaoProducts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaobaoProducts_OzonProducts_OzonProductId",
                        column: x => x.OzonProductId,
                        principalTable: "OzonProducts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OzonProducts_Sku",
                table: "OzonProducts",
                column: "Sku",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TaobaoProducts_OzonProductId",
                table: "TaobaoProducts",
                column: "OzonProductId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TaobaoProducts");

            migrationBuilder.DropTable(
                name: "OzonProducts");
        }
    }
}
