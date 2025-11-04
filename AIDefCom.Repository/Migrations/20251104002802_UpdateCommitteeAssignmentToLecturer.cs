using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AIDefCom.Repository.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCommitteeAssignmentToLecturer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CommitteeAssignments_AspNetUsers_UserId",
                table: "CommitteeAssignments");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "CommitteeAssignments",
                newName: "LecturerId");

            migrationBuilder.RenameIndex(
                name: "IX_CommitteeAssignments_UserId",
                table: "CommitteeAssignments",
                newName: "IX_CommitteeAssignments_LecturerId");

            migrationBuilder.AddForeignKey(
                name: "FK_CommitteeAssignments_Lecturers_LecturerId",
                table: "CommitteeAssignments",
                column: "LecturerId",
                principalTable: "Lecturers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CommitteeAssignments_Lecturers_LecturerId",
                table: "CommitteeAssignments");

            migrationBuilder.RenameColumn(
                name: "LecturerId",
                table: "CommitteeAssignments",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_CommitteeAssignments_LecturerId",
                table: "CommitteeAssignments",
                newName: "IX_CommitteeAssignments_UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_CommitteeAssignments_AspNetUsers_UserId",
                table: "CommitteeAssignments",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
