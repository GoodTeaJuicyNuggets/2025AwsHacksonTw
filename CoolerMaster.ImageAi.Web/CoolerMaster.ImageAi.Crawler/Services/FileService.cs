using CoolerMaster.ImageAi.Shared.Models;
using CoolerMaster.ImageAi.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoolerMaster.ImageAi.Crawler.Services
{
    public class FileService
    {
        public async Task<List<ProductImage>> DownloadImages(List<string> imageUrls, string category, string productName, int productId)
        {
            var baseFolder = Path.Combine(AppContext.BaseDirectory, "files", category);
            Directory.CreateDirectory(baseFolder);

            var result = new List<ProductImage>();

            for (int i = 0; i < imageUrls.Count; i++)
            {
                string sanitizedName = string.Join("_", productName.Split(Path.GetInvalidFileNameChars()));
                string fileName = $"{sanitizedName}_{i + 1}.jpg";
                string filePath = Path.Combine(baseFolder, fileName);
                string relativePath = Path.Combine("files", category, fileName);

                var productImage = new ProductImage
                {
                    ImageUrl = relativePath,
                    ProductId = productId,
                    ProductCategory = category,
                    ImageSource = ImageSource.Official
                };

                if (File.Exists(filePath))
                {
                    Console.WriteLine($"📁 檔案已存在，略過下載：{filePath}");
                }
                else
                {
                    try
                    {
                        using var client = new HttpClient();
                        var imageData = await client.GetByteArrayAsync(imageUrls[i]);
                        await File.WriteAllBytesAsync(filePath, imageData);
                        Console.WriteLine($"🖼️ 已下載：{filePath}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"❌ 下載失敗：{fileName}，原因：{ex.Message}");
                    }
                }

                result.Add(productImage);
            }

            return result;
        }
    }

}
