using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace rupbes.Classes
{
    public static class Loger
    {
        //пишет сообщение в конец файла; logPath-путь к файлу, message - сообщение 
        public static void WriteRecord(string logPath, string message)
        {
            using (StreamWriter sw = new StreamWriter(logPath, true))
            {
                sw.WriteLine("--BEGIN OF RECORD--");
                sw.WriteLine(message);
                sw.WriteLine("--END--");
            }
        }
    }
}