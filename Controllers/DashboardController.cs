using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Valen_SistemaStock.Data;
using Valen_SistemaStock.Models.Enum;
using Valen_SistemaStock.Models.ViewModels;

namespace Valen_SistemaStock.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly AppDbContext _context;

        public DashboardController(AppDbContext context)
        {
            _context = context;
        }

        // GET: DashboardController
        public async Task<IActionResult> Index()
        {
            // ===================== KPIs =====================
            var model = new DashboardViewModel();

            model.ValorTotalInventario = await _context.StockDepositos
                .Include(s => s.Producto)
                .SumAsync(s => s.Cantidad * s.Producto!.PrecioCosto);

            model.TotalProductosActivos = await _context.Productos.CountAsync(p => p.Activo);
            model.TotalProveedoresActivos = await _context.Proveedores.CountAsync(p => p.Activo);
            model.TotalClientesActivos = await _context.Clientes.CountAsync(c => c.Activo);
            model.TotalDepositosActivos = await _context.Depositos.CountAsync(d => d.Activo);

            model.MovimientosHoy = await _context.Movimientos
                .CountAsync(m => m.Fecha >= DateTime.Today && m.Fecha < DateTime.Today.AddDays(1));

            model.ValorVentasHoy = await _context.Movimientos
                .Include(s => s.Producto)
                .Where(m => m.Fecha >= DateTime.Today && m.Fecha < DateTime.Today.AddDays(1) && m.Tipo == TipoMovimiento.Salida)
                .SumAsync(s => s.Cantidad * s.Producto!.PrecioVenta);

            // ============ Tablas ============
            // Stock total por producto activo
            var stockProductos = await _context.Productos
                .AsNoTracking()
                .Include(p => p.Categoria)
                .Include(p => p.Stocks)
                .Where(p => p.Activo)
                .Select(p => new
                {
                    p.id,
                    p.Nombre,
                    Categoria = p.Categoria,
                    p.StockMinimo,
                    p.StockTotal
                })
                .ToListAsync();

            // Alertas: BajoStockMinimo = StockTotal <= StockMinimo
            var alertas = stockProductos
                .Where(p => p.StockTotal <= p.StockMinimo)
                .OrderBy(p => p.StockTotal)
                .Select(p => new AlertaReposicion
                {
                    idProducto = p.id,
                    Producto = p.Nombre,
                    Categoria = p.Categoria.Nombre,
                    StockTotal = p.StockTotal,
                    StockMinimo = p.StockMinimo
                })
                .ToList();

            model.AlertasReposicion = alertas.Take(15).ToList();

            // ================= Gráficos =================
            model.TopMayorStock = stockProductos
                .OrderByDescending(p => p.StockTotal).Take(5)
                .Select(p => new StockProducto { Producto = p.Nombre, StockTotal = p.StockTotal })
                .ToList();

            model.TopMenorStock = stockProductos
                .OrderBy(p => p.StockTotal).Take(5)
                .Select(p => new StockProducto { Producto = p.Nombre, StockTotal = p.StockTotal })
                .ToList();

            return View(model);
        }
    }
}
