using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AIDefCom.Repository.Migrations
{
    /// <inheritdoc />
    public partial class UpdateRelationshipsAndMoveTotalScore : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MemberNotes_Groups_GroupId",
                table: "MemberNotes");

            migrationBuilder.DropIndex(
                name: "IX_Transcripts_SessionId",
                table: "Transcripts");

            migrationBuilder.DropIndex(
                name: "IX_Reports_SessionId",
                table: "Reports");

            migrationBuilder.DropIndex(
                name: "IX_Recordings_TranscriptId",
                table: "Recordings");

            migrationBuilder.DropIndex(
                name: "IX_MemberNotes_GroupId",
                table: "MemberNotes");

            migrationBuilder.DropColumn(
                name: "GroupId",
                table: "MemberNotes");

            migrationBuilder.DropColumn(
                name: "TotalScore",
                table: "Groups");

            migrationBuilder.AddColumn<int>(
                name: "SessionId",
                table: "MemberNotes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "TotalScore",
                table: "DefenseSessions",
                type: "float",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Transcripts_SessionId",
                table: "Transcripts",
                column: "SessionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reports_SessionId",
                table: "Reports",
                column: "SessionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Recordings_TranscriptId",
                table: "Recordings",
                column: "TranscriptId",
                unique: true,
                filter: "[TranscriptId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_MemberNotes_SessionId",
                table: "MemberNotes",
                column: "SessionId");

            migrationBuilder.AddForeignKey(
                name: "FK_MemberNotes_DefenseSessions_SessionId",
                table: "MemberNotes",
                column: "SessionId",
                principalTable: "DefenseSessions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MemberNotes_DefenseSessions_SessionId",
                table: "MemberNotes");

            migrationBuilder.DropIndex(
                name: "IX_Transcripts_SessionId",
                table: "Transcripts");

            migrationBuilder.DropIndex(
                name: "IX_Reports_SessionId",
                table: "Reports");

            migrationBuilder.DropIndex(
                name: "IX_Recordings_TranscriptId",
                table: "Recordings");

            migrationBuilder.DropIndex(
                name: "IX_MemberNotes_SessionId",
                table: "MemberNotes");

            migrationBuilder.DropColumn(
                name: "SessionId",
                table: "MemberNotes");

            migrationBuilder.DropColumn(
                name: "TotalScore",
                table: "DefenseSessions");

            migrationBuilder.AddColumn<string>(
                name: "GroupId",
                table: "MemberNotes",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<double>(
                name: "TotalScore",
                table: "Groups",
                type: "float",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Transcripts_SessionId",
                table: "Transcripts",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_Reports_SessionId",
                table: "Reports",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_Recordings_TranscriptId",
                table: "Recordings",
                column: "TranscriptId");

            migrationBuilder.CreateIndex(
                name: "IX_MemberNotes_GroupId",
                table: "MemberNotes",
                column: "GroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_MemberNotes_Groups_GroupId",
                table: "MemberNotes",
                column: "GroupId",
                principalTable: "Groups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
