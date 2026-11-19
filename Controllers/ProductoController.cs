using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Valen_SistemaStock.Data;
using Valen_SistemaStock.Models.Entities;
using Valen_SistemaStock.Models.ViewModels;

namespace Valen_SistemaStock.Controllers
{
    [Authorize(Roles = "Administrador,Encargado")]
    public class ProductoController : Controller
    {
        private readonly AppDbContext _context;

        public ProductoController(AppDbContext context)
        {
            _context = context;
        }

        // GET: PRODUCTOS
        public async Task<IActionResult> Index(string? buscar, int? buscarCategoria)
        {
            var productos = _context.Productos
                .Include(p => p.Categoria)
                .AsQueryable();

            if (!string.IsNullOrEmpty(buscar))
                productos = productos.Where(p => p.Nombre.Contains(buscar) || p.Codigo!.Contains(buscar));

            if (buscarCategoria.HasValue)
                productos = productos.Where(p => p.idCategoria == buscarCategoria.Value);

            ViewBag.Categorias = new SelectList(await _context.Categorias.ToListAsync(), "id", "Nombre");

            var listaProductos = await productos.ToListAsync();

            // Mapeo manual de Lista de Entidades -> Lista de ViewModels
            var listaViewModels = listaProductos.Select(p => new ProductoViewModel
            {
                id = p.id,
                Codigo = p.Codigo,
                Nombre = p.Nombre,
                Descripcion = p.Descripcion,
                PrecioCosto = p.PrecioCosto,
                PrecioVenta = p.PrecioVenta,
                StockMinimo = p.StockMinimo,
                CategoriaNombre = p.Categoria.Nombre,
                UnidadDeMedida = p.UnidadDeMedida,
                Activo = p.Activo
            }).ToList();

            return View(listaViewModels);
        }

        // GET: PRODUCTOS/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var producto = await _context.Productos.Include(p => p.Categoria)
                .FirstOrDefaultAsync(m => m.id == id);
            if (producto == null)
            {
                return NotFound();
            }

            // Mapeo manual de Entidad -> ViewModel
            var model = new ProductoViewModel
            {
                id = producto.id,
                Codigo = producto.Codigo,
                Nombre = producto.Nombre,
                Descripcion = producto.Descripcion,
                PrecioCosto = producto.PrecioCosto,
                PrecioVenta = producto.PrecioVenta,
                StockMinimo = producto.StockMinimo,
                CategoriaNombre = producto.Categoria.Nombre,
                UnidadDeMedida = producto.UnidadDeMedida,
                Activo = producto.Activo,
                CreatedAt = producto.CreatedAt,
                UpdatedAt = producto.UpdatedAt
            };

            return View(model);
        }

        // GET: PRODUCTOS/Create
        public async Task<IActionResult> Create()
        {
            var model = new ProductoViewModel
            {
                Categorias = new SelectList(await _context.Categorias.Where(c => c.Activo).ToListAsync(), "id", "Nombre").ToList()
            };
            return View(model);
        }

        // POST: PRODUCTOS/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductoViewModel model)
        {
            if (ModelState.IsValid)
            {
                // 1. Validar que no exista en la base de datos (ignorando mayúsculas/minúsculas)
                bool existeProducto = await _context.Productos
                    .AnyAsync(c => (c.Codigo != null && model.Codigo != null && c.Codigo.ToLower() == model.Codigo.ToLower()) ||
                                   c.Nombre.ToLower() == model.Nombre.ToLower());

                if (existeProducto)
                {
                    // 2. Agregar el error al ModelState para que se muestre en la vista
                    ModelState.AddModelError("Nombre", "Ya existe un producto con ese código o nombre.");

                    // Retorna la misma vista con el modelo para no perder los datos ingresados
                    model.Categorias = new SelectList(await _context.Categorias.Where(c => c.Activo).ToListAsync(), "id", "Nombre").ToList();
                    return View(model);
                }

                var producto = new Producto
                {
                    Codigo = model.Codigo,
                    Nombre = model.Nombre,
                    Descripcion = model.Descripcion,
                    PrecioCosto = model.PrecioCosto,
                    PrecioVenta = model.PrecioVenta,
                    StockMinimo = model.StockMinimo,
                    idCategoria = model.idCategoria!.Value,
                    UnidadDeMedida = model.UnidadDeMedida,
                    Activo = model.Activo
                };
                _context.Add(producto);
                await _context.SaveChangesAsync();
                TempData["Ok"] = "Producto creado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            model.Categorias = new SelectList(await _context.Categorias.Where(c => c.Activo).ToListAsync(), "id", "Nombre").ToList();
            return View(model);
        }

        // GET: PRODUCTOS/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var producto = await _context.Productos.FindAsync(id);
            if (producto == null)
            {
                return NotFound();
            }

            // Mapeo manual de Entidad -> ViewModel (para rellenar el formulario)
            var model = new ProductoViewModel
            {
                id = producto.id,
                Codigo = producto.Codigo,
                Nombre = producto.Nombre,
                Descripcion = producto.Descripcion,
                PrecioCosto = producto.PrecioCosto,
                PrecioVenta = producto.PrecioVenta,
                StockMinimo = producto.StockMinimo,
                idCategoria = producto.idCategoria,
                UnidadDeMedida = producto.UnidadDeMedida,
                Activo = producto.Activo
            };

            model.Categorias = new SelectList(await _context.Categorias.Where(c => c.Activo).ToListAsync(), "id", "Nombre", producto.idCategoria).ToList();
            return View(model);
        }

        // POST: PRODUCTOS/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int? id, ProductoViewModel model)
        {
            if (id != model.id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                bool existeProducto = await _context.Productos
                    .AnyAsync(c => c.id != model.id &&
                                  ((c.Codigo != null && model.Codigo != null && c.Codigo.ToLower() == model.Codigo.ToLower()) ||
                                   c.Nombre.ToLower() == model.Nombre.ToLower()));

                if (existeProducto)
                {
                    ModelState.AddModelError("Nombre", "Ya existe otro producto con ese código o nombre.");
                    model.Categorias = new SelectList(await _context.Categorias.Where(c => c.Activo).ToListAsync(), "id", "Nombre", model.idCategoria).ToList();
                    return View(model);
                }

                try
                {
                    var producto = await _context.Productos.FindAsync(id);
                    if (producto == null) return NotFound();

                    // Actualizar las propiedades del producto con los valores del modelo
                    producto.Codigo = model.Codigo;
                    producto.Nombre = model.Nombre;
                    producto.Descripcion = model.Descripcion;
                    producto.PrecioCosto = model.PrecioCosto;
                    producto.PrecioVenta = model.PrecioVenta;
                    producto.StockMinimo = model.StockMinimo;
                    producto.idCategoria = model.idCategoria!.Value;
                    producto.UnidadDeMedida = model.UnidadDeMedida;
                    producto.Activo = model.Activo;
                    producto.UpdatedAt = DateTime.UtcNow;

                    _context.Update(producto);
                    await _context.SaveChangesAsync();
                    TempData["Ok"] = "Producto actualizado correctamente.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductoExists(model.id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            model.Categorias = new SelectList(await _context.Categorias.Where(c => c.Activo).ToListAsync(), "id", "Nombre", model.idCategoria).ToList();
            return View(model);
        }

        // GET: PRODUCTOS/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var producto = await _context.Productos
                .Include(p => p.Categoria)
                .FirstOrDefaultAsync(m => m.id == id);
            if (producto == null)
            {
                return NotFound();
            }

            // Mapeo manual de Entidad -> ViewModel
            var model = new ProductoViewModel
            {
                id = producto.id,
                Codigo = producto.Codigo,
                Nombre = producto.Nombre,
                Descripcion = producto.Descripcion,
                PrecioCosto = producto.PrecioCosto,
                PrecioVenta = producto.PrecioVenta,
                StockMinimo = producto.StockMinimo,
                idCategoria = producto.idCategoria,
                CategoriaNombre = producto.Categoria?.Nombre,
                UnidadDeMedida = producto.UnidadDeMedida,
                Activo = producto.Activo
            };

            return View(model);
        }

        // POST: PRODUCTOS/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int? id)
        {
            var producto = await _context.Productos
                .Include(p => p.Movimientos)
                .Include(p => p.Stocks)
                .FirstOrDefaultAsync(p => p.id == id);

            if (producto != null)
            {
                // Validamos si no tiene movimientos o stock asociados
                if (producto.Movimientos.Any() || producto.Stocks.Any())
                {
                    // Guardamos el mensaje de error en TempData
                    TempData["Error"] = $"No se puede eliminar el producto '{producto.Nombre}' porque tiene movimientos o stock asociados.";
                    return RedirectToAction(nameof(Index));
                }

                _context.Productos.Remove(producto);
                await _context.SaveChangesAsync();

                // Mensaje de éxito
                TempData["Ok"] = "Producto eliminado con éxito.";
            }
            return RedirectToAction(nameof(Index));
        }

        private bool ProductoExists(int? id)
        {
            return _context.Productos.Any(e => e.id == id);
        }
    }
}
