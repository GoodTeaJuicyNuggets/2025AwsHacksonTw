using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoolerMaster.ImageAi.Crawler.Services
{
    public class CrawlerSharedService
    {
        public void HandlePopups(IWebDriver driver)
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
}
