using BaseConLogin.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BaseConLogin.ViewComponents
{
    public class CategoriasMenuViewComponent : Microsoft.AspNetCore.Mvc.ViewComponent
    {
        private readonly ApplicationDbContext _context;

        public CategoriasMenuViewComponent(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var categorias = await _context.Categorias
                .Where(c => c.Activa)
                .OrderBy(c => c.Nombre)
                .ToListAsync();

            return View(categorias);
        }
    }
}