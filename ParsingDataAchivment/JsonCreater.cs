using Newtonsoft.Json;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ParsingDataAchivment
{
    internal class JsonCreater : ParsingTitleList
    {
        private readonly string _jsonFilePath = "patent.json";
        private readonly string _jsonFilePathId = "id_patent.json";

        public JsonCreater(IWebElement xpath, IWebDriver driver, string id) : base(xpath, driver, id)
        {
        }

        // Новый вариант: если страница уже открыта через поиск
        public JsonCreater(IWebDriver driver, string id) : base(driver, id)
        {
        }

        public async Task CreateJsonAsync()
        {
            if (!int.TryParse(Application, out int newId))
            {
                Console.WriteLine($"Не удалось преобразовать Application='{Application}' в число.");
                return;
            }

            PatentIdStore dataId = await LoadOrCreateIdFileAsync();

            if (dataId.Id.Contains(newId))
            {
                Console.WriteLine($"Патент {newId} уже есть в id_patent.json. Пропуск.");
                return;
            }

            dataId.Id.Add(newId);
            string updateJsonId = JsonConvert.SerializeObject(dataId, Formatting.Indented);
            await File.WriteAllTextAsync(_jsonFilePathId, updateJsonId);

            var data = new PatentRecord
            {
                Id = newId,
                Status = Status,
                Tariff = Tariff,
                StartPattern = StartPattern,
                DataRegistration = DataRegistration,
                DataSend = DataSend,
                DataPublic = DataPublic,
                Citation = ListDocumentCitationInReport,
                Communicate = AdresToCommunication,
                Author = SplitAuthor(Author),
                PatentHolder = PatentHolder,
                Title = Title,
                Color = Color
            };

            List<PatentRecord> patents = new();

            if (File.Exists(_jsonFilePath))
            {
                string existingJson = await File.ReadAllTextAsync(_jsonFilePath);
                if (!string.IsNullOrWhiteSpace(existingJson))
                {
                    patents = JsonConvert.DeserializeObject<List<PatentRecord>>(existingJson) ?? new List<PatentRecord>();
                }
            }

            patents.Add(data);

            string updatedJson = JsonConvert.SerializeObject(patents, Formatting.Indented);
            await File.WriteAllTextAsync(_jsonFilePath, updatedJson);

            Console.WriteLine($"Патент {Application} добавлен в JSON. Всего патентов: {patents.Count}");
        }

        private static List<string> SplitAuthor(string authorString)
        {
            if (string.IsNullOrWhiteSpace(authorString))
                return new List<string>();

            return authorString
                .Split(new[] { "\r\n", "\n", "," }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToList();
        }

        private async Task<PatentIdStore> LoadOrCreateIdFileAsync()
        {
            if (!File.Exists(_jsonFilePathId))
            {
                var initialData = new PatentIdStore();
                string initialJson = JsonConvert.SerializeObject(initialData, Formatting.Indented);
                await File.WriteAllTextAsync(_jsonFilePathId, initialJson);
                return initialData;
            }

            string idCount = await File.ReadAllTextAsync(_jsonFilePathId);

            if (string.IsNullOrWhiteSpace(idCount))
                return new PatentIdStore();

            var data = JsonConvert.DeserializeObject<PatentIdStore>(idCount) ?? new PatentIdStore();
            data.Id ??= new List<int>();
            return data;
        }
    }

    internal class PatentIdStore
    {
        [JsonProperty("id")]
        public List<int> Id { get; set; } = new();
    }

    internal class PatentRecord
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; } = "Пусто";

        [JsonProperty("tariff")]
        public string Tariff { get; set; } = "Пусто";

        [JsonProperty("start_pattern")]
        public string StartPattern { get; set; } = "Пусто";

        [JsonProperty("data_registrate")]
        public string DataRegistration { get; set; } = "Пусто";

        [JsonProperty("data_send")]
        public string DataSend { get; set; } = "Пусто";

        [JsonProperty("data_public")]
        public string DataPublic { get; set; } = "Пусто";

        [JsonProperty("citatioin")]
        public string Citation { get; set; } = "Пусто";

        [JsonProperty("communicate")]
        public string Communicate { get; set; } = "Пусто";

        [JsonProperty("author")]
        public List<string> Author { get; set; } = new();

        [JsonProperty("patent_holder")]
        public string PatentHolder { get; set; } = "Пусто";

        [JsonProperty("title")]
        public string Title { get; set; } = "Пусто";

        [JsonProperty("color")]
        public string Color { get; set; } = "Пусто";
    }
}
