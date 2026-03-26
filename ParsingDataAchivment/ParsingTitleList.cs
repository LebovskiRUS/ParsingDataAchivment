using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;

namespace ParsingDataAchivment
{
    internal class ParsingTitleList
    {
        private IWebDriver driver;
        private IWebElement? xpath_title;
        public string Status { get; set; }
        public string Tariff { get; set; }
        public string Application { get; set; }

        public ParsingTitleList(IWebElement xpath, IWebDriver driver)
        {
            this.xpath_title = xpath;
            this.driver = driver;
            GetInfo();
        }

        public void GetInfo()
        {
            try
            {
                // Получаем текущие handle перед кликом
                string originalWindow = driver.CurrentWindowHandle;

                xpath_title.Click();

                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

                // Проверяем, открылось ли новое окно
                wait.Until(driver => driver.WindowHandles.Count > 1);

                // Если открылось новое окно, переключаемся на него
                if (driver.WindowHandles.Count > 1)
                {
                    foreach (string handle in driver.WindowHandles)
                    {
                        if (handle != originalWindow)
                        {
                            driver.SwitchTo().Window(handle);
                            break;
                        }
                    }

                    // Ждем загрузки содержимого в новом окне
                    wait.Until(driver => driver.FindElement(By.XPath("//*[@id='mainDoc']")));
                }
                else
                {
                    // Если новое окно не открылось, ждем обновления содержимого текущей страницы
                    wait.Until(driver => driver.FindElement(By.XPath("//*[@id='mainDoc']")));
                }

                // Теперь ищем элементы на актуальной странице
                var statusElement = wait.Until(driver =>driver.FindElement(By.XPath("//*[@id='mainDoc']/table[2]/tbody/tr[1]/td[2]")));
                Status = statusElement.Text.Trim();

                var tariffElement = wait.Until(driver =>driver.FindElement(By.XPath("//*[@id='mainDoc']/table[2]/tbody/tr[2]/td[2]")));
                Tariff = tariffElement.Text.Trim();

                var applicationElement = wait.Until(driver => driver.FindElement(By.XPath("//*[@id=\"bib\"]/tbody/tr[2]/td[1]/p[1]/b/a")));
                Application = applicationElement.Text.Trim();

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при получении информации: {ex.Message}");
                Console.WriteLine($"Текущий URL: {driver.Url}");
                Status = "Не найден";
                Tariff = "Не найден";
            }
        }
    }
}