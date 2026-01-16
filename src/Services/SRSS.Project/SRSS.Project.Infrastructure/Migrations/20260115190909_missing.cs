using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SRSS.Project.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class missing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_import_batches_SearchSources_search_source_id",
                table: "import_batches");

            migrationBuilder.DropForeignKey(
                name: "FK_SearchSources_projects_ProjectId",
                table: "SearchSources");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SearchSources",
                table: "SearchSources");

            migrationBuilder.RenameTable(
                name: "SearchSources",
                newName: "search_sources");

            migrationBuilder.RenameColumn(
                name: "Notes",
                table: "search_sources",
                newName: "notes");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "search_sources",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "SourceType",
                table: "search_sources",
                newName: "source_type");

            migrationBuilder.RenameColumn(
                name: "SourceName",
                table: "search_sources",
                newName: "source_name");

            migrationBuilder.RenameColumn(
                name: "ProjectId",
                table: "search_sources",
                newName: "project_id");

            migrationBuilder.RenameColumn(
                name: "PlannedSearchString",
                table: "search_sources",
                newName: "planned_search_string");

            migrationBuilder.RenameColumn(
                name: "ModifiedAt",
                table: "search_sources",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "search_sources",
                newName: "created_at");

            migrationBuilder.RenameIndex(
                name: "IX_SearchSources_ProjectId",
                table: "search_sources",
                newName: "IX_search_sources_project_id");

            migrationBuilder.AlterColumn<string>(
                name: "source_type",
                table: "search_sources",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "source_name",
                table: "search_sources",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddPrimaryKey(
                name: "PK_search_sources",
                table: "search_sources",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_import_batches_search_sources_search_source_id",
                table: "import_batches",
                column: "search_source_id",
                principalTable: "search_sources",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_search_sources_projects_project_id",
                table: "search_sources",
                column: "project_id",
                principalTable: "projects",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_import_batches_search_sources_search_source_id",
                table: "import_batches");

            migrationBuilder.DropForeignKey(
                name: "FK_search_sources_projects_project_id",
                table: "search_sources");

            migrationBuilder.DropPrimaryKey(
                name: "PK_search_sources",
                table: "search_sources");

            migrationBuilder.RenameTable(
                name: "search_sources",
                newName: "SearchSources");

            migrationBuilder.RenameColumn(
                name: "notes",
                table: "SearchSources",
                newName: "Notes");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "SearchSources",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                table: "SearchSources",
                newName: "ModifiedAt");

            migrationBuilder.RenameColumn(
                name: "source_type",
                table: "SearchSources",
                newName: "SourceType");

            migrationBuilder.RenameColumn(
                name: "source_name",
                table: "SearchSources",
                newName: "SourceName");

            migrationBuilder.RenameColumn(
                name: "project_id",
                table: "SearchSources",
                newName: "ProjectId");

            migrationBuilder.RenameColumn(
                name: "planned_search_string",
                table: "SearchSources",
                newName: "PlannedSearchString");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "SearchSources",
                newName: "CreatedAt");

            migrationBuilder.RenameIndex(
                name: "IX_search_sources_project_id",
                table: "SearchSources",
                newName: "IX_SearchSources_ProjectId");

            migrationBuilder.AlterColumn<int>(
                name: "SourceType",
                table: "SearchSources",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "SourceName",
                table: "SearchSources",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255);

            migrationBuilder.AddPrimaryKey(
                name: "PK_SearchSources",
                table: "SearchSources",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_import_batches_SearchSources_search_source_id",
                table: "import_batches",
                column: "search_source_id",
                principalTable: "SearchSources",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SearchSources_projects_ProjectId",
                table: "SearchSources",
                column: "ProjectId",
                principalTable: "projects",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
