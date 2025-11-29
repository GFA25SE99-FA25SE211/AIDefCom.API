using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AIDefCom.Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddProjectTaskSessionRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SessionId",
                table: "Tasks",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_SessionId",
                table: "Tasks",
                column: "SessionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_DefenseSessions_SessionId",
                table: "Tasks",
                column: "SessionId",
                principalTable: "DefenseSessions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_DefenseSessions_SessionId",
                table: "Tasks");

            migrationBuilder.DropIndex(
                name: "IX_Tasks_SessionId",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "SessionId",
                table: "Tasks");
        }
    }
}
