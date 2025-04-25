using CoolerMaster.ImageAi.Crawler.Services;
using CoolerMaster.ImageAi.Shared.Models;
using CoolerMaster.ImageAi.Shared;

// Program.cs
class Program
{
    static async Task Main(string[] args)
    {
        SQLitePCL.Batteries.Init();

        var crawler = new CrawlerService();
        var fileService = new FileService();

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
}