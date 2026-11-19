using Microsoft.AspNetCore.Identity;
using Valen_SistemaStock.Models.Entities;

namespace Valen_SistemaStock.Data
{
    public class SeedRoles
    {
        public static async Task InicializarAsync(IServiceProvider services)
        {
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

            // Crear roles si no existen
            string[] roles = { "Administrador", "Encargado", "Operador", "Consulta" };

            foreach (var rol in roles)
                if (!await roleManager.RoleExistsAsync(rol))
                    await roleManager.CreateAsync(new IdentityRole(rol));

            // Crear usuario administrador por defecto
            const string adminEmail = "admin@sistemastock.com";
            if (await userManager.FindByEmailAsync(adminEmail) == null)
            {
                var admin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    Nombre = "Administrador del Sistema",
                    FechaRegistro = DateTime.UtcNow,
                };
                var result = await userManager.CreateAsync(admin, "Admin@12345");
                if (result.Succeeded)
                    await userManager.AddToRoleAsync(admin, "Administrador");
            }
        }
    }
}
