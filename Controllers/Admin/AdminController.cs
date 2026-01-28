using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BaseConLogin.Controllers.Admin
{
    [Authorize(Roles = "Admin")]
    [Route("admin")]
    public class AdminController : Controller
    {
        // GET: /Admin
        [Route("")]
        public IActionResult Index()
        {
            return View();
        }
    }
}
