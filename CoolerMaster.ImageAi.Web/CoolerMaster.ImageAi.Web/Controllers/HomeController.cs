using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Amazon.BedrockAgentRuntime;
using CoolerMaster.ImageAi.Shared;
using CoolerMaster.ImageAi.Shared.Interfaces;
using CoolerMaster.ImageAi.Shared.Models;
using CoolerMaster.ImageAi.Web.Models;
using CoolerMaster.ImageAi.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

namespace CoolerMaster.ImageAi.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IAwsS3Client _awsS3Client;
        private readonly IAwsBedrockClient _awsBedrockClient;
        private readonly IAwsBedrcokAgentClient _awsBedrcokAgentClient;
        private readonly ProductDbContext _dbContext;
        private readonly ISaveService _saveService;

        public HomeController
            (ILogger<HomeController> logger, IAwsS3Client awsS3Client, IAwsBedrockClient awsBedrockClient, IAwsBedrcokAgentClient awsBedrcokAgentClient, ProductDbContext dbContext, ISaveService save)
        {
            _logger = logger;
            _awsS3Client = awsS3Client;
            _awsBedrockClient = awsBedrockClient;
            _awsBedrcokAgentClient = awsBedrcokAgentClient;
            _dbContext = dbContext;
            _saveService = save;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> CoolerMasterImager
            (string actionType, string taskType, string prompt, 
             string imageData1, string imageData2, string imageData3, string imageData4, string imageData5, 
             string outputImageData,
             ImageParameterViewModel imageParam)
        {
            if (actionType == "SaveImage")
            {
                var isOk = await SaveImageToS3(taskType, outputImageData);
                ViewBag.SaveImageToS3Result = isOk;
            }
            else if(actionType == "SendPrompt")
            {
                string rawBase64Image = await GenBase64Image(taskType, prompt, imageData1, imageData2, imageData3, imageData4, imageData5, imageParam);
                string mimeType = "image/png";
                string base64Image = $"data:{mimeType};base64,{rawBase64Image}";
                ViewBag.GeneratedImage = base64Image;
            }

            ViewBag.SelectedTaskType = taskType;
            return View("Index");
        }
        private async Task<bool> SaveImageToS3(string taskType, string imageData)
        {
            string contentType = null;
            string folderName = "generated-images";
            string fileName = $"{taskType}_{DateTime.Now.ToString("yyyyMMdd_HHmmss")}.png";

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

                return !string.IsNullOrEmpty(await _awsS3Client.UploadImageAsync(imageStream, folderName, fileName, contentType ?? ""));
            }

            return false;
        }
        private async Task<string> GenBase64Image(string taskType, string prompt, 
            string imageData1, string imageData2, string imageData3, string imageData4, string imageData5, 
            ImageParameterViewModel imageParam)
        {
            var imgParam = new ImageParameter
            {
                ImageWidth = imageParam.ImageWidth,
                ImageHeight = imageParam.ImageHeight,
                Seed = imageParam.Seed,
                ImageQuality = imageParam.ImageQuality,
                CfgScale = imageParam.CfgScale,
                NegativeText = imageParam.NegativeText,
                NumberOfImages = imageParam.NumberOfImages
            };

            
            if(taskType == "generateVariation")
            {
                List<string> base64ImagesList = new List<string>();
                if (!string.IsNullOrEmpty(imageData1))
                {
                    imageData1 = imageData1.Substring(imageData1.IndexOf(',') + 1);
                    base64ImagesList.Add(imageData1);
                }
                if (!string.IsNullOrEmpty(imageData2))
                {
                    imageData2 = imageData2.Substring(imageData2.IndexOf(',') + 1);
                    base64ImagesList.Add(imageData2);
                }
                if (!string.IsNullOrEmpty(imageData3))
                {
                    imageData3 = imageData3.Substring(imageData3.IndexOf(',') + 1);
                    base64ImagesList.Add(imageData3);
                }
                if (!string.IsNullOrEmpty(imageData4))
                {
                    imageData4 = imageData4.Substring(imageData4.IndexOf(',') + 1);
                    base64ImagesList.Add(imageData4);
                }
                if (!string.IsNullOrEmpty(imageData5))
                {
                    imageData5 = imageData5.Substring(imageData5.IndexOf(',') + 1);
                    base64ImagesList.Add(imageData5);
                }
                return await _awsBedrockClient.ImageVariation(prompt, base64ImagesList, imgParam);
            }
            else
            {
                if (!string.IsNullOrEmpty(imageData1))
                {
                    imageData1 = imageData1.Substring(imageData1.IndexOf(',') + 1);
                }
                    
                return await _awsBedrockClient.TextToImage(prompt, imageData1, imgParam);
            }
        }

        public IActionResult Selector(int maxSelection = 1)
        {
            var productImages = _dbContext.ProductImages.Select(pi => new SelectorViewModel()
            {
                ImageId = pi.ImageId,
                Name = pi.Product.Name ?? pi.ImageId.ToString(),
                Category = pi.ProductCategory,
                Source = pi.ImageSource.ToString(),
                ImageUrl = pi.ImageUrl,
                Prompt = string.Join(',', pi.Prompts.Select(x => x.Prompt)),
                ProductDescriptions = pi.Specs.Where(x => x.SpecKey == Consts.SpecKey_Features || x.SpecKey == Consts.SpecKey_Description)
                                              .Select(x => x.SpecValue).ToArray() 
            }).ToList();

            ViewBag.MaxSelection = maxSelection;
            return View(productImages);
        }

        public async Task<IActionResult> S3ImageToByteArrayBase64(string imageUrl)
        {
            byte[] imageBytes = await _awsS3Client.GetImageBytesAsync(imageUrl);
            string contentType = Utils.GetContentTypeFromExtension(imageUrl);
            Response.Headers["Cache-Control"] = "public, max-age=3600"; // §Ö¨ú

            return File(imageBytes, contentType);
        }
        public async Task<IActionResult> S3ImageToByteArrayBase64_2(string imageUrl)
        {
            byte[] imageBytes = await _awsS3Client.GetImageBytesAsync(imageUrl);
            imageBytes = ConvertToJpeg_ImageSharp(imageBytes);
            string contentType = "image/jpg";
            Response.Headers["Cache-Control"] = "public, max-age=3600";

            string base64String = Convert.ToBase64String(imageBytes);
            string base64Data = $"data:{contentType};base64,{base64String}";

            return Ok(new { base64 = base64Data }); 
        }

        public static byte[] ConvertToJpeg_ImageSharp(byte[] originalBytes)
        {
            using (var inputStream = new MemoryStream(originalBytes))
            using (var image = Image.Load(inputStream))
            using (var outputStream = new MemoryStream())
            {
                image.Save(outputStream, new JpegEncoder());
                return outputStream.ToArray();
            }
        }

        public async Task<IActionResult> AgentChat(string inputText)
        {
            string responseText = await _awsBedrcokAgentClient.InvokeAgentAsync(inputText);
            return Json(new { responseText });
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
