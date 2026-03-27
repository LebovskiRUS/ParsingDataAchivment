using OpenQA.Selenium;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ParsingDataAchivment
{
    internal class JsonCreater : ParsingTitleList
    {
        public JsonCreater(IWebElement xpath, IWebDriver driver) : base(xpath, driver)
        {
            CreateJson();
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
                author = Author,
                patent_holder = PatentHolder,
                title = Title

            };
            string json = JsonConvert.SerializeObject(data, Formatting.Indented);
            File.WriteAllText("patent.json", json);
        }
    }
}
