using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiniMarketApp.Migrations
{
    /// <inheritdoc />
    public partial class AgregarFechaEliminacion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "FechaEliminacion",
                table: "Ventas",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaEliminacion",
                table: "Proveedores",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaEliminacion",
                table: "Productos",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaEliminacion",
                table: "Clientes",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaEliminacion",
                table: "Categorias",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FechaEliminacion",
                table: "Ventas");

            migrationBuilder.DropColumn(
                name: "FechaEliminacion",
                table: "Proveedores");

            migrationBuilder.DropColumn(
                name: "FechaEliminacion",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "FechaEliminacion",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "FechaEliminacion",
                table: "Categorias");
        }
    }
}
