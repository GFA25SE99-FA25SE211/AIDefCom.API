using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AIDefCom.Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddRecordingEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Recordings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BlobPath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BlobUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MimeType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DurationSeconds = table.Column<int>(type: "int", nullable: true),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: true),
                    CreatedUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TranscriptId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Recordings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Recordings_Transcripts_TranscriptId",
                        column: x => x.TranscriptId,
                        principalTable: "Transcripts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Recordings_TranscriptId",
                table: "Recordings",
                column: "TranscriptId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Recordings");
        }
    }
}
