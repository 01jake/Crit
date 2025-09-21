using Crit.Data;
using Crit.Server.Data;
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

        // DbSets para las entidades
        public DbSet<ProductoEntity> Productos { get; set; }
        public DbSet<QuejaEntity> Quejas { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configuraci?n para ProductoEntity
            builder.Entity<ProductoEntity>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(p => p.UsuarioQueRegistro)
                      .WithMany()
                      .HasForeignKey(p => p.UsuarioQueRegistroId)
                      .OnDelete(DeleteBehavior.Restrict); // No borrar usuario si tiene productos

                entity.Property(p => p.Nombre)
                      .IsRequired()
                      .HasMaxLength(100);

                entity.Property(p => p.Categoria)
                      .IsRequired()
                      .HasMaxLength(50);

                entity.Property(p => p.FechaIngreso)
                      .HasDefaultValueSql("GETDATE()");

                entity.Property(p => p.Cantidad)
                      .IsRequired();
            });

            // Configuraci?n para QuejaEntity
            // Configuración para QuejaEntity (actualizar la existente)
            builder.Entity<QuejaEntity>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(q => q.Cliente)
                      .WithMany()
                      .HasForeignKey(q => q.ClienteId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.Property(q => q.NombreCliente)
                      .IsRequired()
                      .HasMaxLength(100);

                entity.Property(q => q.NumeroAfiliacion)
                      .HasMaxLength(50);

                entity.Property(q => q.Correo)
                      .IsRequired()
                      .HasMaxLength(256);

                // ✅ NUEVOS CAMPOS
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
            });
        }
        public DbSet<Producto> Producto { get; set; } = default!;
        public DbSet<Queja> Queja { get; set; } = default!;
    }
}