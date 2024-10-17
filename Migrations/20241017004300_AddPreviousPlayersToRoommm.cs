using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bingoo.Migrations
{
    /// <inheritdoc />
    public partial class AddPreviousPlayersToRoommm : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PreviousPlayersJson",
                table: "Rooms",
                newName: "PreviousPlayers");

            migrationBuilder.AddColumn<string>(
                name: "Players",
                table: "Rooms",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Players",
                table: "Rooms");

            migrationBuilder.RenameColumn(
                name: "PreviousPlayers",
                table: "Rooms",
                newName: "PreviousPlayersJson");
        }
    }
}
