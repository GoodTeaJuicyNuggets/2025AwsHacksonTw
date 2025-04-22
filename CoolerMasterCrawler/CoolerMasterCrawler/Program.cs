using System.Globalization;
using System.Net;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;

public class Product
{
    public int Id { get; set; } // Primary Key
    public string ProductCategory { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string ProductPageUrl { get; set; }
    public List<string> ImageUrls { get; set; } // Stored as List<string>
    public List<string> LocalImagePaths { get; set; } // Relative paths to downloaded images
}

public class ProductDbContext : DbContext
{
    public DbSet<Product> Products { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var dbPath = Path.Combine(AppContext.BaseDirectory, "sqlite", "products.db");
        Directory.CreateDirectory(Path.GetDirectoryName(dbPath));
        optionsBuilder.UseSqlite($"Data Source={dbPath}");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure ValueConverter for ImageUrls and LocalImagePaths
        var listToJsonConverter = new ValueConverter<List<string>, string>(
            v => JsonSerializer.Serialize(v, new JsonSerializerOptions { WriteIndented = false }),
            v => JsonSerializer.Deserialize<List<string>>(v, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<string>()
        );

        modelBuilder.Entity<Product>()
            .Property(p => p.ImageUrls)
            .HasConversion(listToJsonConverter);

        modelBuilder.Entity<Product>()
            .Property(p => p.LocalImagePaths)
            .HasConversion(listToJsonConverter);
    }
}

class Program
{
    static async Task Main(string[] args)
    {
        // Initialize SQLite native support
        SQLitePCL.Batteries.Init();

        var allProducts = new List<Product>();
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

        var options = new ChromeOptions();
        options.AddArgument("--disable-gpu");
        options.AddArgument("--headless");

        using var driver = new ChromeDriver(options);
        var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));

        foreach (var category in catalogCategories)
        {
            string catalogUrl = baseUrl + category + "/";
            Console.WriteLine($"\n=== 🚀 開始處理分類：{category} ===");

            driver.Navigate().GoToUrl(catalogUrl);
            HandlePopups(driver);

            int page = 1;

            while (true)
            {
                Console.WriteLine($"📄 處理第 {page} 頁");

                try
                {
                    wait.Until(d => d.FindElements(By.XPath("//li[contains(@class, 'ais-Hits-item')]")).Count > 0);
                }
                catch
                {
                    Console.WriteLine("⚠️ 無產品元素，跳過此分類");
                    break;
                }

                var productsOnPage = driver.FindElements(By.XPath("//li[contains(@class, 'ais-Hits-item')]"));

                for (int i = 0; i < productsOnPage.Count; i++)
                {
                    try
                    {
                        var productElement = driver.FindElements(By.XPath("//li[contains(@class, 'ais-Hits-item')]"))[i];

                        string name = "", productPageUrl = "", description = "";
                        List<string> imageUrls = new();

                        name = productElement.FindElement(By.XPath(".//h3")).Text.Trim();
                        productPageUrl = productElement.FindElement(By.XPath(".//a")).GetAttribute("href");

                        string imageUrl = productElement.FindElement(By.XPath(".//img")).GetAttribute("src");
                        if (!imageUrl.StartsWith("http")) imageUrl = "https://www.coolermaster.com" + imageUrl;
                        imageUrls.Add(imageUrl);

                        var imageElement = productElement.FindElement(By.XPath(".//img"));
                        var actions = new Actions(driver);
                        actions.MoveToElement(imageElement).Perform();
                        wait.Until(d => d.FindElement(By.XPath(".//img")).GetAttribute("src") != imageUrl);

                        imageElement = productElement.FindElement(By.XPath(".//img"));
                        var newImageUrl = imageElement.GetAttribute("src");
                        if (!imageUrls.Contains(newImageUrl) && newImageUrl.StartsWith("http"))
                        {
                            imageUrls.Add(newImageUrl);
                        }

                        try
                        {
                            description = productElement.FindElement(By.XPath(".//p[contains(@class, 'body-s')]")).Text.Trim();
                        }
                        catch { }

                        // Download images and generate local paths
                        var localImagePaths = await DownloadImages(imageUrls, category, name);

                        allProducts.Add(new Product
                        {
                            Name = name,
                            ImageUrls = imageUrls,
                            LocalImagePaths = localImagePaths,
                            ProductPageUrl = productPageUrl,
                            Description = description,
                            ProductCategory = category
                        });

                        Console.WriteLine($"✅ {name} [{category}]");
                    }
                    catch (StaleElementReferenceException)
                    {
                        Console.WriteLine("⚠️ 發生 stale element，略過此產品");
                        continue;
                    }
                }

                try
                {
                    var nextPageLi = driver.FindElement(By.XPath("//li[contains(@class, 'ais-Pagination-item--nextPage')]"));

                    if (!nextPageLi.GetAttribute("class").Contains("ais-Pagination-item--disabled"))
                    {
                        var nextLink = nextPageLi.FindElement(By.TagName("a"));
                        ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView(true);", nextLink);
                        ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", nextLink);
                        await Task.Delay(2000);
                        page++;
                        continue;
                    }
                }
                catch
                {
                    Console.WriteLine("🛑 沒有下一頁");
                }

                break;
            }
        }

        // Save to SQLite database
        using (var dbContext = new ProductDbContext())
        {
            dbContext.Database.EnsureCreated();
            dbContext.Products.AddRange(allProducts);
            await dbContext.SaveChangesAsync();
        }

        Console.WriteLine($"\n✅ 完成，共 {allProducts.Count} 筆產品資訊，已儲存至 SQLite 資料庫");
    }

    static async Task<List<string>> DownloadImages(List<string> imageUrls, string category, string productName)
    {
        var baseFolder = Path.Combine(AppContext.BaseDirectory, "files", category);
        Directory.CreateDirectory(baseFolder);

        var localPaths = new List<string>();

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
                localPaths.Add(relativePath);
                Console.WriteLine($"🖼️ 已下載：{filePath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ 下載失敗：{fileName}，原因：{ex.Message}");
            }
        }

        return localPaths;
    }

    static void HandlePopups(IWebDriver driver)
    {
        IJavaScriptExecutor js = (IJavaScriptExecutor)driver;

        try
        {
            var allowAllBtn = driver.FindElements(By.XPath("//button[contains(text(),'Allow all')]"));
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
            var continueBtns = driver.FindElements(By.XPath("//button[.//span[text()='Yes, continue here']]"));
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
