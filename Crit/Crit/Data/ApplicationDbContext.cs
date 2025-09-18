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
            builder.Entity<QuejaEntity>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(q => q.Cliente)
                      .WithMany()
                      .HasForeignKey(q => q.ClienteId)
                      .OnDelete(DeleteBehavior.Restrict); // No borrar usuario si tiene quejas

                entity.Property(q => q.NombreCliente)
                      .IsRequired()
                      .HasMaxLength(100);

                entity.Property(q => q.NumeroAfiliacion)
                      .HasMaxLength(50);

                entity.Property(q => q.Correo)
                      .IsRequired()
                      .HasMaxLength(256);

                entity.Property(q => q.DescripcionQueja)
                      .IsRequired()
                      .HasMaxLength(500);

                entity.Property(q => q.Fecha)
                      .HasDefaultValueSql("GETDATE()");

                entity.Property(q => q.Estatus)
                      .HasDefaultValue(EstatusQueja.Pendiente);
            });
        }
        public DbSet<Producto> Producto { get; set; } = default!;
        public DbSet<Queja> Queja { get; set; } = default!;
    }
}