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

        public DbSet<Proyectos> Proyectos { get; set; }
        public DbSet<BaseConLogin.Models.ContactMessage> ContactMessages { get; set; }

        public DbSet<Tag> Tags { get; set; }
        public DbSet<ProyectoTag> ProyectoTags { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ProyectoTag>()
                .HasKey(pt => new { pt.ProyectoId, pt.TagId });

            modelBuilder.Entity<ProyectoTag>()
                .HasOne(pt => pt.Proyecto)
                .WithMany(p => p.ProyectoTags)
                .HasForeignKey(pt => pt.ProyectoId);

            modelBuilder.Entity<ProyectoTag>()
                .HasOne(pt => pt.Tag)
                .WithMany(t => t.ProyectoTags)
                .HasForeignKey(pt => pt.TagId);
        }
    }
}
