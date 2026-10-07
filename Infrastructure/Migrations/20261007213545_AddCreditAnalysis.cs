using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCreditAnalysis : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CreditScore",
                table: "Customers",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Purpose",
                table: "CreditRequests",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "CreditAnalysis",
                columns: table => new
                {
                    CreditAnalysisId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CreditRequestId = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    AnalyzedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CreditAnalysis", x => x.CreditAnalysisId);
                    table.ForeignKey(
                        name: "FK_CreditAnalysis_CreditRequests_CreditRequestId",
                        column: x => x.CreditRequestId,
                        principalTable: "CreditRequests",
                        principalColumn: "CreditRequestId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CreditRuleResult",
                columns: table => new
                {
                    CreditRuleResultId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CreditAnalysisId = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Approved = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CreditRuleResult", x => x.CreditRuleResultId);
                    table.ForeignKey(
                        name: "FK_CreditRuleResult_CreditAnalysis_CreditAnalysisId",
                        column: x => x.CreditAnalysisId,
                        principalTable: "CreditAnalysis",
                        principalColumn: "CreditAnalysisId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CreditAnalysis_CreditRequestId",
                table: "CreditAnalysis",
                column: "CreditRequestId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CreditRuleResult_CreditAnalysisId",
                table: "CreditRuleResult",
                column: "CreditAnalysisId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CreditRuleResult");

            migrationBuilder.DropTable(
                name: "CreditAnalysis");

            migrationBuilder.DropColumn(
                name: "CreditScore",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "Purpose",
                table: "CreditRequests");
        }
    }
}
