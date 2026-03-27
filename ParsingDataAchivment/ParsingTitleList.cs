using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Security.Cryptography.X509Certificates;

namespace ParsingDataAchivment
{
    internal class ParsingTitleList
    {
        private IWebDriver driver;
        private IWebElement? xpath_title;
        public string Status { get; set; }
        public string Tariff { get; set; } //Пошлина
        public string Application { get; set; } //Заявка
        public string StartPattern { get; set; } //Дата начала отсчета срока действия патента:
        public string DataRegistration { get; set; }
        public string DataSend { get; set; }
        public string DataPublic {  get; set; }
        public string ListDocumentCitationInReport { get; set; } //Список документов, цитированных в отчете о поиске:
        public string AdresToCommunication { get; set; }
        public string Author {  get; set; }
        public string PatentHolder { get; set; } //Патентообладатель
        public string Title { get; set; }
        public string Color { get; set; }

        public ParsingTitleList(IWebElement xpath, IWebDriver driver, string id)
        {
            Application = id;
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

                string FindElementForXPath(string xpath)
                {
                    try
                    {
                        var elements = driver.FindElements(By.XPath(xpath));
                        if (elements.Count > 0)
                        {
                            return elements[0].Text.Trim();
                        }
                        return "Пусто";
                    }
                    catch (OpenQA.Selenium.NoSuchElementException)
                    {
                        return "Пусто";
                    }
                }

                // Теперь ищем элементы на актуальной странице
                Status = FindElementForXPath("//*[@id='mainDoc']/table[2]/tbody/tr[1]/td[2]");
                Tariff = FindElementForXPath("//*[@id='mainDoc']/table[2]/tbody/tr[2]/td[2]");
                //Application = FindElementForXPath("//*[@id=\"bib\"]/tbody/tr[2]/td[1]/p[1]/b/a");
                StartPattern = FindElementForXPath("//*[@id=\"bib\"]/tbody/tr[2]/td[1]/p[2]/b").Replace(".", "-");
                DataRegistration = FindElementForXPath("//*[@id=\"bib\"]/tbody/tr[2]/td[1]/p[3]/b").Replace(".", "-");
                DataSend = FindElementForXPath("//*[@id=\"bib\"]/tbody/tr[2]/td[1]/p[5]/b").Replace(".", "-");
                DataPublic = FindElementForXPath("//*[@id=\"bib\"]/tbody/tr[2]/td[1]/p[6]/b[1]/a").Replace(".", "-");
                ListDocumentCitationInReport = FindElementForXPath("//*[@id=\"bib\"]/tbody/tr[2]/td[1]/p[7]/b");//загнать реплес через регикс что бы точки не потерять
                AdresToCommunication = FindElementForXPath("//*[@id=\"bib\"]/tbody/tr[2]/td[1]/p[8]/b");
                Author = FindElementForXPath("//*[@id=\"bibl\"]/p[1]/b");
                PatentHolder = FindElementForXPath("//*[@id=\"bibl\"]/p[2]/b");
                Title = FindElementForXPath("//*[@id=\"B542\"]/b");
                IWebElement color = driver.FindElement(By.XPath("//*[@id=\"mainDoc\"]/table[2]/tbody/tr[1]"));
                Color = color.GetAttribute("class");

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при получении информации: {ex.Message}");
                Console.WriteLine($"Текущий URL: {driver.Url}");
            }
        }

    }
}