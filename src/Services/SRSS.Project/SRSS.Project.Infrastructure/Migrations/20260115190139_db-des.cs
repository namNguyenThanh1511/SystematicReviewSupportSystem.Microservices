using System;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SRSS.Project.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class dbdes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProjectMember_Project_ProjectId",
                table: "ProjectMember");

            migrationBuilder.DropTable(
                name: "ProjectStage");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProjectMember",
                table: "ProjectMember");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Project",
                table: "Project");

            migrationBuilder.DropColumn(
                name: "ProsperoId",
                table: "Project");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Project");

            migrationBuilder.RenameTable(
                name: "ProjectMember",
                newName: "project_members");

            migrationBuilder.RenameTable(
                name: "Project",
                newName: "projects");

            migrationBuilder.RenameColumn(
                name: "Role",
                table: "project_members",
                newName: "role");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "project_members",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "project_members",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "ProjectId",
                table: "project_members",
                newName: "project_id");

            migrationBuilder.RenameColumn(
                name: "ModifiedAt",
                table: "project_members",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "project_members",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "AssignedAt",
                table: "project_members",
                newName: "joined_at");

            migrationBuilder.RenameIndex(
                name: "IX_ProjectMember_ProjectId",
                table: "project_members",
                newName: "IX_project_members_project_id");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "projects",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "projects",
                newName: "description");

            migrationBuilder.RenameColumn(
                name: "Abbreviation",
                table: "projects",
                newName: "abbreviation");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "projects",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "StartDate",
                table: "projects",
                newName: "start_date");

            migrationBuilder.RenameColumn(
                name: "ResearchQuestions",
                table: "projects",
                newName: "research_questions");

            migrationBuilder.RenameColumn(
                name: "ModifiedAt",
                table: "projects",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "InclusionCriteria",
                table: "projects",
                newName: "inclusion_criteria");

            migrationBuilder.RenameColumn(
                name: "ExclusionCriteria",
                table: "projects",
                newName: "exclusion_criteria");

            migrationBuilder.RenameColumn(
                name: "CreatedBy",
                table: "projects",
                newName: "created_by");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "projects",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "projects",
                newName: "phase");

            migrationBuilder.RenameColumn(
                name: "EndDate",
                table: "projects",
                newName: "phase_changed_at");

            migrationBuilder.AlterColumn<string>(
                name: "description",
                table: "projects",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(1000)",
                oldMaxLength: 1000);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "start_date",
                table: "projects",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<Guid>(
                name: "created_by",
                table: "projects",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "actual_end_date",
                table: "projects",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "criteria_version",
                table: "projects",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "expected_end_date",
                table: "projects",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                table: "projects",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "updated_by",
                table: "projects",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_project_members",
                table: "project_members",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_projects",
                table: "projects",
                column: "id");

            migrationBuilder.CreateTable(
                name: "project_audit_logs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    project_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    action = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    old_value = table.Column<JsonDocument>(type: "jsonb", nullable: true),
                    new_value = table.Column<JsonDocument>(type: "jsonb", nullable: true),
                    performed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_project_audit_logs", x => x.id);
                    table.ForeignKey(
                        name: "FK_project_audit_logs_projects_project_id",
                        column: x => x.project_id,
                        principalTable: "projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SearchSources",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceName = table.Column<string>(type: "text", nullable: false),
                    SourceType = table.Column<int>(type: "integer", nullable: false),
                    PlannedSearchString = table.Column<string>(type: "text", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SearchSources", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SearchSources_projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "import_batches",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    project_id = table.Column<Guid>(type: "uuid", nullable: false),
                    search_source_id = table.Column<Guid>(type: "uuid", nullable: false),
                    executed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    executed_search_string = table.Column<string>(type: "text", nullable: false),
                    records_retrieved = table.Column<int>(type: "integer", nullable: false),
                    reason = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    phase_at_import = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_import_batches", x => x.id);
                    table.ForeignKey(
                        name: "FK_import_batches_SearchSources_search_source_id",
                        column: x => x.search_source_id,
                        principalTable: "SearchSources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_import_batches_projects_project_id",
                        column: x => x.project_id,
                        principalTable: "projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_project_members_user_id",
                table: "project_members",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_import_batches_project_id",
                table: "import_batches",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "IX_import_batches_search_source_id",
                table: "import_batches",
                column: "search_source_id");

            migrationBuilder.CreateIndex(
                name: "IX_project_audit_logs_performed_at",
                table: "project_audit_logs",
                column: "performed_at");

            migrationBuilder.CreateIndex(
                name: "IX_project_audit_logs_project_id",
                table: "project_audit_logs",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "IX_project_audit_logs_user_id",
                table: "project_audit_logs",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_SearchSources_ProjectId",
                table: "SearchSources",
                column: "ProjectId");

            migrationBuilder.AddForeignKey(
                name: "FK_project_members_projects_project_id",
                table: "project_members",
                column: "project_id",
                principalTable: "projects",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_project_members_projects_project_id",
                table: "project_members");

            migrationBuilder.DropTable(
                name: "import_batches");

            migrationBuilder.DropTable(
                name: "project_audit_logs");

            migrationBuilder.DropTable(
                name: "SearchSources");

            migrationBuilder.DropPrimaryKey(
                name: "PK_projects",
                table: "projects");

            migrationBuilder.DropPrimaryKey(
                name: "PK_project_members",
                table: "project_members");

            migrationBuilder.DropIndex(
                name: "IX_project_members_user_id",
                table: "project_members");

            migrationBuilder.DropColumn(
                name: "actual_end_date",
                table: "projects");

            migrationBuilder.DropColumn(
                name: "criteria_version",
                table: "projects");

            migrationBuilder.DropColumn(
                name: "expected_end_date",
                table: "projects");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "projects");

            migrationBuilder.DropColumn(
                name: "updated_by",
                table: "projects");

            migrationBuilder.RenameTable(
                name: "projects",
                newName: "Project");

            migrationBuilder.RenameTable(
                name: "project_members",
                newName: "ProjectMember");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "Project",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "description",
                table: "Project",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "abbreviation",
                table: "Project",
                newName: "Abbreviation");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Project",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                table: "Project",
                newName: "ModifiedAt");

            migrationBuilder.RenameColumn(
                name: "start_date",
                table: "Project",
                newName: "StartDate");

            migrationBuilder.RenameColumn(
                name: "research_questions",
                table: "Project",
                newName: "ResearchQuestions");

            migrationBuilder.RenameColumn(
                name: "inclusion_criteria",
                table: "Project",
                newName: "InclusionCriteria");

            migrationBuilder.RenameColumn(
                name: "exclusion_criteria",
                table: "Project",
                newName: "ExclusionCriteria");

            migrationBuilder.RenameColumn(
                name: "created_by",
                table: "Project",
                newName: "CreatedBy");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "Project",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "phase_changed_at",
                table: "Project",
                newName: "EndDate");

            migrationBuilder.RenameColumn(
                name: "phase",
                table: "Project",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "role",
                table: "ProjectMember",
                newName: "Role");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "ProjectMember",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "ProjectMember",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                table: "ProjectMember",
                newName: "ModifiedAt");

            migrationBuilder.RenameColumn(
                name: "project_id",
                table: "ProjectMember",
                newName: "ProjectId");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "ProjectMember",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "joined_at",
                table: "ProjectMember",
                newName: "AssignedAt");

            migrationBuilder.RenameIndex(
                name: "IX_project_members_project_id",
                table: "ProjectMember",
                newName: "IX_ProjectMember_ProjectId");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Project",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "StartDate",
                table: "Project",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)),
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "CreatedBy",
                table: "Project",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProsperoId",
                table: "Project",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Project",
                type: "bytea",
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Project",
                table: "Project",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProjectMember",
                table: "ProjectMember",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "ProjectStage",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    CompletionPercentage = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    StageName = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectStage", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectStage_Project_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Project",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProjectStage_ProjectId",
                table: "ProjectStage",
                column: "ProjectId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectMember_Project_ProjectId",
                table: "ProjectMember",
                column: "ProjectId",
                principalTable: "Project",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
