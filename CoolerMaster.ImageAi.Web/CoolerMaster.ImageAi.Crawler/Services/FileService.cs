using CoolerMaster.ImageAi.Shared.Models;
using CoolerMaster.ImageAi.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace CoolerMaster.ImageAi.Crawler.Services
{
    public class FileService
    {
        private readonly AwsS3Client _s3Client;

        public FileService(AwsS3Client s3Client)
        {
            _s3Client = s3Client;
        }

        public async Task<List<ProductImage>> DownloadImages(List<string> imageUrls, string category, string productName, int productId)
        {
            var result = new List<ProductImage>();

            for (int i = 0; i < imageUrls.Count; i++)
            {
                string fileName = Path.GetFileName(imageUrls[i]);

                var productImage = new ProductImage
                {
                    ProductId = productId,
                    ProductCategory = category,
                    ImageSource = ImageSource.Official
                };

                try
                {
                    using var client = new HttpClient();
                    var imageData = await client.GetByteArrayAsync(imageUrls[i]);

                    // Get the content type from the file extension
                    string contentType = Utils.GetContentTypeFromExtension(Path.GetExtension(fileName));

                    // 上傳到 S3 並獲取 URL
                    using var imageDataStream = new MemoryStream(imageData);
                    string s3Url = await _s3Client.UploadImageAsync(imageDataStream, category, fileName, contentType);

                    productImage.ImageUrl = s3Url;

                    Console.WriteLine($"🖼️ 已上傳到 S3：{s3Url}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ 上傳失敗：{fileName}，原因：{ex.Message}");
                }

                result.Add(productImage);
            }

            return result;
        }
    }
}
