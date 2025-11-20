using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AIDefCom.Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddSoftDeleteToEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "StudentGroups",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Semesters",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Rubrics",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Majors",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "MajorRubrics",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Groups",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "DefenseSessions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "CouncilRoles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "CommitteeAssignments",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "StudentGroups");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Semesters");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Rubrics");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Majors");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "MajorRubrics");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Groups");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "DefenseSessions");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "CouncilRoles");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "CommitteeAssignments");
        }
    }
}
