using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AIDefCom.Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddTotalScoreToGroup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "TotalScore",
                table: "Groups",
                type: "float",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotalScore",
                table: "Groups");
        }
    }
}
