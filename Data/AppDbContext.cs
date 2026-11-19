using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Valen_SistemaStock.Models.Entities;

namespace Valen_SistemaStock.Data
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<ApplicationUser>(options)
    {
        public DbSet<Producto> Productos { get; set; }
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Movimiento> Movimientos { get; set; }
        public DbSet<Deposito> Depositos { get; set; }
        public DbSet<Proveedor> Proveedores { get; set; }
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<StockDeposito> StockDepositos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Esta línea configura primero los tipos de identidad integrados
            base.OnModelCreating(modelBuilder);

            // Evitar delete cascade
            modelBuilder.Entity<Producto>()
                .HasOne(m => m.Categoria) // Un Producto tiene una Categoria
                .WithMany(m => m.Productos) // Una Categoria tiene muchos Productos
                .HasForeignKey(m => m.idCategoria) // La clave foránea en Producto
                .OnDelete(DeleteBehavior.Restrict); // Evita que operaciones en cascada afecten a la categoría

            //Datos semilla de prueba para las tabla
            modelBuilder.Entity<Categoria>().HasData(
                new Categoria { id = 1, Nombre = "Alimentos", Activo = true, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), UpdatedAt = null },
                new Categoria { id = 2, Nombre = "Limpieza", Activo = true, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), UpdatedAt = null },
                new Categoria { id = 3, Nombre = "Lacteos", Activo = true, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), UpdatedAt = null }
            );

            modelBuilder.Entity<Deposito>().HasData(
                new Deposito { id = 1, Nombre = "Depósito General", Ubicacion = "Planta baja", Activo = true, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), UpdatedAt = null },
                new Deposito { id = 2, Nombre = "Depósito Principal", Ubicacion = "Planta alta", Activo = true, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), UpdatedAt = null },
                new Deposito { id = 3, Nombre = "Depósito Secundario", Ubicacion = "Planta baja", Activo = true, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), UpdatedAt = null }
            );

            modelBuilder.Entity<Proveedor>().HasData(
                new Proveedor { id = 1, Nombre = "Proveedor A", Activo = true, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), UpdatedAt = null }
            );

            modelBuilder.Entity<Cliente>().HasData(
                new Cliente { id = 1, Nombre = "Cliente A", Activo = true, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), UpdatedAt = null }
            );
        }
    }
}
