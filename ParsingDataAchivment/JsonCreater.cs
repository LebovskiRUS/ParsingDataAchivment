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
        // Асинхронная версия
        public async Task CreateJsonAsync()
        {
            await Task.Run(() => CreateJson());
        }
        public void CreateJson()
        {
            int newId = Convert.ToInt32(Application);

            // Загружаем или создаем файл с id
            dynamic data_id = LoadOrCreateIdFile();

            // Проверяем существование id
            foreach (int id in data_id.id)
            {
                if(id == newId) { return; }
            }

            // Добавляем новый id
            data_id.id.Add(newId);
            string updateJsonId = JsonConvert.SerializeObject(data_id, Formatting.Indented);
            File.WriteAllText(_jsonFilePathId, updateJsonId);

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

        private dynamic LoadOrCreateIdFile()
        {
            if (!File.Exists(_jsonFilePathId))
            {
                // Создаем файл с пустым списком
                var initialData = new { id = new List<int>() };
                string initialJson = JsonConvert.SerializeObject(initialData, Formatting.Indented);
                File.WriteAllText(_jsonFilePathId, initialJson);
                return initialData;
            }

            string id_count = File.ReadAllText(_jsonFilePathId);

            if (string.IsNullOrWhiteSpace(id_count))
            {
                return new { id = new List<int>() };
            }

            var data = JsonConvert.DeserializeObject<dynamic>(id_count);

            // Если свойство id отсутствует, добавляем его
            if (data.id == null)
            {
                data.id = new List<int>();
            }

            return data;
        }

    }
}
