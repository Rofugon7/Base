using BaseConLogin.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BaseConLogin.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // =========================
        // DbSets
        // =========================
        public DbSet<Proyectos> Proyectos { get; set; }
        public DbSet<ContactMessage> ContactMessages { get; set; }

        public DbSet<Tag> Tags { get; set; }
        public DbSet<ProyectoTag> ProyectoTags { get; set; }

        public DbSet<Tienda> Tiendas { get; set; }

        public DbSet<ProductoBase> ProductosBase { get; set; }
        public DbSet<ProductoSimple> ProductosSimples { get; set; }
        public DbSet<ProductoConfigurable> ProductosConfigurables { get; set; }
        public DbSet<UsuarioTienda> UsuariosTiendas { get; set; }

        // =========================
        // Model configuration
        // =========================
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ProductoBase>()
                .Property(p => p.PrecioBase)
                .HasPrecision(18, 2);

            // -------------------------
            // Proyecto <-> Tag (N:N)
            // -------------------------
            modelBuilder.Entity<ProyectoTag>()
                .HasKey(pt => new { pt.ProyectoId, pt.TagId });

            modelBuilder.Entity<ProyectoTag>()
                .HasOne(pt => pt.Proyecto)
                .WithMany(p => p.ProyectoTags)
                .HasForeignKey(pt => pt.ProyectoId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ProyectoTag>()
                .HasOne(pt => pt.Tag)
                .WithMany(t => t.ProyectoTags)
                .HasForeignKey(pt => pt.TagId)
                .OnDelete(DeleteBehavior.Cascade);

            // -------------------------
            // Tienda -> ProductoBase (1:N)
            // -------------------------
            modelBuilder.Entity<ProductoBase>()
                .HasOne(p => p.Tienda)
                .WithMany(t => t.Productos)
                .HasForeignKey(p => p.TiendaId)
                .OnDelete(DeleteBehavior.Restrict);

            // -------------------------
            // ProductoBase <-> Proyectos (1:1)
            // -------------------------
            modelBuilder.Entity<Proyectos>()
                .HasOne(p => p.Producto)
                .WithOne()
                .HasForeignKey<Proyectos>(p => p.ProductoBaseId)
                .OnDelete(DeleteBehavior.Cascade);

            // -------------------------
            // ProductoBase <-> ProductoSimple (1:1)
            // -------------------------
            modelBuilder.Entity<ProductoSimple>()
                .HasOne(p => p.Producto)
                .WithOne()
                .HasForeignKey<ProductoSimple>(p => p.ProductoBaseId)
                .OnDelete(DeleteBehavior.Cascade);

            // -------------------------
            // ProductoBase <-> ProductoConfigurable (1:1)
            // -------------------------
            modelBuilder.Entity<ProductoConfigurable>()
                .HasOne(p => p.Producto)
                .WithOne()
                .HasForeignKey<ProductoConfigurable>(p => p.ProductoBaseId)
                .OnDelete(DeleteBehavior.Cascade);

            // -------------------------
            // Usuario ↔ Tienda
            // -------------------------

            modelBuilder.Entity<UsuarioTienda>()
                .HasKey(ut => new { ut.UserId, ut.TiendaId });

            modelBuilder.Entity<UsuarioTienda>()
                .HasOne(ut => ut.Tienda)
                .WithMany(t => t.UsuariosTiendas)
                .HasForeignKey(ut => ut.TiendaId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UsuarioTienda>()
                .HasOne(ut => ut.User)
                .WithMany()
                .HasForeignKey(ut => ut.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Tienda>()
        .HasQueryFilter(t => t.Activa);
        }
    }
}