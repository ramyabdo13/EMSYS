using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EMSYS.Migrations
{
    /// <inheritdoc />
    public partial class RenameCountryToGovernate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Country");

            migrationBuilder.RenameColumn(
                name: "CountryName",
                table: "UserProfile",
                newName: "GovernateName");

            migrationBuilder.CreateTable(
                name: "Governate",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Governate", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Governate");

            migrationBuilder.RenameColumn(
                name: "GovernateName",
                table: "UserProfile",
                newName: "CountryName");

            migrationBuilder.CreateTable(
                name: "Country",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Country", x => x.Id);
                });
        }
    }
}
