using Microsoft.AspNetCore.Mvc;

namespace ChatApp.Controllers
{
    public class MessageController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
