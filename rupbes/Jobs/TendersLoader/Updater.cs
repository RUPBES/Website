using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Threading;
using System.IO;
using System.Web.Hosting;
using rupbes.Models.DatabaseBes;
using rupbes.Classes;

namespace rupbes.Jobs.TendersLoader
{
    public class Updater : IJob
    {
        //добавляет тендеры в базу и ведет журнал
        public async Task Execute(IJobExecutionContext context)
        {
            //путь к файлу с url icetrade, откуда мы будем получать тендеры
            string path = HostingEnvironment.MapPath(@"~/Jobs/TendersLoader/url.txt");
            //путь к файлу журнала
            string logFile = HostingEnvironment.MapPath(@"~/Jobs/TendersLoader/log.txt");
            string url;
            try
            {
                //получаем url из файла
                using (StreamReader sr = new StreamReader(path))
                {
                    url = sr.ReadLine();
                }

                //возвращает null или непустой список тендеров
                var tenders = await Parser.GetTendersAsync(url);
                if (tenders != null)
                {
                    //обновляем таблицу с тендерами
                    DbSaver.TrendsToDb(tenders);
                    //добавляем запись в журнал: количество добавленных тендеров и время
                    Loger.WriteRecord(logFile, $"Added {tenders.Count} tenders at {DateTime.Now}");
                }
                else
                {
                    //добавляем в журнал запись о том, что на момент запуска программы активных тендеров нет
                    Loger.WriteRecord(logFile, $"No tenders added at {DateTime.Now}");
                }
            }
            catch (Exception e)
            {
                //добавляем запись в случае произошедшей ошибки
                Loger.WriteRecord(logFile, $"Error message: {e.Message} Source: {e.Source} Method: {e.StackTrace} Date: {DateTime.Now}");
            }
        }
    }

    public static class DbSaver
    {
        private static Database _db = new Database();
        //обновляет информацию в таблице тендеров
        public static void TrendsToDb(List<Tenders> tenders)
        {
            //если в таблице что-то есть - удаляем и заменяем новыми записями
            if (_db.Tenders.Any())
            {
                var tendersToRemove = _db.Tenders.ToList();
                _db.Tenders.RemoveRange(tendersToRemove);
                _db.SaveChanges();
            }
            _db.Tenders.AddRange(tenders);
            _db.SaveChanges();
        }
    }

    
}