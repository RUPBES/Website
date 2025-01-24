using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using AngleSharp;
using AngleSharp.Dom;
using System.Threading;
using rupbes.Models.DatabaseBes;

namespace rupbes.Jobs.TendersLoader
{

    public static class Parser
    {
        private static IConfiguration _config = Configuration.Default.WithDefaultLoader();
        //принимает url с icetrade(страница закупок), возвращает список url для каждой закупки на странице 
        private async static Task<List<string>> MakeLinkListAsync(string url)
        {
            var document = await BrowsingContext.New(_config).OpenAsync(url);
            List<string> links = new List<string>();
            //css-селектор для выбора первой ячейки из таблицы
            string linksSelector = "tr[class^=rw-] td:nth-child(1)";
            var linkCells = document.QuerySelectorAll(linksSelector);
            //индексы для определения позиции в строке для её форматирования в методе ниже
            int startPos;
            int lastPos;
            //формирует и возвращает список ссылок на все тендеры со страницы
            //каждая из полученых ссылок, открывает карточку тендера на icetrade
            links = linkCells.Select(x =>
            {
                x.InnerHtml = x.InnerHtml.Trim();
                startPos = 9; lastPos = x.InnerHtml.IndexOf(">");
                return x.InnerHtml.Substring(startPos, lastPos - startPos - 1);

            }).ToList();
            return links;
        }

        //возвращает список объектов-тендеров из icetrade
        public async static Task<List<Tenders>> GetTendersAsync(string url)
        {
            List<string> links = await MakeLinkListAsync(url);
            if (links.Count > 0)
            {
                //css-селекторы для выборки нужных ячеек из таблицы на странице конкретной закупки в icetrade
                string descriptionSelector = "tr.af-title td:nth-child(2)";
                string contactsSelector = "tr.af-customer_contacts td:nth-child(2)";
                string dateSelector = "tr.af-request_end td:nth-child(2)";
                string icetrade_idSelector = "div.h1 div h1";
                //объявляем список тендеров для записи в базу 
                List<Tenders> tenders = new List<Tenders>();
                //на каждом шагу цикла создаем объект тендера и добавляем в подготовленный список
                foreach (var link in links)
                {
                    Tenders tender = new Tenders();
                    //запись в объект тендера ссылки на его карточку в icetrade
                    var document = await BrowsingContext.New(_config).OpenAsync(link);
                    tender.icetrade_link = link;

                    //название тендера(заголовок)
                    var description = document.QuerySelectorAll(descriptionSelector);
                    tender.description = description[0].TextContent.Trim();

                    //контактная информация
                    var contacts = document.QuerySelectorAll(contactsSelector);
                    tender.contacts = contacts[0].TextContent.Trim();

                    //дата и время окончания торгов
                    var date = document.QuerySelectorAll(dateSelector);
                    tender.date = DateTime.Parse(date[0].TextContent.Trim());

                    //номер закупки на icetrade
                    var id = document.QuerySelectorAll(icetrade_idSelector);
                    string icetradeId = id[0].TextContent.Trim();
                    int posN = icetradeId.IndexOf("№");
                    tender.icetrade_id = icetradeId.Substring(posN, icetradeId.Length - posN);

                    //текущее время добавления записи
                    tender.uploadDate = DateTime.Now;
                    tenders.Add(tender);
                    //усыпляем процесс на 7 секунд для обхода блокировки роботов на icetrade
                    Thread.Sleep(7000);
                }
                return tenders;
            }
            return null;
        }
    }
}