using CoolerMaster.ImageAi.Shared.Models;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoolerMaster.ImageAi.Crawler.Services
{
    public class CrawlerDetailService
    {
        private readonly CrawlerSharedService _crawlerSharedService;


        public CrawlerDetailService(CrawlerSharedService crawlerSharedService)
        {
            _crawlerSharedService = crawlerSharedService;
        }

        public List<ImageSpec> ScrapeProductDetail(List<Product> products)
        {
            var result = new List<ImageSpec>();

            var options = new ChromeOptions();
            options.AddArgument("--headless"); // 如果你不需要看到瀏覽器畫面
            using var driver = new ChromeDriver(options);
            driver.Navigate().GoToUrl(products[0].ProductPageUrl);
            Thread.Sleep(1500);

            _crawlerSharedService.HandlePopups(driver);

            foreach (var product in products)
            {
                driver.Navigate().GoToUrl(product.ProductPageUrl);

                Console.WriteLine($"正在處理產品：{product.Name}");

                var specs = new List<ImageSpec>();

                // 1. product_series
                var breadcrumbItems = driver.FindElements(By.CssSelector("ol.list-none li"));
                if (breadcrumbItems.Count > 1)
                {
                    var breadcrumbValue = string.Join(" > ",
                        breadcrumbItems.Take(breadcrumbItems.Count - 1).Select(li => li.Text.Trim()));
                    specs.Add(new ImageSpec { SpecKey = "product_series", SpecValue = breadcrumbValue });
                }

                // 2. info_description
                try
                {
                    var infoDesc = driver.FindElement(By.CssSelector("div.info__description"));
                    specs.Add(new ImageSpec
                    {
                        SpecKey = "info_description",
                        SpecValue = infoDesc.Text.Trim()
                    });
                }
                catch (Exception ex)
                {
                    var infoDesc = driver.FindElements(By.XPath("//div[h5]"));
                    foreach (var desc in infoDesc)
                    {
                        var paragraphs = desc.FindElements(By.TagName("p"));
                        foreach (var paragraph in paragraphs)
                        {
                            var text = paragraph.Text.Trim();
                            specs.Add(new ImageSpec
                            {
                                SpecKey = "info_description",
                                SpecValue = text
                            });
                        }
                    }
                }


                // 3. product_features
                var featureBlocks = driver.FindElements(By.XPath("//div[contains(@class, 'special-m')]/.."));
                foreach (var block in featureBlocks)
                {
                    var title = block.FindElement(By.CssSelector("div.special-m")).Text.Trim();
                    var paragraphs = block.FindElements(By.TagName("p"));
                    var content = string.Join(" ", paragraphs.Select(p => p.Text.Trim()));

                    specs.Add(new ImageSpec
                    {
                        SpecKey = "product_features",
                        SpecValue = $"{title}: {content}"
                    });
                }

                // 4. product_features (h4)
                var featureH4Blocks = driver.FindElements(By.XPath("//div[h4]"));
                foreach (var h4Block in featureH4Blocks)
                {
                    var title = h4Block.FindElement(By.TagName("h4")).Text.Trim();
                    var paragraphs = h4Block.FindElements(By.TagName("p"));
                    var content = string.Join(" ", paragraphs.Select(p => p.Text.Trim()));

                    specs.Add(new ImageSpec
                    {
                        SpecKey = "product_features",
                        SpecValue = $"{title}: {content}"
                    });
                }

                // 5. specifications tab
                try
                {
                    var hasSpecs = driver.FindElements(By.CssSelector("div.sp__accordions")).Count > 0;
                    if (!hasSpecs)
                    {
                        var specTab = driver.FindElement(By.XPath("//div[contains(@class, 'pdp__tab') and @data-label='product-specifications']"));
                        specTab.Click();
                        Thread.Sleep(500);
                    }

                    var accordionSection = driver.FindElement(By.CssSelector("div.sp__accordions"));
                    var keys = accordionSection.FindElements(By.CssSelector("span.ac__key"));
                    var values = accordionSection.FindElements(By.CssSelector("span.ac__value"));

                    for (int i = 0; i < Math.Min(keys.Count, values.Count); i++)
                    {
                        specs.Add(new ImageSpec
                        {
                            SpecKey = keys[i].Text.Trim(),
                            SpecValue = values[i].Text.Trim()
                        });
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("抓取Spec tab資訊錯誤: " + ex.Message);
                }

                // 分配到每張圖片
                foreach (var image in product.ProductImages)
                {
                    foreach (var s in specs)
                    {
                        result.Add(new ImageSpec
                        {
                            ImageId = image.ImageId,
                            SpecKey = s.SpecKey,
                            SpecValue = s.SpecValue
                        });
                    }
                }

                Console.WriteLine($"產品 {product.Name} 的規格已處理完成，共 {specs.Count} 項規格。");
            }
            
            driver.Quit();

            return result;
    }
}
}
