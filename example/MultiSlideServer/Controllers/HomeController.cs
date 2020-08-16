using Microsoft.AspNetCore.Mvc;
using MultiSlideServer.Cache;
using System.Linq;
using System.Text;

namespace MultiSlideServer.Controllers
{
    public class HomeController : Controller
    {
        private ImageProvider _provider;

        public HomeController(ImageProvider provider)
        {
            _provider = provider;
        }

        [HttpGet("/")]
        public IActionResult Index()
        {
            return View(_provider.Images.Select(i => i.Name));
        }

        [HttpGet("/slide/{name}.html")]
        public IActionResult Slide(string name)
        {
            if (!_provider.TryGetImagePath(name, out string path))
            {
                return NotFound();
            }
            ViewData["name"] = name;
            return View();
        }

        [HttpGet("/storage/{name}.dzi")]
        public IActionResult GetDzi(string name)
        {
            if (!_provider.TryGetImagePath(name, out string path))
            {
                return NotFound();
            }
            RetainableDeepZoomGenerator dz = _provider.RetainDeepZoomGenerator(name, path);
            try
            {
                return Content(dz.GetDzi(), "application/xml", Encoding.UTF8);
            }
            finally
            {
                dz.Release();
            }
        }
    }
}
