using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System.Globalization;
using CsvHelper;
using OpenQA.Selenium.Interactions;
using CsvHelper.Configuration;
using System.Text.Json;

class Product
{
    public string ProductCategory { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string ProductPageUrl { get; set; }
    public List<string> ImageUrls { get; set; }
}

class Program
{
    static async Task Main(string[] args)
    {
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

                foreach (var productElement in productsOnPage)
                {
                    string name = "", productPageUrl = "", description = "";
                    List<string> imageUrls = new();

                    name = productElement.FindElement(By.XPath(".//h3")).Text.Trim();
                    productPageUrl = productElement.FindElement(By.XPath(".//a")).GetAttribute("href");
                    string imageUrl = productElement.FindElement(By.XPath(".//img")).GetAttribute("src");
                    if (!imageUrl.StartsWith("http")) imageUrl = "https://www.coolermaster.com" + imageUrl;
                    imageUrls.Add(imageUrl);

                    try
                    {
                        // 重新獲取圖像元素並模擬懸停以加載不同的圖片
                        var imageElement = productElement.FindElement(By.XPath(".//img"));
                        var actions = new Actions(driver);
                        actions.MoveToElement(imageElement).Perform();  // 模擬懸停動作

                        // 等待圖片改變或新圖片加載
                        wait.Until(d => d.FindElement(By.XPath(".//img")).GetAttribute("src") != imageUrl);
                        imageElement = productElement.FindElement(By.XPath(".//img"));
                        var newImageUrl = imageElement.GetAttribute("src");
                        if (!imageUrls.Contains(newImageUrl) && newImageUrl.StartsWith("http"))
                        {
                            imageUrls.Add(newImageUrl);
                        }
                    }
                    catch { }

                    try
                    {
                        description = productElement.FindElement(By.XPath(".//p[contains(@class, 'body-s')]")).Text.Trim();
                    }
                    catch { }

                    allProducts.Add(new Product
                    {
                        Name = name,
                        ImageUrls = imageUrls,
                        ProductPageUrl = productPageUrl,
                        Description = description,
                        ProductCategory = category
                    });

                    Console.WriteLine($"✅ {name} [{category}]");
                }

                // 下一頁邏輯
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

        // 📝 輸出單一 CSV
        var csvFileName = "coolermaster_products.csv";
        using (var writer = new StreamWriter(csvFileName))
        using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
        {
            csv.Context.RegisterClassMap<ProductMap>();
            csv.WriteHeader<Product>();
            csv.NextRecord();

            foreach (var p in allProducts)
            {
                csv.WriteRecord(p);
                csv.NextRecord();
            }
        }

        Console.WriteLine($"\n✅ 完成，共 {allProducts.Count} 筆產品資訊，已寫入：{csvFileName}");
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

    public class ProductMap : ClassMap<Product>
    {
        public ProductMap()
        {
            Map(p => p.ProductCategory);
            Map(p => p.Name);
            Map(p => p.Description);
            Map(p => p.ProductPageUrl);
            Map(p => p.ImageUrls)
                .Convert(args => JsonSerializer.Serialize(args.Value.ImageUrls ?? new List<string>()));
        }
    }
}
