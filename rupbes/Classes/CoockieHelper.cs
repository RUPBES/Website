using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace rupbes.Classes
{
    public static class CoockieHelper
    {
        //список локалей
        private static readonly List<string> locals = new List<string>() { "ru", "be", "en" };

        public static string CheckLocalCoockie()
        {
            string culture = null;

            HttpCookie cultureCoockie = HttpContext.Current.Request.Cookies["lang"];
            if (cultureCoockie != null && locals.Contains(cultureCoockie.Value))
            {
                    culture = cultureCoockie.Value;
            }
            else
            {
                culture = "ru";
            }

            return culture;
        }
    }
}