using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AIDefCom.Repository.Migrations
{
    /// <inheritdoc />
    public partial class UniqueProjectTaskRubricPerSession : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tasks_SessionId",
                table: "Tasks");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_SessionId_RubricId",
                table: "Tasks",
                columns: new[] { "SessionId", "RubricId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tasks_SessionId_RubricId",
                table: "Tasks");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_SessionId",
                table: "Tasks",
                column: "SessionId");
        }
    }
}
