using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bingoo.Migrations
{
    /// <inheritdoc />
    public partial class AddPreviousPlayersToRoom : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Players",
                table: "Rooms",
                newName: "PreviousPlayers");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PreviousPlayers",
                table: "Rooms",
                newName: "Players");
        }
    }
}
