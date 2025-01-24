using BotDetect.Web.Mvc;
using rupbes.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using rupbes.Models;

namespace rupbes.Controllers
{
    public class AuthorizationController : Controller
    {
        private Database db = new Database();

        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [CaptchaValidation("CaptchaCode", "Captcha", "Incorrect")]
        public ActionResult Login(string login, string pass, bool captchaValid)
        {
            if (login != "" && pass != "" && captchaValid)
            {
                MvcCaptcha.ResetCaptcha("Captcha");
                using (MD5 md5Hash = MD5.Create())
                {
                    string salt = "$#^@(as()@&";
                    string pre = salt + pass + salt;
                    string hash = HashHelper.GetMd5Hash(md5Hash, pre);
                    Users user = db.Users.FirstOrDefault(u => u.login == login && u.pass == hash);
                    if (user != null)
                    {
                        FormsAuthentication.SetAuthCookie(user.login, true);
                        return RedirectToAction("Index", "Admin");
                    }
                    else
                    {
                        ViewBag.Message = "Неправильный логин или пароль";
                        return View("Error");
                    }
                }
            }
            else
            {
                ViewBag.Message = "Пустой логини или пароль или не пройдена проверка на спам";
                return View("Error");
            }
        }
    }
}