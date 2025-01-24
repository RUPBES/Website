using System;
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
    public class ResponseVacancyController : Controller
    {
        static Database db = new Database();        // Соединение с БД

        /**
         * Метод принимает в качастве параметра id выбранной вакансии.
         * На основе id присваиваются значения полям модели ResponseVacancy и генерируется представление.
         * Если в базе нет записи с данным id - перенаправление на страницу с вакансиями. 
         * Если параметр метода отствует - перенаправление на страницу с ошибкой.
         */
        [HttpGet]
        public ActionResult Index(int? id)
        {
            ResponseVacancy responseVacancy;
            if (id != null)
            {
                responseVacancy = new ResponseVacancy();
                try
                {
                         // Инициализация полей объекта ResponseVacancy (название вакансии и id вакансии)
                    Vacancies vacancie = db.Vacancies.Find(id);
                    responseVacancy.VacancyName = vacancie.vacancy_ru;
                    responseVacancy.VacancyId = vacancie.id;
                    
                    HttpCookie cookie = Request.Cookies["lang"];
                    if (cookie != null && cookie.Value == "be")
                    {
                        responseVacancy.VacancyName = vacancie.vacancy_bel;
                    }

                    return View(responseVacancy);
                }
                catch (Exception e)
                {
                    return Redirect(@"\Home\Vacancies");    // Перенаправление на страницу с перечнем вакансий
                }                
            }
            else
            {
                return Redirect(@"\Error\Index");           // Перенаправление на страницу с ошибкой
            }
        }

                        // Максимальный размер запроса
        private const int MyMaxContentLength = 2097152;

        [HttpPost]
        [ValidateAntiForgeryToken]
        [CaptchaValidation("CaptchaCode", "Captcha", "Incorrect CAPTCHA code!")]
        public ActionResult Index(ResponseVacancy responseVacancy, HttpPostedFileBase upload = null)
        {          
            ViewBag.CaptchaMessage = "";                    // Сообщение о валидности каптчи
            ViewBag.ValidFileFail = "";                     // Сообщение о валидности прикрепляемого файла

                     // Сравнение длины текущего запроса с максимальной
            if (Request.ContentLength > MyMaxContentLength)
            {
                HttpCookie cookie = Request.Cookies["lang"];
                if (cookie != null && cookie.Value == "be")
                {
                    ViewBag.ValidFileFail = "Занадта вялiкi файл";
                }
                else if (cookie != null && cookie.Value == "en")
                {
                    ViewBag.ValidFileFail = "File too large";
                }
                else
                {
                    ViewBag.ValidFileFail = "Слишком большой файл";
                }
                MvcCaptcha.ResetCaptcha("Captcha");         // Обновление каптчи
                return View(responseVacancy);
            }
            
            if (!ModelState.IsValid)
            {
                MvcCaptcha.ResetCaptcha("Captcha");
              
                HttpCookie cookie = Request.Cookies["lang"];
                if (cookie != null && cookie.Value == "be")
                {
                    ViewBag.CaptchaMessage = "Увядзіце правільныя сімвалы";
                }
                if (cookie != null && cookie.Value == "en")
                {
                    ViewBag.CaptchaMessage = "Please enter correct characters";
                }
                else
                {
                    ViewBag.CaptchaMessage = "Введите правильные символы";
                }
                return View(responseVacancy);
            }
            else
            {
                MvcCaptcha.ResetCaptcha("Captcha");
                
                using (MailMessage mail = new MailMessage())
                {
                        // По id вакансии определяется адрес электронной почты, на который отправится отклик 
                    //var IdDep = db.Vacancies.Where(x => x.id == responseVacancy.VacancyId).ToList()[0];
                    //var contacts = db.Contacts.Where(x => x.id_dep == IdDep.id_dep && x.id_group == 2).ToList();
                    //var emailTo = contacts[0].email;

                    mail.From = new MailAddress("rupbes.appeal@yandex.by");     // Email отправителя письма
                    mail.To.Add(new MailAddress("ok@rupbes.by"));                      // Адресат письма
                    //mail.To.Add(new MailAddress(emailTo));
                    mail.Subject = "Отклик на вакансию";                        // Тема сообщения
                    if (upload != null)
                    {   // Если был выбран валидный файл, то он прикрепляется к сообщению
                        Attachment file = new Attachment(upload.InputStream, upload.FileName);
                        mail.Attachments.Add(file);
                    }
                        // Генерация текста сообщения
                    mail.Body = "\nВакансия: " + responseVacancy.VacancyName + "\r\n\nСоискатель: \n   Имя: " + responseVacancy.FirstName + "\r\n   Фамилия: " + responseVacancy.LastName + "\r\n   Email: " + responseVacancy.Email + "\r\n\nСопроводительное письмо: \r\n   " + responseVacancy.Text;
                    using (SmtpClient client = new SmtpClient())
                    {
                        //client.Host = "smtp.yandex.ru";
                        client.Host = "ms3.g-cloud.by";
                        client.Port = 587; //465
                        client.EnableSsl = true;
                        client.Credentials = new NetworkCredential(/*"ResponseVacancy", "xwbucqozdwguhuxl"*/"rupbes.appeal@yandex.by", "rupbesAPPEAL123!");
                        try
                        {
                            client.Send(mail);                                  // Попытка отправки сообщения
                            return View("MessageSent");
                        }
                        catch (Exception)
                        {
                            ViewBag.FileMessage = "Ошибка отправки отклика";
                            return View();
                        }

                    }

                }
            }                                         

        }
        
    }
}