using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AIDefCom.Repository.Migrations
{
    /// <inheritdoc />
    public partial class UpdateScoreEvaluatorToLecturer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Scores_AspNetUsers_EvaluatorId",
                table: "Scores");

            migrationBuilder.AddForeignKey(
                name: "FK_Scores_Lecturers_EvaluatorId",
                table: "Scores",
                column: "EvaluatorId",
                principalTable: "Lecturers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Scores_Lecturers_EvaluatorId",
                table: "Scores");

            migrationBuilder.AddForeignKey(
                name: "FK_Scores_AspNetUsers_EvaluatorId",
                table: "Scores",
                column: "EvaluatorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
