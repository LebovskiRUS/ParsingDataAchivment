using HtmlAgilityPack;
using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using ParsingDataAchivment;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;

namespace ParsingDataAchivment
{
    class Program
    {
        public static void Main(string[] args)
        {

            ParsingReestrInvation(UrlInfo.urls[0]);
            //ParsingUsellesModel(UrlInfo.urls[1]);

            void ParsingUsellesModel(string url)
            {
                using (IWebDriver driver = new ChromeDriver())
                {
                    try
                    {
                        Console.WriteLine("Открываем url");
                        driver.Navigate().GoToUrl(url);

                        WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(4));
                        var oneElement = wait.Until(driver => driver.FindElement(By.XPath("//*[@id=\"mainpagecontent\"]/div[2]/div/div[2]/div/table/tbody/tr[3]/td[2]/a")));
                        oneElement.Click();

                        var twoElement = wait.Until(driver => driver.FindElement(By.XPath("//*[@id=\"mainpagecontent\"]/div[2]/div[2]/div/ul/ul/li[2]/a[2]")));
                        twoElement.Click();

                        var threeElement = wait.Until(driver => driver.FindElement(By.XPath("//*[@id=\"mainpagecontent\"]/div[2]/div[2]/div/ul/ul/ul/li[1]/a[2]")));
                        threeElement.Click();

                        var fourElement = wait.Until(driver => driver.FindElement(By.XPath("//*[@id=\"mainpagecontent\"]/div[2]/div[2]/div/ul/ul/ul/ul/li[1]/a[2]")));
                        fourElement.Click();

                        var fifeElement = wait.Until(driver => driver.FindElement(By.XPath("//*[@id=\"mainpagecontent\"]/div[2]/div[2]/div/ul/ul/ul/ul/ul/li[1]/a")));
                        fifeElement.Click();

                        var sixElement = wait.Until(driver => driver.FindElement(By.XPath("//*[@id=\"mainpagecontent\"]/div[2]/div/div[4]/div/table/tbody/tr[1]/td[1]/span[2]/a")));
                        string data = sixElement.Text.Trim();
                        Console.WriteLine($"Номер модели: {data}");
                        JsonCreater parsingTitleList = new JsonCreater(sixElement, driver, data);
                        Console.WriteLine("Статус: " + parsingTitleList.Status);
                        Console.WriteLine("Пошлина: " + parsingTitleList.Tariff);
                        Console.WriteLine("Заявка: " + parsingTitleList.Application);
                        Console.WriteLine("Дата начала отсчета срока действия патента: " + parsingTitleList.StartPattern);
                        Console.WriteLine("Дата Регистрации: " + parsingTitleList.DataRegistration);
                        Console.WriteLine("Дата Отправки: " + parsingTitleList.DataSend);
                        Console.WriteLine("Дата Публикации: " + parsingTitleList.DataPublic);
                        Console.WriteLine("Список документов, цитированных в отчете о поиске: " + parsingTitleList.ListDocumentCitationInReport);
                        Console.WriteLine("Адрес для переписки" + parsingTitleList.AdresToCommunication);
                        Console.WriteLine("Автор(ы): " + parsingTitleList.Author);
                        Console.WriteLine("Патентообладатель(и): " + parsingTitleList.PatentHolder);
                        Console.WriteLine("Название патента: " + parsingTitleList.Title);

                        parsingTitleList.CreateJson();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                    finally
                    {
                        Console.WriteLine("Введите что-то для закрытия");
                        Console.ReadLine();
                        driver.Quit(); 
                    }
                }
            }
            
            void ParsingReestrInvation(string url)
            {
                using (IWebDriver driver = new ChromeDriver())
                {
                    try
                    {
                        Console.WriteLine("Открываем url");
                        driver.Navigate().GoToUrl(url);

                        // Явное ожидание появления элемента (до 10 секунд)
                        WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(4));
                        var element = wait.Until(driver => driver.FindElement(By.XPath("//*[@id=\"mainpagecontent\"]/div[2]/div/div[2]/div/table/tbody/tr[2]/td[2]/a")));

                        // Получаем текст элемента для отладки
                        var data = element.Text;
                        Console.WriteLine($"Найден элемент с текстом: {data}");

                        // Кликаем по элементу, чтобы открыть ссылку
                        element.Click();

                        // Ждем загрузки новой страницы
                        var newElement = wait.Until(driver => driver.FindElement(By.XPath("//*[@id=\"mainpagecontent\"]/div[2]/div[2]/div/ul/ul/li[2]")));

                        // Получаем информацию из нового элемента
                        string fullText = newElement.Text;
                        Console.WriteLine("Текст элемента:");
                        Console.WriteLine(fullText); //2700000 - 2799999

                        // Кликаем по элементу, чтобы открыть ссылку на диапозон
                        newElement.Click();

                        // Ждем загрузки новой страницы с диапозоном
                        var twonewElement = wait.Until(driver => driver.FindElement(By.XPath("//*[@id=\"mainpagecontent\"]/div[2]/div[2]/div/ul/ul/ul"))); //Необязательно

                        // Получаем информацию из нового элемента
                        string twofullText = twonewElement.Text;
                        Console.WriteLine("Текст элемента:");
                        Console.WriteLine(twofullText);
                        //2790000 - 2799999
                        //2780000 - 2789999
                        //2770000 - 2779999
                        //2760000 - 2769999
                        //2750000 - 2759999
                        //2740000 - 2749999
                        //2730000 - 2739999
                        //2720000 - 2729999
                        //2710000 - 2719999
                        //2700000 - 2709999

                        var threenewElement = driver.FindElement(By.XPath("//*[@id=\"mainpagecontent\"]/div[2]/div[2]/div/ul/ul/ul/li[1]/a[2]"));
                        string threefullText = threenewElement.Text;
                        Console.WriteLine("Текст элемента:");
                        Console.WriteLine(threefullText); //2790000 - 2799999

                        threenewElement.Click();

                        var fournewElement = wait.Until(driver => driver.FindElement(By.XPath("//*[@id=\"mainpagecontent\"]/div[2]/div[2]/div/ul/ul/ul/ul/li[1]/a[2]")));
                        string fourfullText = fournewElement.Text;
                        Console.WriteLine("Текст элемента:");
                        Console.WriteLine(fourfullText); //2799000 - 2799999

                        fournewElement.Click();

                        var fifenewElement = wait.Until(driver => driver.FindElement(By.XPath("//*[@id=\"mainpagecontent\"]/div[2]/div[2]/div/ul/ul/ul/ul/ul/li[1]/a")));
                        string fifefullText = fifenewElement.Text;
                        Console.WriteLine("Текст элемента:");
                        Console.WriteLine(fifefullText); //2799900 - 2799999

                        fifenewElement.Click(); // открывает ссылку с номерами изобретиний

                        var listReestrsdiapazon = wait.Until(driver => driver.FindElement(By.XPath("//*[@id=\"mainpagecontent\"]/div[2]/div/div[4]/div/table/tbody"))); //Необязательно
                        string listreestrdiapText = listReestrsdiapazon.Text;
                        Console.WriteLine("Текст элемента:");
                        Console.WriteLine(listreestrdiapText);
                        //2799900 PDF 2799925 PDF 2799950 PDF 2799975 PDF
                        //2799901 PDF 2799926 PDF 2799951 PDF 2799976 PDF
                        //2799902 PDF 2799927 PDF 2799952 PDF 2799977 PDF
                        //2799903 PDF 2799928 PDF 2799953 PDF 2799978 PDF
                        //2799904 PDF 2799929 PDF 2799954 PDF 2799979 PDF
                        //2799905 PDF 2799930 PDF 2799955 PDF 2799980 PDF
                        //2799906 PDF 2799931 PDF 2799956 PDF 2799981 PDF
                        //2799907 PDF 2799932 PDF 2799957 PDF 2799982 PDF
                        //2799908 PDF 2799933 PDF 2799958 PDF 2799983 PDF
                        //2799909 PDF 2799934 PDF 2799959 PDF 2799984 PDF
                        //2799910 PDF 2799935 PDF 2799960 PDF 2799985 PDF
                        //2799911 PDF 2799936 PDF 2799961 PDF 2799986 PDF
                        //2799912 PDF 2799937 PDF 2799962 PDF 2799987 PDF
                        //2799913 PDF 2799938 PDF 2799963 PDF 2799988 PDF
                        //2799914 PDF 2799939 PDF 2799964 PDF 2799989 PDF
                        //2799915 PDF 2799940 PDF 2799965 PDF 2799990 PDF
                        //2799916 PDF 2799941 PDF 2799966 PDF 2799991 PDF
                        //2799917 PDF 2799942 PDF 2799967 PDF 2799992 PDF
                        //2799918 PDF 2799943 PDF 2799968 PDF 2799993 PDF
                        //2799919 PDF 2799944 PDF 2799969 PDF 2799994 PDF
                        //2799920 PDF 2799945 PDF 2799970 PDF 2799995 PDF
                        //2799921 PDF 2799946 PDF 2799971 PDF 2799996 PDF
                        //2799922 PDF 2799947 PDF 2799972 PDF 2799997 PDF
                        //2799923 PDF 2799948 PDF 2799973 PDF 2799998 PDF
                        //2799924 PDF 2799949 PDF 2799974 PDF 2799999 PDF

                        var concretReestr = wait.Until(driver => driver.FindElement(By.XPath("//*[@id=\"mainpagecontent\"]/div[2]/div/div[4]/div/table/tbody/tr[2]/td[1]/span[2]/a")));
                        string numberReestr = concretReestr.Text;
                        Console.WriteLine("Текст элемента:");
                        Console.WriteLine(numberReestr); //2799901 Реестр

                        //concretReestr.Click(); //открывает инфу о реестре

                        JsonCreater parsingTitleList = new JsonCreater(concretReestr, driver, numberReestr);
                        Console.WriteLine("Статус: " + parsingTitleList.Status);
                        Console.WriteLine("Пошлина: " + parsingTitleList.Tariff);
                        Console.WriteLine("Заявка: " + parsingTitleList.Application);
                        Console.WriteLine("Дата начала отсчета срока действия патента: " + parsingTitleList.StartPattern);
                        Console.WriteLine("Дата Регистрации: " + parsingTitleList.DataRegistration);
                        Console.WriteLine("Дата Отправки: " + parsingTitleList.DataSend);
                        Console.WriteLine("Дата Публикации: " + parsingTitleList.DataPublic);
                        Console.WriteLine("Список документов, цитированных в отчете о поиске: " + parsingTitleList.ListDocumentCitationInReport);
                        Console.WriteLine("Адрес для переписки" + parsingTitleList.AdresToCommunication);
                        Console.WriteLine("Автор(ы): " + parsingTitleList.Author);
                        Console.WriteLine("Патентообладатель(и): " + parsingTitleList.PatentHolder);
                        Console.WriteLine("Название патента: " + parsingTitleList.Title);

                        parsingTitleList.CreateJson();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Ошибка: {ex.Message}");
                    }
                    finally
                    {
                        // Даем время на просмотр результата перед закрытием
                        Console.WriteLine("Нажмите Enter для закрытия...");
                        Console.ReadLine();
                        driver.Quit();
                    }
                }
            }
        }
    }
}