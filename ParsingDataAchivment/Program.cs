using HtmlAgilityPack;
using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;

namespace FIPSPatentParser
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(new string('=', 60));
            Console.WriteLine("ПАРСЕР РЕЕСТРОВ ФИПС");
            Console.WriteLine(new string('=', 60));

            // ВАРИАНТ 1: Парсинг одного конкретного патента (для тестирования)
            string testUrl = "https://www1.fips.ru/registers-doc-view/fips_servlet?DB=RUPAT&DocNumber=2858419&TypeFile=html";

            Console.WriteLine($"\nТестовый запуск на одном патенте:");
            Console.WriteLine($"URL: {testUrl}");

            var driver = new ChromeDriver();
            try
            {
                driver.Navigate().GoToUrl(testUrl);
                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(15));
                wait.Until(drv => drv.FindElement(By.Id("mainDoc")));
                Thread.Sleep(1000);

                string html = driver.PageSource;
                var patentData = ParsePatentPage(html, testUrl);

                // Сохраняем результат
                var resultList = new List<Dictionary<string, object>> { patentData };
                string json = JsonConvert.SerializeObject(resultList, Newtonsoft.Json.Formatting.Indented);
                File.WriteAllText("test_patent.json", json, Encoding.UTF8);

                Console.WriteLine("✓ Результат сохранен в test_patent.json");

                // Выводим краткую информацию
                if (patentData.ContainsKey("patent_number_full"))
                {
                    Console.WriteLine($"  Номер: {patentData["patent_number_full"]}");
                    string title = patentData.ContainsKey("title") ? patentData["title"].ToString() : "Н/Д";
                    Console.WriteLine($"  Название: {(title.Length > 100 ? title.Substring(0, 100) : title)}...");

                    var statusInfo = patentData.ContainsKey("status_info")
                        ? (Dictionary<string, object>)patentData["status_info"]
                        : null;
                    string status = statusInfo != null && statusInfo.ContainsKey("status")
                        ? statusInfo["status"].ToString()
                        : "Н/Д";
                    Console.WriteLine($"  Статус: {status}");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Ошибка при тестировании: {e.Message}");
            }
            finally
            {
                driver.Quit();
            }

            Console.WriteLine("\n" + new string('=', 60));
            Console.WriteLine("ИНСТРУКЦИЯ ДЛЯ СТУДЕНТОВ");
            Console.WriteLine(new string('=', 60));
            Console.WriteLine(@"
    1. Установите NuGet пакеты:
       Install-Package Selenium.WebDriver
       Install-Package Selenium.WebDriver.ChromeDriver
       Install-Package HtmlAgilityPack
       Install-Package Newtonsoft.Json

    2. Для сбора данных за год нужно:
       - Перейти на страницу реестра (https://www1.fips.ru/registers-web/)
       - Выбрать раздел ""Реестр изобретений"" или ""Полезные модели""
       - Найти навигацию по номерам/годам
       - Собрать ссылки на патенты за нужный год
       - Передать ссылки в функцию CollectPatentsFromList()

    3. Функция ParsePatentPage() извлекает все поля согласно структуре JSON.
            ");
        }

        /// <summary>
        /// Парсит HTML-код страницы патента и возвращает словарь с данными.
        /// </summary>
        static Dictionary<string, object> ParsePatentPage(string htmlSource, string url)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(htmlSource);

            var patentData = new Dictionary<string, object>
            {
                ["url"] = url
            };

            // 1. Основные реквизиты (11), (13) и заголовок
            try
            {
                // Номер патента (11)
                var numberElem = doc.DocumentNode.SelectSingleNode("//div[@id='top4' and contains(@class, 'topfield2')]//a");
                if (numberElem != null)
                {
                    patentData["patent_number_full"] = numberElem.InnerText.Trim();
                    patentData["patent_id"] = Regex.Replace(patentData["patent_number_full"].ToString(), @"\s+", "");
                }

                // Тип документа (13)
                var typeElem = doc.DocumentNode.SelectSingleNode("//div[@id='top6' and contains(@class, 'topfield2')]");
                if (typeElem != null)
                {
                    patentData["patent_type_code"] = typeElem.InnerText.Trim();
                }

                // Определяем тип патента (ИЗ или ПМ) по заголовку
                var titleDiv = doc.DocumentNode.SelectSingleNode("//div[@id='NameDoc']");
                if (titleDiv != null)
                {
                    string titleText = titleDiv.InnerText.Replace("\n", " ").Trim();
                    if (titleText.ToUpper().Contains("ИЗОБРЕТЕНИЯ"))
                        patentData["patent_type"] = "ИЗ";
                    else if (titleText.ToUpper().Contains("ПОЛЕЗНОЙ МОДЕЛИ"))
                        patentData["patent_type"] = "ПМ";
                    else
                        patentData["patent_type"] = "Н/Д";
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"  Ошибка парсинга реквизитов: {e.Message}");
            }

            // 2. Название изобретения (54)
            try
            {
                var b542Tag = doc.DocumentNode.SelectSingleNode("//p[@id='B542']");
                if (b542Tag != null)
                {
                    var boldTag = b542Tag.SelectSingleNode(".//b");
                    if (boldTag != null)
                    {
                        patentData["title"] = boldTag.InnerText.Trim();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"  Ошибка парсинга названия: {e.Message}");
            }

            // 3. Статус патента
            try
            {
                var statusRow = doc.DocumentNode.SelectSingleNode("//table[contains(@class, 'Status')]/tr");
                if (statusRow != null)
                {
                    var statusClass = statusRow.GetAttributeValue("class", "");
                    if (!string.IsNullOrEmpty(statusClass))
                    {
                        patentData["status_color"] = statusClass;
                    }

                    var statusTextElem = statusRow.SelectSingleNode(".//td[contains(@class, 'StatusR')]");
                    if (statusTextElem != null)
                    {
                        string fullStatus = statusTextElem.InnerText.Trim();
                        var match = Regex.Match(fullStatus, @"(.+?)\((.+?)\)");
                        if (match.Success)
                        {
                            var statusInfo = new Dictionary<string, object>
                            {
                                ["status"] = match.Groups[1].Value.Trim(),
                                ["last_change"] = match.Groups[2].Value
                                    .Replace("последнее изменение статуса:", "")
                                    .Trim()
                            };
                            patentData["status_info"] = statusInfo;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"  Ошибка парсинга статуса: {e.Message}");
            }

            // 4. Детали заявки из таблицы #bib
            try
            {
                var bibTable = doc.DocumentNode.SelectSingleNode("//table[@id='bib']");
                if (bibTable != null)
                {
                    var allTds = bibTable.SelectNodes(".//td");
                    if (allTds != null && allTds.Count > 0)
                    {
                        var leftCol = allTds[0];
                        var pTags = leftCol.SelectNodes(".//p");
                        var appDetails = new Dictionary<string, object>();

                        if (pTags != null)
                        {
                            foreach (var p in pTags)
                            {
                                string pText = p.InnerText.Replace("\n", " ").Trim();

                                if (pText.Contains("Заявка:"))
                                {
                                    var link = p.SelectSingleNode(".//a");
                                    if (link != null)
                                        appDetails["application_number"] = link.InnerText.Trim();

                                    var dateMatch = Regex.Match(pText, @"\d{2}\.\d{2}\.\d{4}");
                                    if (dateMatch.Success)
                                        appDetails["application_date"] = dateMatch.Value;
                                }
                                else if (pText.Contains("Дата регистрации:"))
                                {
                                    var dateMatch = Regex.Match(pText, @"\d{2}\.\d{2}\.\d{4}");
                                    if (dateMatch.Success)
                                        appDetails["registration_date"] = dateMatch.Value;
                                }
                                else if (pText.Contains("Опубликовано:"))
                                {
                                    var dateMatch = Regex.Match(pText, @"\d{2}\.\d{2}\.\d{4}");
                                    if (dateMatch.Success)
                                        appDetails["publication_date"] = dateMatch.Value;

                                    var bullMatch = Regex.Match(pText, @"Бюл\.\s*№\s*(\d+)");
                                    if (bullMatch.Success)
                                        appDetails["publication_bulletin_number"] = bullMatch.Groups[1].Value;
                                }
                                else if (pText.Contains("Адрес для переписки:"))
                                {
                                    var addressBold = p.SelectSingleNode(".//b");
                                    if (addressBold != null)
                                        patentData["correspondence_address"] = addressBold.InnerText.Replace("\n", " ").Trim();
                                }
                            }
                        }

                        if (appDetails.Count > 0)
                            patentData["application_details"] = appDetails;
                    }

                    // Правая колонка (авторы и патентообладатели)
                    var rightCol = bibTable.SelectSingleNode(".//td[@id='bibl']");
                    if (rightCol != null)
                    {
                        var pTags = rightCol.SelectNodes(".//p");
                        if (pTags != null)
                        {
                            foreach (var p in pTags)
                            {
                                string pText = p.InnerText.Trim();

                                if (pText.Contains("Автор(ы):") || pText.Contains("(72)"))
                                {
                                    var inventors = new List<string>();
                                    var bTag = p.SelectSingleNode(".//b");
                                    if (bTag != null)
                                    {
                                        var brNodes = bTag.SelectNodes(".//br");
                                        if (brNodes != null && brNodes.Count > 0)
                                        {
                                            string[] parts = bTag.InnerHtml.Split(new[] { "<br>", "<br/>" },
                                                StringSplitOptions.RemoveEmptyEntries);
                                            foreach (var part in parts)
                                            {
                                                string cleaned = part.Trim();
                                                if (!string.IsNullOrEmpty(cleaned) && !cleaned.Contains("Автор(ы)"))
                                                    inventors.Add(cleaned);
                                            }
                                        }
                                        else
                                        {
                                            inventors.Add(bTag.InnerText.Trim());
                                        }
                                    }

                                    if (inventors.Count > 0)
                                        patentData["inventors"] = inventors;
                                }
                                else if (pText.Contains("Патентообладатель(и):") || pText.Contains("(73)"))
                                {
                                    var owners = new List<string>();
                                    var bTag = p.SelectSingleNode(".//b");
                                    if (bTag != null)
                                    {
                                        string[] parts = bTag.InnerHtml.Split(new[] { "<br>", "<br/>" },
                                            StringSplitOptions.RemoveEmptyEntries);
                                        foreach (var part in parts)
                                        {
                                            string cleaned = part.Trim();
                                            if (!string.IsNullOrEmpty(cleaned) && !cleaned.Contains("Патентообладатель"))
                                                owners.Add(cleaned);
                                        }
                                    }

                                    if (owners.Count > 0)
                                        patentData["owners"] = owners;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"  Ошибка парсинга таблицы заявки: {e.Message}");
            }

            // 5. МПК (51) и СПК (52)
            try
            {
                // МПК
                var ipcUl = doc.DocumentNode.SelectSingleNode("//ul[contains(@class, 'ipc')]");
                if (ipcUl != null)
                {
                    var ipcList = new List<Dictionary<string, string>>();
                    var liItems = ipcUl.SelectNodes(".//li");
                    if (liItems != null)
                    {
                        foreach (var li in liItems)
                        {
                            var link = li.SelectSingleNode(".//a");
                            if (link != null)
                            {
                                string codeText = link.InnerText.Trim();
                                var versionMatch = Regex.Match(codeText, @"\(([^)]+)\)");
                                if (versionMatch.Success)
                                {
                                    string version = versionMatch.Groups[1].Value;
                                    string code = codeText.Replace($"({version})", "").Trim();
                                    ipcList.Add(new Dictionary<string, string>
                                    {
                                        ["code"] = code,
                                        ["version"] = version
                                    });
                                }
                            }
                        }
                    }

                    if (ipcList.Count > 0)
                        patentData["ipc"] = ipcList;
                }

                // СПК
                var spkTd = doc.DocumentNode.SelectSingleNode("//td[contains(@class, 'spk')]");
                if (spkTd != null)
                {
                    var cpcList = new List<Dictionary<string, string>>();
                    var boldCode = spkTd.SelectSingleNode(".//b");
                    if (boldCode != null)
                    {
                        string codeText = boldCode.InnerText.Trim();
                        var versionMatch = Regex.Match(spkTd.InnerText, @"\(([^)]+)\)");
                        if (versionMatch.Success)
                        {
                            string version = versionMatch.Groups[1].Value;
                            cpcList.Add(new Dictionary<string, string>
                            {
                                ["code"] = codeText,
                                ["version"] = version
                            });
                        }
                    }

                    if (cpcList.Count > 0)
                        patentData["cpc"] = cpcList;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"  Ошибка парсинга классификации: {e.Message}");
            }

            // 6. Реферат (57)
            try
            {
                var absDiv = doc.DocumentNode.SelectSingleNode("//div[@id='Abs']");
                if (absDiv != null)
                {
                    var pTags = absDiv.SelectNodes(".//p");
                    if (pTags != null && pTags.Count > 1)
                    {
                        string abstractText = pTags[1].InnerText.Replace("\n", " ").Trim();
                        if (abstractText.Contains("Изображение"))
                            abstractText = abstractText.Split(new[] { "Изображение" }, StringSplitOptions.None)[0].Trim();

                        if (!string.IsNullOrEmpty(abstractText))
                            patentData["abstract"] = abstractText;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"  Ошибка парсинга реферата: {e.Message}");
            }

            // 7. Отчет о поиске (56)
            try
            {
                var searchP = doc.DocumentNode.SelectSingleNode("//p[contains(@class, 'B560')]");
                if (searchP != null)
                {
                    var boldTag = searchP.SelectSingleNode(".//b");
                    if (boldTag != null)
                        patentData["search_report"] = boldTag.InnerText.Trim();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"  Ошибка парсинга отчета о поиске: {e.Message}");
            }

            // 8. Формула изобретения
            try
            {
                var claimsStart = doc.DocumentNode.SelectSingleNode("//a[@href='ClStart']");
                if (claimsStart != null)
                {
                    var claimsList = new List<string>();
                    var current = claimsStart.NextSibling;
                    bool foundEnd = false;

                    while (current != null && !foundEnd)
                    {
                        if (current.Name == "a")
                        {
                            var href = current.GetAttributeValue("href", "");
                            if (href == "ClEnd")
                            {
                                foundEnd = true;
                                break;
                            }
                        }

                        if (current.Name == "p")
                        {
                            string text = current.InnerText.Replace("\n", " ").Trim();
                            if (!string.IsNullOrEmpty(text))
                                claimsList.Add(text);
                        }

                        current = current.NextSibling;
                    }

                    if (claimsList.Count > 0)
                        patentData["claims"] = string.Join("\n", claimsList);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"  Ошибка парсинга формулы: {e.Message}");
            }

            // 9. Описание изобретения (разделы)
            try
            {
                var descStart = doc.DocumentNode.SelectSingleNode("//a[@href='DeStart']");
                if (descStart != null)
                {
                    var sections = new List<Dictionary<string, object>>();
                    var currentSection = new Dictionary<string, object>();
                    var current = descStart.NextSibling;
                    bool foundEnd = false;

                    while (current != null && !foundEnd)
                    {
                        if (current.Name == "a")
                        {
                            var href = current.GetAttributeValue("href", "");
                            if (href == "DeEnd")
                            {
                                foundEnd = true;
                                break;
                            }
                        }

                        // Новый раздел с номером
                        if (current.Name == "p" && current.Attributes.Contains("num"))
                        {
                            if (currentSection.ContainsKey("num"))
                                sections.Add(currentSection);

                            string pText = current.InnerText.Replace("\n", " ").Trim();
                            string sectionNum = current.GetAttributeValue("num", "");
                            string sectionTitle = pText;

                            var parts = pText.Split(new[] { ". " }, StringSplitOptions.None);
                            if (parts.Length > 1 && int.TryParse(parts[0], out _))
                            {
                                sectionNum = parts[0];
                                sectionTitle = parts[1];
                            }

                            currentSection = new Dictionary<string, object>
                            {
                                ["num"] = sectionNum,
                                ["title"] = sectionTitle,
                                ["text"] = ""
                            };
                        }
                        // Добавляем текст к текущему разделу
                        else if (current.Name == "p" && currentSection.ContainsKey("num"))
                        {
                            string text = current.InnerText.Replace("\n", " ").Trim();
                            if (!text.ToLower().Contains("фиг") && text.Length > 20)
                            {
                                if (!string.IsNullOrEmpty(currentSection["text"].ToString()))
                                    currentSection["text"] = currentSection["text"] + "\n" + text;
                                else
                                    currentSection["text"] = text;
                            }
                        }

                        current = current.NextSibling;
                    }

                    // Добавляем последний раздел
                    if (currentSection.ContainsKey("num"))
                        sections.Add(currentSection);

                    if (sections.Count > 0)
                        patentData["description_sections"] = sections;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"  Ошибка парсинга описания: {e.Message}");
            }

            return patentData;
        }

        /// <summary>
        /// Собирает данные по списку URL патентов и сохраняет в JSON.
        /// </summary>
        static List<Dictionary<string, object>> CollectPatentsFromList(List<string> patentUrls, string outputFile = "patents_data.json")
        {
            var driver = new ChromeDriver();
            var allPatents = new List<Dictionary<string, object>>();

            try
            {
                for (int i = 0; i < patentUrls.Count; i++)
                {
                    string url = patentUrls[i];
                    Console.WriteLine($"Обработка патента {i + 1}/{patentUrls.Count}: {url}");

                    try
                    {
                        driver.Navigate().GoToUrl(url);
                        var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(15));
                        wait.Until(drv => drv.FindElement(By.Id("mainDoc")));
                        Thread.Sleep(1000);

                        string html = driver.PageSource;
                        var patentData = ParsePatentPage(html, url);

                        if (patentData.Count > 0)
                        {
                            allPatents.Add(patentData);
                            string patentNumber = patentData.ContainsKey("patent_number_full")
                                ? patentData["patent_number_full"].ToString()
                                : "N/A";
                            Console.WriteLine($"  ✓ Успешно спарсен патент {patentNumber}");
                        }
                        else
                        {
                            Console.WriteLine($"  ✗ Не удалось извлечь данные");
                        }

                        // Сохраняем после каждого патента
                        string json = JsonConvert.SerializeObject(allPatents, Newtonsoft.Json.Formatting.Indented);
                        File.WriteAllText(outputFile, json, Encoding.UTF8);

                        Thread.Sleep(2000);
                    }
                    catch (WebDriverTimeoutException)
                    {
                        Console.WriteLine($"  ✗ Таймаут при загрузке {url}");
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"  ✗ Ошибка при обработке {url}: {e.Message}");
                    }
                }

                Console.WriteLine($"\nГотово! Обработано {allPatents.Count} патентов.");
                return allPatents;
            }
            finally
            {
                driver.Quit();
            }
        }

        /// <summary>
        /// Функция для получения списка URL патентов за определенный год.
        /// Это упрощенная версия - студенты должны реализовать логику навигации по реестру.
        /// </summary>
        static List<string> GetPatentUrlsForYear(string baseUrlTemplate, int year, int numPages = 5)
        {
            Console.WriteLine($"Эта функция должна собирать URL патентов за {year} год");
            Console.WriteLine("В текущей версии используйте готовый список URL для тестирования");
            return new List<string>();
        }
    }
}