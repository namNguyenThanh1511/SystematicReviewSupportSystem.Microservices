using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SRSS.Project.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class removenamelang : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NameEn",
                table: "Projects");

            migrationBuilder.RenameColumn(
                name: "NameVn",
                table: "Projects",
                newName: "Name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Projects",
                newName: "NameVn");

            migrationBuilder.AddColumn<string>(
                name: "NameEn",
                table: "Projects",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");
        }
    }
}
