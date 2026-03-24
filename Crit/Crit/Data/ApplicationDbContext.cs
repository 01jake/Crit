using Crit.Data;
using Crit.Server.Data;
using Crit.Shared.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Crit.Server.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole, string>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Producto> Productos { get; set; }
        public DbSet<Venta> Ventas { get; set; }
        public DbSet<DetalleVenta> DetallesVenta { get; set; }
        public DbSet<Servicio> Servicios { get; set; }
        public DbSet<ServicioCliente> ServiciosCliente { get; set; }
        public DbSet<Cotizacion> Cotizaciones { get; set; }
        public DbSet<DetalleCotizacion> DetallesCotizacion { get; set; }
        // DbSets para las entidades
        public DbSet<ArticuloEntity> Articulos { get; set; }
        public DbSet<QuejaEntity> Quejas { get; set; }
        public DbSet<Proveedor> Proveedores { get; set; }
        //public DbSet<Kardex> Kardex { get; set; }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configuración para ArticuloEntity
            builder.Entity<ArticuloEntity>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(a => a.UsuarioQueRegistro)
                      .WithMany()
                      .HasForeignKey(a => a.UsuarioQueRegistroId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.Property(q => q.Nombre)
                   .IsRequired()
                   .HasMaxLength(100);

                entity.Property(a => a.Codigo)
                      .IsRequired()
                      .HasMaxLength(50);

                entity.Property(a => a.Descripcion)
                      .IsRequired()
                      .HasMaxLength(500);

                entity.Property(a => a.FechaRegistro)
                      .HasDefaultValueSql("GETDATE()");

                entity.HasIndex(a => a.Codigo)
                      .IsUnique();
            });

            // Configuración para QuejaEntity
            builder.Entity<QuejaEntity>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(q => q.NombreCliente)
                      .IsRequired()
                      .HasMaxLength(100);

                entity.Property(q => q.NumeroAfiliacion)
                      .HasMaxLength(50);

                entity.Property(q => q.Correo)
                      .IsRequired()
                      .HasMaxLength(256);

                entity.Property(q => q.Titulo)
                      .IsRequired()
                      .HasMaxLength(200);

                entity.Property(q => q.DescripcionQueja)
                      .IsRequired()
                      .HasMaxLength(1000);

                entity.Property(q => q.Categoria)
                      .IsRequired()
                      .HasMaxLength(100);

                entity.Property(q => q.Fecha)
                      .HasDefaultValueSql("GETDATE()");

                entity.Property(q => q.Estatus)
                      .HasDefaultValue(EstatusQueja.Pendiente);

                entity.Property(q => q.Prioridad)
                      .HasDefaultValue(PrioridadQueja.Media);

                entity.Property(q => q.ClienteId)
                      .IsRequired(false);

                entity.HasOne(q => q.Cliente)
                      .WithMany()
                      .HasForeignKey(q => q.ClienteId)
                      .OnDelete(DeleteBehavior.Restrict)
                      .IsRequired(false);

                entity.HasOne(q => q.EmpleadoAsignado)
                      .WithMany()
                      .HasForeignKey(q => q.EmpleadoAsignadoId)
                      .OnDelete(DeleteBehavior.SetNull)
                      .IsRequired(false);
            }); // ⭐ CERRAR AQUÍ QuejaEntity

            // ✅ CONFIGURACIONES DE VENTAS (FUERA de QuejaEntity)

            // Cliente
            builder.Entity<Cliente>()
                .HasIndex(c => c.Email)
                .IsUnique();

            // Producto
            builder.Entity<Producto>()
                .HasIndex(p => p.Codigo)
                .IsUnique();

            // Venta
            builder.Entity<Venta>()
                .HasOne(v => v.Cliente)
                .WithMany(c => c.Ventas)
                .HasForeignKey(v => v.ClienteId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Venta>()
                .HasIndex(v => v.NumeroVenta)
                .IsUnique();

            // DetalleVenta
            builder.Entity<DetalleVenta>()
                .HasOne(dv => dv.Venta)
                .WithMany(v => v.Detalles)
                .HasForeignKey(dv => dv.VentaId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<DetalleVenta>()
                .HasOne(dv => dv.Producto)
                .WithMany(p => p.DetallesVenta)
                .HasForeignKey(dv => dv.ProductoId)
                .OnDelete(DeleteBehavior.Restrict);

            // Servicio
            builder.Entity<Servicio>()
                .HasIndex(s => s.Codigo)
                .IsUnique();

            // ServicioCliente
            builder.Entity<ServicioCliente>()
                .HasOne(sc => sc.Servicio)
                .WithMany(s => s.ServiciosCliente)
                .HasForeignKey(sc => sc.ServicioId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ServicioCliente>()
                .HasOne(sc => sc.Cliente)
                .WithMany()
                .HasForeignKey(sc => sc.ClienteId)
                .OnDelete(DeleteBehavior.Restrict);

            // Cotizacion
            builder.Entity<Cotizacion>()
                .HasOne(c => c.Cliente)
                .WithMany(cl => cl.Cotizaciones)
                .HasForeignKey(c => c.ClienteId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Cotizacion>()
                .HasIndex(c => c.NumeroCotizacion)
                .IsUnique();

            // DetalleCotizacion
            builder.Entity<DetalleCotizacion>()
                .HasOne(dc => dc.Cotizacion)
                .WithMany(c => c.Detalles)
                .HasForeignKey(dc => dc.CotizacionId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<DetalleCotizacion>()
                .HasOne(dc => dc.Producto)
                .WithMany(p => p.DetallesCotizacion)
                .HasForeignKey(dc => dc.ProductoId)
                .OnDelete(DeleteBehavior.Restrict);
        }
        //public DbSet<Producto> Producto { get; set; } = default!;
        public DbSet<Queja> Queja { get; set; } = default!;
        public DbSet<Articulo> Articulo { get; set; } = default!;
        public DbSet<Crit.Shared.Models.Compra> Compra { get; set; } = default!;
    }
}