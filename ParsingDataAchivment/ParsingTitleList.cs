using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Linq;

namespace ParsingDataAchivment
{
    internal class ParsingTitleList
    {
        private readonly IWebDriver driver;
        private readonly IWebElement? xpath_title;

        public string Status { get; set; } = "Пусто";
        public string Tariff { get; set; } = "Пусто";
        public string Application { get; set; } = "Пусто";
        public string StartPattern { get; set; } = "Пусто";
        public string DataRegistration { get; set; } = "Пусто";
        public string DataSend { get; set; } = "Пусто";
        public string DataPublic { get; set; } = "Пусто";
        public string ListDocumentCitationInReport { get; set; } = "Пусто";
        public string AdresToCommunication { get; set; } = "Пусто";
        public string Author { get; set; } = "Пусто";
        public string PatentHolder { get; set; } = "Пусто";
        public string Title { get; set; } = "Пусто";
        public string Color { get; set; } = "Пусто";

        // Старый вариант: есть элемент, по которому надо кликнуть
        public ParsingTitleList(IWebElement xpath, IWebDriver driver, string id) : this(driver, id)
        {
            xpath_title = xpath;
        }

        // Новый вариант: страница уже открыта, кликать не нужно
        public ParsingTitleList(IWebDriver driver, string id)
        {
            this.driver = driver;
            Application = id;
        }

        public void GetInfo(bool clickElement = true)
        {
            try
            {
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

                string? originalWindow = null;
                string? newWindowHandle = null;

                if (clickElement && xpath_title != null)
                {
                    originalWindow = driver.CurrentWindowHandle;
                    xpath_title.Click();

                    try
                    {
                        wait.Until(d =>
                            d.WindowHandles.Count > 1 ||
                            d.FindElements(By.XPath("//*[@id='mainDoc']")).Count > 0);
                    }
                    catch
                    {
                        // если окно/страница открываются медленно
                    }

                    if (driver.WindowHandles.Count > 1)
                    {
                        newWindowHandle = driver.WindowHandles.First(h => h != originalWindow);
                        driver.SwitchTo().Window(newWindowHandle);
                    }
                }

                wait.Until(d => d.FindElements(By.XPath("//*[@id='mainDoc']")).Count > 0);

                string FindElementForXPath(string xpath)
                {
                    try
                    {
                        var element = driver.FindElements(By.XPath(xpath)).FirstOrDefault();
                        return element?.Text.Trim() ?? "Пусто";
                    }
                    catch
                    {
                        return "Пусто";
                    }
                }

                Status = FindElementForXPath("//*[@id='mainDoc']/table[2]/tbody/tr[1]/td[2]");
                Tariff = FindElementForXPath("//*[@id='mainDoc']/table[2]/tbody/tr[2]/td[2]");
                StartPattern = FindElementForXPath("//*[@id=\"bib\"]/tbody/tr[2]/td[1]/p[2]/b").Replace(".", "-");
                DataRegistration = FindElementForXPath("//*[@id=\"bib\"]/tbody/tr[2]/td[1]/p[3]/b").Replace(".", "-");
                DataSend = FindElementForXPath("//*[@id=\"bib\"]/tbody/tr[2]/td[1]/p[5]/b").Replace(".", "-");
                DataPublic = FindElementForXPath("//*[@id=\"bib\"]/tbody/tr[2]/td[1]/p[6]/b[1]/a").Replace(".", "-");
                ListDocumentCitationInReport = FindElementForXPath("//*[@id=\"bib\"]/tbody/tr[2]/td[1]/p[7]/b");
                AdresToCommunication = FindElementForXPath("//*[@id=\"bib\"]/tbody/tr[2]/td[1]/p[8]/b");
                Author = FindElementForXPath("//*[@id=\"bibl\"]/p[1]/b");
                PatentHolder = FindElementForXPath("//*[@id=\"bibl\"]/p[2]/b");
                Title = FindElementForXPath("//*[@id=\"B542\"]/b");

                IWebElement color = driver.FindElement(By.XPath("//*[@id=\"mainDoc\"]/table[2]/tbody/tr[1]"));
                Color = color.GetAttribute("class") ?? "Пусто";

                if (newWindowHandle != null && originalWindow != null)
                {
                    driver.Close();
                    driver.SwitchTo().Window(originalWindow);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при получении информации: {ex.Message}");
                Console.WriteLine($"Текущий URL: {driver.Url}");
            }
        }
    }
}
