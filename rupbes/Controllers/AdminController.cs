using AngleSharp.Css.Values;
using BotDetect.Web.Mvc;
using Newtonsoft.Json.Linq;
using rupbes.Classes;
using rupbes.Models;
using rupbes.Models.DatabaseBes;
using rupbes.Models.Products;
using rupbes.Models.ViewModels;
using rupbes.Providers;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection.PortableExecutable;
using System.Runtime.Remoting.Messaging;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Providers.Entities;
using System.Web.Security;
using System.Web.Services.Description;
using static System.Net.WebRequestMethods;

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
            return View();
        }
        [HttpGet]
        public ActionResult Logoff()//Выход из учетки
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult> ShowGetHashMD5()
        {
            return View("~/Views/Admin/Management/ShowGetHashMD5.cshtml");
        }

        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult> GetHashMD5(string pass)
        {
            using (MD5 md5Hash = MD5.Create())
            {
                string salt = "$#^@(as()@&";
                string pre = salt + pass + salt;
                string hash = HashHelper.GetMd5Hash(md5Hash, pre);
                return Json(new { success = true, message = hash });
            }
        }

        [HttpPost]
        //пока без аттрибутов
        public async Task<ActionResult> UploadAjax()//Загрузка картинок
        {
            //получаем путь куда сохранять из запроса
            string path = Request.Form["path"];
            string link = "https://rupbes.by"; // ссылка для доступа к файлам            
            //добавить запись о добавлении
            Users user = db.Users.FirstOrDefault(x => x.login == User.Identity.Name);
            Usage_report rep = new Usage_report { title = "Добавление картинки", action = "Add", table = "Imgs", date = DateTime.Now, id_user = user.id };
            List<Imgs> addedImgs = new List<Imgs>();

            //перебираем все загруженные файлы из запроса
            foreach (string fileKey in Request.Files)
            {
                HttpPostedFileBase file = Request.Files[fileKey];
                if (file != null && file.ContentLength > 0)
                {
                    string type = file.ContentType.Split('/')[0];

                    if (type == "image")
                    {
                        // получаем имя файла
                        Int32 unixTimestamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
                        string fileName = unixTimestamp.ToString() + ".jpg";
                        string path_fullSize = Server.MapPath("~/Content/Images/" + path + "/" + fileName);
                        string path_min = Server.MapPath("~/Content/Images/" + path + "/min/" + fileName);

                        // сохраняем уменьшенную копию
                        ResizeImgHelper.ResizeImage(file.InputStream, path_fullSize, 5000, 1640, ImageFormat.Jpeg);
                        ResizeImgHelper.ResizeImage(file.InputStream, path_min, 250, 250, ImageFormat.Jpeg);

                        // добавляем в базу информацию о путях к картинке и её уменьшенной копии

                        Imgs img = new Imgs
                        {
                            src = link + "/Content/Images/" + path + "/" + fileName,
                            src_min = link + "/Content/Images/" + path + "/min/" + fileName
                        };
                        img.type_id = await db.Img_types.Where(x => x.type.ToLower() == path.ToLower()).Select(x => x.id).FirstOrDefaultAsync();
                        if (img.type_id == 0)
                            throw new Exception("Тип новости не найден.");
                        db.Imgs.Add(img);
                        db.Usage_report.Add(rep);
                        await db.SaveChangesAsync();
                        addedImgs.Add(img);
                    }

                    else
                    {
                        string path_fullSize = Server.MapPath("~/Content/Files/News/" + file.FileName);
                        using (var fileStream = new FileStream(path_fullSize, FileMode.Create))
                        {
                            file.InputStream.Position = 0;
                            byte[] buffer;
                            using (var binaryReader = new BinaryReader(file.InputStream))
                            {
                                buffer = binaryReader.ReadBytes(file.ContentLength);
                            }
                            fileStream.Write(buffer, 0, buffer.Length);
                        }

                        Imgs img = new Imgs
                        {
                            src = link + "/Content/Files/News/" + file.FileName,
                            src_min = "document",
                            type_id = 1
                        };

                        db.Imgs.Add(img);
                        db.Usage_report.Add(new Usage_report { title = "Добавление файла", action = "Add", table = "Imgs", date = DateTime.Now, id_user = user.id });
                        await db.SaveChangesAsync();
                        addedImgs.Add(img);
                    }
                }
            }
            return PartialView("~/Views/Admin/_AddImage.cshtml", addedImgs);
        }

        //ВАКАНСИИ
        [HttpGet]
        [Authorize(Roles = "ok")]
        public async Task<ActionResult> Vacancies()//Страница с cо списком вакансий для филиала пользователя
        {
            var id_dep = await db.Users.Where(x => x.login == User.Identity.Name).Select(x => x.id_dep).FirstOrDefaultAsync();
            if (id_dep == 0)
            {
                return HttpNotFound("Пользователь не найден.");
            }

            ViewBag.IdDep = id_dep;
            ViewBag.nameDep = await db.Departments.Where(x => x.id == id_dep).Select(x => x.short_name_ru).FirstOrDefaultAsync();

            var vacancies = await db.Vacancies.Where(x => x.id_dep == id_dep).ToListAsync();
            return View("~/Views/Admin/Vacancies/Vacancies.cshtml", vacancies);
        }

        [HttpGet]
        [Authorize(Roles = "admin, ok_master")]
        public async Task<ActionResult> DepartmentsForVacancy()//Страница с выбором филиала
        {
            var departments = await db.Departments.Where(x => x.id < 21).ToListAsync();
            return View("~/Views/Admin/Vacancies/DepartmentsForVacancy.cshtml", departments);
        }

        [HttpGet]
        [Authorize(Roles = "admin, ok_master")]
        public async Task<ActionResult> VacanciesByDepartment(int id)//Страница со списоком вакансий для выбранного филиала
        {
            ViewBag.IdDep = id;
            ViewBag.nameDep = await db.Departments.Where(x => x.id == id).Select(x => x.short_name_ru).FirstOrDefaultAsync();
            var vacancies = await db.Vacancies.Where(x => x.id_dep == id).OrderBy(x => x.vacancy_ru).ToListAsync();
            return View("~/Views/Admin/Vacancies/VacanciesByDepartment.cshtml", vacancies);
        }

        [HttpGet]
        [Authorize(Roles = "admin, ok_master, ok")]
        public async Task<ActionResult> ShowVacancy(int id)//Частичное представление по выбранной вакансии
        {
            var Vacancy = await db.Vacancies.Where(x => x.id == id).FirstOrDefaultAsync();
            return PartialView("~/Views/Admin/Vacancies/_ShowVacancy.cshtml", Vacancy);
        }

        [HttpGet]
        [Authorize(Roles = "admin, ok_master, ok")]
        public async Task<ActionResult> ShowAddVacancy(int departmentId)//Страница добавления новой вакансии
        {
            return PartialView("~/Views/Admin/Vacancies/_ShowAddVacancy.cshtml", departmentId);
        }

        [HttpPost]
        [Authorize(Roles = "admin, ok_master, ok")]
        public async Task<ActionResult> AddVacancy(Vacancies vacancy)//Добавление новой вакансии
        {
            if (vacancy == null || string.IsNullOrWhiteSpace(vacancy.vacancy_ru))
            {
                return Json(new { success = false, message = "Некорректные данные." });
            }

            Users user = await db.Users.FirstOrDefaultAsync(x => x.login == User.Identity.Name);
            Usage_report rep = new Usage_report { title = vacancy.vacancy_ru, action = "Add", table = "Vacancies", date = DateTime.Now, id_user = user.id };

            db.Vacancies.Add(vacancy);
            db.Usage_report.Add(rep);

            await db.SaveChangesAsync();

            return Json(new { success = true, message = "Вакансия успешна добавлена!" });
        }

        [HttpPost]
        [Authorize(Roles = "admin, ok_master, ok")]
        public async Task<ActionResult> EditVacancy(Vacancies vacancyIn)//Редактирование выбранной вакансии
        {
            if (vacancyIn == null || string.IsNullOrWhiteSpace(vacancyIn.vacancy_ru))
            {
                return Json(new { success = false, message = "Некорректные данные." });
            }

            Users user = await db.Users.FirstOrDefaultAsync(x => x.login == User.Identity.Name);
            Usage_report rep = new Usage_report { title = vacancyIn.vacancy_ru, action = "Edit", table = "Vacancies", date = DateTime.Now, id_user = user.id };
            var vacancy = await db.Vacancies.Where(x => x.id == vacancyIn.id).FirstOrDefaultAsync();

            #region Присваивание измененных значений
            if (vacancy != null)
            {
                if (vacancy.vacancy_ru != vacancyIn.vacancy_ru)
                {
                    vacancy.vacancy_ru = vacancyIn.vacancy_ru;
                }

                if (vacancy.vacancy_bel != vacancyIn.vacancy_bel)
                {
                    vacancy.vacancy_bel = vacancyIn.vacancy_bel;
                }

                if (vacancy.vacancy_bel != vacancyIn.vacancy_bel)
                {
                    vacancy.vacancy_bel = vacancyIn.vacancy_bel;
                }

                if (vacancy.requirement_ru != vacancyIn.requirement_ru)
                {
                    vacancy.requirement_ru = vacancyIn.requirement_ru;
                }

                if (vacancy.requirement_bel != vacancyIn.requirement_bel)
                {
                    vacancy.requirement_bel = vacancyIn.requirement_bel;
                }

                if (vacancy.payment != vacancyIn.payment)
                {
                    vacancy.payment = vacancyIn.payment;
                }

                if (vacancy.link != vacancyIn.link)
                {
                    vacancy.link = vacancyIn.link;
                }
            }
            else
            {
                return Json(new { success = false, message = "Вакансия не найдена." });
            }
            #endregion

            db.Usage_report.Add(rep);
            await db.SaveChangesAsync();
            return Json(new { success = true, message = "Вакансия успешно изменена!" });
        }

        [HttpPost]
        [Authorize(Roles = "admin, ok_master, ok")]
        public async Task<ActionResult> DeleteVacancy(int id)//Удаление выбранной вакансии
        {

            Users user = await db.Users.FirstOrDefaultAsync(x => x.login == User.Identity.Name);
            Vacancies vacancy = await db.Vacancies.FindAsync(id); // Используем асинхронный метод

            if (vacancy == null)
            {
                return Json(new { success = false, message = "Вакансия не найдена." });
            }

            Usage_report rep = new Usage_report { title = vacancy.vacancy_ru, action = "Delete", table = "Vacancies", date = DateTime.Now, id_user = user.id };

            db.Vacancies.Remove(vacancy);
            db.Usage_report.Add(rep);
            await db.SaveChangesAsync();
            return Json(new { success = true, message = "Вакансия удалена!" });
        }

        //НОВОСТИ
        [HttpGet]
        [Authorize(Roles = "admin, news")]
        public async Task<ActionResult> NewsCategory()//Главная страница меню новостей
        {
            var newsType = await db.News_type.ToListAsync();
            return View("~/Views/Admin/News/NewsCategory.cshtml", newsType);
        }

        [HttpPost]
        public async Task<ActionResult> ShowNewsByCategory(int id, int page, int count = 20)
        {
            if (page < 1)
            {
                page = 1; // Убедитесь, что номер страницы не меньше 1
            }

            Users user = await db.Users.FirstOrDefaultAsync(x => x.login == User.Identity.Name);
            if (user == null)
            {
                throw new Exception("Пользователь не найден");
            }
            var listNews = new List<News>();
            var totalItems = 0;
            if (User.IsInRole("admin"))
            {
                listNews = await db.News
                        .Where(n => n.type_id == id /*&& n.id_dep == user.id_dep*/) // Фильтрация по type_id
                        .OrderByDescending(n => n.date) // Сортировка по дате
                        .Skip((page - 1) * count) // Пропустить элементы для предыдущих страниц
                        .Take(count) // Взять только count элементов для текущей страницы
                        .ToListAsync(); // Преобразовать в список

                totalItems = await db.News
                            .Where(n => n.type_id == id /*&& n.id_dep == user.id_dep*/)
                            .CountAsync(); // Общее количество элементов
            }
            else
            {
                listNews = await db.News
                      .Where(n => n.type_id == id && n.id_dep == user.id_dep) // Фильтрация по type_id
                      .OrderByDescending(n => n.date) // Сортировка по дате
                      .Skip((page - 1) * count) // Пропустить элементы для предыдущих страниц
                      .Take(count) // Взять только count элементов для текущей страницы
                      .ToListAsync(); // Преобразовать в список

                totalItems = await db.News
                            .Where(n => n.type_id == id && n.id_dep == user.id_dep)
                            .CountAsync(); // Общее количество элементов
            }
            var totalPages = (int)Math.Ceiling((double)totalItems / count); // Общее количество страниц
            if (totalPages < page)
                page = totalPages;

            ViewBag.totalPages = totalPages;
            ViewBag.activePage = page;
            ViewBag.nameNews = await db.News_type.Where(x => x.id == id).Select(x => x.type).FirstOrDefaultAsync();

            return PartialView("~/Views/Admin/News/_NewsByCategory.cshtml", listNews);
        }


        [HttpPost]
        [Authorize(Roles = "admin,news")]
        public async Task<ActionResult> ShowNewsById(int id)//Страница показания новости
        {
            ViewBag.newsType = await db.News_type.ToListAsync();

            // Получаем новость
            News news = await db.News.FindAsync(id);
            if (news == null)
            {
                throw new Exception("Новость не найдена.");
            }

            return PartialView("~/Views/Admin/News/_ShowNewsById.cshtml", news);
        }

        [HttpPost]
        [Authorize(Roles = "admin, news")]
        public async Task<ActionResult> EditNews(News model, int[] img_ids)//Изменение существующей новости
        {
            // Получаем пользователя
            Users user = await db.Users.FirstOrDefaultAsync(x => x.login == User.Identity.Name);
            if (user == null)
            {
                return Json(new { success = false, message = "Пользователь не найден." });
            }

            // Получаем новость
            News news = await db.News.Include(n => n.Imgs).FirstOrDefaultAsync(x => x.id == model.id);
            if (news == null)
            {
                return Json(new { success = false, message = "Новость не найдена." });
            }

            #region Присваивание измененных значений
            if (news.title_ru != model.title_ru)
            {
                news.title_ru = model.title_ru;
            }

            if (news.title_bel != model.title_bel)
            {
                news.title_bel = model.title_bel;
            }

            if (news.body_ru != model.body_ru)
            {
                news.body_ru = model.body_ru;
            }

            if (news.body_bel != model.body_bel)
            {
                news.body_bel = model.body_bel;
            }

            if (news.type_id != model.type_id)
            {
                news.type_id = model.type_id;
            }

            // Обработка изображений
            if (img_ids != null)
            {
                var currentImgIds = news.Imgs.Select(i => i.id).ToList();

                // Удаляем изображения, которые не включены в img_ids
                foreach (var imgId in currentImgIds)
                {
                    if (!img_ids.Contains(imgId))
                    {
                        var imgToRemove = news.Imgs.FirstOrDefault(i => i.id == imgId);
                        if (imgToRemove != null)
                        {
                            news.Imgs.Remove(imgToRemove);
                        }
                    }
                }

                // Добавляем новые изображения
                var ImgList = await db.Imgs.Where(x => img_ids.Contains(x.id)).ToListAsync();
                foreach (Imgs img in ImgList)
                {
                    if (!news.Imgs.Any(i => i.id == img.id)) // Проверка на дублирование
                    {
                        news.Imgs.Add(img);
                    }
                }

            }
            else if (news.Imgs.Count() > 0)
            {
                news.Imgs.Clear();
            }
            #endregion

            Usage_report rep = new Usage_report { title = news.title_ru, action = "Edit", table = "News", date = DateTime.Now, id_user = user.id };
            db.Usage_report.Add(rep);
            await db.SaveChangesAsync();
            return Json(new { success = true, message = "Новость изменена!" });
        }

        [HttpPost]
        [Authorize(Roles = "admin, news")]
        public async Task<ActionResult> DeleteNews(int id)//Удаление новости
        {
            // Получаем пользователя
            Users user = await db.Users.FirstOrDefaultAsync(x => x.login == User.Identity.Name);
            if (user == null)
            {
                return Json(new { success = false, message = "Пользователь не найден." });
            }

            // Получаем новость
            News news = await db.News.FirstOrDefaultAsync(x => x.id == id);
            if (news == null)
            {
                return Json(new { success = false, message = "Новость не найдена." });
            }

            Usage_report rep = new Usage_report { title = news.title_ru, action = "Delete", table = "News", date = DateTime.Now, id_user = user.id };
            db.Usage_report.Add(rep);

            // Удаляем новость
            db.News.Remove(news);
            await db.SaveChangesAsync();

            return Json(new { success = true, message = "Новость удалена!" });
        }

        [HttpGet]
        [Authorize(Roles = "admin,news")]
        public async Task<ActionResult> AddNews()//Страница добавления новой новости
        {
            ViewBag.id_dep = await db.Users.Where(x => x.login == User.Identity.Name).Select(x => x.id_dep).FirstOrDefaultAsync();
            ViewBag.departments = await db.Departments.Where(x => x.id < 21).ToListAsync();
            var newsType = await db.News_type.ToListAsync();
            return View("~/Views/Admin/News/AddNews.cshtml", newsType);
        }

        [HttpPost]
        [Authorize(Roles = "admin,news")]
        public async Task<ActionResult> AddNews(News news, int[] img_ids)//Добавление новой новости
        {
            // Получаем пользователя
            Users user = await db.Users.FirstOrDefaultAsync(x => x.login == User.Identity.Name);
            if (user == null)
            {
                return Json(new { success = false, message = "Пользователь не найден." });
            }

            Usage_report rep = new Usage_report { title = news.title_ru, action = "Add", table = "News", date = DateTime.Now, id_user = user.id };
            db.Usage_report.Add(rep);

            news.date = DateTime.Now;

            if (img_ids != null)
            {
                Imgs img = new Imgs();
                foreach (var id in img_ids)
                {
                    img = await db.Imgs.Where(x => x.id == id).FirstOrDefaultAsync();
                    news.Imgs.Add(img);
                }
            }

            db.News.Add(news);
            await db.SaveChangesAsync();

            return Json(new { success = true, message = "Новость успешна добавлена!" });
        }

        //ОБЪЕКТЫ
        [Authorize(Roles = "admin, obj")]
        [HttpGet]
        public async Task<ActionResult> Objects()//Главная страница меню новостей
        {
            var objects = await db.Objects.OrderByDescending(x => x.id).ToListAsync();
            return View("~/Views/Admin/Objects/Objects.cshtml", objects);
        }

        [HttpPost]
        public async Task<ActionResult> ShowObjectsByPage(int page = 1, int count = 20)
        {
            if (page < 1)
            {
                page = 1; // Убедитесь, что номер страницы не меньше 1
            }

            var listObjects = await db.Objects
                    .OrderByDescending(n => n.id) // Сортировка по дате
                    .Skip((page - 1) * count) // Пропустить элементы для предыдущих страниц
                    .Take(count) // Взять только count элементов для текущей страницы
                    .ToListAsync(); // Преобразовать в список

            var totalItems = await db.Objects
                        .CountAsync(); // Общее количество элементов

            var totalPages = (int)Math.Ceiling((double)totalItems / count); // Общее количество страниц
            if (totalPages < page)
                page = totalPages;

            ViewBag.totalPages = totalPages;
            ViewBag.activePage = page;

            return PartialView("~/Views/Admin/Objects/_Objects.cshtml", listObjects);
        }

        [HttpGet]
        [Authorize(Roles = "admin, obj")]
        public async Task<ActionResult> ShowObject(int id)//Частичное представление по выбранному объекту
        {
            ViewBag.Departments = await db.Departments.Where(x => x.id < 42).ToListAsync();
            var objectView = await db.Objects.Where(x => x.id == id).FirstOrDefaultAsync();
            return PartialView("~/Views/Admin/Objects/_ShowObject.cshtml", objectView);
        }

        [HttpGet]
        [Authorize(Roles = "admin, obj")]
        public async Task<ActionResult> AddObject()//Частичное представление по выбранному объекту
        {
            ViewBag.Departments = await db.Departments.Where(x => x.id < 42).ToListAsync();
            return View("~/Views/Admin/Objects/AddObject.cshtml");
        }

        [HttpPost]
        [Authorize(Roles = "admin, obj")]
        public async Task<ActionResult> AddObject(Objects objectModel, int[] dep_ids = null, int[] img_ids = null)//Добавление нового объекта
        {
            // Получаем пользователя
            Users user = await db.Users.FirstOrDefaultAsync(x => x.login == User.Identity.Name);
            if (user == null)
            {
                return Json(new { success = false, message = "Пользователь не найден." });
            }

            Usage_report rep = new Usage_report { title = objectModel.title_ru, action = "Add", table = "Objects", date = DateTime.Now, id_user = user.id };
            db.Usage_report.Add(rep);

            Departments department = new Departments();
            foreach (var id in dep_ids)
            {
                department = await db.Departments.Where(x => x.id == id).FirstOrDefaultAsync();
                objectModel.Departments.Add(department);
            }

            if (img_ids != null)
            {
                Imgs img = new Imgs();
                foreach (var id in img_ids)
                {
                    img = await db.Imgs.Where(x => x.id == id).FirstOrDefaultAsync();
                    objectModel.Imgs.Add(img);
                }
            }

            db.Objects.Add(objectModel);
            await db.SaveChangesAsync();

            return Json(new { success = true, message = "Объект успешно добавлен!" });
        }

        [HttpPost]
        [Authorize(Roles = "admin, obj")]
        public async Task<ActionResult> EditObject(Objects objectModel, int[] dep_ids = null, int[] img_ids = null)//Редактирование нового объекта
        {
            // Получаем пользователя
            Users user = await db.Users.FirstOrDefaultAsync(x => x.login == User.Identity.Name);
            if (user == null)
            {
                return Json(new { success = false, message = "Пользователь не найден." });
            }

            // Получаем объект
            Objects objects = await db.Objects.Include(o => o.Imgs).Include(o => o.Departments).FirstOrDefaultAsync(x => x.id == objectModel.id);
            if (objects == null)
            {
                return Json(new { success = false, message = "Объект не найден." });
            }

            #region Присваивание измененных значений
            if (objects.title_ru != objectModel.title_ru)
            {
                objects.title_ru = objectModel.title_ru;
            }

            if (objects.title_bel != objectModel.title_bel)
            {
                objects.title_bel = objectModel.title_bel;
            }

            if (objects.title_eng != objectModel.title_eng)
            {
                objects.title_eng = objectModel.title_eng;
            }

            if (objects.desc_ru != objectModel.desc_ru)
            {
                objects.desc_ru = objectModel.desc_ru;
            }

            if (objects.desc_bel != objectModel.desc_bel)
            {
                objects.desc_bel = objectModel.desc_bel;
            }

            if (objects.desc_eng != objectModel.desc_eng)
            {
                objects.desc_eng = objectModel.desc_eng;
            }

            // Обработка изображений
            if (img_ids != null)
            {
                var currentImgIds = objects.Imgs.Select(i => i.id).ToList();

                // Удаляем изображения, которые не включены в img_ids
                foreach (var imgId in currentImgIds)
                {
                    if (!img_ids.Contains(imgId))
                    {
                        var imgToRemove = objects.Imgs.FirstOrDefault(i => i.id == imgId);
                        if (imgToRemove != null)
                        {
                            objects.Imgs.Remove(imgToRemove);
                        }
                    }

                    // Добавляем новые изображения
                    var ImgList = await db.Imgs.Where(x => img_ids.Contains(x.id)).ToListAsync();
                    foreach (var img in ImgList)
                    {
                        if (!objects.Imgs.Any(i => i.id == img.id)) // Проверка на дублирование
                        {
                            objects.Imgs.Add(img);
                        }
                    }
                }
            }

            // Обработка организаций
            if (dep_ids != null)
            {
                var currentDepIds = objects.Departments.Select(i => i.id).ToList();

                // Удаляем организации, которые не включены в img_ids
                foreach (var depId in currentDepIds)
                {
                    if (!dep_ids.Contains(depId))
                    {
                        var depToRemove = objects.Departments.FirstOrDefault(i => i.id == depId);
                        if (depToRemove != null)
                        {
                            objects.Departments.Remove(depToRemove);
                        }
                    }

                    // Добавляем новые изображения
                    var DepList = await db.Departments.Where(x => dep_ids.Contains(x.id)).ToListAsync();
                    foreach (var dep in DepList)
                    {
                        if (!objects.Departments.Any(i => i.id == dep.id)) // Проверка на дублирование
                        {
                            objects.Departments.Add(dep);
                        }
                    }
                }
            }
            #endregion

            Usage_report rep = new Usage_report { title = objectModel.title_ru, action = "Add", table = "Objects", date = DateTime.Now, id_user = user.id };
            db.Usage_report.Add(rep);

            await db.SaveChangesAsync();

            return Json(new { success = true, message = "Изменения по объекту успешно сохранены!" });
        }

        [HttpPost]
        [Authorize(Roles = "admin, obj")]
        public async Task<ActionResult> DeleteObject(int id)//Удаление новости
        {
            // Получаем пользователя
            Users user = await db.Users.FirstOrDefaultAsync(x => x.login == User.Identity.Name);
            if (user == null)
            {
                return Json(new { success = false, message = "Пользователь не найден." });
            }

            // Получаем новость
            Objects objects = await db.Objects.FirstOrDefaultAsync(x => x.id == id);
            if (objects == null)
            {
                return Json(new { success = false, message = "Объект не найден." });
            }

            Usage_report rep = new Usage_report { title = objects.title_ru, action = "Delete", table = "Objects", date = DateTime.Now, id_user = user.id };
            db.Usage_report.Add(rep);

            // Удаляем новость
            db.Objects.Remove(objects);
            await db.SaveChangesAsync();

            return Json(new { success = true, message = "Объект удален!" });
        }

        //Сертификаты
        [HttpGet]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult> Certificates()//Главная страница меню сертификатов
        {
            var сertificates = await db.Certificates.OrderByDescending(x => x.id).ToListAsync();
            return View("~/Views/Admin/Certificates/Certificates.cshtml", сertificates);
        }

        [HttpPost]
        public async Task<ActionResult> ShowCertificatesByPage(int page = 1, int count = 20)
        {
            if (page < 1)
            {
                page = 1; // Убедитесь, что номер страницы не меньше 1
            }

            var listCertificates = await db.Certificates
                    .OrderByDescending(n => n.id) // Сортировка по дате
                    .Skip((page - 1) * count) // Пропустить элементы для предыдущих страниц
                    .Take(count) // Взять только count элементов для текущей страницы
                    .ToListAsync(); // Преобразовать в список

            var totalItems = await db.Certificates
                        .CountAsync(); // Общее количество элементов

            var totalPages = (int)Math.Ceiling((double)totalItems / count); // Общее количество страниц
            if (totalPages < page)
                page = totalPages;

            ViewBag.totalPages = totalPages;
            ViewBag.activePage = page;

            return PartialView("~/Views/Admin/Certificates/_Certificates.cshtml", listCertificates);
        }


        [HttpGet]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult> ShowCertificate(int id)//Частичное представление по выбранному сертификату
        {
            ViewBag.Departments = await db.Departments.Where(x => x.id < 42).ToListAsync();
            var certificateView = await db.Certificates.Where(x => x.id == id).FirstOrDefaultAsync();
            return PartialView("~/Views/Admin/Certificates/_ShowCertificate.cshtml", certificateView);
        }


        [HttpGet]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult> AddCertificate()//Страница добавления новго сертификата
        {
            ViewBag.Departments = await db.Departments.Where(x => x.id < 42).ToListAsync();
            return View("~/Views/Admin/Certificates/AddCertificate.cshtml");
        }
        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult> AddCertificate(Certificates certificateModel, int[] img_ids)//Добавление новго сертификата
        {
            // Получаем пользователя
            Users user = await db.Users.FirstOrDefaultAsync(x => x.login == User.Identity.Name);
            if (user == null)
            {
                return Json(new { success = false, message = "Пользователь не найден." });
            }

            if (certificateModel.id_dep == null || certificateModel.id_dep == 0)
            {
                return Json(new { success = false, message = "Не выбрана организация." });
            }

            Usage_report rep = new Usage_report { title = certificateModel.name, action = "Add", table = "Certificates", date = DateTime.Now, id_user = user.id };
            db.Usage_report.Add(rep);

            if (img_ids != null)
            {
                Imgs img = new Imgs();
                foreach (var id in img_ids)
                {
                    img = await db.Imgs.Where(x => x.id == id).FirstOrDefaultAsync();
                    certificateModel.Imgs.Add(img);
                }
            }

            db.Certificates.Add(certificateModel);

            await db.SaveChangesAsync();
            return Json(new { success = true, message = "Сертификат успешно добавлен!" });
        }

        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult> EditCertificate(Certificates certificateModel, int[] img_ids = null)//Редактирование объекта
        {
            // Получаем пользователя
            Users user = await db.Users.FirstOrDefaultAsync(x => x.login == User.Identity.Name);
            if (user == null)
            {
                return Json(new { success = false, message = "Пользователь не найден." });
            }

            if (certificateModel.id_dep == null || certificateModel.id_dep == 0)
            {
                return Json(new { success = false, message = "Не выбрана организация." });
            }

            // Получаем сертификат
            Certificates certificates = await db.Certificates.Include(o => o.Imgs).FirstOrDefaultAsync(x => x.id == certificateModel.id);
            if (certificates == null)
            {
                return Json(new { success = false, message = "Сертификат не найден." });
            }

            #region Присваивание измененных значений
            if (certificates.name != certificateModel.name)
            {
                certificates.name = certificateModel.name;
            }

            if (certificates.name_bel != certificateModel.name_bel)
            {
                certificates.name_bel = certificateModel.name_bel;
            }

            if (certificates.name_eng != certificateModel.name_eng)
            {
                certificates.name_eng = certificateModel.name_eng;
            }
            if (certificates.id_dep != certificateModel.id_dep)
            {
                certificates.id_dep = certificateModel.id_dep;
            }

            // Обработка изображений
            if (img_ids != null)
            {
                var currentImgIds = certificates.Imgs.Select(i => i.id).ToList();

                // Удаляем изображения, которые не включены в img_ids
                foreach (var imgId in currentImgIds)
                {
                    if (!img_ids.Contains(imgId))
                    {
                        var imgToRemove = certificates.Imgs.FirstOrDefault(i => i.id == imgId);
                        if (imgToRemove != null)
                        {
                            certificates.Imgs.Remove(imgToRemove);
                        }
                    }

                    // Добавляем новые изображения
                    var ImgList = await db.Imgs.Where(x => img_ids.Contains(x.id)).ToListAsync();
                    foreach (var img in ImgList)
                    {
                        if (!certificates.Imgs.Any(i => i.id == img.id)) // Проверка на дублирование
                        {
                            certificates.Imgs.Add(img);
                        }
                    }
                }
            }
            #endregion

            Usage_report rep = new Usage_report { title = certificateModel.name, action = "Edit", table = "Certificates", date = DateTime.Now, id_user = user.id };
            db.Usage_report.Add(rep);

            await db.SaveChangesAsync();

            return Json(new { success = true, message = "Изменения по сертификату успешно сохранены!" });
        }

        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult> DeleteCertificate(int id)//Удаление новости
        {
            // Получаем пользователя
            Users user = await db.Users.FirstOrDefaultAsync(x => x.login == User.Identity.Name);
            if (user == null)
            {
                return Json(new { success = false, message = "Пользователь не найден." });
            }

            // Получаем новость
            Certificates certificates = await db.Certificates.FirstOrDefaultAsync(x => x.id == id);
            if (certificates == null)
            {
                return Json(new { success = false, message = "Сертификат не найден." });
            }

            Usage_report rep = new Usage_report { title = certificates.name, action = "Delete", table = "Certificates", date = DateTime.Now, id_user = user.id };
            db.Usage_report.Add(rep);

            // Удаляем новость
            db.Certificates.Remove(certificates);
            await db.SaveChangesAsync();

            return Json(new { success = true, message = "Сертификат удален!" });
        }

        //Руководители
        [HttpGet]
        [Authorize(Roles = "admin, ok_master")]
        public async Task<ActionResult> DepartmentsForBoss()//Страница с выбором филиала
        {
            var departments = await db.Departments.Where(x => x.id < 21).ToListAsync();
            return View("~/Views/Admin/Bosses/DepartmentsForBoss.cshtml", departments);
        }

        [HttpGet]
        [Authorize(Roles = "ok")]
        public async Task<ActionResult> Bosses()//Главная страница меню боссов
        {
            var id_dep = await db.Users.Where(x => x.login == User.Identity.Name).Select(x => x.id_dep).FirstOrDefaultAsync();
            if (id_dep == 0)
            {
                return HttpNotFound("Пользователь не найден.");
            }

            ViewBag.IdDep = id_dep;
            ViewBag.nameDep = await db.Departments.Where(x => x.id == id_dep).Select(x => x.short_name_ru).FirstOrDefaultAsync();

            var bosses = await db.Bosses.Where(x => x.id_dep == id_dep).OrderBy(x => x.name).ToListAsync();
            return View("~/Views/Admin/Bosses/Bosses.cshtml", bosses);
        }

        [HttpGet]
        [Authorize(Roles = "admin, ok_master")]
        public async Task<ActionResult> BossesByDepartment(int id)//Страница со списоком вакансий для выбранного филиала
        {
            ViewBag.IdDep = id;
            ViewBag.nameDep = await db.Departments.Where(x => x.id == id).Select(x => x.short_name_ru).FirstOrDefaultAsync();
            var bosses = await db.Bosses.Where(x => x.id_dep == id).OrderBy(x => x.name).ToListAsync();
            return View("~/Views/Admin/Bosses/BossesByDepartment.cshtml", bosses);
        }

        [HttpGet]
        [Authorize(Roles = "admin, ok_master, ok")]
        public async Task<ActionResult> ShowBoss(int id)//Показать выбранного руководителя
        {
            var bossView = await db.Bosses.Where(x => x.id == id).FirstOrDefaultAsync();
            return PartialView("~/Views/Admin/Bosses/_ShowBoss.cshtml", bossView);
        }

        [HttpGet]
        [Authorize(Roles = "admin, ok_master, ok")]
        public async Task<ActionResult> ShowAddBoss(int id)//Страница добавления нового босса
        {
            ViewBag.Id_dep = id;
            return PartialView("~/Views/Admin/Bosses/_ShowAddBoss.cshtml");
        }

        [HttpPost]
        [Authorize(Roles = "admin, ok_master, ok")]
        public async Task<ActionResult> AddBoss(Bosses bossModel, int img_ids = 0)//Добавление нового босса
        {
            // Получаем пользователя
            Users user = await db.Users.FirstOrDefaultAsync(x => x.login == User.Identity.Name);
            if (user == null)
            {
                return Json(new { success = false, message = "Пользователь не найден." });
            }

            if (img_ids == 0)
            {
                return Json(new { success = false, message = "Выберите фотографию." });
            }

            Usage_report rep = new Usage_report { title = bossModel.name, action = "Add", table = "Bosses", date = DateTime.Now, id_user = user.id };
            db.Usage_report.Add(rep);

            bossModel.id_img = img_ids;

            db.Bosses.Add(bossModel);

            await db.SaveChangesAsync();
            return Json(new { success = true, message = "Руководитель успешно добавлен!" });
        }

        [HttpPost]
        [Authorize(Roles = "admin, ok_master, ok")]
        public async Task<ActionResult> EditBoss(Bosses bossModel, int img_ids = 0)//Изменение босса
        {
            // Получаем пользователя
            Users user = await db.Users.FirstOrDefaultAsync(x => x.login == User.Identity.Name);
            if (user == null)
            {
                return Json(new { success = false, message = "Пользователь не найден." });
            }

            if (img_ids == 0)
            {
                return Json(new { success = false, message = "Выберите фотографию." });
            }

            // Получаем объект
            Bosses bosses = await db.Bosses.Include(o => o.Imgs).FirstOrDefaultAsync(x => x.id == bossModel.id);
            if (bosses == null)
            {
                return Json(new { success = false, message = "Руководитель не найден." });
            }

            #region Присваивание измененных значений
            bossModel.id_img = img_ids;
            PropertyUpdater.UpdateProperties(bosses, bossModel);
            #endregion

            Usage_report rep = new Usage_report { title = bossModel.name, action = "Add", table = "Bosses", date = DateTime.Now, id_user = user.id };
            db.Usage_report.Add(rep);

            await db.SaveChangesAsync();

            return Json(new { success = true, message = "Информация о руководителе успешна изменена!" });
        }

        [HttpPost]
        [Authorize(Roles = "admin, ok_master, ok")]
        public async Task<ActionResult> DeleteBoss(int id)//Удаление босса
        {
            // Получаем пользователя
            Users user = await db.Users.FirstOrDefaultAsync(x => x.login == User.Identity.Name);
            if (user == null)
            {
                return Json(new { success = false, message = "Пользователь не найден." });
            }

            // Получаем руководителя
            Bosses bosses = await db.Bosses.FirstOrDefaultAsync(x => x.id == id);
            if (bosses == null)
            {
                return Json(new { success = false, message = "Руководитель не найден." });
            }

            Usage_report rep = new Usage_report { title = bosses.name, action = "Delete", table = "Bosses", date = DateTime.Now, id_user = user.id };
            db.Usage_report.Add(rep);

            // Удаляем новость
            db.Bosses.Remove(bosses);
            await db.SaveChangesAsync();

            return Json(new { success = true, message = "Руководитель удален!" });
        }

        //Аренда
        [HttpGet]
        [Authorize(Roles = "admin, realty_master")]
        public async Task<ActionResult> DepartmentsForRealty()//Меню выбора филиала
        {
            var departments = await db.Departments.Where(x => x.id < 21).ToListAsync();
            return View("~/Views/Admin/Realty/DepartmentsForRealty.cshtml", departments);
        }

        [HttpGet]
        [Authorize(Roles = "admin, realty_master")]
        public async Task<ActionResult> RealtyByDepartment(int id)//Аренда выбранного филиала
        {
            ViewBag.IdDep = id;
            ViewBag.nameDep = await db.Departments.Where(x => x.id == id).Select(x => x.short_name_ru).FirstOrDefaultAsync();
            var realty = await db.Realty.Where(x => x.id_dep == id).ToListAsync();
            return View("~/Views/Admin/Realty/RealtyByDepartment.cshtml", realty);
        }

        [HttpGet]
        [Authorize(Roles = "realty")]
        public async Task<ActionResult> Realty()//Аренда филиала
        {
            var id_dep = await db.Users.Where(x => x.login == User.Identity.Name).Select(x => x.id_dep).FirstOrDefaultAsync();
            if (id_dep == 0)
            {
                return HttpNotFound("Пользователь не найден.");
            }

            ViewBag.IdDep = id_dep;
            ViewBag.nameDep = await db.Departments.Where(x => x.id == id_dep).Select(x => x.short_name_ru).FirstOrDefaultAsync();

            var realty = await db.Realty.Where(x => x.id_dep == id_dep).ToListAsync();
            return View("~/Views/Admin/Realty/RealtyForDepatment.cshtml", realty);
        }

        [HttpGet]
        [Authorize(Roles = "admin, realty_master, realty")]
        public async Task<ActionResult> ShowRealty(int id)//Показать выбранного руководителя
        {
            var realtyView = await db.Realty.Where(x => x.id == id).Include(x => x.Imgs).FirstOrDefaultAsync();
            return PartialView("~/Views/Admin/Realty/_ShowRealty.cshtml", realtyView);
        }

        [HttpGet]
        [Authorize(Roles = "admin, realty_master, realty")]
        public async Task<ActionResult> ShowAddRealty(int id)//Страница добавления нового босса
        {
            ViewBag.Id_dep = id;
            return PartialView("~/Views/Admin/Realty/_ShowAddRealty.cshtml");
        }

        [HttpPost]
        [Authorize(Roles = "admin, realty_master, realty")]
        public async Task<ActionResult> AddRealty(Realty realtyModel, int[] img_ids)//Добавление новой аренды
        {
            // Получаем пользователя
            Users user = await db.Users.FirstOrDefaultAsync(x => x.login == User.Identity.Name);
            if (user == null)
            {
                return Json(new { success = false, message = "Пользователь не найден." });
            }

            Usage_report rep = new Usage_report { title = realtyModel.title, action = "Add", table = "Realty", date = DateTime.Now, id_user = user.id };
            db.Usage_report.Add(rep);

            if (img_ids != null)
            {
                Imgs img = new Imgs();
                foreach (var id in img_ids)
                {
                    img = await db.Imgs.Where(x => x.id == id).FirstOrDefaultAsync();
                    realtyModel.Imgs.Add(img);
                }
            }

            db.Realty.Add(realtyModel);

            await db.SaveChangesAsync();
            return Json(new { success = true, message = "Аренда успешна добавлена!" });
        }

        [HttpPost]
        [Authorize(Roles = "admin, realty_master, realty")]
        public async Task<ActionResult> EditRealty(Realty realtyModel, int[] img_ids)//Изменение босса
        {
            // Получаем пользователя
            Users user = await db.Users.FirstOrDefaultAsync(x => x.login == User.Identity.Name);
            if (user == null)
            {
                return Json(new { success = false, message = "Пользователь не найден." });
            }

            // Получаем аренду
            Realty realty = await db.Realty.Include(o => o.Imgs).FirstOrDefaultAsync(x => x.id == realtyModel.id);
            if (realty == null)
            {
                return Json(new { success = false, message = "Объект аренды не найден." });
            }

            #region Присваивание измененных значений            
            PropertyUpdater.UpdateProperties(realty, realtyModel);
            // Обработка изображений
            if (img_ids != null)
            {
                var currentImgIds = realty.Imgs.Select(i => i.id).ToList();

                // Удаляем изображения, которые не включены в img_ids
                foreach (var imgId in currentImgIds)
                {
                    if (!img_ids.Contains(imgId))
                    {
                        var imgToRemove = realty.Imgs.FirstOrDefault(i => i.id == imgId);
                        if (imgToRemove != null)
                        {
                            realty.Imgs.Remove(imgToRemove);
                        }
                    }

                    // Добавляем новые изображения
                    var ImgList = await db.Imgs.Where(x => img_ids.Contains(x.id)).ToListAsync();
                    foreach (var img in ImgList)
                    {
                        if (!realty.Imgs.Any(i => i.id == img.id)) // Проверка на дублирование
                        {
                            realty.Imgs.Add(img);
                        }
                    }
                }
            }
            #endregion

            Usage_report rep = new Usage_report { title = realtyModel.title, action = "Add", table = "Realty", date = DateTime.Now, id_user = user.id };
            db.Usage_report.Add(rep);

            await db.SaveChangesAsync();

            return Json(new { success = true, message = "Аренда успешна изменена!" });
        }

        [HttpPost]
        [Authorize(Roles = "admin, realty_master, realty")]
        public async Task<ActionResult> DeleteRealty(int id)//Удаление босса
        {
            // Получаем пользователя
            Users user = await db.Users.FirstOrDefaultAsync(x => x.login == User.Identity.Name);
            if (user == null)
            {
                return Json(new { success = false, message = "Пользователь не найден." });
            }

            // Получаем аренду
            Realty realty = await db.Realty.FirstOrDefaultAsync(x => x.id == id);
            if (realty == null)
            {
                return Json(new { success = false, message = "Объект аренды не найден." });
            }

            Usage_report rep = new Usage_report { title = realty.title, action = "Delete", table = "Realty", date = DateTime.Now, id_user = user.id };
            db.Usage_report.Add(rep);

            // Удаляем новость
            db.Realty.Remove(realty);
            await db.SaveChangesAsync();

            return Json(new { success = true, message = "Аренда удалена!" });
        }

        //Продажа имущества
        [HttpGet]
        [Authorize(Roles = "admin, realty_master")]
        public async Task<ActionResult> DepartmentsForSale()//Меню выбора филиала
        {
            var departments = await db.Departments.Where(x => x.id < 21).ToListAsync();
            return View("~/Views/Admin/Sales/DepartmentsForSale.cshtml", departments);
        }

        [HttpGet]
        [Authorize(Roles = "admin, realty_master")]
        public async Task<ActionResult> SaleByDepartment(int id)//Имущество выбранного филиала
        {
            ViewBag.IdDep = id;
            ViewBag.nameDep = await db.Departments.Where(x => x.id == id).Select(x => x.short_name_ru).FirstOrDefaultAsync();
            var sale = await db.Sale.Where(x => x.id_dep == id).ToListAsync();
            return View("~/Views/Admin/Sales/SaleByDepartment.cshtml", sale);
        }

        [HttpGet]
        [Authorize(Roles = "realty")]
        public async Task<ActionResult> Sales()//Имущество филиала
        {
            var id_dep = await db.Users.Where(x => x.login == User.Identity.Name).Select(x => x.id_dep).FirstOrDefaultAsync();
            if (id_dep == 0)
            {
                return HttpNotFound("Пользователь не найден.");
            }

            ViewBag.IdDep = id_dep;
            ViewBag.nameDep = await db.Departments.Where(x => x.id == id_dep).Select(x => x.short_name_ru).FirstOrDefaultAsync();

            var sales = await db.Sale.Where(x => x.id_dep == id_dep).ToListAsync();
            return View("~/Views/Admin/Sales/Sales.cshtml", sales);
        }

        [HttpGet]
        [Authorize(Roles = "admin, realty_master, realty")]
        public async Task<ActionResult> ShowSale(int id)//Показать выбранное имущество
        {
            var saleView = await db.Sale.Where(x => x.id == id).Include(x => x.Imgs).FirstOrDefaultAsync();
            return PartialView("~/Views/Admin/Sales/_ShowSale.cshtml", saleView);
        }

        [HttpGet]
        [Authorize(Roles = "admin, realty_master, realty")]
        public async Task<ActionResult> ShowAddSale(int id)//Страница добавления нового босса
        {
            ViewBag.Id_dep = id;
            return PartialView("~/Views/Admin/Sales/_ShowAddSale.cshtml");
        }

        [HttpPost]
        [Authorize(Roles = "admin, realty_master, realty")]
        public async Task<ActionResult> AddSale(Sale saleModel, int[] img_ids)//Добавление новой аренды
        {
            // Получаем пользователя
            Users user = await db.Users.FirstOrDefaultAsync(x => x.login == User.Identity.Name);
            if (user == null)
            {
                return Json(new { success = false, message = "Пользователь не найден." });
            }

            Usage_report rep = new Usage_report { title = saleModel.title, action = "Add", table = "Sale", date = DateTime.Now, id_user = user.id };
            db.Usage_report.Add(rep);

            if (img_ids != null)
            {
                Imgs img = new Imgs();
                foreach (var id in img_ids)
                {
                    img = await db.Imgs.Where(x => x.id == id).FirstOrDefaultAsync();
                    saleModel.Imgs.Add(img);
                }
            }

            db.Sale.Add(saleModel);

            await db.SaveChangesAsync();
            return Json(new { success = true, message = "Имущество успешно добавлено!" });
        }

        [HttpPost]
        [Authorize(Roles = "admin, realty_master, realty")]
        public async Task<ActionResult> EditSale(Sale saleModel, int[] img_ids)//Изменение босса
        {
            // Получаем пользователя
            Users user = await db.Users.FirstOrDefaultAsync(x => x.login == User.Identity.Name);
            if (user == null)
            {
                return Json(new { success = false, message = "Пользователь не найден." });
            }

            // Получаем аренду
            Sale sale = await db.Sale.Include(o => o.Imgs).FirstOrDefaultAsync(x => x.id == saleModel.id);
            if (sale == null)
            {
                return Json(new { success = false, message = "Имущество не найдено." });
            }

            #region Присваивание измененных значений            
            PropertyUpdater.UpdateProperties(sale, saleModel);
            // Обработка изображений
            if (img_ids != null)
            {
                var currentImgIds = sale.Imgs.Select(i => i.id).ToList();

                // Удаляем изображения, которые не включены в img_ids
                foreach (var imgId in currentImgIds)
                {
                    if (!img_ids.Contains(imgId))
                    {
                        var imgToRemove = sale.Imgs.FirstOrDefault(i => i.id == imgId);
                        if (imgToRemove != null)
                        {
                            sale.Imgs.Remove(imgToRemove);
                        }
                    }

                    // Добавляем новые изображения
                    var ImgList = await db.Imgs.Where(x => img_ids.Contains(x.id)).ToListAsync();
                    foreach (var img in ImgList)
                    {
                        if (!sale.Imgs.Any(i => i.id == img.id)) // Проверка на дублирование
                        {
                            sale.Imgs.Add(img);
                        }
                    }
                }
            }
            #endregion

            Usage_report rep = new Usage_report { title = saleModel.title, action = "Add", table = "Sale", date = DateTime.Now, id_user = user.id };
            db.Usage_report.Add(rep);

            await db.SaveChangesAsync();

            return Json(new { success = true, message = "Имущество успешно изменено!" });
        }

        [HttpPost]
        [Authorize(Roles = "admin, realty_master, realty")]
        public async Task<ActionResult> DeleteSale(int id)//Удаление босса
        {
            // Получаем пользователя
            Users user = await db.Users.FirstOrDefaultAsync(x => x.login == User.Identity.Name);
            if (user == null)
            {
                return Json(new { success = false, message = "Пользователь не найден." });
            }

            // Получаем имущество
            Sale sale = await db.Sale.FirstOrDefaultAsync(x => x.id == id);
            if (sale == null)
            {
                return Json(new { success = false, message = "имущество не найдено." });
            }

            Usage_report rep = new Usage_report { title = sale.title, action = "Delete", table = "Sale", date = DateTime.Now, id_user = user.id };
            db.Usage_report.Add(rep);

            // Удаляем имущество
            db.Sale.Remove(sale);
            await db.SaveChangesAsync();

            return Json(new { success = true, message = "Имущество удалено!" });
        }

        //Техника      
        [HttpGet]
        [Authorize(Roles = "admin, mech_master")]
        public async Task<ActionResult> DepartmentsForMech()//Меню выбора филиала
        {
            var departments = await db.Departments.Where(x => x.id < 21).ToListAsync();
            return View("~/Views/Admin/Mechs/DepartmentsForMech.cshtml", departments);
        }

        [HttpGet]
        [Authorize(Roles = "admin, mech_master")]
        public async Task<ActionResult> MechByDepartment(int id)//Имущество выбранного филиала
        {
            ViewBag.IdDep = id;
            ViewBag.nameDep = await db.Departments.Where(x => x.id == id).Select(x => x.short_name_ru).FirstOrDefaultAsync();
            var mech = await db.Mechanisms.Where(x => x.id_dep == id).ToListAsync();
            return View("~/Views/Admin/Mechs/MechByDepartment.cshtml", mech);
        }

        [HttpGet]
        [Authorize(Roles = "mech")]
        public async Task<ActionResult> Mechs()//Имущество филиала
        {
            var id_dep = await db.Users.Where(x => x.login == User.Identity.Name).Select(x => x.id_dep).FirstOrDefaultAsync();
            if (id_dep == 0)
            {
                return HttpNotFound("Пользователь не найден.");
            }

            ViewBag.IdDep = id_dep;
            ViewBag.nameDep = await db.Departments.Where(x => x.id == id_dep).Select(x => x.short_name_ru).FirstOrDefaultAsync();

            var mechs = await db.Mechanisms.Where(x => x.id_dep == id_dep).ToListAsync();
            return View("~/Views/Admin/Mechs/Mechs.cshtml", mechs);
        }

        [HttpGet]
        [Authorize(Roles = "admin, mech_master, mech")]
        public async Task<ActionResult> ShowMech(int id)//Показать выбранное имущество
        {
            var mechView = await db.Mechanisms.Where(x => x.id == id).Include(x => x.Imgs).FirstOrDefaultAsync();
            return PartialView("~/Views/Admin/Mechs/_ShowMech.cshtml", mechView);
        }

        [HttpGet]
        [Authorize(Roles = "admin, mech_master, mech")]
        public async Task<ActionResult> ShowAddMech(int id)//Страница добавления нового босса
        {
            ViewBag.Id_dep = id;
            return PartialView("~/Views/Admin/Mechs/_ShowAddMech.cshtml");
        }

        [HttpPost]
        [Authorize(Roles = "admin, mech_master, mech")]
        public async Task<ActionResult> AddMech(Mechanisms mechModel, int[] img_ids)//Добавление новой аренды
        {
            // Получаем пользователя
            Users user = await db.Users.FirstOrDefaultAsync(x => x.login == User.Identity.Name);
            if (user == null)
            {
                return Json(new { success = false, message = "Пользователь не найден." });
            }

            Usage_report rep = new Usage_report { title = mechModel.title, action = "Add", table = "Mechanisms", date = DateTime.Now, id_user = user.id };
            db.Usage_report.Add(rep);

            if (img_ids != null)
            {
                Imgs img = new Imgs();
                foreach (var id in img_ids)
                {
                    img = await db.Imgs.Where(x => x.id == id).FirstOrDefaultAsync();
                    mechModel.Imgs.Add(img);
                }
            }

            db.Mechanisms.Add(mechModel);

            await db.SaveChangesAsync();
            return Json(new { success = true, message = "Техника успешно добавлена!" });
        }

        [HttpPost]
        [Authorize(Roles = "admin, mech_master, mech")]
        public async Task<ActionResult> EditMech(Mechanisms mechModel, int[] img_ids)//Изменение босса
        {
            // Получаем пользователя
            Users user = await db.Users.FirstOrDefaultAsync(x => x.login == User.Identity.Name);
            if (user == null)
            {
                return Json(new { success = false, message = "Пользователь не найден." });
            }

            // Получаем технику
            Mechanisms mech = await db.Mechanisms.Include(o => o.Imgs).FirstOrDefaultAsync(x => x.id == mechModel.id);
            if (mech == null)
            {
                return Json(new { success = false, message = "Техника не найдена." });
            }

            #region Присваивание измененных значений            
            PropertyUpdater.UpdateProperties(mech, mechModel);
            // Обработка изображений
            if (img_ids != null)
            {
                var currentImgIds = mech.Imgs.Select(i => i.id).ToList();

                // Удаляем изображения, которые не включены в img_ids
                foreach (var imgId in currentImgIds)
                {
                    if (!img_ids.Contains(imgId))
                    {
                        var imgToRemove = mech.Imgs.FirstOrDefault(i => i.id == imgId);
                        if (imgToRemove != null)
                        {
                            mech.Imgs.Remove(imgToRemove);
                        }
                    }

                    // Добавляем новые изображения
                    var ImgList = await db.Imgs.Where(x => img_ids.Contains(x.id)).ToListAsync();
                    foreach (var img in ImgList)
                    {
                        if (!mech.Imgs.Any(i => i.id == img.id)) // Проверка на дублирование
                        {
                            mech.Imgs.Add(img);
                        }
                    }
                }
            }
            #endregion

            Usage_report rep = new Usage_report { title = mechModel.title, action = "Edit", table = "Mechanisms", date = DateTime.Now, id_user = user.id };
            db.Usage_report.Add(rep);

            await db.SaveChangesAsync();

            return Json(new { success = true, message = "Техника успешна изменена!" });
        }

        [HttpPost]
        [Authorize(Roles = "admin, mech_master, mech")]
        public async Task<ActionResult> DeleteMech(int id)//Удаление техники
        {
            // Получаем пользователя
            Users user = await db.Users.FirstOrDefaultAsync(x => x.login == User.Identity.Name);
            if (user == null)
            {
                return Json(new { success = false, message = "Пользователь не найден." });
            }

            // Получаем технику
            Mechanisms mech = await db.Mechanisms.FirstOrDefaultAsync(x => x.id == id);
            if (mech == null)
            {
                return Json(new { success = false, message = "Техника не найдена." });
            }

            Usage_report rep = new Usage_report { title = mech.title, action = "Delete", table = "Mechanisms", date = DateTime.Now, id_user = user.id };
            db.Usage_report.Add(rep);

            // Удаляем имущество
            db.Mechanisms.Remove(mech);
            await db.SaveChangesAsync();

            return Json(new { success = true, message = "Техника удалена!" });
        }

        //УСЛУГИ

        [HttpGet]
        [Authorize(Roles = "admin, service_master")]
        public async Task<ActionResult> DepartmentsForService()//Меню выбора филиала
        {
            var departments = await db.Departments.Where(x => x.id < 21).ToListAsync();
            return View("~/Views/Admin/Services/DepartmentsForService.cshtml", departments);
        }

        [HttpGet]
        [Authorize(Roles = "admin, service_master")]
        public async Task<ActionResult> ServiceByDepartment(int id)//Услуги выбранного филиала
        {
            ViewBag.IdDep = id;
            ViewBag.nameDep = await db.Departments.Where(x => x.id == id).Select(x => x.short_name_ru).FirstOrDefaultAsync();
            var service = await db.Services.Where(x => x.id_dep == id).OrderBy(x => x.title).ToListAsync();
            return View("~/Views/Admin/Services/ServiceByDepartment.cshtml", service);
        }

        [HttpGet]
        [Authorize(Roles = "service")]
        public async Task<ActionResult> Services()//Услуги филиала
        {
            var id_dep = await db.Users.Where(x => x.login == User.Identity.Name).Select(x => x.id_dep).FirstOrDefaultAsync();
            if (id_dep == 0)
            {
                return HttpNotFound("Пользователь не найден.");
            }

            ViewBag.IdDep = id_dep;
            ViewBag.nameDep = await db.Departments.Where(x => x.id == id_dep).Select(x => x.short_name_ru).FirstOrDefaultAsync();

            var service = await db.Services.Where(x => x.id_dep == id_dep).OrderBy(x => x.title).ToListAsync();
            return View("~/Views/Admin/Services/Services.cshtml", service);
        }

        [HttpGet]
        [Authorize(Roles = "admin, service_master, service")]
        public async Task<ActionResult> ShowService(int id)//Показать выбранную услугу
        {
            var serviceView = await db.Services.Where(x => x.id == id).Include(x => x.Imgs).FirstOrDefaultAsync();
            return PartialView("~/Views/Admin/Services/_ShowService.cshtml", serviceView);
        }

        [HttpGet]
        [Authorize(Roles = "admin, service_master, service")]
        public async Task<ActionResult> ShowAddService(int id)//Страница добавления новой услуги
        {
            ViewBag.Id_dep = id;
            return PartialView("~/Views/Admin/Services/_ShowAddService.cshtml");
        }

        [HttpPost]
        [Authorize(Roles = "admin, service_master, service")]
        public async Task<ActionResult> AddService(Services serviceModel, int[] img_ids)//Добавление новой услуги
        {
            // Получаем пользователя
            Users user = await db.Users.FirstOrDefaultAsync(x => x.login == User.Identity.Name);
            if (user == null)
            {
                return Json(new { success = false, message = "Пользователь не найден." });
            }

            Usage_report rep = new Usage_report { title = serviceModel.title, action = "Add", table = "Services", date = DateTime.Now, id_user = user.id };
            db.Usage_report.Add(rep);

            if (img_ids != null)
            {
                Imgs img = new Imgs();
                foreach (var id in img_ids)
                {
                    img = await db.Imgs.Where(x => x.id == id).FirstOrDefaultAsync();
                    serviceModel.Imgs.Add(img);
                }
            }

            db.Services.Add(serviceModel);

            await db.SaveChangesAsync();
            return Json(new { success = true, message = "Услуга успешно добавлена!" });
        }

        [HttpPost]
        [Authorize(Roles = "admin, service_master, service")]
        public async Task<ActionResult> EditService(Services serviceModel, int[] img_ids)//Изменение услуги
        {
            // Получаем пользователя
            Users user = await db.Users.FirstOrDefaultAsync(x => x.login == User.Identity.Name);
            if (user == null)
            {
                return Json(new { success = false, message = "Пользователь не найден." });
            }

            // Получаем технику
            Services service = await db.Services.Include(o => o.Imgs).FirstOrDefaultAsync(x => x.id == serviceModel.id);
            if (service == null)
            {
                return Json(new { success = false, message = "Услуга не найдена." });
            }

            #region Присваивание измененных значений            
            PropertyUpdater.UpdateProperties(service, serviceModel);
            // Обработка изображений
            if (img_ids != null)
            {
                var currentImgIds = service.Imgs.Select(i => i.id).ToList();

                // Удаляем изображения, которые не включены в img_ids
                foreach (var imgId in currentImgIds)
                {
                    if (!img_ids.Contains(imgId))
                    {
                        var imgToRemove = service.Imgs.FirstOrDefault(i => i.id == imgId);
                        if (imgToRemove != null)
                        {
                            service.Imgs.Remove(imgToRemove);
                        }
                    }

                    // Добавляем новые изображения
                    var ImgList = await db.Imgs.Where(x => img_ids.Contains(x.id)).ToListAsync();
                    foreach (var img in ImgList)
                    {
                        if (!service.Imgs.Any(i => i.id == img.id)) // Проверка на дублирование
                        {
                            service.Imgs.Add(img);
                        }
                    }
                }
            }
            #endregion

            Usage_report rep = new Usage_report { title = serviceModel.title, action = "Edit", table = "Services", date = DateTime.Now, id_user = user.id };
            db.Usage_report.Add(rep);

            await db.SaveChangesAsync();

            return Json(new { success = true, message = "Услуга успешна изменена!" });
        }

        [HttpPost]
        [Authorize(Roles = "admin, service_master, service")]
        public async Task<ActionResult> DeleteService(int id)//Удаление услуги
        {
            // Получаем пользователя
            Users user = await db.Users.FirstOrDefaultAsync(x => x.login == User.Identity.Name);
            if (user == null)
            {
                return Json(new { success = false, message = "Пользователь не найден." });
            }

            // Получаем услугу
            Services service = await db.Services.FirstOrDefaultAsync(x => x.id == id);
            if (service == null)
            {
                return Json(new { success = false, message = "Услуга не найдена." });
            }

            Usage_report rep = new Usage_report { title = service.title, action = "Delete", table = "Services", date = DateTime.Now, id_user = user.id };
            db.Usage_report.Add(rep);

            // Удаляем услугу
            db.Services.Remove(service);
            await db.SaveChangesAsync();

            return Json(new { success = true, message = "Услуга удалена!" });
        }       

        [HttpGet]
        [Authorize(Roles = "product, admin")]
        public async Task<ActionResult> Products()
        {
            // Получаем пользователя
            Users user = await db.Users.FirstOrDefaultAsync(x => x.login == User.Identity.Name);
            if (user == null)
            {
                return Json(new { success = false, message = "Пользователь не найден." });
            }

            var id_dep = await db.Users.Where(x => x.login == User.Identity.Name).Select(x => x.id_dep).FirstOrDefaultAsync();
            if (id_dep == 0)
            {
                return HttpNotFound("Пользователь не найден.");
            }

            ViewBag.nameDep = await db.Departments.Where(d => d.id == id_dep).Select(d => d.short_name_ru).FirstOrDefaultAsync();

            // Получаем группы товаров
            var groups = await db.GroupProducts.ToListAsync();
            if (groups == null)
            {
                return Json(new { success = false, message = "Группы товаров не найдены." });
            }
            return View("~/Views/Admin/Products/Products.cshtml", groups);
        }

        [HttpGet]
        [Authorize(Roles = "product, admin")]
        public async Task<ActionResult> ShowSubGroupForProduct(int id)
        {
            // Получаем пользователя
            Users user = await db.Users.FirstOrDefaultAsync(x => x.login == User.Identity.Name);
            if (user == null)
            {
                return Json(new { success = false, message = "Пользователь не найден." });
            }
            // Получаем подгруппы товаров
            var subGroups = await db.SubGroupProducts.Where(x => x.GroupProductId == id).ToListAsync();
            if (subGroups == null)
            {
                return Json(new { success = false, message = "Подгруппа товаров не найдена." });
            }
            return PartialView("~/Views/Admin/Products/_ShowSubGroupForProduct.cshtml", subGroups);
        }

        [HttpPost]
        [Authorize(Roles = "product, admin")]
        public async Task<ActionResult> ShowProductByCategory(int groupId, int subGroupId, int count = 20, int page = 1)
        {
            // Получаем пользователя
            Users user = await db.Users.FirstOrDefaultAsync(x => x.login == User.Identity.Name);
            if (user == null)
            {
                return Json(new { success = false, message = "Пользователь не найден." });
            }

            if (page < 1)
            {
                page = 1; // Убедитесь, что номер страницы не меньше 1
            }
            var listProduct = new List<Product>();
            var totalItems = 0;
            if (groupId == 0)
            {
                listProduct = await db.Products
                        .Where(p => p.DepartmentId == user.id_dep)
                        .OrderBy(p => p.name) // Сортировка по дате
                        .Skip((page - 1) * count) // Пропустить элементы для предыдущих страниц
                        .Take(count) // Взять только count элементов для текущей страницы
                        .ToListAsync(); // Преобразовать в список

                totalItems = await db.Products
                            .Where(p => p.DepartmentId == user.id_dep)
                            .CountAsync(); // Общее количество элементов
            }
            else if (subGroupId == 0)
            {
                var subGroups = await db.SubGroupProducts.Where(x => x.GroupProductId == groupId).Select(x => x.id).ToListAsync();
                listProduct = await db.Products
                        .Where(p => p.DepartmentId == user.id_dep && subGroups.Contains(p.SubGroupProductId))
                        .OrderBy(p => p.name) // Сортировка по имени
                        .Skip((page - 1) * count) // Пропустить элементы для предыдущих страниц
                        .Take(count) // Взять только count элементов для текущей страницы
                        .ToListAsync(); // Преобразовать в список

                totalItems = await db.Products
                            .Where(p => p.DepartmentId == user.id_dep && subGroups.Contains(p.SubGroupProductId))
                            .CountAsync(); // Общее количество элементов
            }
            else
            {
                listProduct = await db.Products
                        .Where(p => p.DepartmentId == user.id_dep && p.SubGroupProductId == subGroupId)
                        .OrderBy(p => p.name) // Сортировка по дате
                        .Skip((page - 1) * count) // Пропустить элементы для предыдущих страниц
                        .Take(count) // Взять только count элементов для текущей страницы
                        .ToListAsync(); // Преобразовать в список

                totalItems = await db.Products
                            .Where(p => p.DepartmentId == user.id_dep && p.SubGroupProductId == subGroupId)
                            .CountAsync(); // Общее количество элементов
            }

            var totalPages = (int)Math.Ceiling((double)totalItems / count); // Общее количество страниц
            if (totalPages < page)
                page = totalPages;

            ViewBag.totalPages = totalPages;
            ViewBag.activePage = page;
            return PartialView("~/Views/Admin/Products/_ShowProductByCategory.cshtml", listProduct);
        }

        [HttpPost]
        [Authorize(Roles = "product, admin")]
        public async Task<ActionResult> ShowProductsById(int id)
        {
            ViewBag.version = await db.VersionProducts.Where(v => v.ProductId == id).ToListAsync();
            var viewModel = await db.Products.Where(p => p.id == id).FirstOrDefaultAsync();
            return PartialView("~/Views/Admin/Products/_ShowProductsById.cshtml", viewModel);
        }

        [HttpPost]
        [Authorize(Roles = "product, admin")]
        public async Task<ActionResult> ShowProductById(int id)
        {
            ViewBag.groupType = await db.GroupProducts.ToListAsync();
            ViewBag.version = await db.VersionProducts.Where(v => v.ProductId == id).ToListAsync();
            ViewBag.units = await db.Units.ToListAsync();
            ViewBag.properties = await db.Properties.ToListAsync();
            ViewBag.components = await db.Components.ToListAsync();
            var groupId = await
                db.Products
                .Where(p => p.id == id)
                .Select(p => p.SubGroupProduct.GroupProductId)
                .FirstOrDefaultAsync();
            ViewBag.subGroupType = await db.SubGroupProducts.Where(s => s.GroupProductId == groupId).ToListAsync();

            #region приведение
            var viewModel = await
                db.Products
                .Where(p => p.id == id)
                .Select(p => new ProductViewModel
                {
                    id = p.id,
                    DepartmentId = p.DepartmentId,
                    name = p.name,
                    codeTNVD = p.codeTNVD,
                    note = p.note,
                    unitId = p.UnitId,
                    subGroupProduct = p.SubGroupProduct.name,
                    groupProduct = p.SubGroupProduct.GroupProduct.name
                }).FirstOrDefaultAsync();
            viewModel.Imgs = new List<Imgs>();
            var imgsProduct = await db.ImgsProduct.Where(x => x.ProductId == viewModel.id).ToListAsync();
            foreach (var img in imgsProduct)
            {
                viewModel.Imgs.Add(await db.Imgs.Where(x => x.id == img.ImgsId).FirstOrDefaultAsync());
            }
            viewModel.properties = new List<PropertyViewModel>();
            var propProduct = await db.PropertyProducts.Where(x => x.ProductId == viewModel.id).ToListAsync();
            foreach (var prop in propProduct)
            {
                var propView = new PropertyViewModel();
                propView.value = prop.value;
                propView.name = await db.Properties.Where(x => x.id == prop.PropertyId).Select(x => x.name).FirstOrDefaultAsync();
                viewModel.properties.Add(propView);
            }
            viewModel.components = new List<ComponentViewModel>();
            var compProduct = await db.ComponentProducts.Where(x => x.ProductId == viewModel.id).ToListAsync();
            foreach (var compP in compProduct)
            {
                var compView = new ComponentViewModel();
                compView.name = await db.Components.Where(x => x.id == compP.ComponentId).Select(x => x.name).FirstOrDefaultAsync();
                viewModel.components.Add(compView);
            }
            #endregion            

            return PartialView("~/Views/Admin/Products/_ShowProductById.cshtml", viewModel);
        }

        [HttpPost]
        [Authorize(Roles = "product, admin")]
        public async Task<ActionResult> ShowSubGroupSelectForProduct(string name)
        {
            // Получаем подгруппы товаров
            var subGroups = await db.SubGroupProducts.Where(x => x.GroupProduct.name == name).ToListAsync();
            if (subGroups == null)
            {
                throw new Exception("Ошибка при получении листа подгрупп.");
            }
            return PartialView("~/Views/Admin/Products/_ShowSubGroupSelectForProduct.cshtml", subGroups);
        }

        [HttpGet]
        [Authorize(Roles = "product, admin")]
        public async Task<ActionResult> AddProduct()
        {
            // Получаем пользователя
            Users user = await db.Users.FirstOrDefaultAsync(x => x.login == User.Identity.Name);
            if (user == null)
            {
                return Json(new { success = false, message = "Пользователь не найден." });
            }

            ViewBag.groupType = await db.GroupProducts.ToListAsync();
            ViewBag.id_dep = await db.Users.Where(x => x.login == User.Identity.Name).Select(x => x.id_dep).FirstOrDefaultAsync();
            ViewBag.units = await db.Units.ToListAsync();
            ViewBag.properties = await db.Properties.ToListAsync();
            ViewBag.components = await db.Components.ToListAsync();
            // Получаем группы товаров
            var groups = await db.GroupProducts.ToListAsync();
            if (groups == null)
            {
                return Json(new { success = false, message = "Группы товаров не найдены." });
            }
            return View("~/Views/Admin/Products/AddProduct.cshtml");
        }

        [HttpPost]
        [Authorize(Roles = "admin, product")]
        public async Task<ActionResult> AddProduct(ProductViewModel productModel, int[] img_ids)//Добавление нового продукта
        {
            // Получаем пользователя
            Users user = await db.Users.FirstOrDefaultAsync(x => x.login == User.Identity.Name);
            if (user == null)
            {
                return Json(new { success = false, message = "Пользователь не найден." });
            }

            Usage_report rep = new Usage_report { title = productModel.name, action = "Add", table = "Product", date = DateTime.Now, id_user = user.id };
            db.Usage_report.Add(rep);

            #region Product
            var product = new Product();
            var SubGroupProduct = await db.SubGroupProducts.Where(s => s.name == productModel.subGroupProduct).FirstOrDefaultAsync();
            if (SubGroupProduct == null)
            {
                var GroupProduct = await db.GroupProducts.Where(s => s.name == productModel.groupProduct).FirstOrDefaultAsync();
                if (GroupProduct == null)
                {
                    GroupProduct = new GroupProduct();
                    GroupProduct.name = productModel.groupProduct.Trim();
                    db.GroupProducts.Add(GroupProduct);
                }
                SubGroupProduct = new SubGroupProduct();
                SubGroupProduct.name = productModel.subGroupProduct.Trim();
                SubGroupProduct.GroupProduct = GroupProduct;
                db.SubGroupProducts.Add(SubGroupProduct);
            }
            product.SubGroupProduct = SubGroupProduct;

            product.note = productModel.note.Trim();
            product.DepartmentId = productModel.DepartmentId;
            product.codeTNVD = productModel.codeTNVD.Trim();
            product.name = productModel.name.Trim();
            product.UnitId = productModel.unitId;
            db.Products.Add(product);
            #endregion

            #region Component
            if (productModel.components != null && productModel.components.Count() > 0)
            {
                foreach (var comp in productModel.components)
                {
                    var component = await db.Components.Where(c => c.name == comp.name).FirstOrDefaultAsync();
                    if (component == null)
                    {
                        component = new Component();
                        component.name = comp.name.Trim();
                        db.Components.Add(component);
                    }
                    db.ComponentProducts.Add(new ComponentProduct
                    {
                        Component = component,
                        Product = product
                    });
                }
            }
            #endregion

            #region Property
            if (productModel.properties != null && productModel.properties.Count() > 0)
            {
                foreach (var prop in productModel.properties)
                {
                    if (prop.value != null && prop.value != "")
                    {
                        var property = await db.Properties.Where(p => p.name == prop.name).FirstOrDefaultAsync();
                        if (property == null)
                        {
                            property = new Property();
                            property.name = prop.name.Trim();
                            db.Properties.Add(property);
                        }
                        db.PropertyProducts.Add(new PropertyProduct
                        {
                            Property = property,
                            Product = product,
                            value = prop.value.Trim()
                        });
                    }
                }
            }
            #endregion

            #region Image
            if (img_ids != null)
            {
                Imgs img = new Imgs();
                foreach (var id in img_ids)
                {
                    img = await db.Imgs.Where(x => x.id == id).FirstOrDefaultAsync();
                    db.ImgsProduct.Add(new Imgs_to_product
                    {
                        Product = product,
                        Imgs = img
                    });
                }
            }
            #endregion

            await db.SaveChangesAsync();

            return Json(new { success = true, message = "Товар успешно добавлен!" });
        }

        [HttpPost]
        [Authorize(Roles = "admin, product")]
        public async Task<ActionResult> EditProduct(ProductViewModel productModel, int[] img_ids)//редактирование продукта
        {
            // Получаем пользователя
            Users user = await db.Users.FirstOrDefaultAsync(x => x.login == User.Identity.Name);
            if (user == null)
            {
                return Json(new { success = false, message = "Пользователь не найден." });
            }

            Usage_report rep = new Usage_report { title = productModel.name, action = "Edit", table = "Product", date = DateTime.Now, id_user = user.id };
            db.Usage_report.Add(rep);

            var product = new Product();
            #region Product
            var SubGroupProduct = await db.SubGroupProducts.Where(s => s.name == productModel.subGroupProduct).FirstOrDefaultAsync();
            if (SubGroupProduct == null)
            {
                var GroupProduct = await db.GroupProducts.Where(s => s.name == productModel.groupProduct).FirstOrDefaultAsync();
                if (GroupProduct == null)
                {
                    GroupProduct = new GroupProduct();
                    GroupProduct.name = productModel.groupProduct.Trim();
                    db.GroupProducts.Add(GroupProduct);
                }
                SubGroupProduct = new SubGroupProduct();
                SubGroupProduct.name = productModel.subGroupProduct.Trim();
                SubGroupProduct.GroupProduct = GroupProduct;
                db.SubGroupProducts.Add(SubGroupProduct);
                await db.SaveChangesAsync();
            }

            product.SubGroupProduct = SubGroupProduct;
            product.SubGroupProductId = SubGroupProduct.id;
            product.note = productModel.note.Trim();
            product.id = productModel.id;
            product.DepartmentId = productModel.DepartmentId;
            product.codeTNVD = productModel.codeTNVD.Trim();
            product.name = productModel.name.Trim();
            product.UnitId = productModel.unitId;
            var productDB = await db.Products.Where(s => s.id == productModel.id).FirstOrDefaultAsync();
            PropertyUpdater.UpdateProperties(productDB, product);
            productDB.SubGroupProduct = product.SubGroupProduct;
            #endregion            

            #region Component
            // Получаем существующие связи
            var existingComponentProducts = await db.ComponentProducts
                .Include(cp => cp.Component)
                .Where(x => x.ProductId == productModel.id)
                .ToListAsync();
            if (productModel.components != null && productModel.components.Count() > 0)
            {
                // Получаем имена компонентов из модели (нормализованные)
                var newComponentNames = productModel.components
                    .Select(c => c.name.Trim().ToLower())
                    .ToList();

                // Находим связи для удаления: те, которых нет в новой модели
                var toRemove = existingComponentProducts
                    .Where(cp => !newComponentNames.Contains(cp.Component.name.Trim().ToLower()))
                    .ToList();
                db.ComponentProducts.RemoveRange(toRemove);

                var existingComponentNamesAfterRemove = existingComponentProducts
                        .Except(toRemove)
                        .Select(cp => cp.Component.name.Trim().ToLower())
                        .ToList();
                // Добавляем новые компоненты из модели
                foreach (var comp in productModel.components)
                {
                    // Нормализуем имя+		existingComponentProducts	Count = 2	System.Collections.Generic.List<rupbes.Models.Products.ComponentProduct>

                    var normalizedName = comp.name.Trim().ToLower();

                    if (existingComponentNamesAfterRemove.Contains(normalizedName))
                        continue;

                    // Ищем компонент по нормализованному имени
                    var component = await db.Components
                        .FirstOrDefaultAsync(c => c.name.ToLower() == normalizedName);

                    if (component == null)
                    {
                        component = new Component { name = comp.name.Trim() }; // Сохраняем без лишних пробелов
                        db.Components.Add(component);
                    }

                    db.ComponentProducts.Add(new ComponentProduct
                    {
                        Component = component,
                        ProductId = productModel.id
                    });
                }
            }
            else if (existingComponentProducts.Count() > 0)
            {
                db.ComponentProducts.RemoveRange(existingComponentProducts);
            }
            #endregion

            #region Property          
            // Загрузка существующих связей
            var existingPropertyProducts = await db.PropertyProducts
                .Include(pp => pp.Property)
                .Where(pp => pp.ProductId == productModel.id)
                .ToListAsync();

            // Словарь существующих связей по нормализованному имени
            var existingDict = existingPropertyProducts
                .ToDictionary(pp => pp.Property.name.Trim().ToLower());
            if (productModel.properties != null && productModel.properties.Count() > 0)
            {
                // Нормализованные имена из модели
                var modelPropertyNames = new HashSet<string>(
                    productModel.properties?.Select(p => p.name.Trim().ToLower()) ?? Enumerable.Empty<string>()
                );

                // Загрузка существующих свойств из базы, которые есть в модели
                var existingProperties = await db.Properties
                    .Where(p => modelPropertyNames.Contains(p.name.Trim().ToLower()))
                    .ToListAsync();

                var existingPropertiesDict = existingProperties
                    .ToDictionary(p => p.name.Trim().ToLower());

                // Удаляем связи, которых нет в модели
                foreach (var existing in existingPropertyProducts)
                {
                    var normalizedName = existing.Property.name.Trim().ToLower();
                    if (!modelPropertyNames.Contains(normalizedName))
                    {
                        db.PropertyProducts.Remove(existing);
                    }
                }

                // Теперь обрабатываем каждое свойство в модели
                foreach (var propModel in productModel.properties)
                {
                    var normalizedName = propModel.name.Trim().ToLower();

                    // Ищем свойство в загруженных свойствах
                    if (!existingPropertiesDict.TryGetValue(normalizedName, out var property))
                    {
                        property = new Property { name = propModel.name.Trim() };
                        db.Properties.Add(property);
                        existingPropertiesDict[normalizedName] = property;
                    }

                    // Проверяем, есть ли уже связь для этого свойства и продукта
                    if (existingDict.TryGetValue(normalizedName, out var existingLink))
                    {
                        // Если значение изменилось, обновляем
                        if (existingLink.value != propModel.value)
                        {
                            existingLink.value = propModel.value;
                        }
                    }
                    else
                    {
                        // Создаем новую связь
                        db.PropertyProducts.Add(new PropertyProduct
                        {
                            ProductId = productModel.id,
                            Property = property, // используем объект property
                            value = propModel.value
                        });
                    }
                }
            }
            else if (existingPropertyProducts.Count() > 0)
            {
                db.PropertyProducts.RemoveRange(existingPropertyProducts);
            }
            #endregion

            #region Image
            if (img_ids != null && img_ids.Any())
            {
                // Получим существующие связи продукта с изображениями
                var existingImgLinks = await db.ImgsProduct
                    .Where(ip => ip.ProductId == product.id) // Используем product.id
                    .Select(ip => ip.ImgsId)
                    .ToListAsync();

                // Список для новых связей
                var newLinks = new List<Imgs_to_product>();

                foreach (var id in img_ids)
                {
                    // Проверяем, существует ли уже такая связь
                    if (!existingImgLinks.Contains(id))
                    {
                        // Проверяем существование самого изображения
                        var img = await db.Imgs.FindAsync(id);
                        if (img != null)
                        {
                            newLinks.Add(new Imgs_to_product
                            {
                                ProductId = product.id, // Используем Id вместо объекта
                                ImgsId = id
                            });
                        }
                    }
                }

                // Добавляем все новые связи разом
                db.ImgsProduct.AddRange(newLinks);
            }
            #endregion

            await db.SaveChangesAsync();

            return Json(new { success = true, message = "Товар успешно сохранен!" });
        }

        [HttpPost]
        [Authorize(Roles = "admin, product")]
        public async Task<ActionResult> DeleteProduct(int id)//Удаление товара
        {
            // Получаем пользователя
            Users user = await db.Users.FirstOrDefaultAsync(x => x.login == User.Identity.Name);
            if (user == null)
            {
                return Json(new { success = false, message = "Пользователь не найден." });
            }

            // Получаем товар
            Product product = await db.Products.FirstOrDefaultAsync(x => x.id == id);
            if (product == null)
            {
                return Json(new { success = false, message = "Товар не найден." });
            }

            Usage_report rep = new Usage_report { title = product.name, action = "Delete", table = "Product", date = DateTime.Now, id_user = user.id };
            db.Usage_report.Add(rep);

            // Удаляем услугу
            db.Products.Remove(product);
            await db.SaveChangesAsync();

            return Json(new { success = true, message = "Товар удален!" });
        }     

        [Authorize(Roles = "admin, product")]
        public async Task<ActionResult> ShowAddVersionProduct(int id)//Показ формы добавления версии товара
        {            
            ViewBag.productId = id;
            var product = await db.Products.FindAsync(id);
            if (product == null)
            {
                return HttpNotFound("Товар не найден");
            }
            ViewBag.productName = product.name;
            ViewBag.properties = await db.Properties.ToListAsync();           
            return PartialView("~/Views/Admin/Products/_ShowAddVersionProduct.cshtml");
        }

        [HttpPost]
        [Authorize(Roles = "admin, product")]
        public async Task<ActionResult> AddVersionProduct(VersionProductViewModel productModel, int[] img_ids)//Добавление нового продукта
        {
            // Получаем пользователя
            Users user = await db.Users.FirstOrDefaultAsync(x => x.login == User.Identity.Name);
            if (user == null)
            {
                return Json(new { success = false, message = "Пользователь не найден." });
            }

            Usage_report rep = new Usage_report { title = productModel.name, action = "Add", table = "VersionProduct", date = DateTime.Now, id_user = user.id };
            db.Usage_report.Add(rep);

            #region VersionProduct
            var versionProduct = new VersionProduct();
            versionProduct.note = productModel.note.Trim();
            versionProduct.name = productModel.name.Trim();
            versionProduct.ProductId = productModel.ProductId;
            db.VersionProducts.Add(versionProduct);
            #endregion          

            #region Property
            if (productModel.properties != null && productModel.properties.Count() > 0)
            {
                foreach (var prop in productModel.properties)
                {
                    if (prop.value != null && prop.value != "")
                    {
                        var property = await db.Properties.Where(p => p.name == prop.name).FirstOrDefaultAsync();
                        if (property == null)
                        {
                            property = new Property();
                            property.name = prop.name.Trim();
                            db.Properties.Add(property);
                        }
                        db.PropertyVersions.Add(new PropertyVersion
                        {
                            Property = property,
                            VersionProduct = versionProduct,
                            value = prop.value.Trim()
                        });
                    }
                }
            }
            #endregion

            #region Image
            if (img_ids != null)
            {
                Imgs img = new Imgs();
                foreach (var id in img_ids)
                {
                    img = await db.Imgs.Where(x => x.id == id).FirstOrDefaultAsync();
                    db.ImgsVersionProduct.Add(new Imgs_to_versionProduct
                    {
                        VersionProduct = versionProduct,
                        Imgs = img
                    });
                }
            }
            #endregion

            await db.SaveChangesAsync();

            return Json(new { success = true, message = "Версия товара успешно добавлена!" });
        }

        [Authorize(Roles = "admin, product")]
        public async Task<ActionResult> ShowVersionProduct(int id)//Показ формы добавления версии товара
        {                     
            #region приведение
            var viewModel = await
                db.VersionProducts
                .Where(p => p.id == id)
                .Select(p => new VersionProductViewModel
                {
                    id = p.id,
                    ProductId = p.ProductId,
                    name = p.name,                    
                    note = p.note
                }).FirstOrDefaultAsync();
            viewModel.Imgs = new List<Imgs>();
            var imgsProduct = await db.ImgsVersionProduct.Where(x => x.VersionProductId == viewModel.id).ToListAsync();
            foreach (var img in imgsProduct)
            {
                viewModel.Imgs.Add(await db.Imgs.Where(x => x.id == img.ImgsId).FirstOrDefaultAsync());
            }
            viewModel.properties = new List<PropertyViewModel>();
            var propVersionProduct = await db.PropertyVersions.Where(x => x.VersionId == viewModel.id).ToListAsync();
            foreach (var prop in propVersionProduct)
            {
                var propView = new PropertyViewModel();
                propView.value = prop.value;
                propView.name = await db.Properties.Where(x => x.id == prop.PropertyId).Select(x => x.name).FirstOrDefaultAsync();
                viewModel.properties.Add(propView);
            }            
            #endregion            
            ViewBag.properties = await db.Properties.ToListAsync();
            return PartialView("~/Views/Admin/Products/_ShowVersionProduct.cshtml", viewModel);
        }

        [HttpPost]
        [Authorize(Roles = "admin, product")]
        public async Task<ActionResult> EditVersionProduct(VersionProductViewModel productModel, int[] img_ids)//редактирование продукта
        {
            // Получаем пользователя
            Users user = await db.Users.FirstOrDefaultAsync(x => x.login == User.Identity.Name);
            if (user == null)
            {
                return Json(new { success = false, message = "Пользователь не найден." });
            }

            Usage_report rep = new Usage_report { title = productModel.name, action = "Edit", table = "VersionProduct", date = DateTime.Now, id_user = user.id };
            db.Usage_report.Add(rep);

            var product = new VersionProduct();
            #region VersionProduct                       
            product.note = productModel.note.Trim();
            product.id = productModel.id;
            product.ProductId = productModel.ProductId;            
            product.name = productModel.name.Trim();
            
            var productDB = await db.VersionProducts.Where(s => s.id == productModel.id).FirstOrDefaultAsync();
            PropertyUpdater.UpdateProperties(productDB, product);            
            #endregion                      

            #region Property          
            // Загрузка существующих связей
            var existingPropertyProducts = await db.PropertyVersions
                .Include(pp => pp.Property)
                .Where(pp => pp.VersionId == productModel.id)
                .ToListAsync();

            // Словарь существующих связей по нормализованному имени
            var existingDict = existingPropertyProducts
                .ToDictionary(pp => pp.Property.name.Trim().ToLower());
            if (productModel.properties != null && productModel.properties.Count() > 0)
            {
                // Нормализованные имена из модели
                var modelPropertyNames = new HashSet<string>(
                    productModel.properties?.Select(p => p.name.Trim().ToLower()) ?? Enumerable.Empty<string>()
                );

                // Загрузка существующих свойств из базы, которые есть в модели
                var existingProperties = await db.Properties
                    .Where(p => modelPropertyNames.Contains(p.name.Trim().ToLower()))
                    .ToListAsync();

                var existingPropertiesDict = existingProperties
                    .ToDictionary(p => p.name.Trim().ToLower());

                // Удаляем связи, которых нет в модели
                foreach (var existing in existingPropertyProducts)
                {
                    var normalizedName = existing.Property.name.Trim().ToLower();
                    if (!modelPropertyNames.Contains(normalizedName))
                    {
                        db.PropertyVersions.Remove(existing);
                    }
                }

                // Теперь обрабатываем каждое свойство в модели
                foreach (var propModel in productModel.properties)
                {
                    var normalizedName = propModel.name.Trim().ToLower();

                    // Ищем свойство в загруженных свойствах
                    if (!existingPropertiesDict.TryGetValue(normalizedName, out var property))
                    {
                        property = new Property { name = propModel.name.Trim() };
                        db.Properties.Add(property);
                        existingPropertiesDict[normalizedName] = property;
                    }

                    // Проверяем, есть ли уже связь для этого свойства и продукта
                    if (existingDict.TryGetValue(normalizedName, out var existingLink))
                    {
                        // Если значение изменилось, обновляем
                        if (existingLink.value != propModel.value)
                        {
                            existingLink.value = propModel.value;
                        }
                    }
                    else
                    {
                        // Создаем новую связь
                        db.PropertyVersions.Add(new PropertyVersion
                        {
                            VersionId = productModel.id,
                            Property = property, // используем объект property
                            value = propModel.value
                        });
                    }
                }
            }
            else if (existingPropertyProducts.Count() > 0)
            {
                db.PropertyVersions.RemoveRange(existingPropertyProducts);
            }
            #endregion

            #region Image
            if (img_ids != null && img_ids.Any())
            {
                // Получим существующие связи продукта с изображениями
                var existingImgLinks = await db.ImgsVersionProduct
                    .Where(ip => ip.VersionProductId == product.id) // Используем product.id
                    .Select(ip => ip.ImgsId)
                    .ToListAsync();

                // Список для новых связей
                var newLinks = new List<Imgs_to_versionProduct>();

                foreach (var id in img_ids)
                {
                    // Проверяем, существует ли уже такая связь
                    if (!existingImgLinks.Contains(id))
                    {
                        // Проверяем существование самого изображения
                        var img = await db.Imgs.FindAsync(id);
                        if (img != null)
                        {
                            newLinks.Add(new Imgs_to_versionProduct
                            {
                                VersionProductId = product.id, // Используем Id вместо объекта
                                ImgsId = id
                            });
                        }
                    }
                }

                // Добавляем все новые связи разом
                db.ImgsVersionProduct.AddRange(newLinks);
            }
            #endregion

            await db.SaveChangesAsync();

            return Json(new { success = true, message = "Версия товара успешно сохранена!" });
        }

        [HttpPost]
        [Authorize(Roles = "admin, product")]
        public async Task<ActionResult> DeleteVersionProduct(int id)//Удаление товара
        {
            // Получаем пользователя
            Users user = await db.Users.FirstOrDefaultAsync(x => x.login == User.Identity.Name);
            if (user == null)
            {
                return Json(new { success = false, message = "Пользователь не найден." });
            }

            // Получаем версию товара
            var product = await db.VersionProducts.FirstOrDefaultAsync(x => x.id == id);
            if (product == null)
            {
                return Json(new { success = false, message = "Товар не найден." });
            }

            Usage_report rep = new Usage_report { title = product.name, action = "Delete", table = "VersionProduct", date = DateTime.Now, id_user = user.id };
            db.Usage_report.Add(rep);

            // Удаляем услугу
            db.VersionProducts.Remove(product);
            await db.SaveChangesAsync();

            return Json(new { success = true, message = "Версия товара удалена!" });
        }

        [Authorize(Roles = "admin, product")]
        public async Task<ActionResult> GroupProducts()//Группы товаров
        {
            // Получаем пользователя
            Users user = await db.Users.FirstOrDefaultAsync(x => x.login == User.Identity.Name);
            if (user == null)
            {
                return Json(new { success = false, message = "Пользователь не найден." });
            }

            // Получаем товар
            var groupProducts = await db.GroupProducts.OrderBy(x => x.name.Length).ToListAsync();
            if (groupProducts == null)
            {
                return Json(new { success = false, message = "Группа не найдена." });
            }

            return View("~/Views/Admin/Products/GroupProducts.cshtml", groupProducts);
        }

        [Authorize(Roles = "admin, product")]
        public async Task<ActionResult> AddGroupProduct(GroupProduct groupProduct)//Группы товаров
        {
            // Получаем пользователя
            Users user = await db.Users.FirstOrDefaultAsync(x => x.login == User.Identity.Name);
            if (user == null)
            {
                return Json(new { success = false, message = "Пользователь не найден." });
            }

            db.GroupProducts.Add(groupProduct);
            await db.SaveChangesAsync();

            return Json(new { success = true, message = "Группа товаров добавлена." });
        }

        [Authorize(Roles = "admin, product")]
        public async Task<ActionResult> ShowAddGroupProduct()//Группы товаров
        {
            // Получаем пользователя
            Users user = await db.Users.FirstOrDefaultAsync(x => x.login == User.Identity.Name);
            if (user == null)
            {
                return Json(new { success = false, message = "Пользователь не найден." });
            }

            return PartialView("~/Views/Admin/Products/_ShowAddGroupProduct.cshtml");
        }

        [Authorize(Roles = "admin, product")]
        public async Task<ActionResult> ShowAddSubGroupProduct(int id)//Группы товаров
        {
            // Получаем пользователя
            Users user = await db.Users.FirstOrDefaultAsync(x => x.login == User.Identity.Name);
            if (user == null)
            {
                return Json(new { success = false, message = "Пользователь не найден." });
            }
            ViewBag.groupProductId = id;            

            return PartialView("~/Views/Admin/Products/_ShowAddSubGroupProduct.cshtml");
        }

        [Authorize(Roles = "admin, product")]
        public async Task<ActionResult> AddSubGroupProduct(SubGroupProduct subGroupProduct)//Группы товаров
        {
            // Получаем пользователя
            Users user = await db.Users.FirstOrDefaultAsync(x => x.login == User.Identity.Name);
            if (user == null)
            {
                return Json(new { success = false, message = "Пользователь не найден." });
            }

            db.SubGroupProducts.Add(subGroupProduct);
            await db.SaveChangesAsync();

            return Json(new { success = true, message = "Подгруппа товаров добавлена." });
        }

        [Authorize(Roles = "admin, product")]
        public async Task<ActionResult> ShowSubGroupProducts(int id)//Подгруппы товаров
        {
            // Получаем пользователя
            Users user = await db.Users.FirstOrDefaultAsync(x => x.login == User.Identity.Name);
            if (user == null)
            {
                return Json(new { success = false, message = "Пользователь не найден." });
            }
            ViewBag.groupId = id;
            ViewBag.groupName = await db.GroupProducts.Where(x => x.id == id).Select(x => x.name).FirstOrDefaultAsync();

            // Получаем товар
            var subGroupProducts = await db.SubGroupProducts.Where(x => x.GroupProductId == id).OrderBy(x => x.name.Length).ToListAsync();
            if (subGroupProducts == null)
            {
                return Json(new { success = false, message = "Подгруппы не найдены." });
            }

            return PartialView("~/Views/Admin/Products/_ShowSubGroupProducts.cshtml", subGroupProducts);
        }

        [Authorize(Roles = "admin, product")]
        public async Task<ActionResult> ShowSubGroupProduct(int id)//Подгруппы товаров
        {
            // Получаем пользователя
            Users user = await db.Users.FirstOrDefaultAsync(x => x.login == User.Identity.Name);
            if (user == null)
            {
                return Json(new { success = false, message = "Пользователь не найден." });
            }           

            // Получаем товар
            var subGroupProduct = await db.SubGroupProducts.Where(x => x.id == id).FirstOrDefaultAsync();
            if (subGroupProduct == null)
            {
                return Json(new { success = false, message = "Подгруппа не найдена." });
            }

            return PartialView("~/Views/Admin/Products/_ShowSubGroupProduct.cshtml", subGroupProduct);
        }

        [Authorize(Roles = "admin, product")]
        public async Task<ActionResult> EditGroupProduct(GroupProduct modelGroupProduct)//Группы товаров
        {
            // Получаем пользователя
            Users user = await db.Users.FirstOrDefaultAsync(x => x.login == User.Identity.Name);
            if (user == null)
            {
                return Json(new { success = false, message = "Пользователь не найден." });
            }

            // Получаем группу
            var groupProduct = await db.GroupProducts.Where(x => x.id == modelGroupProduct.id).FirstOrDefaultAsync();
            if (groupProduct == null)
            {
                return Json(new { success = false, message = "Группа не найдена." });
            }

            groupProduct.name = modelGroupProduct.name;

            await db.SaveChangesAsync();

            return Json(new { success = true, message = "Название группы товаров изменено." });
        }

        [Authorize(Roles = "admin, product")]
        public async Task<ActionResult> EditSubGroupProduct(SubGroupProduct modelSubGroupProduct)//Группы товаров
        {
            // Получаем пользователя
            Users user = await db.Users.FirstOrDefaultAsync(x => x.login == User.Identity.Name);
            if (user == null)
            {
                return Json(new { success = false, message = "Пользователь не найден." });
            }

            // Получаем подгруппу
            var subGroupProduct = await db.SubGroupProducts.Where(x => x.id == modelSubGroupProduct.id).FirstOrDefaultAsync();
            if (subGroupProduct == null)
            {
                return Json(new { success = false, message = "Группа не найдена." });
            }

            subGroupProduct.name = modelSubGroupProduct.name;
            subGroupProduct.GroupProductId = modelSubGroupProduct.GroupProductId;

            await db.SaveChangesAsync();

            return Json(new { success = true, message = "Название группы товаров изменено." });
        }

        public ActionResult Message(string message, string header, string textButton)
        {
            return PartialView("_Message", new ModalViewModel(message, header, textButton));
        }

        public ActionResult MessageWithReload(string message, string header, string textButton)
        {
            ViewData["reload"] = "Yes";
            return PartialView("_Message", new ModalViewModel(message, header, textButton));
        }
    }
}