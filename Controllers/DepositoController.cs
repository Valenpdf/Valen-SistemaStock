using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Valen_SistemaStock.Data;
using Valen_SistemaStock.Models.Entities;
using Valen_SistemaStock.Models.ViewModels;

namespace Valen_SistemaStock.Controllers
{
    [Authorize(Roles = "Administrador,Encargado")]
    public class DepositoController : Controller
    {
        private readonly AppDbContext _context;

        public DepositoController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Deposito
        public async Task<IActionResult> Index()
        {
            var depositos = await _context.Depositos.ToListAsync();

            var listaViewModels = depositos.Select(d => new DepositoViewModel
            {
                id = d.id,
                Nombre = d.Nombre,
                Ubicacion = d.Ubicacion,
                Activo = d.Activo
            }).ToList();

            return View(listaViewModels);
        }

        // GET: Deposito/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var deposito = await _context.Depositos.FirstOrDefaultAsync(d => d.id == id);
            if (deposito == null) return NotFound();

            var model = new DepositoViewModel
            {
                id = deposito.id,
                Nombre = deposito.Nombre,
                Ubicacion = deposito.Ubicacion,
                Activo = deposito.Activo
            };

            return View(model);
        }

        // GET: Deposito/Create
        public IActionResult Create()
        {
            return View(new DepositoViewModel());
        }

        // POST: Deposito/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DepositoViewModel model)
        {
            if (ModelState.IsValid)
            {
                bool existeDeposito = await _context.Depositos
                    .AnyAsync(d => d.Nombre.ToLower() == model.Nombre.ToLower());

                if (existeDeposito)
                {
                    ModelState.AddModelError("Nombre", "Ya existe un depósito con este nombre.");
                    return View(model);
                }

                var nuevoDeposito = new Deposito
                {
                    Nombre = model.Nombre,
                    Ubicacion = model.Ubicacion,
                    Activo = model.Activo
                };

                _context.Depositos.Add(nuevoDeposito);
                await _context.SaveChangesAsync();

                TempData["Ok"] = "¡El depósito se ha creado con éxito!";
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // GET: Deposito/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var deposito = await _context.Depositos.FindAsync(id);
            if (deposito == null) return NotFound();

            var model = new DepositoViewModel
            {
                id = deposito.id,
                Nombre = deposito.Nombre,
                Ubicacion = deposito.Ubicacion,
                Activo = deposito.Activo
            };

            return View(model);
        }

        // POST: Deposito/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int? id, DepositoViewModel model)
        {
            if (id != model.id) return NotFound();

            if (ModelState.IsValid)
            {
                bool existeDeposito = await _context.Depositos
                    .AnyAsync(d => d.Nombre.ToLower() == model.Nombre.ToLower() && d.id != model.id);

                if (existeDeposito)
                {
                    ModelState.AddModelError("Nombre", "Ya existe un depósito con este nombre.");
                    return View(model);
                }

                var deposito = await _context.Depositos.FindAsync(id);
                if (deposito == null) return NotFound();

                deposito.Nombre = model.Nombre;
                deposito.Ubicacion = model.Ubicacion;
                deposito.Activo = model.Activo;
                deposito.UpdatedAt = DateTime.UtcNow;

                _context.Depositos.Update(deposito);
                await _context.SaveChangesAsync();

                TempData["Ok"] = "¡El depósito se ha modificado con éxito!";
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // GET: Deposito/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var deposito = await _context.Depositos.FindAsync(id);
            if (deposito == null) return NotFound();

            var model = new DepositoViewModel
            {
                id = deposito.id,
                Nombre = deposito.Nombre,
                Ubicacion = deposito.Ubicacion,
                Activo = deposito.Activo
            };

            return View(model);
        }

        // POST: Deposito/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int? id)
        {
            var deposito = await _context.Depositos
                .Include(d => d.Stocks)
                .Include(d => d.MovimientosOrigen)
                .Include(d => d.MovimientosDestino)
                .FirstOrDefaultAsync(d => d.id == id);

            if (deposito != null)
            {
                if (deposito.Stocks.Any() || deposito.MovimientosOrigen.Any() || deposito.MovimientosDestino.Any())
                {
                    TempData["Error"] = $"No se puede eliminar el depósito '{deposito.Nombre}' porque tiene stock o movimientos asociados.";
                    return RedirectToAction(nameof(Index));
                }

                _context.Depositos.Remove(deposito);
                await _context.SaveChangesAsync();
                TempData["Ok"] = "Depósito eliminado con éxito.";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool DepositoExists(int? id)
        {
            return _context.Depositos.Any(e => e.id == id);
        }
    }
}
