using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net;
using System.Net.Mail;
using BotDetect.Web.Mvc;
using rupbes.Models;

namespace rupbes.Controllers
{
    [Filters.Culture]
    public class OneWindowController : Controller
    {
        [HttpGet]
        public ActionResult Index()
        {
            HttpCookie cookie = Request.Cookies["lang"];
            if (cookie != null && cookie.Value == "be")
            {
                return View("IndexBel");
            }
            else if (cookie != null && cookie.Value == "en")
            {
                return View("IndexEng");
            }
            else
            {
                return View();
            }
                
        }

        [HttpGet]
        public ActionResult Person(string message=" ")
        {
            HttpCookie cookie = Request.Cookies["lang"];
            ViewBag.FileMessage = "";
            if (message == "Error")
            {
                if (cookie != null && cookie.Value == "be")
                {
                    ViewBag.FileMessage = "Занадта вялiкi файл";
                }
                if (cookie != null && cookie.Value == "en")
                {
                    ViewBag.FileMessage = "File too large";
                }
                else
                {
                    ViewBag.FileMessage = "Слишком большой файл";
                }
            }
            return View();
        }

        [HttpGet]
        public ActionResult Legal(string message = " ")
        {
            HttpCookie cookie = Request.Cookies["lang"];
            ViewBag.FileMessage = "";
            if (message == "Error")
            {
                if (cookie != null && cookie.Value == "be")
                {
                    ViewBag.FileMessage = "Занадта вялiкi файл";
                }
                else if (cookie != null && cookie.Value == "en")
                {
                    ViewBag.FileMessage = "File too large";
                }
                else
                {
                    ViewBag.FileMessage = "Слишком большой файл";
                }
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [CaptchaValidation("CaptchaCode", "Captcha", "Incorrect CAPTCHA code!")]
        public ActionResult Person(PersonMessage person, HttpPostedFileBase upload=null)
        {
            ViewBag.FileMessage = "";
            ViewBag.CaptchaMessage = "";
            HttpCookie cookie = Request.Cookies["lang"];
            if (!ModelState.IsValid)
            {
                //captcha or model validation failed
                if(cookie!=null && cookie.Value == "be")
                {
                    ViewBag.CaptchaMessage = "Увядзіце правільныя сімвалы";
                }
                else if (cookie != null && cookie.Value == "en")
                {
                    ViewBag.CaptchaMessage = "Please enter correct characters";
                }
                else
                {
                    ViewBag.CaptchaMessage = "Введите правильные символы";
                }
                return View();
            }
            else
            {
                MvcCaptcha.ResetCaptcha("Captcha");
                //captcha and model validation passed
                using (MailMessage mail = new MailMessage())
                {
                    mail.From = new MailAddress(/*"admin@rupbes.by"*/"rupbes.appeal@yandex.by");
                    mail.To.Add(new MailAddress("rupbes@rupbes.by"));
                    mail.Subject = person.Theme;
                    if (upload != null)
                    {
                        Attachment file = new Attachment(upload.InputStream, upload.FileName);
                        mail.Attachments.Add(file);
                    }
                    
                    mail.Body = " Обратившееся лицо: \r\n" + person.Sender + "\r\n Адресс лица: \r\n" + person.Adress + "\r\n" + person.Email + "\r\n Обратился к: \r\n" + person.Reciver + "\r\n Текст обращения: \r\n" + person.Text;
                    using (SmtpClient client = new SmtpClient())
                    {
                        client.Host = "ms3.g-cloud.by" /*"mailbe05.hoster.by"*/;
                        client.Port = 587 /*465*/;
                        client.EnableSsl = true;
                        client.Credentials = new NetworkCredential(/*"admin@rupbes.by", "Qwerty123!"*/"rupbes.appeal@yandex.by", "rupbesAPPEAL123!");
                        try
                        {
                            client.Send(mail);
                            return View("MessageSent");
                        }
                        catch(Exception)
                        {
                            ViewBag.FileMessage = "Ошибка отправки обращения";
                            return View();
                        }

                    }

                }
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [CaptchaValidation("CaptchaCode", "Captcha", "Incorrect CAPTCHA code!")]
        public ActionResult Legal(LegalMessage legal, HttpPostedFileBase upload = null)
        {
            ViewBag.FileMessage = "";
            ViewBag.CaptchaMessage = "";
            HttpCookie cookie = Request.Cookies["lang"];
            if (!ModelState.IsValid)
            {
                //captcha or model validation failed
                if (cookie != null && cookie.Value == "be")
                {
                    ViewBag.CaptchaMessage = "Увядзіце правільныя сімвалы";
                }
                else if (cookie != null && cookie.Value == "en")
                {
                    ViewBag.CaptchaMessage = "Please enter correct characters";
                }
                else
                {
                    ViewBag.CaptchaMessage = "Введите правильные символы";
                }
                return View();
            }
            else
            {
                MvcCaptcha.ResetCaptcha("Captcha");
                //captcha and model validation passed
                using (MailMessage mail = new MailMessage())
                {
                    mail.From = new MailAddress(/*"admin@rupbes.by"*/"rupbes.appeal@yandex.by");
                    mail.To.Add(new MailAddress("rupbes@rupbes.by"));
                    mail.Subject = legal.Theme;
                    if (upload != null)
                    {
                        Attachment file = new Attachment(upload.InputStream, upload.FileName);
                        mail.Attachments.Add(file);
                    }

                    mail.Body = " Обратившееся лицо: \r\n" + legal.Sender+ "\r\n" + legal.SenderName+ "\r\n Адресс лица: \r\n" + legal.Adress + "\r\n" + legal.Email + "\r\n Обратился к: \r\n" + legal.Receiver + "\r\n Текст обращения: \r\n" + legal.Text;
                    using (SmtpClient client = new SmtpClient())
                    {
                        client.Host = "ms3.g-cloud.by" /*"mailbe05.hoster.by"*/;
                        client.Port = 587; /*465*/
                        client.EnableSsl = true;
                        client.Credentials = new NetworkCredential(/*"admin@rupbes.by", "Qwerty123!"*/"rupbes.appeal@yandex.by", "rupbesAPPEAL123!");
                        try
                        {
                            client.Send(mail);
                            return View("MessageSent");
                        }
                        catch (Exception)
                        {
                            ViewBag.FileMessage = "Ошибка отправки обращения";
                            return View();
                        }

                    }

                }
            }
        }
    }
}