using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TMS201.Migrations
{
    /// <inheritdoc />
    public partial class d : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SRN",
                table: "Tickets");

            migrationBuilder.RenameColumn(
                name: "SRN",
                table: "TicketBackups",
                newName: "SerialNo");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SerialNo",
                table: "TicketBackups",
                newName: "SRN");

            migrationBuilder.AddColumn<string>(
                name: "SRN",
                table: "Tickets",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
