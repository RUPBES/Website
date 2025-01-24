using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace rupbes.Controllers
{
    [Filters.Culture]
    public class AssistController : Controller
    {
        [HttpPost]
        public ActionResult ChangeCulture(string lang)
        {
            string returnUrl = Request.UrlReferrer.AbsolutePath;
            // Сохраняем выбранную культуру в куки
            HttpCookie langCookie = Request.Cookies["lang"];
            if (langCookie != null)
                langCookie.Value = lang;   // если куки уже установлено, то обновляем значение
            else
            {
                langCookie = new HttpCookie("lang")
                {
                    HttpOnly = true,
                    Value = lang,
                    Expires = DateTime.Now.AddYears(1)
                };
            }
            Response.Cookies.Add(langCookie);
            return Redirect(returnUrl);
        }
        [HttpPost]
        public ActionResult ChangeTheme()
        {
            string returnUrl = Request.UrlReferrer.AbsolutePath;

            HttpCookie themeCookie = Request.Cookies["theme"];
            if (themeCookie==null || themeCookie.Value != "poorEyesight")
            {
                themeCookie = new HttpCookie("theme")
                {
                    HttpOnly = true,
                    Value = "poorEyesight",
                    Expires = DateTime.Now.AddYears(1)
                };
            }
            else
            {
                themeCookie = new HttpCookie("theme")
                {
                    HttpOnly = true,
                    Value = "default",
                    Expires = DateTime.Now.AddYears(1)
                };
            }
            Response.Cookies.Add(themeCookie);
            return Redirect(returnUrl);
        }
    }
}