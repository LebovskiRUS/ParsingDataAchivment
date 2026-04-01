# Установка и настройка проекта

## Необходимые библиотеки
Установите следующие библиотеки через управление пакетами NuGet в `Program.cs`:

```csharp
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
```

## Структура проекта

### UrlInfo.cs
Файл, хранящий URL-адрес целевого сайта для парсинга:
```
https://www1.fips.ru/registers-web/action?acName=clickRegister&regName=RUPM
```

### Program.cs
Основной файл выполнения. Отвечает за:
- Запуск и управление браузером (Chrome)
- Навигацию по диапазонам страниц
- Координацию процессов парсинга и сохранения данных

### ParsingTitleList.cs
Файл, отвечающий за парсинг данных из HTML-документа, открытого в Program.cs. Извлекает необходимую информацию о патентах.

### JsonCreate.cs
Файл для создания и сохранения JSON-файлов:

Файл:	
- patent.json
- id_patent.json
Содержание:
- Полные данные о патентах
- Идентификаторы (ID) патентов

### Путь сохранения результатов:
```
ParsingDataAchivment\ParsingDataAchivment\bin\Debug\net10.0\
```

### Запуск проекта:
- Установите необходимые NuGet-пакеты
- Убедитесь, что UrlInfo.cs содержит актуальный URL
- Запустите Program.cs
- Результаты парсинга будут автоматически сохранены в указанную директорию
