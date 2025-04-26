using CoolerMaster.ImageAi.Crawler.Services;
using CoolerMaster.ImageAi.Shared.Models;
using CoolerMaster.ImageAi.Shared;
using Microsoft.EntityFrameworkCore;
using CoolerMaster.ImageAi.Shared.Configurations;

// Program.cs
class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("請選擇操作：\n0. 爬蟲產品列表圖片\n1. 爬蟲產品細節");

        if (Console.ReadLine() == "0")
        {
            var crawlerSharedService = new CrawlerSharedService();
            var crawler = new CrawlerService(crawlerSharedService);
            var s3Client = new AwsS3Client(new AwsS3Config()
            {
                AccessKeyId = Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID"),
                SecretAccessKey = Environment.GetEnvironmentVariable("AWS_SECRET_ACCESS_KEY"),
                BucketName = Environment.GetEnvironmentVariable("AWS_BUCKET_NAME"),
                Region = Environment.GetEnvironmentVariable("AWS_REGION")
            });
            var fileService = new FileService(s3Client);

            var baseUrl = "https://www.coolermaster.com/en-global/catalog/";
            var catalogCategories = new List<string>
                {
                    "cooling",
                    "pc-cases",
                    "power-supplies",
                    "gaming-accessories",
                    "furniture",
                    "systems",
                    "masterhub",
                    "max",
                    "immersive-experience"
                };
            var allProducts = crawler.ScrapeProducts(baseUrl, catalogCategories);
            var newProducts = new List<Product>();

            using (var dbContext = new ProductDbContext())
            {
                dbContext.Database.EnsureCreated();

                var existingProducts = dbContext.Products
                    .Where(p => allProducts.Select(ap => ap.Name).Contains(p.Name))
                    .ToList();

                newProducts = allProducts
                    .Where(ap => !existingProducts.Any(ep => ep.Name == ap.Name && ep.ProductCategory == ap.ProductCategory))
                    .ToList();

                foreach (var product in newProducts)
                {
                    product.ProductImages = await fileService.DownloadImages(product.ImageUrls, product.ProductCategory, product.Name, product.Id);
                }

                dbContext.Products.AddRange(newProducts);
                await dbContext.SaveChangesAsync();
            }

            Console.WriteLine($"\n✅ 完成，共 {newProducts.Count} 筆產品資訊，已儲存至 SQLite 資料庫");
        }
        else //if (Console.ReadLine() == "1")
        {
            var crawlerSharedService = new CrawlerSharedService();
            var crawlerDetail = new CrawlerDetailService(crawlerSharedService);

            using var db = new ProductDbContext();
            var products = db.Products
                .Include(p => p.ProductImages)
                .ToList();

            using (var dbContext = new ProductDbContext())
            {
                dbContext.Database.EnsureCreated();

                var productSpecs = crawlerDetail.ScrapeProductDetail(products);
                db.ImageSpecs.AddRange(productSpecs);
                db.SaveChanges();
            }
        }
    }
}
