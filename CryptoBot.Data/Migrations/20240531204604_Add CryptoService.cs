using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CryptoBot.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCryptoService : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Key",
                table: "Accounts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Secret",
                table: "Accounts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Key",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "Secret",
                table: "Accounts");
        }
    }
}
