using System;
using System.Diagnostics;
using CoolerMaster.ImageAi.Web.Interfaces;
using CoolerMaster.ImageAi.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace CoolerMaster.ImageAi.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IWebHostEnvironment _env;
        private readonly IAwsS3Client _awsS3Client;
        public HomeController(ILogger<HomeController> logger, IWebHostEnvironment env, IAwsS3Client awsS3Client)
        {
            _logger = logger;
            _env = env;
            _awsS3Client = awsS3Client;
        }

        public IActionResult Index()
        {
            return View();
        }
        public async Task<IActionResult> GenerateImage(string taskType, string prompt, string imageData, ImageGenerationViewModel imageGeneration)
        {
            string contentType = null;
            string folderName = "generated-images";
            string fileName = $"{taskType}_{DateTime.Now.ToString("yyyyMMdd_HHmmss")}";

            if (!string.IsNullOrEmpty(imageData))
            {
                byte[] imageBytes = Convert.FromBase64String(imageData.Substring(imageData.IndexOf(',') + 1));
                Stream imageStream = new MemoryStream(imageBytes);

                int commaIndex = imageData.IndexOf(',');
                if (commaIndex != -1 && imageData.StartsWith("data:"))
                {
                    string metadata = imageData.Substring(5, commaIndex - 5); 
                    string[] metadataParts = metadata.Split(';');
                    if (metadataParts.Length > 0 && metadataParts[0].Contains('/'))
                    {
                        contentType = metadataParts[0];
                    }
                }

                bool uploadSuccess = await _awsS3Client.UploadImageAsync(imageStream, folderName, fileName, contentType ?? "");
            }

            

            return View("Index");
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
