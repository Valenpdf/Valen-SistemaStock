using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Valen_SistemaStock.Models.Entities;
using Valen_SistemaStock.Models.ViewModels.Usuario;

namespace Valen_SistemaStock.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class UsuarioController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UsuarioController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }

        // ---------- LISTADO ----------
        public async Task<IActionResult> Index()
        {
            var usuarios = await _userManager.Users
                .OrderBy(u => u.Nombre)
                .ToListAsync();

            var lista = new List<UsuarioViewModel>();
            foreach (var u in usuarios)
            {
                var roles = await _userManager.GetRolesAsync(u);
                lista.Add(new UsuarioViewModel
                {
                    Id = u.Id,
                    Nombre = u.Nombre,
                    Email = u.Email ?? "",
                    Rol = roles.FirstOrDefault() ?? "Sin rol",
                    Activo = u.Activo
                });
            }

            return View(lista);
        }

        // ---------- CREAR ----------
        public async Task<IActionResult> Crear()
        {
            ViewBag.Roles = await CargarRoles();
            return View(new CrearViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(CrearViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    Nombre = model.Nombre,
                    EmailConfirmed = true,
                    Activo = true
                };

                // 1. Intentar crear el usuario primero
                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    // 2. Asignar el rol SOLO si el usuario se creó correctamente
                    if (!string.IsNullOrEmpty(model.Rol))
                    {
                        var roleResult = await _userManager.AddToRoleAsync(user, model.Rol);
                        if (!roleResult.Succeeded)
                        {
                            foreach (var error in roleResult.Errors)
                            {
                                ModelState.AddModelError(string.Empty, error.Description);
                            }
                            ViewBag.Roles = await CargarRoles(model.Rol);
                            return View(model);
                        }
                    }

                    // 3. Redirigir
                    TempData["Ok"] = "Usuario creado exitosamente.";
                    return RedirectToAction(nameof(Index));
                }

                // 4. Si la creación falló, agregar los errores al ModelState para mostrarlos en la vista
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // Si el ModelState no es válido o hubo errores, recargar los roles para que la vista no falle
            ViewBag.Roles = await CargarRoles(model.Rol);
            return View(model);
        }

        private async Task<SelectList> CargarRoles(string? rolSeleccionado = null)
        {
            var roles = await _roleManager.Roles.OrderBy(r => r.Name).ToListAsync();
            return new SelectList(roles, "Name", "Name", rolSeleccionado);
        }

        // ---------- EDITAR ----------
        [HttpGet]
        public async Task<IActionResult> Editar(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var rol = await _userManager.GetRolesAsync(user);
            ViewBag.Roles = await CargarRoles(rol.FirstOrDefault());

            return View(new EditarViewModel
            {
                Id = user.Id,
                Nombre = user.Nombre,
                Email = user.Email ?? "",
                Rol = rol.FirstOrDefault() ?? "",
                Activo = user.Activo
            });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(EditarViewModel model)
        {
            if (ModelState.IsValid)
            {
                // 1. Busco el usuario en la base de datos
                var user = await _userManager.FindByIdAsync(model.Id);
                if (user == null) return NotFound();

                // 2. Validar que no se desactive a sí mismo
                if (user.Id == _userManager.GetUserId(User) && !model.Activo)
                {
                    ModelState.AddModelError("", "No puedes desactivar tu propio usuario.");
                    ViewBag.Roles = await CargarRoles(model.Rol);
                    return View(model);
                }

                // 3. Actualizar datos básicos del usuario
                user.Nombre = model.Nombre;
                user.Email = model.Email;
                user.UserName = model.Email;
                user.Activo = model.Activo;

                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    foreach (var e in result.Errors)
                        ModelState.AddModelError("", e.Description);
                    ViewBag.Roles = await CargarRoles(model.Rol);
                    return View(model);
                }

                // 4. Actualizar el rol del usuario
                var rolActual = await _userManager.GetRolesAsync(user);
                var removeResult = await _userManager.RemoveFromRolesAsync(user, rolActual);
                if (!removeResult.Succeeded)
                {
                    ModelState.AddModelError("", "Error al remover el rol anterior.");
                    ViewBag.Roles = await CargarRoles(model.Rol);
                    return View(model);
                }

                var addResult = await _userManager.AddToRoleAsync(user, model.Rol);
                if (!addResult.Succeeded)
                {
                    ModelState.AddModelError("", "Error al asignar el nuevo rol.");
                    ViewBag.Roles = await CargarRoles(model.Rol);
                    return View(model);
                }

                TempData["Ok"] = "Usuario actualizado.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Roles = await CargarRoles(model.Rol);
            return View(model);
        }

        // ---------- RESETEAR PASSWORD ----------
        [HttpGet]
        public async Task<IActionResult> ResetearPassword(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            return View(new ResetearPasswordViewModel { Id = user.Id, Email = user.Email ?? "" });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetearPassword(ResetearPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null) return NotFound();

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, model.NuevaPassword);

            if (!result.Succeeded)
            {
                foreach (var e in result.Errors)
                    ModelState.AddModelError("", e.Description);
                return View(model);
            }

            TempData["Ok"] = "Contraseña actualizada.";
            return RedirectToAction(nameof(Index));
        }
    }
}
