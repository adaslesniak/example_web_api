using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApiPlayground.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    WeightInKg = table.Column<float>(type: "real", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WidthInCm = table.Column<int>(type: "int", nullable: true),
                    HeightInCm = table.Column<int>(type: "int", nullable: true),
                    DepthInCm = table.Column<int>(type: "int", nullable: true),
                    PricePer100 = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => new { x.Name, x.WeightInKg });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Products");
        }
    }
}
