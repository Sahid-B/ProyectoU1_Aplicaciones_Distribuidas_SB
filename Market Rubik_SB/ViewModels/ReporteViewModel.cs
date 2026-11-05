using System;
using System.Collections.Generic;
using MiniMarketApp.Models;

namespace MiniMarketApp.ViewModels
{
    public class TotalVentasDto
    {
        public decimal TotalVentas { get; set; }
        public int NumeroVentas { get; set; }
        public decimal PromedioPorVenta { get; set; }
    }

    public class VentasPorMesDto
    {
        public int Mes { get; set; }
        public int Anio { get; set; }
        public decimal Total { get; set; }
        public int Cantidad { get; set; }
    }

    public class TopProductoDto
    {
        public string Producto { get; set; } = string.Empty;
        public int CantidadVendida { get; set; }
    }

    public class ClienteMasComprasDto
    {
        public string Cliente { get; set; } = string.Empty;
        public decimal TotalComprado { get; set; }
    }

    public class ProductoStockDto
    {
        public string Nombre { get; set; } = string.Empty;
        public int Stock { get; set; }
    }

    public class VentasPorEstadoDto
    {
        public string Estado { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public decimal Total { get; set; }
    }

    public class ProveedorMasProductosDto
    {
        public string Proveedor { get; set; } = string.Empty;
        public int CantidadProductos { get; set; }
    }

    public class CategoriaMasVendidaDto
    {
        public string Categoria { get; set; } = string.Empty;
        public int CantidadVendida { get; set; }
    }

    public class PromedioVentaClienteDto
    {
        public string Cliente { get; set; } = string.Empty;
        public decimal PromedioVentas { get; set; }
    }

    public class ResumenGeneralDto
    {
        public int ClientesActivos { get; set; }
        public int ClientesInactivos { get; set; }
        public int ProductosActivos { get; set; }
        public int ProductosInactivos { get; set; }
        public int VentasActivas { get; set; }
        public int VentasInactivas { get; set; }
        public int TotalVentasUltimoMes { get; set; }
        public decimal TotalIngresosUltimoMes { get; set; }
    }

    public class PagosPorPasarelaDto
    {
        public string Pasarela { get; set; } = string.Empty;
        public int CantidadPagos { get; set; }
        public decimal TotalRecaudado { get; set; }
    }

    public class PagosPorEstadoDto
    {
        public string Estado { get; set; } = string.Empty;
        public int CantidadPagos { get; set; }
    }

    public class MovimientosPorTipoDto
    {
        public string TipoMovimiento { get; set; } = string.Empty;
        public int CantidadMovimientos { get; set; }
    }

    public class ComprasPorProveedorDto
    {
        public string Proveedor { get; set; } = string.Empty;
        public int CantidadCompras { get; set; }
        public decimal TotalInvertido { get; set; }
    }

    public class AuditoriaPorAccionDto
    {
        public string Accion { get; set; } = string.Empty;
        public int CantidadEventos { get; set; }
    }
}
