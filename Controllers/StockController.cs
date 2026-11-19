using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Valen_SistemaStock.Data;
using Valen_SistemaStock.Models.ViewModels;

namespace Valen_SistemaStock.Controllers
{
    [Authorize]
    public class StockController : Controller
    {
        private readonly AppDbContext _context;

        public StockController(AppDbContext context)
        {
            _context = context;
        }

        // GET: StockController
        public async Task<IActionResult> Index(string? buscar, int? idDeposito)
        {
            var query = _context.Productos
                .Include(p => p.Stocks)
                    .ThenInclude(s => s.Deposito)
                .Where(p => p.Activo)
                .AsQueryable();

            if (!string.IsNullOrEmpty(buscar))
                query = query.Where(p => p.Nombre.Contains(buscar) || p.Codigo!.Contains(buscar));

            if (idDeposito.HasValue)
                query = query.Where(p => p.Stocks.Any(s => s.idDeposito == idDeposito.Value && s.Cantidad != 0));

            ViewBag.Depositos = new SelectList(await _context.Depositos.Where(d => d.Activo).ToListAsync(), "id", "Nombre");

            var productos = await query.OrderBy(p => p.Nombre).ToListAsync();

            var listaViewModels = productos.Select(p => new StockViewModel
            {
                id = p.id,
                Nombre = p.Nombre,
                StockTotal = p.StockTotal,
                Stocks = p.Stocks
                    .Where(s => s.Cantidad != 0 && (!idDeposito.HasValue || s.idDeposito == idDeposito.Value))
                    .Select(s => new StockDepositoViewModel
                    {
                        DepositoNombre = s.Deposito?.Nombre ?? "",
                        Cantidad = s.Cantidad
                    }).ToList()
            }).ToList();

            return View(listaViewModels);
        }
    }
}
