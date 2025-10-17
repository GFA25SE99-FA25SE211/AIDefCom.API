using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AIDefCom.Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddUserIntoRecording : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Recordings",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Recordings_UserId",
                table: "Recordings",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Recordings_AspNetUsers_UserId",
                table: "Recordings",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Recordings_AspNetUsers_UserId",
                table: "Recordings");

            migrationBuilder.DropIndex(
                name: "IX_Recordings_UserId",
                table: "Recordings");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Recordings");
        }
    }
}
