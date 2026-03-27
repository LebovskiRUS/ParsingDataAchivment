using OpenQA.Selenium;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ParsingDataAchivment
{
    internal class JsonCreater : ParsingTitleList
    {
        private readonly string _jsonFilePath = "patent.json";
        private readonly string _jsonFilePathId = "id_patent.json";
        public JsonCreater(IWebElement xpath, IWebDriver driver, string id) : base(xpath, driver, id)
        {
           
        }

        public void CreateJson()
        {

            var data = new
            {
                id = Convert.ToInt32(Application),
                status = Status,
                tariff = Tariff,
                start_pattern = StartPattern,
                data_registrate = DataRegistration,
                data_send = DataSend,
                data_public = DataPublic,
                citatioin = ListDocumentCitationInReport,
                communicate = AdresToCommunication,
                author = SplitAuthor(Author),
                patent_holder = PatentHolder,
                title = Title,
                color = Color

            };

            // Читаем существующие патенты или создаем новый список
            List<object> patents;
            if (File.Exists(_jsonFilePath))
            {
                string existingJson = File.ReadAllText(_jsonFilePath);
                patents = JsonConvert.DeserializeObject<List<object>>(existingJson) ?? new List<object>();
            }
            else
            {
                patents = new List<object>();
            }

            // Добавляем новый патент
            patents.Add(data);

            // Сохраняем обновленный список
            string updatedJson = JsonConvert.SerializeObject(patents, Formatting.Indented);
            File.WriteAllText(_jsonFilePath, updatedJson);

            Console.WriteLine($"Патент {Application} добавлен в JSON. Всего патентов: {patents.Count}");
        }

        private List<string> SplitAuthor(string authorString)
        {
            if(string.IsNullOrEmpty(authorString)) 
                return new List<string>();
            var authors = authorString.Split([",\r\n",","], StringSplitOptions.RemoveEmptyEntries);
            return authors.Select(x => x.Trim()).ToList();
        }
    }
}
