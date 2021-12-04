using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Vetrina.Server.Migrations
{
    public partial class Initial_Create_Vetrina_Database : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Promotions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PromotionStartingFrom = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PromotionEndingAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Price = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OfficialPrice = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DiscountPercentage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DescriptionRaw = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DescriptionSearch = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Store = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Promotions", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Promotions");
        }
    }
}
