using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MiniMarketApp.Migrations
{
    /// <inheritdoc />
    public partial class AddFidelizacionYPromociones : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "DescuentoPromociones",
                table: "Ventas",
                type: "numeric(10,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "DescuentoPuntos",
                table: "Ventas",
                type: "numeric(10,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "PuntosUtilizados",
                table: "Ventas",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PuntosAcumulados",
                table: "Clientes",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Promociones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TipoDescuento = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ValorDescuento = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    FechaInicio = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    FechaFin = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CategoriaId = table.Column<int>(type: "integer", nullable: true),
                    ProductoId = table.Column<int>(type: "integer", nullable: true),
                    CantidadMinima = table.Column<int>(type: "integer", nullable: false),
                    Activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Promociones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Promociones_Categorias_CategoriaId",
                        column: x => x.CategoriaId,
                        principalTable: "Categorias",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Promociones_Productos_ProductoId",
                        column: x => x.ProductoId,
                        principalTable: "Productos",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Promociones_Activo_FechaInicio_FechaFin",
                table: "Promociones",
                columns: new[] { "Activo", "FechaInicio", "FechaFin" });

            migrationBuilder.CreateIndex(
                name: "IX_Promociones_CategoriaId",
                table: "Promociones",
                column: "CategoriaId");

            migrationBuilder.CreateIndex(
                name: "IX_Promociones_ProductoId",
                table: "Promociones",
                column: "ProductoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Promociones");

            migrationBuilder.DropColumn(
                name: "DescuentoPromociones",
                table: "Ventas");

            migrationBuilder.DropColumn(
                name: "DescuentoPuntos",
                table: "Ventas");

            migrationBuilder.DropColumn(
                name: "PuntosUtilizados",
                table: "Ventas");

            migrationBuilder.DropColumn(
                name: "PuntosAcumulados",
                table: "Clientes");
        }
    }
}
