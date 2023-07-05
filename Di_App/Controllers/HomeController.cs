using Di_App.Models;
using Di_App.Services;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text;

namespace Di_App.Controllers
{
    public class HomeController : Controller
    {
        private readonly IScopedGuidService _scoped1;
        private readonly IScopedGuidService _scoped2;

        private readonly ITransientGuidService _transient1;
        private readonly ITransientGuidService _transient2;

        private readonly ISingletonGuidService _singleton1;
        private readonly ISingletonGuidService _singleton2;
        public HomeController(ISingletonGuidService singletonGuid1,
            ISingletonGuidService singletonGuid2,
            IScopedGuidService scopedGuid1, IScopedGuidService scopedGuid2,
            ITransientGuidService transientGuid1, ITransientGuidService transientGuid2)
        {
            _scoped1 = scopedGuid1;
            _scoped2 =scopedGuid2;
            _transient1 = transientGuid1;
            _transient2 = transientGuid2;
            _singleton1 = singletonGuid1;   
            _singleton2 = singletonGuid2;   
        }

        public IActionResult Index()
        {
            StringBuilder message = new StringBuilder();
            message.Append($"Transient1 :{ _transient1.GetGuid()}\n");
            message.Append($"Transient2 :{ _transient2.GetGuid()}\n\n");

            message.Append($"Scoped1 :{_scoped1.GetGuid()}\n");
            message.Append($"Scoped2 :{_scoped2.GetGuid()}\n\n");

            message.Append($"singleton1 :{_singleton1.GetGuid()}\n");
            message.Append($"singleton2 :{_singleton2.GetGuid()}\n\n");
            return Ok(message.ToString());
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}