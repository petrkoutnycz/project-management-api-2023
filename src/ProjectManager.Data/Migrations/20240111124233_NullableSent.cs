using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;

#nullable disable

namespace ProjectManager.Data.Migrations
{
    /// <inheritdoc />
    public partial class NullableSent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Instant>(
                name: "SentAt",
                table: "Email",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(Instant),
                oldType: "timestamp with time zone");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Instant>(
                name: "SentAt",
                table: "Email",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: NodaTime.Instant.FromUnixTimeTicks(0L),
                oldClrType: typeof(Instant),
                oldType: "timestamp with time zone",
                oldNullable: true);
        }
    }
}
