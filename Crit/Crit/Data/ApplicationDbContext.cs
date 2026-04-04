using Crit.Data;
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

        // Core
        public DbSet<Empresa> Empresas { get; set; }
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Proveedor> Proveedores { get; set; }
        public DbSet<Producto> Productos { get; set; }

        // Ventas / Compras
        public DbSet<Venta> Ventas { get; set; }
        public DbSet<DetalleVenta> DetallesVenta { get; set; }
        public DbSet<Compra> Compra { get; set; }

        // Servicios / Cotizaciones
        public DbSet<Servicio> Servicios { get; set; }
        public DbSet<ServicioCliente> ServiciosCliente { get; set; }
        public DbSet<Cotizacion> Cotizaciones { get; set; }
        public DbSet<DetalleCotizacion> DetallesCotizacion { get; set; }

        // Soporte / Artículos
        public DbSet<ArticuloEntity> Articulos { get; set; }
        public DbSet<QuejaEntity> Quejas { get; set; }
        public DbSet<Articulo> Articulo { get; set; } = default!;
        public DbSet<Queja> Queja { get; set; } = default!;

        // CxC / CxP
        public DbSet<CuentaPorCobrar> CuentasPorCobrar { get; set; }
        public DbSet<CuentaPorPagar> CuentasPorPagar { get; set; }
        public DbSet<PagoCliente> PagosCliente { get; set; }
        public DbSet<PagoProveedor> PagosProveedor { get; set; }

        // Caja / Gastos
        public DbSet<CajaSesion> CajaSesiones { get; set; }
        public DbSet<CajaMovimiento> CajaMovimientos { get; set; }
        public DbSet<Gasto> Gastos { get; set; }

        // Inventario
        public DbSet<Almacen> Almacenes { get; set; }
        public DbSet<InventarioPorAlmacen> InventarioPorAlmacen { get; set; }
        public DbSet<MovimientoInventario> MovimientosInventario { get; set; }
        public DbSet<TraspasoAlmacen> TraspasosAlmacen { get; set; }
        public DbSet<OrdenReabastecimiento> OrdenesReabastecimiento { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            ConfigureIdentity(builder);
            ConfigureArticulos(builder);
            ConfigureQuejas(builder);
            ConfigureVentas(builder);
            ConfigureServiciosYCotizaciones(builder);
            ConfigureFinanzas(builder);
            ConfigureCajaYGastos(builder);
            ConfigureInventario(builder);
            ConfigurePrecision(builder);
        }

        private static void ConfigureIdentity(ModelBuilder builder)
        {
            builder.Entity<ApplicationUser>()
                .HasOne(u => u.Empresa)
                .WithMany()
                .HasForeignKey(u => u.EmpresaId)
                .OnDelete(DeleteBehavior.NoAction);
        }

        private static void ConfigureArticulos(ModelBuilder builder)
        {
            builder.Entity<ArticuloEntity>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(a => a.UsuarioQueRegistro)
                    .WithMany()
                    .HasForeignKey(a => a.UsuarioQueRegistroId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.Property(a => a.Nombre)
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
        }

        private static void ConfigureQuejas(ModelBuilder builder)
        {
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
            });
        }

        private static void ConfigureVentas(ModelBuilder builder)
        {
            builder.Entity<Cliente>()
                .HasIndex(c => new { c.Email, c.EmpresaId })
                .IsUnique();

            builder.Entity<Proveedor>()
                .HasIndex(p => new { p.Email, p.EmpresaId })
                .IsUnique();

            builder.Entity<Producto>()
                .HasIndex(p => new { p.Codigo, p.EmpresaId })
                .IsUnique();

            builder.Entity<Venta>()
                .HasOne(v => v.Cliente)
                .WithMany(c => c.Ventas)
                .HasForeignKey(v => v.ClienteId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Venta>()
                .HasOne(v => v.Almacen)
                .WithMany()
                .HasForeignKey(v => v.AlmacenId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Venta>()
                .HasIndex(v => v.NumeroVenta)
                .IsUnique();

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

            builder.Entity<Compra>()
                .HasOne(c => c.Almacen)
                .WithMany()
                .HasForeignKey(c => c.AlmacenId)
                .OnDelete(DeleteBehavior.NoAction);
        }

        private static void ConfigureServiciosYCotizaciones(ModelBuilder builder)
        {
            builder.Entity<Servicio>()
                .HasIndex(s => s.Codigo)
                .IsUnique();

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

            builder.Entity<Cotizacion>()
                .HasOne(c => c.Cliente)
                .WithMany(cl => cl.Cotizaciones)
                .HasForeignKey(c => c.ClienteId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Cotizacion>()
                .HasIndex(c => c.NumeroCotizacion)
                .IsUnique();

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

        private static void ConfigureFinanzas(ModelBuilder builder)
        {
            builder.Entity<CuentaPorCobrar>()
                .HasOne(c => c.Cliente)
                .WithMany(c => c.CuentasPorCobrar)
                .HasForeignKey(c => c.ClienteId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<CuentaPorCobrar>()
                .HasOne(c => c.Venta)
                .WithOne(v => v.CuentaPorCobrar)
                .HasForeignKey<CuentaPorCobrar>(c => c.VentaId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<PagoCliente>()
                .HasOne(p => p.CuentaPorCobrar)
                .WithMany(c => c.Pagos)
                .HasForeignKey(p => p.CuentaPorCobrarId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<CuentaPorPagar>()
                .HasOne(c => c.Proveedor)
                .WithMany(p => p.CuentasPorPagar)
                .HasForeignKey(c => c.ProveedorId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<CuentaPorPagar>()
                .HasOne(c => c.Compra)
                .WithOne(cmp => cmp.CuentaPorPagar)
                .HasForeignKey<CuentaPorPagar>(c => c.CompraId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<PagoProveedor>()
                .HasOne(p => p.CuentaPorPagar)
                .WithMany(c => c.Pagos)
                .HasForeignKey(p => p.CuentaPorPagarId)
                .OnDelete(DeleteBehavior.NoAction);
        }

        private static void ConfigureCajaYGastos(ModelBuilder builder)
        {
            builder.Entity<CajaMovimiento>()
                .HasOne(x => x.CajaSesion)
                .WithMany(x => x.Movimientos)
                .HasForeignKey(x => x.CajaSesionId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Gasto>()
                .HasOne(x => x.CajaSesion)
                .WithMany()
                .HasForeignKey(x => x.CajaSesionId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Gasto>()
                .HasOne(x => x.Proveedor)
                .WithMany()
                .HasForeignKey(x => x.ProveedorId)
                .OnDelete(DeleteBehavior.NoAction);
        }

        private static void ConfigureInventario(ModelBuilder builder)
        {
            builder.Entity<InventarioPorAlmacen>()
                .HasIndex(x => new { x.ProductoId, x.AlmacenId })
                .IsUnique();

            builder.Entity<InventarioPorAlmacen>()
                .HasOne(x => x.Producto)
                .WithMany()
                .HasForeignKey(x => x.ProductoId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<InventarioPorAlmacen>()
                .HasOne(x => x.Almacen)
                .WithMany()
                .HasForeignKey(x => x.AlmacenId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<TraspasoAlmacen>()
                .HasOne(x => x.AlmacenOrigen)
                .WithMany()
                .HasForeignKey(x => x.AlmacenOrigenId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<TraspasoAlmacen>()
                .HasOne(x => x.AlmacenDestino)
                .WithMany()
                .HasForeignKey(x => x.AlmacenDestinoId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<TraspasoAlmacen>()
                .HasOne(x => x.Producto)
                .WithMany()
                .HasForeignKey(x => x.ProductoId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<OrdenReabastecimiento>()
                .HasOne(x => x.Producto)
                .WithMany()
                .HasForeignKey(x => x.ProductoId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<OrdenReabastecimiento>()
                .HasOne(x => x.Almacen)
                .WithMany()
                .HasForeignKey(x => x.AlmacenId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<OrdenReabastecimiento>()
                .HasOne(x => x.Compra)
                .WithMany()
                .HasForeignKey(x => x.CompraId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<OrdenReabastecimiento>()
                .HasOne(x => x.TraspasoAlmacen)
                .WithMany()
                .HasForeignKey(x => x.TraspasoAlmacenId)
                .OnDelete(DeleteBehavior.NoAction);
        }

        private static void ConfigurePrecision(ModelBuilder builder)
        {
            builder.Entity<CuentaPorCobrar>().Property(x => x.Subtotal).HasColumnType("decimal(18,2)");
            builder.Entity<CuentaPorCobrar>().Property(x => x.Descuento).HasColumnType("decimal(18,2)");
            builder.Entity<CuentaPorCobrar>().Property(x => x.IVA).HasColumnType("decimal(18,2)");
            builder.Entity<CuentaPorCobrar>().Property(x => x.Total).HasColumnType("decimal(18,2)");
            builder.Entity<CuentaPorCobrar>().Property(x => x.TotalPagado).HasColumnType("decimal(18,2)");

            builder.Entity<CuentaPorPagar>().Property(x => x.Subtotal).HasColumnType("decimal(18,2)");
            builder.Entity<CuentaPorPagar>().Property(x => x.Descuento).HasColumnType("decimal(18,2)");
            builder.Entity<CuentaPorPagar>().Property(x => x.IVA).HasColumnType("decimal(18,2)");
            builder.Entity<CuentaPorPagar>().Property(x => x.Total).HasColumnType("decimal(18,2)");
            builder.Entity<CuentaPorPagar>().Property(x => x.TotalPagado).HasColumnType("decimal(18,2)");

            builder.Entity<PagoCliente>().Property(x => x.Monto).HasColumnType("decimal(18,2)");
            builder.Entity<PagoCliente>().Property(x => x.SaldoAnterior).HasColumnType("decimal(18,2)");
            builder.Entity<PagoCliente>().Property(x => x.SaldoPosterior).HasColumnType("decimal(18,2)");

            builder.Entity<PagoProveedor>().Property(x => x.Monto).HasColumnType("decimal(18,2)");
            builder.Entity<PagoProveedor>().Property(x => x.SaldoAnterior).HasColumnType("decimal(18,2)");
            builder.Entity<PagoProveedor>().Property(x => x.SaldoPosterior).HasColumnType("decimal(18,2)");

            builder.Entity<CajaSesion>().Property(x => x.MontoInicial).HasColumnType("decimal(18,2)");
            builder.Entity<CajaSesion>().Property(x => x.MontoFinal).HasColumnType("decimal(18,2)");
            builder.Entity<CajaSesion>().Property(x => x.TotalIngresos).HasColumnType("decimal(18,2)");
            builder.Entity<CajaSesion>().Property(x => x.TotalEgresos).HasColumnType("decimal(18,2)");

            builder.Entity<CajaMovimiento>().Property(x => x.Monto).HasColumnType("decimal(18,2)");
            builder.Entity<CajaMovimiento>().Property(x => x.SaldoAnterior).HasColumnType("decimal(18,2)");
            builder.Entity<CajaMovimiento>().Property(x => x.SaldoPosterior).HasColumnType("decimal(18,2)");

            builder.Entity<Gasto>().Property(x => x.Monto).HasColumnType("decimal(18,2)");

            builder.Entity<InventarioPorAlmacen>().Property(x => x.Stock).HasColumnType("decimal(18,2)");
            builder.Entity<InventarioPorAlmacen>().Property(x => x.StockMinimo).HasColumnType("decimal(18,2)");
            builder.Entity<InventarioPorAlmacen>().Property(x => x.StockMaximo).HasColumnType("decimal(18,2)");

            builder.Entity<TraspasoAlmacen>().Property(x => x.Cantidad).HasColumnType("decimal(18,2)");

            builder.Entity<OrdenReabastecimiento>().Property(x => x.StockActual).HasColumnType("decimal(18,2)");
            builder.Entity<OrdenReabastecimiento>().Property(x => x.StockMinimo).HasColumnType("decimal(18,2)");
            builder.Entity<OrdenReabastecimiento>().Property(x => x.CantidadSugerida).HasColumnType("decimal(18,2)");
        }
    }
}
