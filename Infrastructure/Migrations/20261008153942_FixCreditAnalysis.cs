using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixCreditAnalysis : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CreditAnalysis_CreditRequests_CreditRequestId",
                table: "CreditAnalysis");

            migrationBuilder.DropForeignKey(
                name: "FK_CreditRuleResult_CreditAnalysis_CreditAnalysisId",
                table: "CreditRuleResult");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CreditRuleResult",
                table: "CreditRuleResult");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CreditAnalysis",
                table: "CreditAnalysis");

            migrationBuilder.RenameTable(
                name: "CreditRuleResult",
                newName: "CreditRuleResults");

            migrationBuilder.RenameTable(
                name: "CreditAnalysis",
                newName: "CreditAnalyses");

            migrationBuilder.RenameIndex(
                name: "IX_CreditRuleResult_CreditAnalysisId",
                table: "CreditRuleResults",
                newName: "IX_CreditRuleResults_CreditAnalysisId");

            migrationBuilder.RenameIndex(
                name: "IX_CreditAnalysis_CreditRequestId",
                table: "CreditAnalyses",
                newName: "IX_CreditAnalyses_CreditRequestId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CreditRuleResults",
                table: "CreditRuleResults",
                column: "CreditRuleResultId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CreditAnalyses",
                table: "CreditAnalyses",
                column: "CreditAnalysisId");

            migrationBuilder.AddForeignKey(
                name: "FK_CreditAnalyses_CreditRequests_CreditRequestId",
                table: "CreditAnalyses",
                column: "CreditRequestId",
                principalTable: "CreditRequests",
                principalColumn: "CreditRequestId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CreditRuleResults_CreditAnalyses_CreditAnalysisId",
                table: "CreditRuleResults",
                column: "CreditAnalysisId",
                principalTable: "CreditAnalyses",
                principalColumn: "CreditAnalysisId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CreditAnalyses_CreditRequests_CreditRequestId",
                table: "CreditAnalyses");

            migrationBuilder.DropForeignKey(
                name: "FK_CreditRuleResults_CreditAnalyses_CreditAnalysisId",
                table: "CreditRuleResults");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CreditRuleResults",
                table: "CreditRuleResults");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CreditAnalyses",
                table: "CreditAnalyses");

            migrationBuilder.RenameTable(
                name: "CreditRuleResults",
                newName: "CreditRuleResult");

            migrationBuilder.RenameTable(
                name: "CreditAnalyses",
                newName: "CreditAnalysis");

            migrationBuilder.RenameIndex(
                name: "IX_CreditRuleResults_CreditAnalysisId",
                table: "CreditRuleResult",
                newName: "IX_CreditRuleResult_CreditAnalysisId");

            migrationBuilder.RenameIndex(
                name: "IX_CreditAnalyses_CreditRequestId",
                table: "CreditAnalysis",
                newName: "IX_CreditAnalysis_CreditRequestId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CreditRuleResult",
                table: "CreditRuleResult",
                column: "CreditRuleResultId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CreditAnalysis",
                table: "CreditAnalysis",
                column: "CreditAnalysisId");

            migrationBuilder.AddForeignKey(
                name: "FK_CreditAnalysis_CreditRequests_CreditRequestId",
                table: "CreditAnalysis",
                column: "CreditRequestId",
                principalTable: "CreditRequests",
                principalColumn: "CreditRequestId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CreditRuleResult_CreditAnalysis_CreditAnalysisId",
                table: "CreditRuleResult",
                column: "CreditAnalysisId",
                principalTable: "CreditAnalysis",
                principalColumn: "CreditAnalysisId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
