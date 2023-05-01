using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Messenger.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateforRepositorypattern : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Chats_ChatId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_ChatUser_Messages_NewestMessageId",
                table: "ChatUser");

            migrationBuilder.DropIndex(
                name: "IX_ChatUser_NewestMessageId",
                table: "ChatUser");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_ChatId",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_IntId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "NewestMessageId",
                table: "ChatUser");

            migrationBuilder.DropColumn(
                name: "CountOfMesages",
                table: "Chats");

            migrationBuilder.DropColumn(
                name: "ChatId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "IntId",
                table: "AspNetUsers");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "NewestMessageId",
                table: "ChatUser",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CountOfMesages",
                table: "Chats",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ChatId",
                table: "AspNetUsers",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IntId",
                table: "AspNetUsers",
                type: "integer",
                nullable: false,
                defaultValue: 0)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.CreateIndex(
                name: "IX_ChatUser_NewestMessageId",
                table: "ChatUser",
                column: "NewestMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_ChatId",
                table: "AspNetUsers",
                column: "ChatId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_IntId",
                table: "AspNetUsers",
                column: "IntId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Chats_ChatId",
                table: "AspNetUsers",
                column: "ChatId",
                principalTable: "Chats",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ChatUser_Messages_NewestMessageId",
                table: "ChatUser",
                column: "NewestMessageId",
                principalTable: "Messages",
                principalColumn: "Id");
        }
    }
}
