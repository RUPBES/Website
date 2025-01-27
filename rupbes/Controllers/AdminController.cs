using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Security.Cryptography;
using System.Web.Security;
using rupbes.Classes;
using rupbes.Models;
using BotDetect.Web.Mvc;
using System.Data.Entity;
using System.Drawing.Imaging;
using rupbes.Providers;
using rupbes.Models.DatabaseBes;
using System.Net;
using System.IO;
using System.Text;
using rupbes.Models.Products;
using rupbes.Models.ViewModels;
using System.Diagnostics;
using Newtonsoft.Json.Linq;

namespace rupbes.Controllers
{
    [Authorize]
    [Filters.Culture]
    public class AdminController : Controller
    {
        private Models.Database db = new Models.Database();
        private Models.DatabaseBes.Database db1 = new Models.DatabaseBes.Database();
        private CustomRoleProvider roleProvider = new CustomRoleProvider();

        //ОБЩИЕ МЕТОДЫ
        [HttpGet]
        public ActionResult Index()//Страница админки
        {
            ViewBag.provider = roleProvider;
            return View();
        }
        [HttpGet]
        public ActionResult Logoff()//Выход из учетки
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Index");
        }
        [HttpGet]
        public ActionResult Error(string message)
        {
            return View(message);
        }

        [HttpGet]
        [Authorize(Roles = "admin")]
        public ActionResult HashGenPage()
        {
            return View("HashGenPage");
        }

        [HttpPost]
        [Authorize(Roles = "admin")]
        public string HashGen(string pass)
        {
            using (MD5 md5Hash = MD5.Create())
            {
                string salt = "$#^@(as()@&";
                string pre = salt + pass + salt;
                string hash = HashHelper.GetMd5Hash(md5Hash, pre);
                return hash;
            }
        }

        [HttpPost]
        //пока без аттрибутов
        public ActionResult UploadAjax()//Загрузка картинок
        {
            Users user = db.Users.FirstOrDefault(x => x.login == User.Identity.Name);
            Usage_report rep = new Usage_report { title = "Добавление картинки", action = "Add", table = "Imgs", date = DateTime.Now, id_user = user.id };
            //перебираем все загруженные файлы из запроса
            HttpPostedFileBase upload = Request.Files[0];
            string referer = "All";
            if (Request.Headers[8].Split('/')[4] == "CreateNews" || Request.Headers[8].Split('/')[4] == "EditNews")
            {
                referer = "News";
            }
            else if (Request.Headers[8].Split('/')[4] == "CreateObject" || Request.Headers[8].Split('/')[4] == "EditObject")
            {
                referer = "Object";
            }
            else if (Request.Headers[8].Split('/')[4] == "CreateCertificate" || Request.Headers[8].Split('/')[4] == "EditCertificate")
            {
                referer = "Certificate";
            }
            else if (Request.Headers[8].Split('/')[4] == "AddBoss" || Request.Headers[8].Split('/')[4] == "EditBoss" || Request.Headers[8].Split('/')[4] == "CreateBoss" || Request.Headers[8].Split('/')[4] == "EditDepBoss")
            {
                referer = "Boss";
            }
            else if (Request.Headers[8].Split('/')[4] == "Department")
            {
                referer = "Department";
            }
            else if (Request.Headers[8].Split('/')[4] == "AddRealty" || Request.Headers[8].Split('/')[4] == "EditRealty" || Request.Headers[8].Split('/')[4] == "CreateRealty" || Request.Headers[8].Split('/')[4] == "EditDepRealty")
            {
                referer = "Realty";
            }
            else if (Request.Headers[8].Split('/')[4] == "AddSale" || Request.Headers[8].Split('/')[4] == "EditSale" || Request.Headers[8].Split('/')[4] == "CreateSale" || Request.Headers[8].Split('/')[4] == "EditDepSale")
            {
                referer = "Sale";
            }
            else if (Request.Headers[8].Split('/')[4] == "AddMech" || Request.Headers[8].Split('/')[4] == "EditMech" || Request.Headers[8].Split('/')[4] == "CreateMech" || Request.Headers[8].Split('/')[4] == "EditDepMech")
            {
                referer = "Mech";
            }
            else if (Request.Headers[8].Split('/')[4] == "AddService" || Request.Headers[8].Split('/')[4] == "EditService" || Request.Headers[8].Split('/')[4] == "CreateService" || Request.Headers[8].Split('/')[4] == "EditDepService")
            {
                referer = "Service";
            }
            else if (Request.Headers[8].Split('/')[4] == "Products" || Request.Headers[8].Split('/')[4] == "EditProduct" || Request.Headers[8].Split('/')[4] == "CreateProduct" || Request.Headers[8].Split('/')[4] == "EditDepProduct")
            {
                referer = "Product";
            }
            else if (Request.Headers[8].Split('/')[4] == "AddVersionProduct" || Request.Headers[8].Split('/')[4] == "EditVersionProduct" || Request.Headers[8].Split('/')[4] == "CreateVersionProduct" || Request.Headers[8].Split('/')[4] == "EditDepVersionProduct")
            {
                referer = "VersionProduct";
            }
            string type = upload.ContentType.Split('/')[0];

            try
            {
                if (upload != null && type == "image")
                {
                    // получаем имя файла
                    Int32 unixTimestamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
                    string fileName = unixTimestamp.ToString() + ".jpg";
                    string path_fullSize = Server.MapPath("~/Content/Images/" + referer + "/" + fileName);
                    string path_min = Server.MapPath("~/Content/Images/" + referer + "/min/" + fileName);

                    // сохраняем уменьшенную копию
                    ResizeImgHelper.ResizeImage(upload.InputStream, path_fullSize, 5000, 1640, ImageFormat.Jpeg);
                    ResizeImgHelper.ResizeImage(upload.InputStream, path_min, 250, 250, ImageFormat.Jpeg);

                    // добавляем в базу информацию о путях к картинке и её уменьшенной копии
                    string link = db.Departments.Find(3).link;
                    Imgs img = new Imgs
                    {
                        src = link + "/Content/Images/" + referer + "/" + fileName,
                        src_min = link + "/Content/Images/" + referer + "/min/" + fileName
                    };
                    if (referer == "News")
                    {
                        img.type_id = 1;
                    }
                    else if (referer == "Object")
                    {
                        img.type_id = 2;
                    }
                    else if (referer == "Certificate")
                    {
                        img.type_id = 5;
                    }
                    else if (referer == "Boss")
                    {
                        img.type_id = 6;
                    }
                    else if (referer == "Department")
                    {
                        img.type_id = 9;
                    }
                    else if (referer == "Realty")
                    {
                        img.type_id = 7;
                    }
                    else if (referer == "Sale")
                    {
                        img.type_id = 10;
                    }
                    else if (referer == "Mech")
                    {
                        img.type_id = 8;
                    }
                    else if (referer == "Service")
                    {
                        img.type_id = 4;
                    }
                    else if (referer == "Product")
                    {
                        img.type_id = 11;
                    }
                    else if (referer == "VersionProduct")
                    {
                        img.type_id = 12;
                    }
                    try
                    {
                        db.Imgs.Add(img);
                        db.Usage_report.Add(rep);
                        db.SaveChanges();
                        ViewBag.AddedImage = img;
                        if (referer == "Boss" || referer == "Department")
                        {
                            return PartialView("AddSingleImage");
                        }
                        else
                        {
                            return PartialView("_AddImage");
                        }

                    }
                    catch (Exception)
                    {
                        return PartialView("Error");
                    }
                }

                else if (upload != null)
                {
                    string path_fullSize = Server.MapPath("~/Content/Files/News/" + upload.FileName);
                    using (var fileStream = new FileStream(path_fullSize, FileMode.Create))
                    {
                        upload.InputStream.Position = 0;
                        byte[] buffer;
                        using (var binaryReader = new BinaryReader(upload.InputStream))
                        {
                            buffer = binaryReader.ReadBytes(upload.ContentLength);
                        }
                        fileStream.Write(buffer, 0, buffer.Length);
                    }
                    //добавляем в базу информацию о путях к картинке и её уменьшенной копии
                    string link = db.Departments.Find(3).link;
                    Imgs img = new Imgs
                    {
                        src = link + "/Content/Files/News/" + upload.FileName,
                        src_min = "document",
                        type_id = 1
                    };
                    try
                    {
                        db.Imgs.Add(img);
                        db.Usage_report.Add(new Usage_report { title = "Добавление файла", action = "Add", table = "Imgs", date = DateTime.Now, id_user = user.id });
                        db.SaveChanges();
                        ViewBag.AddedImage = img;
                        return PartialView("_AddImage");
                    }
                    catch (Exception)
                    {
                        return PartialView("Error");
                    }
                }
            }
            catch (Exception e)
            {
                return Content(e.Message);
            }

            return View("Error");
        }

        [HttpGet]
        public ActionResult DeleteImgs()//Страница удаление картинок
        {
            if (roleProvider.IsUserInRole(User.Identity.Name, "news"))
            {
                ViewBag.Imgs = db.Imgs.Where(x => x.type_id == 1).ToList();
            }
            else if (roleProvider.IsUserInRole(User.Identity.Name, "obj"))
            {
                ViewBag.Imgs = db.Imgs.Where(x => x.type_id == 2).ToList();
            }
            else if (roleProvider.IsUserInRole(User.Identity.Name, "admin"))
            {
                ViewBag.Imgs = db.Imgs.ToList();
            }
            else if (roleProvider.IsUserInRole(User.Identity.Name, "ok_master"))
            {
                ViewBag.Imgs = db.Imgs.Where(x => x.type_id == 6).ToList();
            }
            else if (roleProvider.IsUserInRole(User.Identity.Name, "realty_master"))
            {
                ViewBag.Imgs = db.Imgs.Where(x => x.type_id == 7 || x.type_id == 10).ToList();
            }
            else if (roleProvider.IsUserInRole(User.Identity.Name, "mech_master"))
            {
                ViewBag.Imgs = db.Imgs.Where(x => x.type_id == 8).ToList();
            }
            else if (roleProvider.IsUserInRole(User.Identity.Name, "service_master"))
            {
                ViewBag.Imgs = db.Imgs.Where(x => x.type_id == 4).ToList();
            }
            return View();
        }
        [HttpPost]
        public ActionResult DeleteImgs(int[] ids)//Удаление картинок
        {
            Users user = db.Users.FirstOrDefault(x => x.login == User.Identity.Name);
            Usage_report rep = new Usage_report { title = "Удаление картинки", action = "Delete", table = "Imgs", date = DateTime.Now, id_user = user.id };
            if (ids != null)
            {
                try
                {
                    foreach (int id in ids)
                    {
                        Imgs img = db.Imgs.Find(id);
                        string link = db.Departments.Find(3).link;

                        if (img.src_min != "document")
                        {
                            System.IO.File.Delete(Server.MapPath("~" + img.src.Substring(link.Length)));//удаляем оригинал
                            System.IO.File.Delete(Server.MapPath("~" + img.src_min.Substring(link.Length)));//миниатюру
                        }
                        else
                        {
                            System.IO.File.Delete(Server.MapPath("~" + img.src.Substring(link.Length)));//удаляем оригинал
                        }

                        db.Imgs.Remove(img);
                        db.Usage_report.Add(rep);
                        db.SaveChanges();
                    }
                    return RedirectToAction("DeleteImgs");
                }
                catch (Exception e)
                {
                    return RedirectToAction("Error", new { message = e.Message });
                }
            }
            else
            {
                return RedirectToAction("Error", new { message = "Для удаления ничего не выбрано" });
            }
        }
        //ВАКАНСИИ
        [HttpGet]
        [Authorize(Roles = "admin, ok_master")]
        public ActionResult Vacancies()//Страница с выбором филиала
        {
            ViewBag.Departments = db.Departments.ToList();
            return View();
        }
        [HttpGet]
        [Authorize(Roles = "admin, ok_master")]
        public ActionResult DepVacancy(int id)//Страница со списоком вакансий для выбранного филиала
        {
            ViewBag.Departments = db.Departments.ToList();
            ViewBag.Department = db.Departments.Find(id);
            ViewBag.Vacancies = db.Vacancies.Where(x => x.id_dep == id).ToList();
            return View();
        }
        [HttpPost]
        [Authorize(Roles = "admin, ok_master, ok")]
        public ActionResult DeleteVacancy(int id)//Удаление выбранной вакансии
        {
            try
            {
                Users user = db.Users.FirstOrDefault(x => x.login == User.Identity.Name);
                Vacancies vacancy = db.Vacancies.Find(id);
                Usage_report rep = new Usage_report { title = vacancy.vacancy_ru, action = "Delete", table = "Vacancies", date = DateTime.Now, id_user = user.id };
                if (vacancy.id_dep == user.id_dep || user.Roles.role == "admin" || user.Roles.role == "ok_master")
                {
                    try
                    {
                        db.Vacancies.Remove(vacancy);
                        db.Usage_report.Add(rep);
                        db.SaveChanges();
                        return PartialView("_VacSuc");
                    }
                    catch (Exception e)
                    {
                        return PartialView("_VacFail");
                    }
                }
                else
                {
                    return PartialView("_VacFail");
                }
            }
            catch
            {
                return PartialView("_VacFail");
            }
        }
        [HttpPost]
        [Authorize(Roles = "admin, ok_master")]
        public ActionResult EditVacancy(Vacancies vacancy)//Редактирование выбранной вакансии
        {
            Users user = db.Users.FirstOrDefault(x => x.login == User.Identity.Name);
            Usage_report rep = new Usage_report { title = vacancy.vacancy_ru, action = "Edit", table = "Vacancies", date = DateTime.Now, id_user = user.id };
            if (vacancy.vacancy_ru != null && vacancy.vacancy_bel != null)
            {
                db.Entry(vacancy).State = EntityState.Modified;
                db.Usage_report.Add(rep);
                try
                {
                    db.SaveChanges();
                    return PartialView("_VacSuc");
                }
                catch
                {
                    return PartialView("_VacFail");
                }

            }
            else
            {
                return PartialView("_ValidVacFail");
            }

        }
        [HttpGet]
        [Authorize(Roles = "admin, ok_master")]
        public ActionResult AddVacancy()//Страница добавления вакансии
        {
            ViewBag.Departments = db.Departments.ToList();
            return View();
        }
        [HttpGet]
        [Authorize(Roles = "admin, ok_master")]
        public ActionResult AddDepVacancy(int id)//Страница добавления вакансии определенному филиалу
        {
            ViewBag.Dep = db.Departments.Find(id);
            return View();
        }
        [HttpPost]
        [Authorize(Roles = "admin, ok_master")]
        public ActionResult AddVacancy(Vacancies vacancy)//Добавление новой вакансии
        {

            if (vacancy.vacancy_ru != null && vacancy.vacancy_bel != null)
            {
                Users user = db.Users.FirstOrDefault(x => x.login == User.Identity.Name);
                Usage_report rep = new Usage_report { title = vacancy.vacancy_ru, action = "Add", table = "Vacancies", date = DateTime.Now, id_user = user.id };
                try
                {
                    db.Vacancies.Add(vacancy);
                    db.Usage_report.Add(rep);
                    db.SaveChanges();
                    return PartialView("_AddVacSuc");
                }
                catch
                {
                    return PartialView("_VacFail");
                }
            }
            else
            {
                return PartialView("_ValidVacFail");
            }
        }

        //Отдел кадров для филиалов
        [HttpGet]
        [Authorize(Roles = "ok")]
        public ActionResult AllVacancies()//Список всех вакансий филиала
        {
            Models.Users user = db.Users.FirstOrDefault(x => x.login == User.Identity.Name);
            ViewBag.Vacancies = db.Vacancies.Where(x => x.id_dep == user.id_dep).ToList();
            return View();
        }
        [HttpGet]
        [Authorize(Roles = "ok")]
        public ActionResult CreateVacancy()//Страница добавления новой вакансии
        {
            return View();
        }
        [HttpPost]
        [Authorize(Roles = "ok")]
        public ActionResult CreateVacancy(Vacancies vacancy)//Добавление вакансии для филиала
        {
            if (vacancy.vacancy_ru == null || vacancy.vacancy_bel == null)
            {
                return PartialView("_ValidVacFail");
            }
            else
            {
                Users user = db.Users.FirstOrDefault(x => x.login == User.Identity.Name);
                Usage_report rep = new Usage_report { title = vacancy.vacancy_ru, action = "Add", table = "Vacancies", date = DateTime.Now, id_user = user.id };
                vacancy.id_dep = user.id_dep;
                db.Vacancies.Add(vacancy);
                db.Usage_report.Add(rep);
                try
                {
                    db.SaveChanges();
                    return PartialView("_AddVacSuc");
                }
                catch (Exception)
                {
                    return PartialView("_VacFail");
                }
            }

        }
        [HttpPost]
        [Authorize(Roles = "ok")]
        public ActionResult EditDepVacancy(int id, string vacancy_ru, string vacancy_bel, string requirement_ru, string requirement_bel, string payment)//Изменение вакансии для филиала
        {

            if (vacancy_ru == "" || vacancy_bel == "")
            {
                return PartialView("_ValidVacFail");
            }
            else
            {
                try
                {
                    Vacancies vacancy = db.Vacancies.Find(id);
                    Users user = db.Users.FirstOrDefault(x => x.login == User.Identity.Name);
                    if (vacancy.id_dep == user.id_dep)
                    {
                        vacancy.vacancy_ru = vacancy_ru;
                        vacancy.vacancy_bel = vacancy_bel;
                        vacancy.requirement_ru = requirement_ru;
                        vacancy.requirement_bel = requirement_bel;
                        vacancy.payment = payment;
                        Usage_report rep = new Usage_report { title = vacancy.vacancy_ru, action = "Edit", table = "Vacancies", date = DateTime.Now, id_user = user.id };
                        db.Entry(vacancy).State = EntityState.Modified;
                        db.Usage_report.Add(rep);
                        try
                        {
                            db.SaveChanges();
                            return PartialView("_VacSuc");
                        }
                        catch (Exception)
                        {
                            return PartialView("_VacFail");
                        }
                    }
                    else
                    {
                        return PartialView("_VacFail");
                    }
                }
                catch (Exception)
                {
                    return PartialView("_VacFail");
                }

            }
        }
        [HttpGet]
        [Authorize(Roles = "ok")]
        public ActionResult AllBosses()//Все боссы филиала
        {
            Models.Users user = db.Users.FirstOrDefault(x => x.login == User.Identity.Name);
            ViewBag.Bosses = db.Bosses.Where(x => x.id_dep == user.id_dep).ToList();
            return View();
        }
        [HttpGet]
        [Authorize(Roles = "ok")]
        public ActionResult CreateBoss()//Страница добавления нового босса
        {
            ViewBag.Imgs = db.Imgs.Where(x => x.type_id == 6).ToList();
            return View();
        }
        [HttpPost]
        [Authorize(Roles = "ok")]
        public ActionResult CreateBoss(string name, string name_bel, string name_eng, string post, string post_bel, string post_eng, string desc, string desc_bel, string desc_eng, int id_img, string meet_day, string meet_day_bel, string meet_day_eng, string meet_time, string phone)
        {
            Users user = db.Users.FirstOrDefault(x => x.login == User.Identity.Name);
            Bosses boss = new Bosses { name = name, name_bel = name_bel, name_eng = name_eng, post = post, post_bel = post_bel, post_eng = post_eng, desc = desc, desc_bel = desc_bel, desc_eng = desc_eng, id_img = id_img, meet_day = meet_day, meet_day_bel = meet_day_bel, meet_day_eng = meet_day_eng, meet_time = meet_time, phone = phone, id_dep = user.id_dep };
            Usage_report rep = new Usage_report { title = boss.name, action = "Add", table = "Bosses", date = DateTime.Now, id_user = user.id };
            db.Bosses.Add(boss);
            db.Usage_report.Add(rep);
            try
            {
                db.SaveChanges();
                return RedirectToAction("AllBosses");
            }
            catch (Exception e)
            {
                return RedirectToAction("Error", new { message = e.Message });
            }
        }
        [HttpGet]
        [Authorize(Roles = "ok")]
        public ActionResult EditDepBoss(int id)
        {
            try
            {
                Users user = db.Users.FirstOrDefault(x => x.login == User.Identity.Name);
                Bosses boss = db.Bosses.Find(id);
                if (boss.id_dep == user.id_dep)
                {
                    ViewBag.Imgs = db.Imgs.Where(x => x.type_id == 6).ToList();
                    return View(boss);
                }
                else
                {
                    return RedirectToAction("Error", new { message = "Ошибка доступа" });
                }
            }
            catch (Exception e)
            {
                return RedirectToAction("Error", new { message = e.Message });
            }
        }
        [HttpPost]
        [Authorize(Roles = "ok")]
        public ActionResult EditDepBoss(int id, string name, string name_bel, string name_eng, string post, string post_bel, string post_eng, string desc, string desc_bel, string desc_eng, int id_img, string meet_day, string meet_day_bel, string meet_day_eng, string meet_time, string phone)
        {
            try
            {
                Users user = db.Users.FirstOrDefault(x => x.login == User.Identity.Name);
                Bosses boss = db.Bosses.Find(id);
                if (boss.id_dep == user.id_dep)
                {
                    boss.name = name;
                    boss.name_bel = name_bel;
                    boss.name_eng = name_eng;
                    boss.post = post;
                    boss.post_bel = post_bel;
                    boss.post_eng = post_eng;
                    boss.desc = desc;
                    boss.desc_bel = desc_bel;
                    boss.desc_eng = desc_eng;
                    boss.id_img = id_img;
                    boss.meet_day = meet_day;
                    boss.meet_day_bel = meet_day_bel;
                    boss.meet_day_eng = meet_day_eng;
                    boss.meet_time = meet_time;
                    boss.phone = phone;
                    Usage_report rep = new Usage_report { title = boss.name, action = "Edit", table = "Bosses", date = DateTime.Now, id_user = user.id };
                    db.Entry(boss).State = EntityState.Modified;
                    db.Usage_report.Add(rep);
                    try
                    {
                        db.SaveChanges();
                        return RedirectToAction("AllBosses");
                    }
                    catch (Exception)
                    {
                        return View("Error");
                    }
                }
                else
                {
                    return View("Error");
                }
            }
            catch
            {
                return View("Error");
            }
        }


        //НОВОСТИ
        [HttpGet]
        [Authorize(Roles = "admin, news")]
        public ActionResult News()//Главная страница меню новостей
        {
            var newsType = db.News_type.ToList();
            return View(newsType);
        }

        [HttpPost]
        public ActionResult GetNews(int id, int page)
        {
            var ans = db.News.Where(n => n.type_id == id).OrderByDescending(n => n.date).ToList();
            List<News> news = new List<News>();
            if (page <= (ans.Count / 20) + 1 && ans.Count % 20 != 0 || page <= (ans.Count / 20) && ans.Count % 20 == 0)
            {
                if (page == (ans.Count / 20) + 1 && ans.Count % 20 != 0)
                    news = ans.GetRange((page - 1) * 20, ans.Count % 20);
                else news = ans.GetRange((page - 1) * 20, 20);
            }
            return PartialView("_News", news);
        }

        [HttpGet]
        [Authorize(Roles = "admin,news")]
        public ActionResult CreateNews()//Страница добавления новой новости
        {
            Users user = db.Users.FirstOrDefault(x => x.login == User.Identity.Name);

            ViewBag.Types = db.News_type.ToList();
            ViewBag.Imgs = db.Imgs.Where(x => x.type_id == 1).ToList();

            if (user.Roles.role == "admin")
            {
                ViewBag.Departments = db.Departments.ToList();
                return View("CreateNewsAdmin");
            }
            else
            {

            }
            return View();
        }
        [HttpPost]
        [Authorize(Roles = "admin,news")]
        public ActionResult AddNews(News news, int[] img_ids)//Добавление новой новости
        {
            Users user = db.Users.FirstOrDefault(x => x.login == User.Identity.Name);
            Usage_report rep = new Usage_report { title = news.title_ru, action = "Add", table = "News", date = DateTime.Now, id_user = user.id };
            news.date = DateTime.Now;
            if (user.Roles.role != "admin")
            {
                news.id_dep = user.id_dep;
            }

            if (news != null)
            {

                Imgs img = new Imgs();
                if (img_ids != null)
                {
                    foreach (int id in img_ids)
                    {
                        img = db.Imgs.Find(id);
                        news.Imgs.Add(img);
                    }
                }

                try
                {
                    db.News.Add(news);
                    db.Usage_report.Add(rep);
                    db.SaveChanges();
                }
                catch (Exception e)
                {
                    return RedirectToAction("Error", new { message = e.Message });
                }
            }
            return RedirectToAction("News");
        }
        [HttpGet]
        [Authorize(Roles = "admin,news")]
        public ActionResult EditNews(int id)//Страница изменения существующей новости
        {
            Users user = db.Users.FirstOrDefault(x => x.login == User.Identity.Name);

            ViewBag.Types = db.News_type.ToList();
            News news = db.News.Find(id);
            ViewBag.Imgs = news.Imgs;
            if (user.Roles.role == "admin" || news.id_dep == user.id_dep)
            {
                return View(news);
            }
            else
            {
                return RedirectToAction("Error", new { message = "Ошибка доступа" });
            }
        }
        [HttpPost]
        [Authorize(Roles = "admin, news")]
        public ActionResult EditNews(int id, string title_ru, string title_bel, string body_ru, string body_bel, int type_id, int[] img_ids)//Изменение существующей новости
        {
            Users user = db.Users.FirstOrDefault(x => x.login == User.Identity.Name);
            News news = db.News.Find(id);
            if (user.Roles.role == "admin" || news.id_dep == user.id_dep)
            {
                news.title_ru = title_ru;
                news.title_bel = title_bel;
                news.body_ru = body_ru;
                news.body_bel = body_bel;
                news.type_id = type_id;
                news.Imgs.Clear();
                if (img_ids != null)
                {
                    foreach (Imgs img in db.Imgs.Where(x => img_ids.Contains(x.id)))
                    {
                        news.Imgs.Add(img);
                    }
                }

                Usage_report rep = new Usage_report { title = news.title_ru, action = "Edit", table = "News", date = DateTime.Now, id_user = user.id };
                db.Entry(news).State = EntityState.Modified;
                db.Usage_report.Add(rep);
                try
                {
                    db.SaveChanges();
                    return RedirectToAction("News");
                }
                catch (Exception e)
                {
                    return RedirectToAction("Error", new { message = e.Message });
                }
            }
            else
            {
                return RedirectToAction("Error", new { message = "Ошибка доступа" });
            }

        }
        [HttpGet]
        [Authorize(Roles = "admin, news")]
        public ActionResult DeleteNews(int id)//Удаление новости
        {
            Users user = db.Users.FirstOrDefault(x => x.login == User.Identity.Name);
            News news = db.News.Find(id);
            if (user.Roles.role == "admin" || news.id_dep == user.id_dep)
            {
                Usage_report rep = new Usage_report { title = news.title_ru, action = "Delete", table = "News", date = DateTime.Now, id_user = user.id };
                db.News.Remove(news);
                try
                {
                    db.SaveChanges();
                    return RedirectToAction("News");
                }
                catch (Exception e)
                {
                    return RedirectToAction("Error", new { message = e.Message });
                }
            }
            else
            {
                return RedirectToAction("Error", new { message = "Ошибка доступа" });
            }
        }

        //ОБЪЕКТЫ
        [Authorize(Roles = "admin, obj")]
        [HttpGet]
        public ActionResult Objects()//Главная страница меню новостей
        {
            return View(db.Objects.ToList());
        }
        [Authorize(Roles = "admin, obj")]
        [HttpGet]
        public ActionResult CreateObject()//Страница добавления нового объекта
        {
            ViewBag.Departments = db.Departments.ToList();
            ViewBag.Imgs = db.Imgs.Where(x => x.type_id == 2).ToList();
            return View();
        }
        [Authorize(Roles = "admin, obj")]
        [HttpPost]
        public ActionResult AddObject(string title_ru, string title_bel, string title_eng, string desc_ru, string desc_bel, string desc_eng, int[] dep_ids, int[] img_ids)//Добавление нового объекта
        {
            Objects obj = new Objects
            {
                title_ru = title_ru,
                title_bel = title_bel,
                title_eng = title_eng,
                desc_ru = desc_ru,
                desc_bel = desc_bel,
                desc_eng = desc_eng
            };
            Users user = db.Users.FirstOrDefault(x => x.login == User.Identity.Name);
            Usage_report rep = new Usage_report { title = obj.title_ru, action = "Add", table = "Objects", date = DateTime.Now, id_user = user.id };
            if (dep_ids != null)
            {
                foreach (int id in dep_ids)
                {
                    Departments department = new Departments();
                    department = db.Departments.Find(id);
                    obj.Departments.Add(department);
                }
            }

            if (img_ids != null)
            {
                foreach (int id in img_ids)
                {
                    Imgs img = new Imgs();
                    img = db.Imgs.Find(id);
                    obj.Imgs.Add(img);
                }
            }
            try
            {
                db.Objects.Add(obj);
                db.Usage_report.Add(rep);
                db.SaveChanges();
            }
            catch (Exception e)
            {
                return RedirectToAction("Error", new { message = e.Message });
            }

            return RedirectToAction("Objects");
        }
        [Authorize(Roles = "admin, obj")]
        [HttpGet]
        public ActionResult DeleteObject(int id)//Удаление объекта
        {
            Users user = db.Users.FirstOrDefault(x => x.login == User.Identity.Name);
            Objects obj = db.Objects.Find(id);
            Usage_report rep = new Usage_report { title = obj.title_ru, action = "Delete", table = "Objects", date = DateTime.Now, id_user = user.id };
            db.Objects.Remove(obj);
            db.Usage_report.Add(rep);
            try
            {
                db.SaveChanges();
                return RedirectToAction("Objects");
            }
            catch (Exception e)
            {
                return RedirectToAction("Error", new { message = e.Message });
            }
        }
        [Authorize(Roles = "admin, obj")]
        [HttpGet]
        public ActionResult EditObject(int id)//Страница изменения объекта
        {
            ViewBag.Imgs = db.Imgs.Where(x => x.type_id == 2).ToList();
            ViewBag.Departments = db.Departments.ToList();
            return View(db.Objects.Find(id));
        }
        [Authorize(Roles = "admin, obj")]
        [HttpPost]
        public ActionResult EditObject(int id, string title_ru, string title_bel, string title_eng, string desc_ru, string desc_bel, string desc_eng, int[] dep_ids, int[] img_ids)//Изменение объекта
        {
            Objects obj = db.Objects.Find(id);
            obj.title_ru = title_ru;
            obj.title_bel = title_bel;
            obj.title_eng = title_eng;
            obj.desc_ru = desc_ru;
            obj.desc_bel = desc_bel;
            obj.desc_eng = desc_eng;
            obj.Departments.Clear();
            obj.Imgs.Clear();
            Users user = db.Users.FirstOrDefault(x => x.login == User.Identity.Name);
            Usage_report rep = new Usage_report { title = obj.title_ru, action = "Edit", table = "Objects", date = DateTime.Now, id_user = user.id };

            if (dep_ids != null)
            {
                foreach (int dep_id in dep_ids)
                {
                    Departments department = new Departments();
                    department = db.Departments.Find(dep_id);
                    obj.Departments.Add(department);
                }
            }

            if (img_ids != null)
            {
                foreach (int img_id in img_ids)
                {
                    Imgs img = new Imgs();
                    img = db.Imgs.Find(img_id);
                    obj.Imgs.Add(img);
                }
            }
            db.Entry(obj).State = EntityState.Modified;
            db.Usage_report.Add(rep);
            try
            {
                db.SaveChanges();
                return RedirectToAction("Objects");
            }
            catch (Exception e)
            {
                return RedirectToAction("Error", new { message = e.Message });
            }
        }

        //Сертификаты
        [HttpGet]
        [Authorize(Roles = "admin")]
        public ActionResult Certificates()//Главная страница меню сертификатов
        {

            return View(db.Certificates.ToList());
        }
        [HttpGet]
        [Authorize(Roles = "admin")]
        public ActionResult CreateCertificate()//Страница добавления новго сертификата
        {
            ViewBag.Imgs = db.Imgs.Where(x => x.Img_types.id == 5).ToList();
            ViewBag.Departments = db.Departments.ToList();
            return View();
        }
        [HttpPost]
        [Authorize(Roles = "admin")]
        public ActionResult AddCertificate(string name, string name_bel, string name_eng, int[] img_ids, int id_dep)//Добавление новго сертификата
        {
            Certificates certificate = new Certificates { name = name, name_bel = name_bel, name_eng = name_eng, id_dep = id_dep };
            Users user = db.Users.FirstOrDefault(x => x.login == User.Identity.Name);
            Usage_report rep = new Usage_report { title = certificate.name, action = "Add", table = "Certificates", date = DateTime.Now, id_user = user.id };

            foreach (int id in img_ids)
            {
                Imgs img = new Imgs();
                img = db.Imgs.Find(id);
                certificate.Imgs.Add(img);
            }
            db.Certificates.Add(certificate);
            db.Usage_report.Add(rep);
            try
            {
                db.SaveChanges();
            }
            catch (Exception e)
            {
                return RedirectToAction("Error", new { message = e.Message });
            }
            return RedirectToAction("Certificates");
        }
        [HttpGet]
        [Authorize(Roles = "admin")]
        public ActionResult DeleteCertificate(int id)//Удаление сертификата
        {
            Certificates certificate = db.Certificates.Find(id);
            Users user = db.Users.FirstOrDefault(x => x.login == User.Identity.Name);
            Usage_report rep = new Usage_report { title = certificate.name, action = "Delete", table = "Certificates", date = DateTime.Now, id_user = user.id };
            db.Certificates.Remove(certificate);
            db.Usage_report.Add(rep);
            try
            {
                db.SaveChanges();
                return RedirectToAction("Certificates");
            }
            catch (Exception e)
            {
                return RedirectToAction("Error", new { message = e.Message });
            }

        }
        [HttpGet]
        [Authorize(Roles = "admin")]
        public ActionResult EditCertificate(int id)//Страница изменения сертификата
        {
            ViewBag.Imgs = db.Imgs.Where(x => x.type_id == 5).ToList();
            ViewBag.Departments = db.Departments.ToList();
            return View(db.Certificates.Find(id));
        }
        [HttpPost]
        [Authorize(Roles = "admin")]
        public ActionResult EditCertificate(int id, string name, string name_bel, string name_eng, int[] img_ids, int id_dep)//Изменение сертификата
        {
            Certificates certificate = db.Certificates.Find(id);
            certificate.name = name;
            certificate.name_bel = name_bel;
            certificate.name_eng = name_eng;
            certificate.id_dep = id_dep;
            certificate.Imgs.Clear();
            Users user = db.Users.FirstOrDefault(x => x.login == User.Identity.Name);
            Usage_report rep = new Usage_report { title = certificate.name, action = "Edit", table = "Certificates", date = DateTime.Now, id_user = user.id };

            if (img_ids != null)
            {
                foreach (int img_id in img_ids)
                {
                    Imgs img = new Imgs();
                    img = db.Imgs.Find(img_id);
                    certificate.Imgs.Add(img);
                }
            }
            db.Entry(certificate).State = EntityState.Modified;
            db.Usage_report.Add(rep);
            try
            {
                db.SaveChanges();
                return RedirectToAction("Certificates");
            }
            catch (Exception e)
            {
                return RedirectToAction("Error", new { message = e.Message });
            }
        }

        //Руководители
        [HttpGet]
        [Authorize(Roles = "admin, ok_master")]
        public ActionResult Bosses()//Главная страница меню боссов
        {
            return View(db.Departments.ToList());
        }
        [HttpGet]
        [Authorize(Roles = "admin, ok_master")]
        public ActionResult DepBosses(int id)//Боссы выбранного филиала
        {
            ViewBag.Dep = db.Departments.Find(id);
            ViewBag.Bosses = db.Bosses.Where(x => x.id_dep == id).ToList();
            return View();
        }
        [HttpGet]
        [Authorize(Roles = "admin, ok_master")]
        public ActionResult AddBoss(int id)//Страница добавления нового босса
        {
            ViewBag.Id_dep = id;
            ViewBag.Imgs = db.Imgs.Where(x => x.type_id == 6).ToList();
            return View();
        }
        [HttpPost]
        [Authorize(Roles = "admin, ok_master")]
        public ActionResult AddBoss(int id_dep, string name, string name_bel, string name_eng, string post, string post_bel, string post_eng, string desc, string desc_bel, string desc_eng, int id_img, string meet_day, string meet_day_bel, string meet_day_eng, string meet_time, string phone)//Добавление нового босса
        {
            Bosses boss = new Bosses { id_dep = id_dep, name = name, name_bel = name_bel, name_eng = name_eng, post = post, post_bel = post_bel, post_eng = post_eng, desc = desc, desc_bel = desc_bel, desc_eng = desc_eng, id_img = id_img, meet_day = meet_day, meet_day_bel = meet_day_bel, meet_day_eng = meet_day_eng, meet_time = meet_time, phone = phone };
            Users user = db.Users.FirstOrDefault(x => x.login == User.Identity.Name);
            Usage_report rep = new Usage_report { title = boss.name, action = "Add", table = "Bosses", date = DateTime.Now, id_user = user.id };
            db.Bosses.Add(boss);
            db.Usage_report.Add(rep);
            try
            {
                db.SaveChanges();
                return RedirectToAction("DepBosses", new { id = id_dep });
            }
            catch (Exception e)
            {
                return RedirectToAction("Error", new { message = e.Message });
            }

        }
        [HttpGet]
        [Authorize(Roles = "admin, ok_master, ok")]
        public ActionResult DeleteBoss(int id)//Удаление босса
        {
            try
            {
                Users user = db.Users.FirstOrDefault(x => x.login == User.Identity.Name);

                Bosses boss = db.Bosses.Find(id);
                Usage_report rep = new Usage_report { title = boss.name, action = "Delete", table = "Bosses", date = DateTime.Now, id_user = user.id };
                int id_dep = boss.id_dep;
                if (id_dep == user.id_dep || user.Roles.role == "admin" || user.Roles.role == "ok_master")
                {
                    db.Bosses.Remove(db.Bosses.Find(id));
                    db.Usage_report.Add(rep);
                    try
                    {
                        db.SaveChanges();
                        if (user.Roles.role == "admin" || user.Roles.role == "ok_master")
                        {
                            return RedirectToAction("DepBosses", new { id = id_dep });
                        }
                        else
                        {
                            return RedirectToAction("AllBosses");
                        }
                    }
                    catch (Exception e)
                    {
                        return RedirectToAction("Error", new { message = e.Message });
                    }
                }
                else
                {
                    return View("Error");
                }
            }
            catch
            {
                return View("Error");
            }
        }
        [HttpGet]
        [Authorize(Roles = "admin, ok_master")]
        public ActionResult EditBoss(int id)//Страница изменения босса
        {
            ViewBag.Imgs = db.Imgs.Where(x => x.type_id == 6).ToList();
            return View(db.Bosses.Find(id));
        }
        [HttpPost]
        [Authorize(Roles = "admin, ok_master")]
        public ActionResult EditBoss(int id, int id_dep, string name, string name_bel, string name_eng, string post, string post_bel, string post_eng, string desc, string desc_bel, string desc_eng, int id_img, string meet_day, string meet_day_bel, string meet_day_eng, string meet_time, string phone)//Изменение босса
        {
            Bosses boss = db.Bosses.Find(id);
            boss.name = name;
            boss.name_bel = name_bel;
            boss.name_eng = name_eng;
            boss.post = post;
            boss.post_bel = post_bel;
            boss.post_eng = post_eng;
            boss.desc = desc;
            boss.desc_bel = desc_bel;
            boss.desc_eng = desc_eng;
            boss.id_img = id_img;
            boss.meet_day = meet_day;
            boss.meet_day_bel = meet_day_bel;
            boss.meet_day_eng = meet_day_eng;
            boss.meet_time = meet_time;
            boss.phone = phone;
            Users user = db.Users.FirstOrDefault(x => x.login == User.Identity.Name);
            Usage_report rep = new Usage_report { title = boss.name, action = "Edit", table = "Bosses", date = DateTime.Now, id_user = user.id };
            db.Entry(boss).State = EntityState.Modified;
            db.Usage_report.Add(rep);
            try
            {
                db.SaveChanges();
                return RedirectToAction("DepBosses", new { id = id_dep });
            }
            catch (Exception e)
            {
                return RedirectToAction("Error", new { message = e.Message });
            }
        }

        //Аренда
        [HttpGet]
        [Authorize(Roles = "admin, realty_master")]
        public ActionResult Realty()//Меню выбора филиала
        {
            return View(db.Departments.ToList());
        }
        [HttpGet]
        [Authorize(Roles = "admin, realty_master")]
        public ActionResult DepRealty(int id)//Аренда выбранного филиала
        {

            ViewBag.Dep = db.Departments.Find(id);
            ViewBag.Realty = db.Realty.Where(x => x.id_dep == id).ToList();
            return View();
        }
        [HttpGet]
        [Authorize(Roles = "admin, realty_master")]
        public ActionResult AddRealty(int id)//Страница добавления для выбранного филиала
        {
            ViewBag.Id_dep = id;
            ViewBag.Imgs = db.Imgs.Where(x => x.type_id == 7).ToList();
            return View();
        }
        [HttpPost]
        [Authorize(Roles = "admin, realty_master")]
        public ActionResult AddRealty(int id_dep, string title, string title_bel, string adress, string adress_bel, string desc, string desc_bel, int[] img_ids)//Добавление аренды
        {
            Realty realty = new Realty { id_dep = id_dep, title = title, title_bel = title_bel, adress = adress, adress_bel = adress_bel, desc = desc, desc_bel = desc_bel };
            Users user = db.Users.FirstOrDefault(x => x.login == User.Identity.Name);
            Usage_report rep = new Usage_report { title = realty.title, action = "Add", table = "Realty", date = DateTime.Now, id_user = user.id };
            if (img_ids != null)
            {
                foreach (int id in img_ids)
                {
                    realty.Imgs.Add(db.Imgs.Find(id));
                }
            }
            db.Realty.Add(realty);
            db.Usage_report.Add(rep);
            try
            {
                db.SaveChanges();
                return RedirectToAction("DepRealty", new { id = id_dep });
            }
            catch (Exception e)
            {
                return RedirectToAction("Error", new { message = e.Message });
            }
        }
        [HttpGet]
        [Authorize(Roles = "admin, realty_master")]
        public ActionResult EditRealty(int id)//Изменение аренды
        {
            ViewBag.Imgs = db.Imgs.Where(x => x.type_id == 7).ToList();
            return View(db.Realty.Find(id));
        }
        [HttpPost]
        [Authorize(Roles = "admin, realty_master")]
        public ActionResult EditRealty(int id, int id_dep, string title, string title_bel, string adress, string adress_bel, string desc, string desc_bel, int[] img_ids)//Изменения аренды
        {
            try
            {
                Realty realty = db.Realty.Find(id);
                realty.id_dep = id_dep;
                realty.title = title;
                realty.title_bel = title_bel;
                realty.adress = adress;
                realty.adress_bel = adress_bel;
                realty.desc = desc;
                realty.desc_bel = desc_bel;
                Users user = db.Users.FirstOrDefault(x => x.login == User.Identity.Name);
                Usage_report rep = new Usage_report { title = realty.title, action = "Edit", table = "Realty", date = DateTime.Now, id_user = user.id };
                realty.Imgs.Clear();
                if (img_ids != null)
                {
                    foreach (int img_id in img_ids)
                    {
                        realty.Imgs.Add(db.Imgs.Find(img_id));
                    }
                }
                db.Entry(realty).State = EntityState.Modified;
                db.Usage_report.Add(rep);
                try
                {
                    db.SaveChanges();
                    return RedirectToAction("DepRealty", new { id = id_dep });
                }
                catch (Exception e)
                {
                    return RedirectToAction("Error", new { message = e.Message });
                }
            }
            catch (Exception e)
            {
                return RedirectToAction("Error", new { message = e.Message });
            }
        }
        [HttpGet]
        [Authorize(Roles = "admin, realty_master, realty")]
        public ActionResult DeleteRealty(int id)//Удаление аренды
        {
            try
            {
                Users user = db.Users.FirstOrDefault(x => x.login == User.Identity.Name);
                Realty realty = db.Realty.Find(id);
                Usage_report rep = new Usage_report { title = realty.title, action = "Delete", table = "Realty", date = DateTime.Now, id_user = user.id };
                int id_dep = realty.id_dep;
                if (id_dep == user.id_dep || user.Roles.role == "admin" || user.Roles.role == "realty_master")
                {
                    db.Realty.Remove(realty);
                    db.Usage_report.Add(rep);
                    try
                    {
                        db.SaveChanges();
                        if (user.Roles.role == "admin" || user.Roles.role == "realty_master")
                        {
                            return RedirectToAction("DepRealty", new { id = id_dep });
                        }
                        else
                        {
                            return RedirectToAction("AllRealty");
                        }
                    }
                    catch (Exception e)
                    {
                        return RedirectToAction("Error", new { message = e.Message });
                    }
                }
                else
                {
                    return RedirectToAction("Error", new { message = "Недостаночно прав доступа" });
                }
            }
            catch (Exception e)
            {
                return RedirectToAction("Error", new { message = e.Message });
            }
        }

        [HttpGet]
        [Authorize(Roles = "realty")]
        public ActionResult AllRealty()//Меню для филиалов
        {
            Users user = db.Users.FirstOrDefault(x => x.login == User.Identity.Name);
            ViewBag.Realty = db.Realty.Where(x => x.id_dep == user.id_dep).ToList();
            return View();
        }
        [HttpGet]
        [Authorize(Roles = "realty")]
        public ActionResult CreateRealty()//Страница добавления для филиалов
        {
            ViewBag.Imgs = db.Imgs.Where(x => x.type_id == 7).ToList();
            return View();
        }
        [HttpPost]
        [Authorize(Roles = "realty")]
        public ActionResult CreateRealty(string title, string title_bel, string adress, string adress_bel, string desc, string desc_bel, int[] img_ids)//Добавление для филилала
        {
            Users user = db.Users.FirstOrDefault(x => x.login == User.Identity.Name);
            Realty realty = new Realty { id_dep = user.id_dep, title = title, title_bel = title_bel, adress = adress, adress_bel = adress_bel, desc = desc, desc_bel = desc_bel };
            Usage_report rep = new Usage_report { title = realty.title, action = "Add", table = "Realty", date = DateTime.Now, id_user = user.id };
            if (img_ids != null)
            {
                foreach (int id in img_ids)
                {
                    realty.Imgs.Add(db.Imgs.Find(id));
                }
            }
            db.Realty.Add(realty);
            db.Usage_report.Add(rep);
            try
            {
                db.SaveChanges();
                return RedirectToAction("AllRealty");
            }
            catch (Exception e)
            {
                return RedirectToAction("Error", new { message = e.Message });
            }
        }
        [HttpGet]
        [Authorize(Roles = "realty")]
        public ActionResult EditDepRealty(int id)//Страница изменения для филиала
        {
            Users user = db.Users.FirstOrDefault(x => x.login == User.Identity.Name);
            Realty realty = db.Realty.Find(id);
            if (realty.id_dep == user.id_dep)
            {
                ViewBag.Imgs = db.Imgs.Where(x => x.type_id == 7).ToList();
                return View(db.Realty.Find(id));
            }
            else
            {
                return View("Error");
            }
        }
        [HttpPost]
        [Authorize(Roles = "realty")]
        public ActionResult EditDepRealty(int id, string title, string title_bel, string adress, string adress_bel, string desc, string desc_bel, int[] img_ids)//Измененеие для филиала
        {
            try
            {
                Users user = db.Users.FirstOrDefault(x => x.login == User.Identity.Name);
                Realty realty = db.Realty.Find(id);
                if (realty.id_dep == user.id_dep)
                {
                    realty.title = title;
                    realty.title_bel = title_bel;
                    realty.adress = adress;
                    realty.adress_bel = adress_bel;
                    realty.desc = desc;
                    realty.desc_bel = desc_bel;
                    Usage_report rep = new Usage_report { title = realty.title, action = "Edit", table = "Realty", date = DateTime.Now, id_user = user.id };
                    realty.Imgs.Clear();
                    if (img_ids != null)
                    {
                        foreach (int img_id in img_ids)
                        {
                            realty.Imgs.Add(db.Imgs.Find(img_id));
                        }
                    }
                    db.Entry(realty).State = EntityState.Modified;
                    db.Usage_report.Add(rep);
                    try
                    {
                        db.SaveChanges();
                        return RedirectToAction("AllRealty");
                    }
                    catch (Exception e)
                    {
                        return RedirectToAction("Error", new { message = e.Message });
                    }
                }
                else
                {
                    return RedirectToAction("Error", new { message = "Недостаточно прав доступа" });
                }
            }
            catch (Exception e)
            {
                return RedirectToAction("Error", new { message = e.Message });
            }
        }

        //Продажа имущества
        [HttpGet]
        [Authorize(Roles = "admin, realty_master")]
        public ActionResult Sale()//Меню выбора филиала
        {
            return View(db.Departments.ToList());
        }
        [HttpGet]
        [Authorize(Roles = "admin, realty_master")]
        public ActionResult DepSale(int id)//Продажа имущества выбранного филиала
        {

            ViewBag.Dep = db.Departments.Find(id);
            ViewBag.Sale = db.Sale.Where(x => x.id_dep == id).ToList();
            return View();
        }
        [HttpGet]
        [Authorize(Roles = "admin, realty_master")]
        public ActionResult AddSale(int id)//Страница добавления для выбранного филиала
        {
            ViewBag.Id_dep = id;
            ViewBag.Imgs = db.Imgs.Where(x => x.type_id == 10).ToList();
            return View();
        }
        [HttpPost]
        [Authorize(Roles = "admin, realty_master")]
        public ActionResult AddSale(int id_dep, string title, string title_bel, string adress, string adress_bel, string desc, string desc_bel, int[] img_ids)//Добавление продоваемой имущества
        {
            Sale sale = new Sale { id_dep = id_dep, title = title, title_bel = title_bel, adress = adress, adress_bel = adress_bel, desc = desc, desc_bel = desc_bel };
            Users user = db.Users.FirstOrDefault(x => x.login == User.Identity.Name);
            Usage_report rep = new Usage_report { title = sale.title, action = "Add", table = "Sale", date = DateTime.Now, id_user = user.id };
            if (img_ids != null)
            {
                foreach (int id in img_ids)
                {
                    sale.Imgs.Add(db.Imgs.Find(id));
                }
            }
            db.Sale.Add(sale);
            db.Usage_report.Add(rep);
            try
            {
                db.SaveChanges();
                return RedirectToAction("DepSale", new { id = id_dep });
            }
            catch (Exception e)
            {
                return RedirectToAction("Error", new { message = e.Message });
            }
        }
        [HttpGet]
        [Authorize(Roles = "admin, realty_master")]
        public ActionResult EditSale(int id)//Изменение продоваемого имущества
        {
            ViewBag.Imgs = db.Imgs.Where(x => x.type_id == 10).ToList();
            return View(db.Sale.Find(id));
        }
        [HttpPost]
        [Authorize(Roles = "admin, realty_master")]
        public ActionResult EditSale(int id, int id_dep, string title, string title_bel, string adress, string adress_bel, string desc, string desc_bel, int[] img_ids)//Изменение продоваемого имущества
        {
            try
            {
                Sale sale = db.Sale.Find(id);
                sale.id_dep = id_dep;
                sale.title = title;
                sale.title_bel = title_bel;
                sale.adress = adress;
                sale.adress_bel = adress_bel;
                sale.desc = desc;
                sale.desc_bel = desc_bel;
                Users user = db.Users.FirstOrDefault(x => x.login == User.Identity.Name);
                Usage_report rep = new Usage_report { title = sale.title, action = "Edit", table = "Sale", date = DateTime.Now, id_user = user.id };
                sale.Imgs.Clear();
                if (img_ids != null)
                {
                    foreach (int img_id in img_ids)
                    {
                        sale.Imgs.Add(db.Imgs.Find(img_id));
                    }
                }
                db.Entry(sale).State = EntityState.Modified;
                db.Usage_report.Add(rep);
                try
                {
                    db.SaveChanges();
                    return RedirectToAction("DepSale", new { id = id_dep });
                }
                catch (Exception e)
                {
                    return RedirectToAction("Error", new { message = e.Message });
                }
            }
            catch (Exception e)
            {
                return RedirectToAction("Error", new { message = e.Message });
            }
        }
        [HttpGet]
        [Authorize(Roles = "admin, realty_master, realty")]
        public ActionResult DeleteSale(int id)//Удаление продоваемого имущества
        {
            try
            {
                Users user = db.Users.FirstOrDefault(x => x.login == User.Identity.Name);
                Sale sale = db.Sale.Find(id);
                Usage_report rep = new Usage_report { title = sale.title, action = "Delete", table = "Sale", date = DateTime.Now, id_user = user.id };
                int id_dep = sale.id_dep;
                if (id_dep == user.id_dep || user.Roles.role == "admin" || user.Roles.role == "realty_master")
                {
                    db.Sale.Remove(sale);
                    db.Usage_report.Add(rep);
                    try
                    {
                        db.SaveChanges();
                        if (user.Roles.role == "admin" || user.Roles.role == "realty_master")
                        {
                            return RedirectToAction("DepSale", new { id = id_dep });
                        }
                        else
                        {
                            return RedirectToAction("AllSale");
                        }
                    }
                    catch (Exception e)
                    {
                        return RedirectToAction("Error", new { message = e.Message });
                    }
                }
                else
                {
                    return RedirectToAction("Error", new { message = "Недостаночно прав доступа" });
                }
            }
            catch (Exception e)
            {
                return RedirectToAction("Error", new { message = e.Message });
            }
        }

        [HttpGet]
        [Authorize(Roles = "realty")]
        public ActionResult AllSale()//Меню для филиалов
        {
            Users user = db.Users.FirstOrDefault(x => x.login == User.Identity.Name);
            ViewBag.Sale = db.Sale.Where(x => x.id_dep == user.id_dep).ToList();
            return View();
        }
        [HttpGet]
        [Authorize(Roles = "realty")]
        public ActionResult CreateSale()//Страница добавления для филиалов
        {
            ViewBag.Imgs = db.Imgs.Where(x => x.type_id == 10).ToList();
            return View();
        }
        [HttpPost]
        [Authorize(Roles = "realty")]
        public ActionResult CreateSale(string title, string title_bel, string adress, string adress_bel, string desc, string desc_bel, int[] img_ids)//Добавление для филилала
        {
            Users user = db.Users.FirstOrDefault(x => x.login == User.Identity.Name);
            Sale sale = new Sale { id_dep = user.id_dep, title = title, title_bel = title_bel, adress = adress, adress_bel = adress_bel, desc = desc, desc_bel = desc_bel };
            Usage_report rep = new Usage_report { title = sale.title, action = "Add", table = "Sale", date = DateTime.Now, id_user = user.id };
            if (img_ids != null)
            {
                foreach (int id in img_ids)
                {
                    sale.Imgs.Add(db.Imgs.Find(id));
                }
            }
            db.Sale.Add(sale);
            db.Usage_report.Add(rep);
            try
            {
                db.SaveChanges();
                return RedirectToAction("AllSale");
            }
            catch (Exception e)
            {
                return RedirectToAction("Error", new { message = e.Message });
            }
        }
        [HttpGet]
        [Authorize(Roles = "realty")]
        public ActionResult EditDepSale(int id)//Страница изменения для филиала
        {
            Users user = db.Users.FirstOrDefault(x => x.login == User.Identity.Name);
            Sale sale = db.Sale.Find(id);
            if (sale.id_dep == user.id_dep)
            {
                ViewBag.Imgs = db.Imgs.Where(x => x.type_id == 10).ToList();
                return View(db.Sale.Find(id));
            }
            else
            {
                return View("Error");
            }
        }
        [HttpPost]
        [Authorize(Roles = "realty")]
        public ActionResult EditDepSale(int id, string title, string title_bel, string adress, string adress_bel, string desc, string desc_bel, int[] img_ids)//Измененеие для филиала
        {
            try
            {
                Users user = db.Users.FirstOrDefault(x => x.login == User.Identity.Name);
                Sale sale = db.Sale.Find(id);
                if (sale.id_dep == user.id_dep)
                {
                    sale.title = title;
                    sale.title_bel = title_bel;
                    sale.adress = adress;
                    sale.adress_bel = adress_bel;
                    sale.desc = desc;
                    sale.desc_bel = desc_bel;
                    Usage_report rep = new Usage_report { title = sale.title, action = "Edit", table = "Sale", date = DateTime.Now, id_user = user.id };
                    sale.Imgs.Clear();
                    if (img_ids != null)
                    {
                        foreach (int img_id in img_ids)
                        {
                            sale.Imgs.Add(db.Imgs.Find(img_id));
                        }
                    }
                    db.Entry(sale).State = EntityState.Modified;
                    db.Usage_report.Add(rep);
                    try
                    {
                        db.SaveChanges();
                        return RedirectToAction("AllSale");
                    }
                    catch (Exception e)
                    {
                        return RedirectToAction("Error", new { message = e.Message });
                    }
                }
                else
                {
                    return RedirectToAction("Error", new { message = "Недостаточно прав доступа" });
                }
            }
            catch (Exception e)
            {
                return RedirectToAction("Error", new { message = e.Message });
            }
        }

        //Техника
        [HttpGet]
        [Authorize(Roles = "admin, mech_master")]
        public ActionResult Mechs()//Меню выбора филиала
        {
            return View(db.Departments.ToList());
        }
        [HttpGet]
        [Authorize(Roles = "admin, mech_master")]
        public ActionResult DepMechs(int id)//Меню для филиала
        {
            ViewBag.Dep = db.Departments.Find(id);
            ViewBag.Mechs = db.Mechanisms.Where(x => x.id_dep == id).ToList();
            return View();
        }
        [HttpGet]
        [Authorize(Roles = "admin, mech_master")]
        public ActionResult AddMech(int id)//Страница добавления техники
        {
            ViewBag.Id_dep = id;
            ViewBag.Imgs = db.Imgs.Where(x => x.type_id == 8).ToList();
            return View();
        }
        [HttpPost]
        [Authorize(Roles = "admin, mech_master")]
        public ActionResult AddMech(int id_dep, string title, string title_bel, string desc, string desc_bel, int[] img_ids)//Добавление техникив
        {
            Mechanisms mech = new Mechanisms { id_dep = id_dep, title = title, title_bel = title_bel, desc = desc, desc_bel = desc_bel };
            Users user = db.Users.FirstOrDefault(x => x.login == User.Identity.Name);
            Usage_report rep = new Usage_report { title = mech.title, action = "Add", table = "Mechanisms", date = DateTime.Now, id_user = user.id };
            if (img_ids != null)
            {
                foreach (int id in img_ids)
                {
                    mech.Imgs.Add(db.Imgs.Find(id));
                }
            }
            db.Mechanisms.Add(mech);
            db.Usage_report.Add(rep);
            try
            {
                db.SaveChanges();
                return RedirectToAction("DepMechs", new { id = id_dep });
            }
            catch (Exception e)
            {
                return RedirectToAction("Error", new { message = e.Message });
            }
        }
        [HttpGet]
        [Authorize(Roles = "admin, mech_master")]
        public ActionResult EditMech(int id)//Изменение техники
        {
            ViewBag.Imgs = db.Imgs.Where(x => x.type_id == 8).ToList();
            return View(db.Mechanisms.Find(id));
        }
        [HttpPost]
        [Authorize(Roles = "admin, mech_master")]
        public ActionResult EditMech(int id, int id_dep, string title, string title_bel, string desc, string desc_bel, int[] img_ids)//Изменения техники
        {
            try
            {
                Mechanisms mech = db.Mechanisms.Find(id);
                mech.id_dep = id_dep;
                mech.title = title;
                mech.title_bel = title_bel;
                mech.desc = desc;
                mech.desc_bel = desc_bel;
                Users user = db.Users.FirstOrDefault(x => x.login == User.Identity.Name);
                Usage_report rep = new Usage_report { title = mech.title, action = "Edit", table = "Mechanisms", date = DateTime.Now, id_user = user.id };
                mech.Imgs.Clear();
                if (img_ids != null)
                {
                    foreach (int img_id in img_ids)
                    {
                        mech.Imgs.Add(db.Imgs.Find(img_id));
                    }
                }
                db.Entry(mech).State = EntityState.Modified;
                db.Usage_report.Add(rep);
                try
                {
                    db.SaveChanges();
                    return RedirectToAction("DepMechs", new { id = id_dep });
                }
                catch (Exception e)
                {
                    return RedirectToAction("Error", new { message = e.Message });
                }
            }
            catch (Exception e)
            {
                return RedirectToAction("Error", new { message = e.Message });
            }
        }
        [HttpGet]
        [Authorize(Roles = "admin, mech_master, mech")]
        public ActionResult DeleteMech(int id)//Удаление техники
        {
            try
            {
                Users user = db.Users.FirstOrDefault(x => x.login == User.Identity.Name);
                Mechanisms mech = db.Mechanisms.Find(id);
                int id_dep = mech.id_dep;
                Usage_report rep = new Usage_report { title = mech.title, action = "Delete", table = "Mechanisms", date = DateTime.Now, id_user = user.id };
                if (id_dep == user.id_dep || user.Roles.role == "admin" || user.Roles.role == "mech_master")
                {
                    db.Mechanisms.Remove(mech);
                    db.Usage_report.Add(rep);
                    try
                    {
                        db.SaveChanges();
                        if (user.Roles.role == "admin" || user.Roles.role == "mech_master")
                        {
                            return RedirectToAction("DepMechs", new { id = id_dep });
                        }
                        else
                        {
                            return RedirectToAction("AllMechs");
                        }
                    }
                    catch (Exception)
                    {
                        return View("Error");
                    }
                }
                else
                {
                    return View("Error");
                }
            }
            catch
            {
                return View("Error");
            }
        }

        [HttpGet]
        [Authorize(Roles = "mech")]
        public ActionResult AllMechs()//Меню для филиалов
        {
            Users user = db.Users.FirstOrDefault(x => x.login == User.Identity.Name);
            ViewBag.Mechs = db.Mechanisms.Where(x => x.id_dep == user.id_dep).ToList();
            return View();
        }
        [HttpGet]
        [Authorize(Roles = "mech")]
        public ActionResult CreateMech()//Страница добавления для филиалов
        {
            ViewBag.Imgs = db.Imgs.Where(x => x.type_id == 8).ToList();
            return View();
        }
        [HttpPost]
        [Authorize(Roles = "mech")]
        public ActionResult CreateMech(string title, string title_bel, string desc, string desc_bel, int[] img_ids)//Добавление для филилала
        {
            Users user = db.Users.FirstOrDefault(x => x.login == User.Identity.Name);
            Mechanisms mech = new Mechanisms { id_dep = user.id_dep, title = title, title_bel = title_bel, desc = desc, desc_bel = desc_bel };
            Usage_report rep = new Usage_report { title = mech.title, action = "Add", table = "Mechanisms", date = DateTime.Now, id_user = user.id };
            if (img_ids != null)
            {
                foreach (int id in img_ids)
                {
                    mech.Imgs.Add(db.Imgs.Find(id));
                }
            }
            db.Mechanisms.Add(mech);
            db.Usage_report.Add(rep);
            try
            {
                db.SaveChanges();
                return RedirectToAction("AllMechs");
            }
            catch (Exception e)
            {
                return RedirectToAction("Error", new { message = e.Message });
            }
        }
        [HttpGet]
        [Authorize(Roles = "mech")]
        public ActionResult EditDepMech(int id)//Страница изменения для филиала
        {
            Users user = db.Users.FirstOrDefault(x => x.login == User.Identity.Name);
            Mechanisms mech = db.Mechanisms.Find(id);
            if (mech.id_dep == user.id_dep)
            {
                ViewBag.Imgs = db.Imgs.Where(x => x.type_id == 8).ToList();
                return View(db.Mechanisms.Find(id));
            }
            else
            {
                return View("Error");
            }
        }
        [HttpPost]
        [Authorize(Roles = "mech")]
        public ActionResult EditDepMech(int id, string title, string title_bel, string desc, string desc_bel, int[] img_ids)//Измененеие для филиала
        {
            try
            {
                Users user = db.Users.FirstOrDefault(x => x.login == User.Identity.Name);
                Mechanisms mech = db.Mechanisms.Find(id);
                if (mech.id_dep == user.id_dep)
                {
                    mech.title = title;
                    mech.title_bel = title_bel;
                    mech.desc = desc;
                    mech.desc_bel = desc_bel;
                    Usage_report rep = new Usage_report { title = mech.title, action = "Edit", table = "Mechanisms", date = DateTime.Now, id_user = user.id };
                    mech.Imgs.Clear();
                    if (img_ids != null)
                    {
                        foreach (int img_id in img_ids)
                        {
                            mech.Imgs.Add(db.Imgs.Find(img_id));
                        }
                    }
                    db.Entry(mech).State = EntityState.Modified;
                    db.Usage_report.Add(rep);
                    try
                    {
                        db.SaveChanges();
                        return RedirectToAction("AllMechs");
                    }
                    catch (Exception e)
                    {
                        return RedirectToAction("Error", new { message = e.Message });
                    }
                }
                else
                {
                    return RedirectToAction("Error", new { message = "Недостаточно прав доступа" });
                }
            }
            catch (Exception e)
            {
                return RedirectToAction("Error", new { message = e.Message });
            }
        }

        //УСЛУГИ
        [HttpGet]
        [Authorize(Roles = "admin, service_master")]
        public ActionResult Services()//Меню выбора филиала
        {
            return View(db.Departments.ToList());
        }
        [HttpGet]
        [Authorize(Roles = "admin, service_master")]
        public ActionResult DepServices(int id)//Меню для филиала
        {
            ViewBag.Dep = db.Departments.Find(id);
            ViewBag.Services = db.Services.Where(x => x.id_dep == id).ToList();
            return View();
        }
        [HttpGet]
        [Authorize(Roles = "admin, service_master")]
        public ActionResult AddService(int id)//Страница добавления услуги
        {
            ViewBag.Id_dep = id;
            ViewBag.Imgs = db.Imgs.Where(x => x.type_id == 4).ToList();
            return View();
        }
        [Authorize(Roles = "admin, service_master")]
        public ActionResult AddService(int id_dep, string title, string title_bel, string desc, string desc_bel, int[] img_ids)//Добавление услуги
        {
            Services service = new Services { id_dep = id_dep, title = title, title_bel = title_bel, desc = desc, desc_bel = desc_bel };
            Users user = db.Users.FirstOrDefault(x => x.login == User.Identity.Name);
            Usage_report rep = new Usage_report { title = service.title, action = "Add", table = "Services", date = DateTime.Now, id_user = user.id };
            if (img_ids != null)
            {
                foreach (int id in img_ids)
                {
                    service.Imgs.Add(db.Imgs.Find(id));
                }
            }
            db.Services.Add(service);
            db.Usage_report.Add(rep);
            try
            {
                db.SaveChanges();
                return RedirectToAction("DepServices", new { id = id_dep });
            }
            catch (Exception e)
            {
                return RedirectToAction("Error", new { message = e.Message });
            }
        }
        [HttpGet]
        [Authorize(Roles = "admin, service_master")]
        public ActionResult EditService(int id)//Страница Изменение услуги
        {
            ViewBag.Imgs = db.Imgs.Where(x => x.type_id == 4).ToList();
            return View(db.Services.Find(id));
        }
        [HttpPost]
        [Authorize(Roles = "admin, service_master")]
        public ActionResult EditService(int id, int id_dep, string title, string title_bel, string desc, string desc_bel, int[] img_ids)//Изменения услуги
        {
            try
            {
                Services service = db.Services.Find(id);
                service.id_dep = id_dep;
                service.title = title;
                service.title_bel = title_bel;
                service.desc = desc;
                service.desc_bel = desc_bel;
                Users user = db.Users.FirstOrDefault(x => x.login == User.Identity.Name);
                Usage_report rep = new Usage_report { title = service.title, action = "Edit", table = "Services", date = DateTime.Now, id_user = user.id };
                service.Imgs.Clear();
                if (img_ids != null)
                {
                    foreach (int img_id in img_ids)
                    {
                        service.Imgs.Add(db.Imgs.Find(img_id));
                    }
                }
                db.Entry(service).State = EntityState.Modified;
                db.Usage_report.Add(rep);
                try
                {
                    db.SaveChanges();
                    return RedirectToAction("DepServices", new { id = id_dep });
                }
                catch (Exception e)
                {
                    return RedirectToAction("Error", new { message = e.Message });
                }
            }
            catch (Exception e)
            {
                return RedirectToAction("Error", new { message = e.Message });
            }
        }
        [HttpGet]
        [Authorize(Roles = "admin, service_master, service")]
        public ActionResult DeleteService(int id)//Удаление услуги
        {
            try
            {
                Users user = db.Users.FirstOrDefault(x => x.login == User.Identity.Name);
                Services service = db.Services.Find(id);
                Usage_report rep = new Usage_report { title = service.title, action = "Delete", table = "Services", date = DateTime.Now, id_user = user.id };
                int id_dep = service.id_dep;
                if (id_dep == user.id_dep || user.Roles.role == "admin" || user.Roles.role == "service_master")
                {
                    db.Services.Remove(service);
                    try
                    {
                        db.SaveChanges();
                        if (user.Roles.role == "admin" || user.Roles.role == "service_master")
                        {
                            return RedirectToAction("DepServices", new { id = id_dep });
                        }
                        else
                        {
                            return RedirectToAction("AllServices");
                        }
                    }
                    catch (Exception e)
                    {
                        return RedirectToAction("Error", new { message = e.Message });
                    }
                }
                else
                {
                    return RedirectToAction("Error", new { message = "Недостаточно прав доступа" });
                }
            }
            catch (Exception e)
            {
                return RedirectToAction("Error", new { message = e.Message });
            }
        }

        [HttpGet]
        [Authorize(Roles = "service")]
        public ActionResult AllServices()//Меню для филиалов
        {
            Users user = db.Users.FirstOrDefault(x => x.login == User.Identity.Name);
            ViewBag.Services = db.Services.Where(x => x.id_dep == user.id_dep).ToList();
            return View();
        }
        [HttpGet]
        [Authorize(Roles = "service")]
        public ActionResult CreateService()//Страница добавления для филиалов
        {
            ViewBag.Imgs = db.Imgs.Where(x => x.type_id == 4).ToList();
            return View();
        }
        [HttpPost]
        [Authorize(Roles = "service")]
        public ActionResult CreateService(string title, string title_bel, string desc, string desc_bel, int[] img_ids)//Добавление для филилала
        {
            Users user = db.Users.FirstOrDefault(x => x.login == User.Identity.Name);
            Services service = new Services { id_dep = user.id_dep, title = title, title_bel = title_bel, desc = desc, desc_bel = desc_bel };
            Usage_report rep = new Usage_report { title = service.title, action = "Add", table = "Services", date = DateTime.Now, id_user = user.id };
            if (img_ids != null)
            {
                foreach (int id in img_ids)
                {
                    service.Imgs.Add(db.Imgs.Find(id));
                }
            }
            db.Services.Add(service);
            db.Usage_report.Add(rep);
            try
            {
                db.SaveChanges();
                return RedirectToAction("AllServices");
            }
            catch (Exception e)
            {
                return RedirectToAction("Error", new { message = e.Message });
            }
        }
        [HttpGet]
        [Authorize(Roles = "service")]
        public ActionResult EditDepService(int id)//Страница изменения для филиала
        {
            Users user = db.Users.FirstOrDefault(x => x.login == User.Identity.Name);
            Services service = db.Services.Find(id);
            if (service.id_dep == user.id_dep)
            {
                ViewBag.Imgs = db.Imgs.Where(x => x.type_id == 4).ToList();
                return View(db.Services.Find(id));
            }
            else
            {
                return View("Error");
            }
        }
        [HttpPost]
        [Authorize(Roles = "service")]
        public ActionResult EditDepService(int id, string title, string title_bel, string desc, string desc_bel, int[] img_ids)//Измененеие для филиала
        {
            try
            {
                Users user = db.Users.FirstOrDefault(x => x.login == User.Identity.Name);
                Services service = db.Services.Find(id);
                if (service.id_dep == user.id_dep)
                {
                    service.title = title;
                    service.title_bel = title_bel;
                    service.desc = desc;
                    service.desc_bel = desc_bel;
                    Usage_report rep = new Usage_report { title = service.title, action = "Edit", table = "Services", date = DateTime.Now, id_user = user.id };
                    service.Imgs.Clear();
                    if (img_ids != null)
                    {
                        foreach (int img_id in img_ids)
                        {
                            service.Imgs.Add(db.Imgs.Find(img_id));
                        }
                    }
                    db.Entry(service).State = EntityState.Modified;
                    db.Usage_report.Add(rep);
                    try
                    {
                        db.SaveChanges();
                        return RedirectToAction("AllServices");
                    }
                    catch (Exception e)
                    {
                        return RedirectToAction("Error", new { message = e.Message });
                    }
                }
                else
                {
                    return RedirectToAction("Error", new { message = "Недостаточно прав доступа" });
                }
            }
            catch (Exception e)
            {
                return RedirectToAction("Error", new { message = e.Message });
            }
        }

        //Инфа о предприятиях
        [HttpGet]
        [Authorize(Roles = "admin")]
        public ActionResult Departments()//Все филиалы
        {
            return View(db.Departments.ToList());
        }
        [HttpGet]
        [Authorize(Roles = "admin")]
        public ActionResult Department(int id)//Страница редактирования предприятия
        {
            Departments dep = db.Departments.Find(id);
            ViewBag.Imgs = db.Imgs.Where(x => x.type_id == 9).ToList();
            return View(dep);
        }
        [HttpPost]
        [Authorize(Roles = "admin")]
        public ActionResult EditDepartment(int id, string name_ru, string name_bel, string name_eng, string adress_ru, string adress_bel, string adress_eng, string desc_ru, string desc_bel, string desc_eng, int id_img, string link, string short_name_ru, string short_name_bel, string short_name_eng, string main_text_ru, string main_text_bel, string main_text_eng)
        {
            Departments dep = db.Departments.Find(id);
            dep.name_ru = name_ru;
            dep.name_bel = name_bel;
            dep.name_eng = name_eng;
            dep.adress_ru = adress_ru;
            dep.adress_bel = adress_bel;
            dep.adress_eng = adress_eng;
            dep.desc_ru = desc_ru;
            dep.desc_bel = desc_bel;
            dep.desc_eng = desc_eng;
            dep.id_img = id_img;
            dep.link = link;
            dep.short_name_ru = short_name_ru;
            dep.short_name_bel = short_name_bel;
            dep.short_name_eng = short_name_eng;
            dep.main_text_bel = main_text_bel;
            dep.main_text_ru = main_text_ru;
            Users user = db.Users.FirstOrDefault(x => x.login == User.Identity.Name);
            Usage_report rep = new Usage_report { title = dep.name_ru, action = "Edit", table = "Departments", date = DateTime.Now, id_user = user.id };
            db.Entry(dep).State = EntityState.Modified;
            db.Usage_report.Add(rep);
            try
            {
                db.SaveChanges();
                return RedirectToAction("Departments");
            }
            catch (Exception e)
            {
                return RedirectToAction("Error", new { message = e.Message });
            }
        }


        //ОТЧЕТ ОБ ИСПОЛЬЗОВАНИИ АДМИНКИ
        [HttpGet]
        [Authorize(Roles = "admin")]
        public ActionResult UsageReport()
        {
            return View();
        }
        [HttpGet]
        [Authorize(Roles = "admin")]
        public ActionResult NewsReport()
        {
            return View("Report", db.Usage_report.Where(x => x.table == "News"));
        }
        [HttpGet]
        [Authorize(Roles = "admin")]
        public ActionResult ObjectsReport()
        {
            return View("Report", db.Usage_report.Where(x => x.table == "Objects"));
        }
        [HttpGet]
        [Authorize(Roles = "admin")]
        public ActionResult VacanciesReport()
        {
            return View("Report", db.Usage_report.Where(x => x.table == "Vacancies"));
        }
        [HttpGet]
        [Authorize(Roles = "admin")]
        public ActionResult ImgsReport()
        {
            return View("Report", db.Usage_report.Where(x => x.table == "Imgs"));
        }
        [HttpGet]
        [Authorize(Roles = "admin")]
        public ActionResult RealtyReport()
        {
            return View("Report", db.Usage_report.Where(x => x.table == "Realty"));
        }
        [HttpGet]
        [Authorize(Roles = "admin")]
        public ActionResult SaleReport()
        {
            return View("Report", db.Usage_report.Where(x => x.table == "Sale"));
        }
        [HttpGet]
        [Authorize(Roles = "admin")]
        public ActionResult MechsReport()
        {
            return View("Report", db.Usage_report.Where(x => x.table == "Mechanisms"));
        }
        [HttpGet]
        [Authorize(Roles = "admin")]
        public ActionResult ServicesReport()
        {
            return View("Report", db.Usage_report.Where(x => x.table == "Services"));
        }
        [HttpGet]
        [Authorize(Roles = "admin")]
        public ActionResult BossesReport()
        {
            return View("Report", db.Usage_report.Where(x => x.table == "Bosses"));
        }
        [HttpGet]
        [Authorize(Roles = "admin")]
        public ActionResult CertificatesReport()
        {
            return View("Report", db.Usage_report.Where(x => x.table == "Certificates"));
        }

        //контакты
        [HttpGet]
        [Authorize(Roles = "admin")]
        public ActionResult AllEmployees()//Страница сотрудников
        {
            var emp = db1.Employees.ToList();

            return View(emp);
        }

        [HttpGet]
        [Authorize(Roles = "admin")]
        public ActionResult EditEmployee(int? id)
        {
            if (id != null)
            {
                Employee emp = db1.Employees.Find(id);
                if (emp != null)
                {
                    var persons = db1.Persons.ToList();
                    var departments = db1.Departments.ToList();
                    var posts = db1.Posts.ToList();

                    ViewBag.Departments = db1.Departments.Select(d => new
                    {
                        id = d.Id,
                        text = d.Name + "   |   " + d.Company.Abbreviation
                    }).ToList();
                    ViewBag.Persons = persons;
                    ViewBag.Posts = posts;
                    return View(emp);
                }
            }
            return HttpNotFound();
        }

        [HttpPost]
        [Authorize(Roles = "admin")]
        public ActionResult EditEmployee(int? id, Person person, int DepartmentId, int PostId, List<string> ContactName)
        {
            string phone = ContactName[0].Replace(" ", "");
            string email = ContactName[1].Replace(" ", "");

            Employee emp = db1.Employees.Find(id);
            emp.Person.LastName = person.LastName;
            emp.Person.FirstName = person.FirstName;
            emp.Person.FatherName = person.FatherName;
            emp.DepartmentId = DepartmentId;
            emp.PostId = PostId;

            emp.Contacts.Remove(emp.Contacts.Where(x => x.ContactsTypeId == 2).FirstOrDefault());
            Contact ph = db1.Contacts.Where(x => x.ContactName == phone & x.ContactsTypeId == 2).FirstOrDefault();
            if (ph != null)
            {
                emp.Contacts.Add(ph);
            }
            else
            {
                Contact contactPhone = new Contact();
                contactPhone.ContactName = phone;
                contactPhone.ContactsTypeId = 2; //телефон
                emp.Contacts.Add(contactPhone);
            }
            //emp.Contacts.Where(x => x.ContactsTypeId == 2).FirstOrDefault().ContactName = phone;
            emp.Contacts.Where(x => x.ContactsTypeId == 1).FirstOrDefault().ContactName = email;
            db1.Entry(emp).State = EntityState.Modified;
            db1.SaveChanges();
            return RedirectToAction("AllEmployees");
        }

        [HttpGet]
        [Authorize(Roles = "admin")]
        public ActionResult AddEmployee()
        {
            ViewBag.Departments = db1.Departments.Select(d => new
            {
                id = d.Id,
                text = d.Name + "   |   " + d.Company.Abbreviation
            }).ToList();
            ViewBag.Posts = db1.Posts.ToList();

            return View();
        }

        [HttpPost]
        [Authorize(Roles = "admin")]
        public ActionResult AddEmployee(Person person, Employee emp, string phone, string email)
        {
            phone = phone.Replace(" ", "");
            email = email.Replace(" ", "");
            Contact ph = db1.Contacts.Where(x => x.ContactName == phone & x.ContactsTypeId == 2).FirstOrDefault();
            if (ph != null)
            {
                emp.Contacts.Add(ph);
            }
            else
            {
                Contact contactPhone = new Contact();
                contactPhone.ContactName = phone;
                contactPhone.ContactsTypeId = 2; //телефон
                emp.Contacts.Add(contactPhone);
            }
            Contact contactEmail = new Contact();
            contactEmail.ContactName = email;
            contactEmail.ContactsTypeId = 1; //Email
            emp.Contacts.Add(contactEmail);

            db1.Employees.Add(emp);
            db1.SaveChanges();

            return RedirectToAction("AllEmployees");
        }

        [HttpGet]
        [Authorize(Roles = "admin")]
        public ActionResult DeleteEmployee(int? id)
        {
            if (id != null)
            {
                Employee emp = db1.Employees.Find(id);
                Contact email = emp.Contacts.Where(x => x.ContactsTypeId == 1).FirstOrDefault();

                if (email != null)
                {
                    db1.Contacts.Remove(email);
                }
                db1.Employees.Remove(emp);
                db1.SaveChanges();
            }
            return RedirectToAction("AllEmployees");
        }

        [HttpGet]
        [Authorize(Roles = "product, admin")]
        public ActionResult Products()
        {
            Users user = db.Users.FirstOrDefault(x => x.login == User.Identity.Name);
            return View(db.Products.ToList());
        }

        [HttpGet]
        [Authorize(Roles = "product, admin")]
        public ActionResult CreateProduct()//Страница добавления для филиалов
        {
            Users user = db.Users.FirstOrDefault(x => x.login == User.Identity.Name);
            var viewmodel = new ProductViewModel();
            viewmodel.DepartmentId = user.id_dep;
            ViewBag.units = db.Units.ToList();
            ViewBag.groups = db.GroupProducts.ToList();
            ViewBag.property = db.Properties.ToList();
            return PartialView("_AddProduct", viewmodel);
        }

        [HttpPost]
        [Authorize(Roles = "product, admin")]
        public ActionResult CreateProduct(ProductViewModel model, int[] img_ids)
        {
            Users user = db.Users.FirstOrDefault(x => x.login == User.Identity.Name);
            var subGroupAdd = new SubGroupProduct();
            var group = db.GroupProducts.Where(x => x.name == model.groupProduct).FirstOrDefault();
            if (group == null)
            {
                group = new GroupProduct { name = model.groupProduct };
                db.GroupProducts.Add(group);
                var subgroup = new SubGroupProduct { name = model.subGroupProduct, GroupProduct = group };
                db.SubGroupProducts.Add(subgroup);
                subGroupAdd = subgroup;
            }
            else
            {
                SubGroupProduct subgroup = db.SubGroupProducts.Where(x => x.GroupProduct.name == group.name && x.name == model.subGroupProduct).FirstOrDefault();
                if (subgroup == null)
                {
                    subgroup = new SubGroupProduct { name = model.subGroupProduct, GroupProduct = group };
                    db.SubGroupProducts.Add(subgroup);
                }
                subGroupAdd = subgroup;
            }
            db.SaveChanges();
            Product product = new Product
            {
                name = model.name,
                codeTNVD = model.codeTNVD,
                note = model.note,
                UnitId = model.UnitId,
                SubGroupProductId = subGroupAdd.id,
                DepartmentId = model.DepartmentId
            };
            if (img_ids != null)
            {
                foreach (int id in img_ids)
                {
                    var x = new Imgs_to_product();
                    x.Product = product;
                    x.ImgsId = id;
                    db.ImgsProduct.Add(x);
                }
            }
            db.SaveChanges();
            if (model.properties.Count() > 0)
            {
                foreach (var property in model.properties)
                {
                    var PP = new PropertyProduct();
                    PP.ProductId = product.id;

                    var prop = db.Properties?.Where(x => x.name == property.name)?.FirstOrDefault();
                    if (prop == null)
                    {
                        var props = new Property();
                        props.name = property.name;
                        db.Properties.Add(props);
                        PP.Property = props;
                    }
                    else PP.Property = prop;

                    PP.count = property.count;
                    db.PropertyProducts.Add(PP);
                }
            }
            Usage_report rep = new Usage_report { title = product.name, action = "Add", table = "Product", date = DateTime.Now, id_user = user.id };
            db.Products.Add(product);
            db.Usage_report.Add(rep);
            try
            {
                db.SaveChanges();
                return RedirectToAction("Products");
            }
            catch (Exception e)
            {
                return RedirectToAction("Error", new { message = e.Message });
            }
        }

        [HttpGet]
        [Authorize(Roles = "product, admin")]
        public ActionResult ShowVersionProduct(int productId)
        {
            var modelView = new List<VersionProductViewModel>();
            var model = db.VersionProducts.Where(x => x.ProductId == productId).ToList();
            foreach (var item in model)
            {
                var itemView = new VersionProductViewModel();
                itemView.name = item.name;
                itemView.note = item.note;
                itemView.properties = new List<PropertyViewModel>();
                foreach (var prop in db.PropertyVersions.Where(x => x.VersionId == item.id))
                {
                    var property = db.Properties.Where(x => x.id == prop.PropertyId).FirstOrDefault();
                    itemView.properties.Add(new PropertyViewModel { name = property.name, count = prop.count });
                }
                modelView.Add(itemView);
            }
            var product = db.Products.Where(x => x.id == productId).FirstOrDefault();
            var imgs = new List<Imgs>();
            foreach (var imgsToProduct in db.ImgsProduct.Where(x => x.ProductId == product.id))
            {
                var img = db.Imgs.Where(x => x.id == imgsToProduct.ImgsId).FirstOrDefault();
                imgs.Add(img);
            }
            ViewBag.imgs = imgs;
            ViewData["productName"] = product.name;
            ViewData["codeTNVED"] = product.codeTNVD;
            ViewData["unitName"] = product.Unit.name;
            ViewData["departmentName"] = product.Departments.short_name_ru;
            ViewData["productId"] = product.id;
            var productProperties = new List<PropertyViewModel>();
            foreach (var prop in db.PropertyProducts.Where(x => x.ProductId == product.id))
            {
                var property = db.Properties.Where(x => x.id == prop.PropertyId).FirstOrDefault();
                productProperties.Add(new PropertyViewModel { name = property.name, count = prop.count });
            }
            ViewBag.properties = productProperties;
            return PartialView("_ShowVersionProduct", modelView);
        }

        [HttpGet]
        [Authorize(Roles = "product, admin")]
        public ActionResult ShowGroup()
        {
            var model = db.GroupProducts.ToList();
            return View("ShowGroup", model);
        }

        [HttpGet]
        [Authorize(Roles = "product, admin")]
        public ActionResult ShowSubGroup(int id)
        {
            var model = db.SubGroupProducts.Where(x => x.GroupProductId == id).ToList();
            ViewBag.ProductId = id;
            return PartialView("_ShowSubGroup", model);
        }

        [HttpGet]
        [Authorize(Roles = "product, admin")]
        public ActionResult ShowProduct()
        {
            Users user = db.Users.FirstOrDefault(x => x.login == User.Identity.Name);
            var model = db.Products.Where(x => x.DepartmentId == user.id_dep).ToList();
            return PartialView("_ShowProduct", model);
        }


        [HttpGet]
        [Authorize(Roles = "product, admin")]
        public ActionResult ShowSubGroupSelect(string name)
        {
            var model = db.SubGroupProducts.Where(x => x.GroupProduct.name == name).ToList();
            return PartialView("_ShowSubGroupSelect", model);
        }

        [HttpGet]
        [Authorize(Roles = "product, admin")]
        public ActionResult CreateVersionProduct(int productId)//Страница добавления для филиалов
        {
            Users user = db.Users.FirstOrDefault(x => x.login == User.Identity.Name);
            var model = new VersionProductViewModel();
            model.ProductId = productId;
            var nameProduct = db.Products.Where(x => x.id == productId).Select(x => x.name).FirstOrDefault();
            model.name = nameProduct;
            ViewBag.property = db.Properties.ToList();
            return PartialView("_AddVersionProduct", model);
        }

        [HttpPost]
        [Authorize(Roles = "product, admin")]
        public ActionResult CreateVersionProduct(VersionProductViewModel model, int[] img_ids, int productId)
        {
            Users user = db.Users.FirstOrDefault(x => x.login == User.Identity.Name);
            var versionProduct = new VersionProduct
            {
                name = model.name,
                isSale = model.isSale,
                note = model.note,
                ProductId = productId
            };
            if (img_ids != null)
            {
                foreach (int id in img_ids)
                {
                    var x = new Imgs_to_versionProduct();
                    x.VersionProduct = versionProduct;
                    x.ImgsId = id;
                    db.ImgsVersionProduct.Add(x);
                }
            }
            Usage_report rep = new Usage_report { title = versionProduct.name, action = "Add", table = "VersionProduct", date = DateTime.Now, id_user = user.id };
            db.VersionProducts.Add(versionProduct);
            db.Usage_report.Add(rep);
            if (model.properties.Count() > 0)
            {
                foreach (var property in model.properties)
                {
                    var PV = new PropertyVersion();
                    PV.VersionProduct = versionProduct;
                    var prop = db.Properties.Where(x => x.name == property.name).FirstOrDefault();
                    if (prop == null)
                    {
                        prop = new Property();
                        prop.name = property.name;
                        db.Properties.Add(prop);
                    }
                    PV.Property = prop;
                    PV.count = property.count;
                    db.PropertyVersions.Add(PV);
                }
            }
            try
            {
                db.SaveChanges();
                return RedirectToAction("Products");
            }
            catch (Exception e)
            {
                return RedirectToAction("Error", new { message = e.Message });
            }
        }

        [HttpGet]
        [Authorize(Roles = "product, admin")]
        public ActionResult CreateGroup()
        {                        
            return PartialView("_AddGroup");
        }

        [HttpPost]
        [Authorize(Roles = "product, admin")]
        public ActionResult CreateGroup(GroupProduct model)
        {
            Users user = db.Users.FirstOrDefault(x => x.login == User.Identity.Name);            
            Usage_report rep = new Usage_report { title = model.name, action = "Add", table = "GroupProduct", date = DateTime.Now, id_user = user.id };
            db.GroupProducts.Add(model);
            db.Usage_report.Add(rep);            
            try
            {
                db.SaveChanges();
                return RedirectToAction("ShowGroup");
            }
            catch (Exception e)
            {
                return RedirectToAction("Error", new { message = e.Message });
            }
        }

        [HttpGet]
        [Authorize(Roles = "product, admin")]
        public ActionResult CreateSubGroup(int id)
        {
            ViewBag.ProductId = id;
            return PartialView("_AddSubGroup");
        }

        [HttpPost]
        [Authorize(Roles = "product, admin")]
        public ActionResult CreateSubGroup(SubGroupProduct model)
        {
            Users user = db.Users.FirstOrDefault(x => x.login == User.Identity.Name);
            Usage_report rep = new Usage_report { title = model.name, action = "Add", table = "GroupProduct", date = DateTime.Now, id_user = user.id };
            db.SubGroupProducts.Add(model);
            db.Usage_report.Add(rep);
            try
            {
                db.SaveChanges();
                return RedirectToAction("ShowGroup");
            }
            catch (Exception e)
            {
                return RedirectToAction("Error", new { message = e.Message });
            }
        }
    }
}