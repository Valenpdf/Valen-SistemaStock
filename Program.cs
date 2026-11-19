using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Valen_SistemaStock.Data;
using Valen_SistemaStock.Infrastructure;
using Valen_SistemaStock.Mappings;
using Valen_SistemaStock.Models.Entities;

var builder = WebApplication.CreateBuilder(args);

// Configurar DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 1. Configurar ASP.NET Core Identity (Etapa 6)
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddErrorDescriber<EspanolIdentityErrorDescriber>()
.AddDefaultTokenProviders();

builder.Services.AddControllersWithViews();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Cuenta/Login";
    options.AccessDeniedPath = "/Cuenta/AccesoDenegado";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
});

builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddProfile<MappingProfile>();
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

// 2. Habilitar la autenticación y autorización (Etapa 6)
app.UseAuthentication(); // <- OBLIGATORIO: Identifica quién es el usuario
app.UseAuthorization();  // <- OBLIGATORIO: Revisa los permisos del usuario

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}")
    .WithStaticAssets();

// Inicialización de roles y usuarios iniciales (Etapa 6)
using (var scope = app.Services.CreateScope())
{
    await SeedRoles.InicializarAsync(scope.ServiceProvider);
}

app.Run();
