using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CryptoBot.Data.Migrations
{
    /// <inheritdoc />
    public partial class SetNullableSelectedAccount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Chats_SelectedAccountId",
                table: "Chats");

            migrationBuilder.AlterColumn<Guid>(
                name: "SelectedAccountId",
                table: "Chats",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.CreateIndex(
                name: "IX_Chats_SelectedAccountId",
                table: "Chats",
                column: "SelectedAccountId",
                unique: true,
                filter: "[SelectedAccountId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Chats_SelectedAccountId",
                table: "Chats");

            migrationBuilder.AlterColumn<Guid>(
                name: "SelectedAccountId",
                table: "Chats",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Chats_SelectedAccountId",
                table: "Chats",
                column: "SelectedAccountId",
                unique: true);
        }
    }
}
