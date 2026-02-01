using BaseConLogin.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BaseConLogin.ViewComponents
{
    public class HeaderSearchViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;

        public HeaderSearchViewComponent(ApplicationDbContext context)
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
