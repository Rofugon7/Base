using BaseConLogin.Models;
using BaseConLogin.Models.interfaces;
using BaseConLogin.Services.Tiendas;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace BaseConLogin.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
       

        private readonly ITiendaContext _tiendaContext;
        public ITiendaContext TiendaContext => _tiendaContext;

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, ITiendaContext tiendaContext)
       : base(options)
        {
            _tiendaContext = tiendaContext ?? throw new ArgumentNullException(nameof(tiendaContext));
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

        public DbSet<ProductoSimple> ProductoSimples { get; set; } = null!;

        public DbSet<ProductoConfigurable> ProductosConfigurables { get; set; }
        public DbSet<UsuarioTienda> UsuariosTiendas { get; set; }

        public DbSet<CarritoPersistente> Carritos { get; set; }
        public DbSet<CarritoPersistenteItem> CarritoItems { get; set; }

        public DbSet<Pedido> Pedidos { get; set; }
        public DbSet<PedidoItem> PedidoItems { get; set; }


        // =========================
        // Model configuration
        // =========================
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (typeof(ITenantEntity).IsAssignableFrom(entityType.ClrType))
                {
                    var parameter = Expression.Parameter(entityType.ClrType, "e");
                    var tiendaContext = Expression.Constant(this.TiendaContext, typeof(ITiendaContext));
                    var body = Expression.Equal(
                        Expression.Property(parameter, nameof(ITenantEntity.TiendaId)),
                        Expression.Property(tiendaContext, nameof(ITiendaContext.TiendaId))
                    );

                    var lambda = Expression.Lambda(body, parameter);
                    modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
                }
            }

            // -------------------------
            // Configuración de precios
            // -------------------------
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
            // Usuario ↔ Tienda (N:N)
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

            // -------------------------
            // Carrito Persistente <-> Items
            // -------------------------
            modelBuilder.Entity<CarritoPersistente>()
                .HasMany(c => c.Items)
                .WithOne(i => i.CarritoPersistente)
                .HasForeignKey(i => i.CarritoPersistenteId)
                .OnDelete(DeleteBehavior.Cascade);

            // -------------------------
            // Filtros globales multi-tienda
            // -------------------------

            modelBuilder.Entity<Tienda>()
                .HasQueryFilter(t => t.Activa);

            modelBuilder.Entity<ProductoBase>()
                .HasQueryFilter(p => p.Tienda.Activa);

            modelBuilder.Entity<CarritoPersistente>()
                .HasQueryFilter(c => c.Tienda.Activa);

            modelBuilder.Entity<UsuarioTienda>()
                .HasQueryFilter(u => u.Tienda.Activa);

            modelBuilder.Entity<ProductoSimple>()
        .ToTable("productosSimples");
        }
    }
}
