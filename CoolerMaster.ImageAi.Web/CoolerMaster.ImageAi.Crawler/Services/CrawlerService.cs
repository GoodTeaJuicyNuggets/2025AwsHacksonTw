using CoolerMaster.ImageAi.Shared.Models;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoolerMaster.ImageAi.Crawler.Services
{
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
                productElement = _driver.FindElements(By.XPath("//li[contains(@class, 'ais-Hits-item')]"))[index];
                string imageUrl = productElement.FindElement(By.XPath(".//img")).GetAttribute("src");
                if (!imageUrl.StartsWith("http")) imageUrl = "https://www.coolermaster.com" + imageUrl;
                imageUrls.Add(imageUrl);

                productElement = _driver.FindElements(By.XPath("//li[contains(@class, 'ais-Hits-item')]"))[index];
                var imageElement = productElement.FindElement(By.XPath(".//img"));
                var actions = new Actions(_driver);
                actions.MoveToElement(imageElement).Perform();
                _wait.Until(d => d.FindElement(By.XPath(".//img")).GetAttribute("src") != imageUrl);

                productElement = _driver.FindElements(By.XPath("//li[contains(@class, 'ais-Hits-item')]"))[index];
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

}
