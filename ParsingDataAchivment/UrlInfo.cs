using System;
using System.Collections.Generic;
using System.Text;

namespace ParsingDataAchivment
{
    internal class UrlInfo
    {
        static public List<string> urls = new List<string>();
        static public List<string> titles = new List<string>();

        static UrlInfo()
        {
            urls = new List<string>() 
                { "https://www1.fips.ru/registers-web/action?acName=clickRegister&regName=RUPAT"
                , "https://www1.fips.ru/registers-web/action?acName=clickRegister&regName=RUPM" };
            titles = new List<string>() { "Реестр_Изобретений", "Модели" };
        }
        

    }
}
