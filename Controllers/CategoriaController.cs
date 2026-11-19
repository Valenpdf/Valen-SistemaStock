using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Valen_SistemaStock.Data;
using Valen_SistemaStock.Models.Entities;
using Valen_SistemaStock.Models.ViewModels;

namespace Valen_SistemaStock.Controllers
{
    [Authorize(Roles = "Administrador,Encargado")]
    public class CategoriaController : Controller
    {
        private readonly AppDbContext _context;

        public CategoriaController(AppDbContext context)
        {
            _context = context;
        }

        // GET: CATEGORIAS
        public async Task<IActionResult> Index()
        {
            // Traemos las categorías de la base de datos
            var categorias = await _context.Categorias.ToListAsync();

            // Mapeo manual de Lista de Entidades -> Lista de ViewModels
            var listaViewModels = categorias.Select(c => new CategoriaViewModel
            {
                id = c.id,
                Nombre = c.Nombre,
                Descripcion = c.Descripcion,
                Activo = c.Activo
            }).ToList();

            return View(listaViewModels);
        }

        // GET: CATEGORIAS/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var categoria = await _context.Categorias.Include(c => c.Productos).FirstOrDefaultAsync(c => c.id == id);
            if (categoria == null) return NotFound();

            // Mapeo manual de Entidad -> ViewModel
            var model = new CategoriaViewModel
            {
                id = categoria.id,
                Nombre = categoria.Nombre,
                Descripcion = categoria.Descripcion,
                Activo = categoria.Activo,
                CreatedAt = categoria.CreatedAt,
                UpdatedAt = categoria.UpdatedAt,
                CantidadProductos = categoria.Productos.Count
            };

            return View(model);
        }

        // GET: CATEGORIAS/Create
        public IActionResult Create()
        {
            return View(new CategoriaViewModel());
        }

        // POST: CATEGORIAS/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategoriaViewModel model)
        {
            if (ModelState.IsValid)
            {
                // 1. Validar que el nombre no exista en la base de datos (ignorando mayúsculas/minúsculas)
                bool existeCategoria = await _context.Categorias
                    .AnyAsync(c => c.Nombre.ToLower() == model.Nombre.ToLower());

                if (existeCategoria)
                {
                    // 2. Agregar el error al ModelState para que se muestre en la vista
                    ModelState.AddModelError("Nombre", "Ya existe una categoría con este nombre.");

                    // Retorna la misma vista con el modelo para no perder los datos ingresados
                    return View(model);
                }

                // Mapeo manual de ViewModel -> Entidad (para guardar en la DB)
                var nuevaCategoria = new Categoria
                {
                    Nombre = model.Nombre,
                    Descripcion = model.Descripcion,
                    Activo = model.Activo
                };

                _context.Categorias.Add(nuevaCategoria);
                await _context.SaveChangesAsync();

                // Guardamos el mensaje en TempData antes de redirigir
                TempData["Ok"] = "¡La categoría se ha creado con éxito!";
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // GET: CATEGORIAS/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var categoria = await _context.Categorias.FindAsync(id);
            if (categoria == null) return NotFound();

            // Mapeo manual de Entidad -> ViewModel (para rellenar el formulario)
            var model = new CategoriaViewModel
            {
                id = categoria.id,
                Nombre = categoria.Nombre,
                Descripcion = categoria.Descripcion,
                Activo = categoria.Activo
            };

            return View(model);
        }

        // POST: CATEGORIAS/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int? id, CategoriaViewModel model)
        {
            if (id != model.id) return NotFound();

            if (ModelState.IsValid)
            {
                bool existeCategoria = await _context.Categorias
                    .AnyAsync(c => c.Nombre.ToLower() == model.Nombre.ToLower() && c.id != model.id);

                if (existeCategoria)
                {
                    ModelState.AddModelError("Nombre", "Ya existe una categoría con este nombre.");
                    return View(model);
                }

                var categoria = await _context.Categorias.FindAsync(id);
                if (categoria == null) return NotFound();

                // Mapeo manual de actualización: asignamos los datos modificados del ViewModel a la entidad rastreada
                categoria.Nombre = model.Nombre;
                categoria.Descripcion = model.Descripcion;
                categoria.Activo = model.Activo;
                categoria.UpdatedAt = DateTime.UtcNow;

                _context.Categorias.Update(categoria);
                await _context.SaveChangesAsync();

                // Guardamos el mensaje en TempData antes de redirigir
                TempData["Ok"] = "¡La categoría se ha modificado con éxito!";
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // GET: CATEGORIAS/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var categoria = await _context.Categorias.FindAsync(id);
            if (categoria == null) return NotFound();

            // Mapeo manual de Entidad -> ViewModel
            var model = new CategoriaViewModel
            {
                id = categoria.id,
                Nombre = categoria.Nombre,
                Descripcion = categoria.Descripcion,
                Activo = categoria.Activo
            };

            return View(model);
        }

        // POST: CATEGORIAS/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int? id)
        {
            var categoria = await _context.Categorias
                .Include(c => c.Productos)
                .FirstOrDefaultAsync(c => c.id == id);

            if (categoria != null)
            {
                // Validamos si tiene productos asociados
                if (categoria.Productos.Any())
                {
                    // Guardamos el mensaje de error en TempData
                    TempData["Error"] = $"No se puede eliminar la categoría '{categoria.Nombre}' porque tiene productos asociados.";
                    return RedirectToAction(nameof(Index));
                }

                _context.Categorias.Remove(categoria);
                await _context.SaveChangesAsync();

                // Mensaje de éxito
                TempData["Ok"] = "Categoría eliminada con éxito.";
            }
            return RedirectToAction(nameof(Index));
        }

        private bool CategoriaExists(int? id)
        {
            return _context.Categorias.Any(e => e.id == id);
        }
    }
}
