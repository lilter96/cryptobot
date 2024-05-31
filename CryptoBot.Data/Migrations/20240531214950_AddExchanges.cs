using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CryptoBot.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddExchanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Key",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "Secret",
                table: "Accounts");

            migrationBuilder.AddColumn<Guid>(
                name: "SelectedAccountId",
                table: "Chats",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "ExchangeId",
                table: "Accounts",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "Exchanges",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModificationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Exchange = table.Column<int>(type: "int", nullable: false),
                    EncryptedKey = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EncryptedSecret = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Exchanges", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Chats_SelectedAccountId",
                table: "Chats",
                column: "SelectedAccountId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_ExchangeId",
                table: "Accounts",
                column: "ExchangeId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Accounts_Exchanges_ExchangeId",
                table: "Accounts",
                column: "ExchangeId",
                principalTable: "Exchanges",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Chats_Accounts_SelectedAccountId",
                table: "Chats",
                column: "SelectedAccountId",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Accounts_Exchanges_ExchangeId",
                table: "Accounts");

            migrationBuilder.DropForeignKey(
                name: "FK_Chats_Accounts_SelectedAccountId",
                table: "Chats");

            migrationBuilder.DropTable(
                name: "Exchanges");

            migrationBuilder.DropIndex(
                name: "IX_Chats_SelectedAccountId",
                table: "Chats");

            migrationBuilder.DropIndex(
                name: "IX_Accounts_ExchangeId",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "SelectedAccountId",
                table: "Chats");

            migrationBuilder.DropColumn(
                name: "ExchangeId",
                table: "Accounts");

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
    }
}
