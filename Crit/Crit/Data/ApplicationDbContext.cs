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
        public DbSet<ArticuloEntity> Articulos { get; set; }
        public DbSet<QuejaEntity> Quejas { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configuraci?n para ProductoEntity
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

                // Índice único para el código
                entity.HasIndex(a => a.Codigo)
                      .IsUnique();
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
                entity.Property(q => q.ClienteId)
                      .IsRequired(false); // Permitir null

                entity.HasOne(q => q.Cliente)
                      .WithMany()
                      .HasForeignKey(q => q.ClienteId)
                      .OnDelete(DeleteBehavior.Restrict)
                      .IsRequired(false);
            });
        }
        //public DbSet<Producto> Producto { get; set; } = default!;
        public DbSet<Queja> Queja { get; set; } = default!;
        public DbSet<Articulo> Articulo { get; set; } = default!;
    }
}