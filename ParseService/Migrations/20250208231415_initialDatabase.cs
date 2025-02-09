using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ParseService.Migrations
{
    /// <inheritdoc />
    public partial class initialDatabase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Announcements",
                columns: table => new
                {
                    AnnId = table.Column<int>(type: "INTEGER", nullable: false),
                    AnnTitle = table.Column<string>(type: "TEXT", nullable: false),
                    AnnDesc = table.Column<string>(type: "TEXT", nullable: false),
                    CTime = table.Column<long>(type: "INTEGER", nullable: false),
                    Language = table.Column<string>(type: "TEXT", nullable: false),
                    AnnUrl = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Announcements", x => x.AnnId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Announcements");
        }
    }
}
