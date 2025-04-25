using System.Diagnostics;
using CoolerMaster.ImageAi.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace CoolerMaster.ImageAi.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }
        public IActionResult GenerateImage(string taskType, string prompt, ImageGenerationViewModel imageGeneration)
        {
            return View("Index");
        }

        public IActionResult Selector()
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
