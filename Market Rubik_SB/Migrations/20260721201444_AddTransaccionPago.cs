using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MiniMarketApp.Migrations
{
    /// <inheritdoc />
    public partial class AddTransaccionPago : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.CreateTable(
                name: "TransaccionesPago",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    VentaId = table.Column<int>(type: "integer", nullable: false),
                    ClientTransactionId = table.Column<string>(type: "text", nullable: false),
                    PayphonePaymentUrl = table.Column<string>(type: "text", nullable: true),
                    PayphoneTransactionId = table.Column<string>(type: "text", nullable: true),
                    AmountInCents = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    ConfirmedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Provider = table.Column<string>(type: "text", nullable: false),
                    PayPalOrderId = table.Column<string>(type: "text", nullable: true),
                    PayPalCaptureId = table.Column<string>(type: "text", nullable: true),
                    PayPalApprovalUrl = table.Column<string>(type: "text", nullable: true),
                    GatewayResponse = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransaccionesPago", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TransaccionesPago_Ventas_VentaId",
                        column: x => x.VentaId,
                        principalTable: "Ventas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TransaccionesPago_VentaId",
                table: "TransaccionesPago",
                column: "VentaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TransaccionesPago");
        }
    }
}
