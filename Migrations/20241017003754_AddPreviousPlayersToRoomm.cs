using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bingoo.Migrations
{
    /// <inheritdoc />
    public partial class AddPreviousPlayersToRoomm : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PreviousPlayers",
                table: "Rooms",
                newName: "PreviousPlayersJson");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PreviousPlayersJson",
                table: "Rooms",
                newName: "PreviousPlayers");
        }
    }
}
