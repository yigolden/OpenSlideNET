using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace SingleSlideServer.Controllers
{
    public class HomeController : Controller
    {
        private ImageProvider _provider;

        public HomeController(ImageProvider provider)
        {
            _provider = provider;
        }

        [HttpGet("/image.dzi")]
        public IActionResult GetDzi()
        {
            return Content(_provider.DeepZoomGenerator.GetDzi(), "application/xml", Encoding.UTF8);
        }
    }
}
