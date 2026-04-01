using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Threading.Tasks;

namespace ParsingDataAchivment
{
    class Program
    {
        public static async Task Main(string[] args)
        {
            //======================== Уберите два комента ниже, Чтобы код заработал, поставил навсякий =======================================
            //await ParsingPatentAndModel(UrlInfo.urls[0], "//*[@id=\"mainpagecontent\"]/div[2]/div/div[2]/div/table/tbody/tr[2]/td[2]/a", 2700000, 2799999); //патенты
            //await ParsingPatentAndModel(UrlInfo.urls[1], "//*[@id=\"mainpagecontent\"]/div[2]/div/div[2]/div/table/tbody/tr[3]/td[2]/a", 240000, 299999); //модели
        }

        static void PrintPatentInfo(ParsingTitleList patent)
        {
            Console.WriteLine("Статус: " + patent.Status);
            Console.WriteLine("Пошлина: " + patent.Tariff);
            Console.WriteLine("Заявка: " + patent.Application);
            Console.WriteLine("Дата начала отсчета срока действия патента: " + patent.StartPattern);
            Console.WriteLine("Дата Регистрации: " + patent.DataRegistration);
            Console.WriteLine("Дата Отправки: " + patent.DataSend);
            Console.WriteLine("Дата Публикации: " + patent.DataPublic);
            Console.WriteLine("Список документов, цитированных в отчете о поиске: " + patent.ListDocumentCitationInReport);
            Console.WriteLine("Адрес для переписки: " + patent.AdresToCommunication);
            Console.WriteLine("Автор(ы): " + patent.Author);
            Console.WriteLine("Патентообладатель(и): " + patent.PatentHolder);
            Console.WriteLine("Название патента: " + patent.Title);
            Console.WriteLine("Цвет: " + patent.Color);
        }

        static async Task ParsingPatentAndModel(string url, string xp,int diap1, int diap2)
        {
            var options = new ChromeOptions();

            // Если хочешь меньше грузить ПК — можно включить headless
            //options.AddArgument("--headless=new");

            options.AddArgument("--disable-gpu");
            options.AddArgument("--disable-notifications");
            options.AddArgument("--no-sandbox");
            options.AddArgument("--disable-dev-shm-usage");

            // Отключаем картинки
            options.AddUserProfilePreference("profile.managed_default_content_settings.images", 2);

            // Чтобы не ждать полной загрузки всех ресурсов
            options.PageLoadStrategy = PageLoadStrategy.Eager;

            using IWebDriver driver = new ChromeDriver(options);
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(12));

            try
            {
                // Открываем страницу один раз
                driver.Navigate().GoToUrl(url);

                var open = wait.Until(d =>
                    d.FindElement(By.XPath(xp)));
                open.Click();

                // Ждём поле поиска
                wait.Until(d => d.FindElements(By.Id("searchParValue")).Count > 0);

                string searchPageHandle = driver.CurrentWindowHandle;

                for (int patentId = diap1; patentId <= diap2; patentId++)
                {

                    try
                    {
                        // Запоминаем окна до поиска
                        var handlesBefore = driver.WindowHandles.ToList();

                        var search = wait.Until(d => d.FindElement(By.Id("searchParValue")));
                        search.Clear();
                        search.SendKeys(patentId.ToString());
                        search.SendKeys(Keys.Enter);

                        // Ждём либо новую вкладку, либо появление карточки
                        wait.Until(d =>
                            d.WindowHandles.Count != handlesBefore.Count ||
                            d.FindElements(By.Id("mainDoc")).Count > 0 ||
                            d.FindElements(By.Id("B542")).Count > 0 ||
                            d.FindElements(By.Id("bibl")).Count > 0);

                        // Если открылось новое окно — переключаемся на него
                        if (driver.WindowHandles.Count > handlesBefore.Count)
                        {
                            var newHandle = driver.WindowHandles.First(h => !handlesBefore.Contains(h));
                            driver.SwitchTo().Window(newHandle);
                        }

                        // Даём странице ещё чуть времени появиться
                        wait.Until(d =>
                            d.FindElements(By.Id("mainDoc")).Count > 0 ||
                            d.FindElements(By.Id("B542")).Count > 0 ||
                            d.FindElements(By.Id("bibl")).Count > 0);

                        // Проверка: если карточки нет, пропускаем номер
                        bool patentPageExists =
                            driver.FindElements(By.Id("mainDoc")).Count > 0 ||
                            driver.FindElements(By.Id("B542")).Count > 0 ||
                            driver.FindElements(By.Id("bibl")).Count > 0;

                        if (!patentPageExists)
                        {
                            Console.WriteLine($"[{patentId}] не найден");
                            ReturnToSearchPage(driver, wait, url, searchPageHandle);
                            continue;
                        }

                        var patent = new JsonCreater(driver, patentId.ToString());
                        patent.GetInfo(false);

                        Console.WriteLine($"[{patentId}] найден");
                        PrintPatentInfo(patent);

                        await patent.CreateJsonAsync();

                        // Возвращаемся назад к поиску
                        ReturnToSearchPage(driver, wait, url, searchPageHandle);
                    }
                    catch (WebDriverTimeoutException)
                    {
                        Console.WriteLine($"[{patentId}] не найден / таймаут");
                        ReturnToSearchPage(driver, wait, url, searchPageHandle);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[{patentId}] ошибка: {ex.Message}");
                        ReturnToSearchPage(driver, wait, url, searchPageHandle);
                    }

                    await Task.Delay(3000); // чтобы сайт на быстрый парсер не ругался
                }
            }
            finally
            {
                driver.Quit();
            }
        }

        static void ReturnToSearchPage(IWebDriver driver, WebDriverWait wait, string url, string searchPageHandle)
        {
            try
            {
                // Если открылась новая вкладка — закрываем её и возвращаемся
                if (driver.CurrentWindowHandle != searchPageHandle)
                {
                    driver.Close();
                    driver.SwitchTo().Window(searchPageHandle);
                }

                // Если мы в том же окне — возвращаемся назад
                if (driver.Url != url)
                {
                    driver.Navigate().Back();
                }

                wait.Until(d => d.FindElements(By.Id("searchParValue")).Count > 0);
            }
            catch
            {
                // Если Back не сработал — просто заново открываем страницу поиска
                driver.Navigate().GoToUrl(url);
                wait.Until(d => d.FindElements(By.Id("searchParValue")).Count > 0);
            }
        }


    }
}
