using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mechanics.Infra.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentsTableSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Payments",
                schema: "Mechanics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    WorkOrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BudgetId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MercadoPagoPaymentId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    MercadoPagoPreferenceId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ExternalReference = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    StatusDetail = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CheckoutUrl = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    SandboxCheckoutUrl = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSDATETIME()"),
                    LastWebhookReceivedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Payments_Budgets_BudgetId",
                        column: x => x.BudgetId,
                        principalSchema: "Mechanics",
                        principalTable: "Budgets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Payments_WorkOrders_WorkOrderId",
                        column: x => x.WorkOrderId,
                        principalSchema: "Mechanics",
                        principalTable: "WorkOrders",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Payments_BudgetId",
                schema: "Mechanics",
                table: "Payments",
                column: "BudgetId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_ExternalReference",
                schema: "Mechanics",
                table: "Payments",
                column: "ExternalReference");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_MercadoPagoPaymentId",
                schema: "Mechanics",
                table: "Payments",
                column: "MercadoPagoPaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_MercadoPagoPreferenceId",
                schema: "Mechanics",
                table: "Payments",
                column: "MercadoPagoPreferenceId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_WorkOrderId",
                schema: "Mechanics",
                table: "Payments",
                column: "WorkOrderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Payments",
                schema: "Mechanics");
        }
    }
}
