using BaseConLogin.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Xml.Linq;

namespace BaseConLogin.Controllers.Front
{
    [Route("sitemap.xml")]
    public class SitemapController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SitemapController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            XNamespace ns = "http://www.sitemaps.org/schemas/sitemap/0.9";

            var baseUrl = $"{Request.Scheme}://{Request.Host}";

            // =========================
            // DATOS
            // =========================

            var tiendas = await _context.Tiendas
                .Where(t => t.Activa && t.Slug != null)
                .ToListAsync();

            var proyectos = await _context.Proyectos
                .Include(p => p.Producto)
                    .ThenInclude(pr => pr.Tienda)
                .Where(p =>
                    p.Producto.Activo &&
                    p.Slug != null)
                .ToListAsync();

            var tags = await _context.Tags
                .Where(t => t.Slug != null)
                .ToListAsync();

            // =========================
            // XML
            // =========================

            var urlset = new XElement(ns + "urlset");

            string hoy = DateTime.UtcNow.ToString("yyyy-MM-dd");

            // =========================
            // HOME
            // =========================

            urlset.Add(new XElement(ns + "url",
                new XElement(ns + "loc", $"{baseUrl}/"),
                new XElement(ns + "lastmod", hoy),
                new XElement(ns + "changefreq", "daily"),
                new XElement(ns + "priority", "1.0")
            ));

            // =========================
            // TIENDAS
            // =========================

            foreach (var tienda in tiendas)
            {
                urlset.Add(new XElement(ns + "url",
                    new XElement(ns + "loc", $"{baseUrl}/t/{tienda.Slug}"),
                    new XElement(ns + "lastmod", hoy),
                    new XElement(ns + "changefreq", "weekly"),
                    new XElement(ns + "priority", "0.9")
                ));
            }

            // =========================
            // PROYECTOS
            // =========================

            foreach (var proyecto in proyectos)
            {
                if (proyecto.Producto?.Tienda == null)
                    continue;

                var tiendaSlug = proyecto.Producto.Tienda.Slug;

                if (string.IsNullOrEmpty(tiendaSlug))
                    continue;

                urlset.Add(new XElement(ns + "url",
                    new XElement(ns + "loc",
                        $"{baseUrl}/t/{tiendaSlug}/proyectos/{proyecto.Slug}"
                    ),
                    new XElement(ns + "lastmod", hoy),
                    new XElement(ns + "changefreq", "weekly"),
                    new XElement(ns + "priority", "0.8")
                ));
            }

            // =========================
            // TAGS
            // =========================

            foreach (var tag in tags)
            {
                urlset.Add(new XElement(ns + "url",
                    new XElement(ns + "loc", $"{baseUrl}/tags/{tag.Slug}"),
                    new XElement(ns + "lastmod", hoy),
                    new XElement(ns + "changefreq", "monthly"),
                    new XElement(ns + "priority", "0.6")
                ));
            }

            // =========================
            // DOCUMENTO FINAL
            // =========================

            var sitemap = new XDocument(
                new XDeclaration("1.0", "utf-8", "yes"),
                urlset
            );

            return Content(
                sitemap.ToString(),
                "application/xml"
            );
        }
    }
}
