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

        public IActionResult Selector(int maxSelection = 1)
        {
            var products = new List<object>
            {
                new { Id = 1, Name = "官網商品 1", Category = "pc-cases", Source = "official", ImageUrl = "https://a.storyblok.com/f/281110/960x960/39f09cd23c/elite-301-white-gallery-02.png/m/960x0/smart" },
                new { Id = 2, Name = "官網商品 2", Category = "pc-cases", Source = "official", ImageUrl = "https://a.storyblok.com/f/281110/960x960/39f09cd23c/elite-301-white-gallery-02.png/m/960x0/smart" },
                new { Id = 3, Name = "上傳商品 1", Category = "power-supply", Source = "uploaded", ImageUrl = "https://a.storyblok.com/f/281110/960x960/39f09cd23c/elite-301-white-gallery-02.png/m/960x0/smart" },
                new { Id = 4, Name = "AI 草稿 1", Category = "cooler", Source = "ai-draft", ImageUrl = "https://a.storyblok.com/f/281110/960x960/39f09cd23c/elite-301-white-gallery-02.png/m/960x0/smart" },
                new { Id = 5, Name = "AI 成品 1", Category = "pc-cases", Source = "ai-final", ImageUrl = "https://a.storyblok.com/f/281110/960x960/39f09cd23c/elite-301-white-gallery-02.png/m/960x0/smart" }
            };

            ViewBag.MaxSelection = maxSelection;
            return View(products);
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
