using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SystemResourceMonitorAPI.Migrations
{
    /// <inheritdoc />
    public partial class FullSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Alerts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Message = table.Column<string>(type: "text", nullable: false),
                    Time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Severity = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CurrentValue = table.Column<double>(type: "double precision", nullable: false),
                    ThresholdValue = table.Column<double>(type: "double precision", nullable: false),
                    IsResolved = table.Column<bool>(type: "boolean", nullable: false),
                    ResolvedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Alerts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProcessMetrics",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ProcessName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ProcessId = table.Column<int>(type: "integer", nullable: false),
                    MemoryUsageMb = table.Column<double>(type: "double precision", nullable: false),
                    CpuTimeSeconds = table.Column<double>(type: "double precision", nullable: false),
                    ThreadCount = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcessMetrics", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Username = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    Role = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastLoginAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SystemMetrics",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CpuUsagePercent = table.Column<double>(type: "double precision", nullable: false),
                    TotalMemoryMb = table.Column<double>(type: "double precision", nullable: false),
                    UsedMemoryMb = table.Column<double>(type: "double precision", nullable: false),
                    FreeMemoryMb = table.Column<double>(type: "double precision", nullable: false),
                    MemoryUsagePercent = table.Column<double>(type: "double precision", nullable: false),
                    DiskUsagePercent = table.Column<double>(type: "double precision", nullable: true),
                    DiskTotalGb = table.Column<double>(type: "double precision", nullable: true),
                    DiskFreeGb = table.Column<double>(type: "double precision", nullable: true),
                    NetworkBytesSent = table.Column<long>(type: "bigint", nullable: true),
                    NetworkBytesReceived = table.Column<long>(type: "bigint", nullable: true),
                    ActiveProcessCount = table.Column<int>(type: "integer", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemMetrics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SystemMetrics_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SystemMetrics_UserId",
                table: "SystemMetrics",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Alerts");

            migrationBuilder.DropTable(
                name: "ProcessMetrics");

            migrationBuilder.DropTable(
                name: "SystemMetrics");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
