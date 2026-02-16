using BaseConLogin.Models;
using BaseConLogin.Models.interfaces;
using BaseConLogin.Services.Tiendas;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace BaseConLogin.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
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

        public DbSet<UsuarioTienda> UsuariosTiendas { get; set; }

        public DbSet<CarritoPersistente> Carritos { get; set; }
        public DbSet<CarritoPersistenteItem> CarritoItems { get; set; }
        public DbSet<Categoria> Categorias { get; set; } = null!;
        public DbSet<Pedido> Pedidos { get; set; }
        public DbSet<PedidoItem> PedidoItems { get; set; }
        public DbSet<ProductoImagen> ProductoImagenes { get; set; }
        public DbSet<Suscriptor> Suscriptores { get; set; }
        public DbSet<ConfiguracionEmail> ConfiguracionEmails { get; set; }
        public DbSet<Presupuesto> Presupuestos { get; set; }

        public DbSet<Factura> Facturas { get; set; }
        public DbSet<FacturaLinea> FacturaLineas { get; set; }
        public DbSet<TiendaConfig> TiendaConfigs { get; set; }

        public DbSet<TrabajoImpresion> TrabajosImpresion { get; set; }

        public DbSet<PropiedadExtendidaMaestra> PropiedadesMaestras { get; set; }
        public DbSet<ProductoPropiedadConfigurada> ProductoPropiedades { get; set; }


        // =========================
        // Model configuration
        // =========================
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                // IGNORAR entidades de Identity para evitar el error de "Derived Type"
                if (entityType.ClrType.Namespace != null && entityType.ClrType.Namespace.Contains("Microsoft.AspNetCore.Identity"))
                    continue;
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

           

            // -------------------------
            // Categorias
            // -------------------------

            modelBuilder.Entity<ProductoBase>()
          .HasOne(p => p.Categoria)
          .WithMany(c => c.Productos)
          .HasForeignKey(p => p.CategoriaId)
          .OnDelete(DeleteBehavior.SetNull);


            // Configuración para el motor de precios
            modelBuilder.Entity<ProductoPropiedadConfigurada>()
                .HasOne(p => p.Producto)
                .WithMany(b => b.PropiedadesExtendidas)
                .HasForeignKey(p => p.ProductoBaseId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ProductoPropiedadConfigurada>()
        .Property(p => p.Valor)
        .HasPrecision(18, 8);
        }
    }
}
