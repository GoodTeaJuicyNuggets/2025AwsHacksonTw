using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.EntityFrameworkCore;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium;
using System.Text.Json;

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

// File/FileService.cs
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

            try
            {
                using var client = new HttpClient();
                var imageData = await client.GetByteArrayAsync(imageUrls[i]);
                await File.WriteAllBytesAsync(filePath, imageData);
                result.Add(new ProductImage()
                {
                    ImageUrl = relativePath,
                    ProductId = productId
                });
                Console.WriteLine($"🖼️ 已下載：{filePath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ 下載失敗：{fileName}，原因：{ex.Message}");
            }
        }

        return result;
    }
}

// Crawler/CrawlerService.cs
public class CrawlerService
{
    private readonly IWebDriver _driver;
    private readonly WebDriverWait _wait;

    public CrawlerService()
    {
        var options = new ChromeOptions();
        options.AddArgument("--disable-gpu");
        options.AddArgument("--headless");

        _driver = new ChromeDriver(options);
        _wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(30));
    }

    public List<Product> ScrapeProducts(string baseUrl, List<string> categories)
    {
        var allProducts = new List<Product>();

        foreach (var category in categories)
        {
            string catalogUrl = baseUrl + category + "/";
            Console.WriteLine($"\n=== 🚀 開始處理分類：{category} ===");

            _driver.Navigate().GoToUrl(catalogUrl);
            HandlePopups();

            int page = 1;

            while (true)
            {
                Console.WriteLine($"📄 處理第 {page} 頁");

                try
                {
                    _wait.Until(d => d.FindElements(By.XPath("//li[contains(@class, 'ais-Hits-item')]")).Count > 0);
                }
                catch
                {
                    Console.WriteLine("⚠️ 無產品元素，跳過此分類");
                    break;
                }

                var productsOnPage = _driver.FindElements(By.XPath("//li[contains(@class, 'ais-Hits-item')]"));

                for (int i = 0; i < productsOnPage.Count; i++)
                {
                    try
                    {
                        var product = ExtractProduct(i, category);
                        allProducts.Add(product);
                        Console.WriteLine($"✅ {product.Name} [{category}]");
                    }
                    catch (StaleElementReferenceException)
                    {
                        Console.WriteLine("⚠️ 發生 stale element，略過此產品");
                        continue;
                    }
                }

                if (!NavigateToNextPage())
                {
                    break;
                }

                page++;
            }
        }

        return allProducts;
    }

    private Product ExtractProduct(int index, string category)
    {
        var productElement = _driver.FindElements(By.XPath("//li[contains(@class, 'ais-Hits-item')]"))[index];

        string name = productElement.FindElement(By.XPath(".//h3")).Text.Trim();
        string productPageUrl = productElement.FindElement(By.XPath(".//a")).GetAttribute("href");
        string description = productElement.FindElement(By.XPath(".//p[contains(@class, 'body-s')]"))?.Text.Trim() ?? "";

        var imageUrls = new List<string>();

        try
        {
            string imageUrl = productElement.FindElement(By.XPath(".//img")).GetAttribute("src");
            if (!imageUrl.StartsWith("http")) imageUrl = "https://www.coolermaster.com" + imageUrl;
            imageUrls.Add(imageUrl);

            var imageElement = productElement.FindElement(By.XPath(".//img"));
            var actions = new Actions(_driver);
            actions.MoveToElement(imageElement).Perform();
            _wait.Until(d => d.FindElement(By.XPath(".//img")).GetAttribute("src") != imageUrl);

            imageElement = productElement.FindElement(By.XPath(".//img"));
            var newImageUrl = imageElement.GetAttribute("src");
            if (!imageUrls.Contains(newImageUrl) && newImageUrl.StartsWith("http"))
            {
                imageUrls.Add(newImageUrl);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ 抓圖發生錯誤：{ex.Message}");
        }

        return new Product
        {
            Name = name,
            ProductPageUrl = productPageUrl,
            Description = description,
            ProductCategory = category,
            ImageUrls = imageUrls
        };
    }

    private bool NavigateToNextPage()
    {
        try
        {
            var nextPageLi = _driver.FindElement(By.XPath("//li[contains(@class, 'ais-Pagination-item--nextPage')]"));

            if (!nextPageLi.GetAttribute("class").Contains("ais-Pagination-item--disabled"))
            {
                var nextLink = nextPageLi.FindElement(By.TagName("a"));
                ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].scrollIntoView(true);", nextLink);
                ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", nextLink);
                Thread.Sleep(2000);
                return true;
            }
        }
        catch
        {
            Console.WriteLine("🛑 沒有下一頁");
        }

        return false;
    }

    private void HandlePopups()
    {
        IJavaScriptExecutor js = (IJavaScriptExecutor)_driver;

        try
        {
            var allowAllBtn = _driver.FindElements(By.XPath("//button[contains(text(),'Allow all')]"));
            if (allowAllBtn.Count > 0)
            {
                js.ExecuteScript("arguments[0].click();", allowAllBtn[0]);
                Console.WriteLine("👉 點擊了『Allow all』");
                Thread.Sleep(1000);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("⚠️ Allow all 點擊失敗：" + ex.Message);
        }

        try
        {
            var continueBtns = _driver.FindElements(By.XPath("//button[.//span[text()='Yes, continue here']]"));
            if (continueBtns.Count > 0)
            {
                js.ExecuteScript("arguments[0].click();", continueBtns[0]);
                Console.WriteLine("👉 點擊了『Yes, continue here』");
                Thread.Sleep(1000);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("⚠️ Yes, continue here 點擊失敗：" + ex.Message);
        }
    }
}


// DB/Models.cs
public class Product
{
    public int Id { get; set; } // Primary Key
    public string ProductCategory { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string ProductPageUrl { get; set; }
    public List<string> ImageUrls { get; set; } // Stored as List<string>
    public List<ProductImage> ProductImages { get; set; } // Navigation property
}

public class ProductImage
{
    public int ImageId { get; set; } // Primary Key
    public string ImageUrl { get; set; } // Relative path to downloaded image
    public int ProductId { get; set; } // Foreign Key
    public Product Product { get; set; } // Navigation property
}

public class ProductDbContext : DbContext
{
    public DbSet<Product> Products { get; set; }
    public DbSet<ProductImage> ProductImages { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var dbPath = Path.Combine(AppContext.BaseDirectory, "sqlite", "products.db");
        Directory.CreateDirectory(Path.GetDirectoryName(dbPath));
        optionsBuilder.UseSqlite($"Data Source={dbPath}");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>()
            .HasMany(p => p.ProductImages)
            .WithOne(pi => pi.Product)
            .HasForeignKey(pi => pi.ProductId);

        modelBuilder.Entity<ProductImage>()
            .HasKey(pi => pi.ImageId);

        var listToJsonConverter = new ValueConverter<List<string>, string>(
            v => JsonSerializer.Serialize(v, new JsonSerializerOptions { WriteIndented = false }),
            v => JsonSerializer.Deserialize<List<string>>(v, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<string>()
        );

        modelBuilder.Entity<Product>()
            .Property(p => p.ImageUrls)
            .HasConversion(listToJsonConverter);
    }
}