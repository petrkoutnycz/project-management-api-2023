using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;

#nullable disable

namespace ProjectManager.Data.Migrations
{
    /// <inheritdoc />
    public partial class StatusesAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "StatusId",
                table: "Todo",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "Status",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsDone = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    ModifiedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    ModifiedBy = table.Column<string>(type: "text", nullable: false),
                    DeletedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Status", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Status_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Todo_StatusId",
                table: "Todo",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_Status_ProjectId",
                table: "Status",
                column: "ProjectId");

            migrationBuilder.AddForeignKey(
                name: "FK_Todo_Status_StatusId",
                table: "Todo",
                column: "StatusId",
                principalTable: "Status",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Todo_Status_StatusId",
                table: "Todo");

            migrationBuilder.DropTable(
                name: "Status");

            migrationBuilder.DropIndex(
                name: "IX_Todo_StatusId",
                table: "Todo");

            migrationBuilder.DropColumn(
                name: "StatusId",
                table: "Todo");
        }
    }
}
