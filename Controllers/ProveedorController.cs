using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Valen_SistemaStock.Data;
using Valen_SistemaStock.Models.Entities;
using Valen_SistemaStock.Models.ViewModels;

namespace Valen_SistemaStock.Controllers
{
    [Authorize(Roles = "Administrador,Encargado")]
    public class ProveedorController : Controller
    {
        private readonly AppDbContext _context;

        public ProveedorController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Proveedor
        public async Task<IActionResult> Index()
        {
            var proveedores = await _context.Proveedores.ToListAsync();

            var listaViewModels = proveedores.Select(p => new ProveedorViewModel
            {
                id = p.id,
                Nombre = p.Nombre,
                Documento = p.Documento,
                Email = p.Email,
                Telefono = p.Telefono,
                Activo = p.Activo
            }).ToList();

            return View(listaViewModels);
        }

        // GET: Proveedor/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var proveedor = await _context.Proveedores.FirstOrDefaultAsync(p => p.id == id);
            if (proveedor == null) return NotFound();

            var model = new ProveedorViewModel
            {
                id = proveedor.id,
                Nombre = proveedor.Nombre,
                Documento = proveedor.Documento,
                Email = proveedor.Email,
                Telefono = proveedor.Telefono,
                Activo = proveedor.Activo
            };

            return View(model);
        }

        // GET: Proveedor/Create
        public IActionResult Create()
        {
            return View(new ProveedorViewModel());
        }

        // POST: Proveedor/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProveedorViewModel model)
        {
            if (ModelState.IsValid)
            {
                bool existeProveedor = await _context.Proveedores
                    .AnyAsync(p => p.Nombre.ToLower() == model.Nombre.ToLower());

                if (existeProveedor)
                {
                    ModelState.AddModelError("Nombre", "Ya existe un proveedor con este nombre.");
                    return View(model);
                }

                var nuevoProveedor = new Proveedor
                {
                    Nombre = model.Nombre,
                    Documento = model.Documento,
                    Email = model.Email,
                    Telefono = model.Telefono,
                    Activo = model.Activo
                };

                _context.Proveedores.Add(nuevoProveedor);
                await _context.SaveChangesAsync();

                TempData["Ok"] = "¡El proveedor se ha creado con éxito!";
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // GET: Proveedor/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var proveedor = await _context.Proveedores.FindAsync(id);
            if (proveedor == null) return NotFound();

            var model = new ProveedorViewModel
            {
                id = proveedor.id,
                Nombre = proveedor.Nombre,
                Documento = proveedor.Documento,
                Email = proveedor.Email,
                Telefono = proveedor.Telefono,
                Activo = proveedor.Activo
            };

            return View(model);
        }

        // POST: Proveedor/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int? id, ProveedorViewModel model)
        {
            if (id != model.id) return NotFound();

            if (ModelState.IsValid)
            {
                bool existeProveedor = await _context.Proveedores
                    .AnyAsync(p => p.Nombre.ToLower() == model.Nombre.ToLower() && p.id != model.id);

                if (existeProveedor)
                {
                    ModelState.AddModelError("Nombre", "Ya existe un proveedor con este nombre.");
                    return View(model);
                }

                var proveedor = await _context.Proveedores.FindAsync(id);
                if (proveedor == null) return NotFound();

                proveedor.Nombre = model.Nombre;
                proveedor.Documento = model.Documento;
                proveedor.Email = model.Email;
                proveedor.Telefono = model.Telefono;
                proveedor.Activo = model.Activo;
                proveedor.UpdatedAt = DateTime.UtcNow;

                _context.Proveedores.Update(proveedor);
                await _context.SaveChangesAsync();

                TempData["Ok"] = "¡El proveedor se ha modificado con éxito!";
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // GET: Proveedor/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var proveedor = await _context.Proveedores.FindAsync(id);
            if (proveedor == null) return NotFound();

            var model = new ProveedorViewModel
            {
                id = proveedor.id,
                Nombre = proveedor.Nombre,
                Documento = proveedor.Documento,
                Email = proveedor.Email,
                Telefono = proveedor.Telefono,
                Activo = proveedor.Activo
            };

            return View(model);
        }

        // POST: Proveedor/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int? id)
        {
            var proveedor = await _context.Proveedores
                .Include(p => p.Movimientos)
                .FirstOrDefaultAsync(p => p.id == id);

            if (proveedor != null)
            {
                if (proveedor.Movimientos.Any())
                {
                    TempData["Error"] = $"No se puede eliminar el proveedor '{proveedor.Nombre}' porque tiene movimientos asociados.";
                    return RedirectToAction(nameof(Index));
                }

                _context.Proveedores.Remove(proveedor);
                await _context.SaveChangesAsync();
                TempData["Ok"] = "Proveedor eliminado con éxito.";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool ProveedorExists(int? id)
        {
            return _context.Proveedores.Any(e => e.id == id);
        }
    }
}
