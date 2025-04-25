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
        public async Task<IActionResult> GenerateImage(string taskType, string prompt, ImageGenerationViewModel imageGeneration)
        {
            var folderName = "generated-images";
            var fileName = $"{taskType}_{DateTime.Now.ToString("yyyyMMdd_HHmmss")}"; 
            var contentType = "image/jpg";

            var imagePath = Path.Combine(_env.WebRootPath, "pic", "image_01.jpg");
            var fileStream = new FileStream(imagePath, FileMode.Open, FileAccess.Read, FileShare.Read);

            bool uploadSuccess = await _awsS3Client.UploadImageAsync(fileStream, folderName, fileName, contentType);

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
