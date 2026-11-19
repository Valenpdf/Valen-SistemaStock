using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Valen_SistemaStock.Data;
using Valen_SistemaStock.Models.Entities;
using Valen_SistemaStock.Models.ViewModels;

namespace Valen_SistemaStock.Controllers
{
    [Authorize(Roles = "Administrador,Encargado")]
    public class ClienteController : Controller
    {
        private readonly AppDbContext _context;

        public ClienteController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Cliente
        public async Task<IActionResult> Index()
        {
            var clientes = await _context.Clientes.ToListAsync();

            var listaViewModels = clientes.Select(c => new ClienteViewModel
            {
                id = c.id,
                Nombre = c.Nombre,
                Documento = c.Documento,
                Email = c.Email,
                Telefono = c.Telefono,
                Activo = c.Activo
            }).ToList();

            return View(listaViewModels);
        }

        // GET: Cliente/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.id == id);
            if (cliente == null) return NotFound();

            var model = new ClienteViewModel
            {
                id = cliente.id,
                Nombre = cliente.Nombre,
                Documento = cliente.Documento,
                Email = cliente.Email,
                Telefono = cliente.Telefono,
                Activo = cliente.Activo
            };

            return View(model);
        }

        // GET: Cliente/Create
        public IActionResult Create()
        {
            return View(new ClienteViewModel());
        }

        // POST: Cliente/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ClienteViewModel model)
        {
            if (ModelState.IsValid)
            {
                bool existeCliente = await _context.Clientes
                    .AnyAsync(c => c.Nombre.ToLower() == model.Nombre.ToLower());

                if (existeCliente)
                {
                    ModelState.AddModelError("Nombre", "Ya existe un cliente con este nombre.");
                    return View(model);
                }

                var nuevoCliente = new Cliente
                {
                    Nombre = model.Nombre,
                    Documento = model.Documento,
                    Email = model.Email,
                    Telefono = model.Telefono,
                    Activo = model.Activo
                };

                _context.Clientes.Add(nuevoCliente);
                await _context.SaveChangesAsync();

                TempData["Ok"] = "¡El cliente se ha creado con éxito!";
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // GET: Cliente/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente == null) return NotFound();

            var model = new ClienteViewModel
            {
                id = cliente.id,
                Nombre = cliente.Nombre,
                Documento = cliente.Documento,
                Email = cliente.Email,
                Telefono = cliente.Telefono,
                Activo = cliente.Activo
            };

            return View(model);
        }

        // POST: Cliente/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int? id, ClienteViewModel model)
        {
            if (id != model.id) return NotFound();

            if (ModelState.IsValid)
            {
                bool existeCliente = await _context.Clientes
                    .AnyAsync(c => c.Nombre.ToLower() == model.Nombre.ToLower() && c.id != model.id);

                if (existeCliente)
                {
                    ModelState.AddModelError("Nombre", "Ya existe un cliente con este nombre.");
                    return View(model);
                }

                var cliente = await _context.Clientes.FindAsync(id);
                if (cliente == null) return NotFound();

                cliente.Nombre = model.Nombre;
                cliente.Documento = model.Documento;
                cliente.Email = model.Email;
                cliente.Telefono = model.Telefono;
                cliente.Activo = model.Activo;
                cliente.UpdatedAt = DateTime.UtcNow;

                _context.Clientes.Update(cliente);
                await _context.SaveChangesAsync();

                TempData["Ok"] = "¡El cliente se ha modificado con éxito!";
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // GET: Cliente/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente == null) return NotFound();

            var model = new ClienteViewModel
            {
                id = cliente.id,
                Nombre = cliente.Nombre,
                Documento = cliente.Documento,
                Email = cliente.Email,
                Telefono = cliente.Telefono,
                Activo = cliente.Activo
            };

            return View(model);
        }

        // POST: Cliente/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int? id)
        {
            var cliente = await _context.Clientes
                .Include(c => c.Movimientos)
                .FirstOrDefaultAsync(c => c.id == id);

            if (cliente != null)
            {
                if (cliente.Movimientos.Any())
                {
                    TempData["Error"] = $"No se puede eliminar el cliente '{cliente.Nombre}' porque tiene movimientos asociados.";
                    return RedirectToAction(nameof(Index));
                }

                _context.Clientes.Remove(cliente);
                await _context.SaveChangesAsync();
                TempData["Ok"] = "Cliente eliminado con éxito.";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool ClienteExists(int? id)
        {
            return _context.Clientes.Any(e => e.id == id);
        }
    }
}
