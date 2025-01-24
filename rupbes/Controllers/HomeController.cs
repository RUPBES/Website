using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using rupbes.Classes;
using System.Data.Entity;
using rupbes.Models;

namespace rupbes.Controllers
{
    [Filters.Culture]
    public class HomeController : Controller
    {
        private Models.Database db = new Models.Database();
        private Models.DatabaseBes.Database db1 = new Models.DatabaseBes.Database();

        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public ActionResult Company()
        {
            HttpCookie cookie = Request.Cookies["lang"];
            Departments mainCompany = db.Departments.FirstOrDefault(x => x.type_id == 1);
            mainCompany.main_text_ru = mainCompany.main_text_ru.Replace(Environment.NewLine, "<br />");
            mainCompany.main_text_bel = mainCompany.main_text_bel.Replace(Environment.NewLine, "<br />");
            mainCompany.main_text_eng = mainCompany.main_text_eng.Replace("\n", "<br />");
            List<Departments> branchCompanies = db.Departments.Where(x => x.type_id != 1 && x.id < 21).ToList();
            foreach (Departments department in branchCompanies)
            {
                department.desc_ru = department.desc_ru.Replace(Environment.NewLine, "<br />");
                department.desc_bel = department.desc_bel.Replace(Environment.NewLine, "<br />");
                department.desc_eng = department.desc_eng.Replace("\n", "<br />");
            }
            List<Dep_types> depTypes = db.Dep_types.Where(x => x.id != 1).ToList();
            List<Certificates> certificates = db.Certificates.Where(x => x.Departments.type_id == 1).ToList();
            if (cookie != null && cookie.Value == "be")
            {
                mainCompany.adress_ru = mainCompany.adress_bel;
                mainCompany.main_text_ru = mainCompany.main_text_bel;
                mainCompany.name_ru = mainCompany.name_bel;
                mainCompany.short_name_ru = mainCompany.short_name_bel;
                ViewBag.MainCompany = mainCompany;
                foreach (Departments department in branchCompanies)
                {
                    department.adress_ru = department.adress_bel;
                    department.desc_ru = department.desc_bel;
                    department.name_ru = department.name_bel;
                    department.short_name_ru = department.short_name_bel;
                }
                ViewBag.BranchCompanies = branchCompanies;

                foreach (Dep_types depType in depTypes)
                {
                    depType.type = depType.type_bel;
                    foreach (Departments department in depType.Departments)
                    {
                        department.short_name_ru = department.short_name_bel;
                    }
                }
                ViewBag.DepTypes = depTypes;

                foreach (Certificates certificate in certificates)
                {
                    certificate.name = certificate.name_bel;
                }
                ViewBag.Certificates = certificates;
            }
            else if (cookie != null && cookie.Value == "en")
            {
                mainCompany.adress_ru = mainCompany.adress_eng;
                mainCompany.main_text_ru = mainCompany.main_text_eng;
                mainCompany.name_ru = mainCompany.name_eng;
                mainCompany.short_name_ru = mainCompany.short_name_eng;
                ViewBag.MainCompany = mainCompany;
                foreach (Departments department in branchCompanies)
                {
                    department.adress_ru = department.adress_eng;
                    department.desc_ru = department.desc_eng;
                    department.name_ru = department.name_eng;
                    department.short_name_ru = department.short_name_eng;
                }
                ViewBag.BranchCompanies = branchCompanies;

                foreach (Dep_types depType in depTypes)
                {
                    depType.type = depType.type_eng;
                    foreach (Departments department in depType.Departments)
                    {
                        department.short_name_ru = department.short_name_eng;
                    }
                }
                ViewBag.DepTypes = depTypes;

                foreach (Certificates certificate in certificates)
                {
                    certificate.name = certificate.name_eng;
                }
                ViewBag.Certificates = certificates;
            }
            else
            {
                ViewBag.MainCompany = mainCompany;
                ViewBag.BranchCompanies = branchCompanies;
                ViewBag.DepTypes = depTypes;
                ViewBag.Certificates = certificates;
            }

            return View();
        }
        [HttpGet]
        public ActionResult Contacts()
        {
            HttpCookie cookie = Request.Cookies["lang"];
            Departments department = db.Departments.FirstOrDefault(x => x.type_id == 1);
            Contacts pressa = db.Contacts.Where(x => x.id_dep == 3).FirstOrDefault(x => x.id_group == 3);

            ContactsInfo contact = new ContactsInfo();
            contact.Phone = department.Deps_to_groups.FirstOrDefault(x => x.id_group == 1).Contacts.phone;
            contact.Email = department.Deps_to_groups.FirstOrDefault(x => x.id_group == 1).Contacts.email;
            contact.Fax = department.Deps_to_groups.FirstOrDefault(x => x.id_group == 1).Contacts.fax;
            contact.Adress = department.adress_ru;
            List<Bosses> bosses = db.Bosses.Where(x => x.id_dep == 3).ToList();
            if (cookie != null && cookie.Value == "be")
            {
                contact.Adress = department.adress_bel;
                pressa.Deps_to_groups.Dep_groups.name = pressa.Deps_to_groups.Dep_groups.name_bel;
                foreach (Bosses boss in bosses)
                {
                    boss.name = boss.name_bel;
                    boss.post = boss.post_bel;
                    boss.meet_day = boss.meet_day_bel;
                }
            }
            if (cookie != null && cookie.Value == "en")
            {
                contact.Adress = department.adress_eng;
                pressa.Deps_to_groups.Dep_groups.name = pressa.Deps_to_groups.Dep_groups.name_eng;
                foreach (Bosses boss in bosses)
                {
                    boss.name = boss.name_eng;
                    boss.post = boss.post_eng;
                    boss.meet_day = boss.meet_day_eng;
                }
            }
            ViewBag.Bosses = bosses;
            ViewBag.Pressa = pressa;
            return View(contact);
        }
        [HttpGet]
        public ActionResult Career()
        {
            HttpCookie cookie = Request.Cookies["lang"];
            Contacts contact = db.Contacts.Where(x => x.id_dep == 3).FirstOrDefault(x => x.id_group == 2);
            if (cookie != null && cookie.Value == "be")
            {
                return View("CareerBel", contact);
            }
            else if (cookie != null && cookie.Value == "en")
            {
                return View("CareerEng", contact);
            }
            else
            {
                return View(contact);
            }

        }
        [HttpGet]
        public ActionResult Vacancies()
        {
            HttpCookie cookie = Request.Cookies["lang"];
            List<Departments> departments = db.Departments.Where(x => x.Vacancies.Count > 0).ToList();
            departments.Sort((x, y) => x.Vacancies.Count.CompareTo(y.Vacancies.Count));
            departments.Reverse();
            if (cookie != null && cookie.Value == "be")
            {
                foreach (Departments department in departments)
                {
                    department.adress_ru = department.adress_bel;
                    department.short_name_ru = department.short_name_bel;
                    foreach (Vacancies vacancy in department.Vacancies)
                    {
                        vacancy.vacancy_ru = vacancy.vacancy_bel;
                        vacancy.requirement_ru = vacancy.requirement_bel;
                    }
                }
            }
            return View(departments);
        }
        [HttpGet]
        public ActionResult News()
        {
            HttpCookie cookie = Request.Cookies["lang"];
            List<News> news = db.News.Where(x => x.News_type.id == 1 & x.Departments.id == 3).OrderByDescending(x => x.date).Take(50).ToList();
            news.Sort((x, y) => x.date.CompareTo(y.date));
            news.Reverse();
            foreach (News itemNews in news)
            {
                itemNews.body_ru = itemNews.body_ru.Replace(Environment.NewLine, "<br />");
                itemNews.body_bel = itemNews.body_bel.Replace(Environment.NewLine, "<br />");
            }
            if (cookie != null && cookie.Value == "be")
            {
                foreach (News itemNews in news)
                {
                    itemNews.title_ru = itemNews.title_bel;
                    itemNews.body_ru = itemNews.body_bel;
                }
            }
            ViewBag.News = news;
            return View();
        }
        [HttpGet]
        public ActionResult ShowNews(int id)
        {
            HttpCookie cookie = Request.Cookies["lang"];
            var news = db.News.Where(x => x.id == id).FirstOrDefault();
            if (cookie != null && cookie.Value == "be")
            {
                news.body_ru = news.body_ru.Replace(Environment.NewLine, "<br />");
                news.title_ru = news.title_bel;
                news.body_ru = news.body_bel;
            }
            return PartialView("_News", news);
        }
        [HttpGet]
        public ActionResult SocialNews()
        {
            HttpCookie cookie = Request.Cookies["lang"];
            List<News> news = db.News.Where(x => x.News_type.id == 2 && x.Departments.id == 3).ToList();
            news.Sort((x, y) => x.date.CompareTo(y.date));
            news.Reverse();
            foreach (News itemNews in news)
            {
                itemNews.body_ru = itemNews.body_ru.Replace(Environment.NewLine, "<br />");
                itemNews.body_bel = itemNews.body_bel.Replace(Environment.NewLine, "<br />");
            }
            if (cookie != null && cookie.Value == "be")
            {
                foreach (News itemNews in news)
                {
                    itemNews.title_ru = itemNews.title_bel;
                    itemNews.body_ru = itemNews.body_bel;
                }
            }
            ViewBag.News = news;
            return View();
        }
        [HttpGet]
        public ActionResult LifeSafetyNews()
        {
            HttpCookie cookie = Request.Cookies["lang"];
            List<News> news = db.News.Where(x => x.News_type.id == 3 & x.Departments.id == 3).ToList();
            news.Sort((x, y) => x.date.CompareTo(y.date));
            news.Reverse();
            foreach (News itemNews in news)
            {
                itemNews.body_ru = itemNews.body_ru.Replace(Environment.NewLine, "<br />");
                itemNews.body_bel = itemNews.body_bel.Replace(Environment.NewLine, "<br />");
            }
            if (cookie != null && cookie.Value == "be")
            {
                foreach (News itemNews in news)
                {
                    itemNews.title_ru = itemNews.title_bel;
                    itemNews.body_ru = itemNews.body_bel;
                }
            }
            ViewBag.News = news;
            return View();
        }

        public ActionResult AntiCorruptionNews()
        {
            HttpCookie cookie = Request.Cookies["lang"];
            List<News> news = db.News.Where(x => x.News_type.id == 4 & x.Departments.id == 3).ToList();
            news.Sort((x, y) => x.date.CompareTo(y.date));
            news.Reverse();
            foreach (News itemNews in news)
            {
                itemNews.body_ru = itemNews.body_ru.Replace(Environment.NewLine, "<br />");
                itemNews.body_bel = itemNews.body_bel.Replace(Environment.NewLine, "<br />");
            }
            if (cookie != null && cookie.Value == "be")
            {
                foreach (News itemNews in news)
                {
                    itemNews.title_ru = itemNews.title_bel;
                    itemNews.body_ru = itemNews.body_bel;
                }
            }
            ViewBag.News = news;
            return View();
        }

        public ActionResult RoadSafetyNews()
        {
            HttpCookie cookie = Request.Cookies["lang"];
            List<News> news = db.News.Where(x => x.News_type.id == 5 & x.Departments.id == 3).ToList();
            news.Sort((x, y) => x.date.CompareTo(y.date));
            news.Reverse();
            foreach (News itemNews in news)
            {
                itemNews.body_ru = itemNews.body_ru.Replace(Environment.NewLine, "<br />");
                itemNews.body_bel = itemNews.body_bel.Replace(Environment.NewLine, "<br />");
            }
            if (cookie != null && cookie.Value == "be")
            {
                foreach (News itemNews in news)
                {
                    itemNews.title_ru = itemNews.title_bel;
                    itemNews.body_ru = itemNews.body_bel;
                }
            }
            ViewBag.News = news;
            return View();
        }

        public ActionResult LegalEducationNews()
        {
            HttpCookie cookie = Request.Cookies["lang"];
            List<News> news = db.News.Where(x => x.News_type.id == 6 & x.Departments.id == 3).ToList();
            news.Sort((x, y) => x.date.CompareTo(y.date));
            news.Reverse();
            foreach (News itemNews in news)
            {
                itemNews.body_ru = itemNews.body_ru.Replace(Environment.NewLine, "<br />");
                itemNews.body_bel = itemNews.body_bel.Replace(Environment.NewLine, "<br />");
            }
            if (cookie != null && cookie.Value == "be")
            {
                foreach (News itemNews in news)
                {
                    itemNews.title_ru = itemNews.title_bel;
                    itemNews.body_ru = itemNews.body_bel;
                }
            }
            ViewBag.News = news;
            return View();
        }

        public ActionResult CurrentYearNews()
        {
            HttpCookie cookie = Request.Cookies["lang"];
            List<News> news = db.News.Where(x => x.News_type.id == 7 & x.Departments.id == 3).ToList();
            news.Sort((x, y) => x.date.CompareTo(y.date));
            news.Reverse();
            foreach (News itemNews in news)
            {
                itemNews.body_ru = itemNews.body_ru.Replace(Environment.NewLine, "<br />");
                itemNews.body_bel = itemNews.body_bel.Replace(Environment.NewLine, "<br />");
            }
            if (cookie != null && cookie.Value == "be")
            {
                foreach (News itemNews in news)
                {
                    itemNews.title_ru = itemNews.title_bel;
                    itemNews.body_ru = itemNews.body_bel;
                }
            }
            ViewBag.News = news;
            return View();
        }

        public ActionResult Objects()
        {
            HttpCookie cookie = Request.Cookies["lang"];
            List<Objects> objects = db.Objects.ToList();
            objects.Sort((x, y) => x.Departments.Count.CompareTo(y.Departments.Count));
            objects.Reverse();
            foreach (Objects obj in objects)
            {
                obj.title_ru = obj.title_ru.Replace(Environment.NewLine, "<br />");
                obj.title_bel = obj.title_bel.Replace(Environment.NewLine, "<br />");
                obj.title_eng = obj.title_eng.Replace("\n", "<br />");
            }
            if (cookie != null && cookie.Value == "be")
            {
                foreach (Objects obj in objects)
                {
                    obj.desc_ru = obj.desc_bel;
                    obj.title_ru = obj.title_bel;
                    foreach (Departments dep in obj.Departments)
                    {
                        dep.short_name_ru = dep.short_name_bel;
                    }
                }
            }
            if (cookie != null && cookie.Value == "en")
            {
                foreach (Objects obj in objects)
                {
                    obj.desc_ru = obj.desc_eng;
                    obj.title_ru = obj.title_eng;
                    foreach (Departments dep in obj.Departments)
                    {
                        dep.short_name_ru = dep.short_name_eng;
                    }
                }
            }
            ViewBag.Objects = objects;
            return View();
        }

        public ActionResult Software()
        {
            HttpCookie cookie = Request.Cookies["lang"];
            List<Objects> objects = db.Objects.ToList();
            objects.Sort((x, y) => x.Departments.Count.CompareTo(y.Departments.Count));
            objects.Reverse();
            foreach (Objects obj in objects)
            {
                obj.title_ru = obj.title_ru.Replace(Environment.NewLine, "<br />");
                obj.title_bel = obj.title_bel.Replace(Environment.NewLine, "<br />");
                obj.title_eng = obj.title_eng.Replace("\n", "<br />");
            }
            if (cookie != null && cookie.Value == "be")
            {
                foreach (Objects obj in objects)
                {
                    obj.desc_ru = obj.desc_bel;
                    obj.title_ru = obj.title_bel;
                    foreach (Departments dep in obj.Departments)
                    {
                        dep.short_name_ru = dep.short_name_bel;
                    }
                }
            }
            if (cookie != null && cookie.Value == "en")
            {
                foreach (Objects obj in objects)
                {
                    obj.desc_ru = obj.desc_eng;
                    obj.title_ru = obj.title_eng;
                    foreach (Departments dep in obj.Departments)
                    {
                        dep.short_name_ru = dep.short_name_eng;
                    }
                }
            }
            ViewBag.Objects = objects;
            return View();
        }

        [HttpGet]
        public ActionResult Bosses()
        {
            HttpCookie cookie = Request.Cookies["lang"];
            List<Bosses> bosses = db.Bosses.Where(x => x.Departments.type_id == 1).ToList();
            foreach (Bosses boss in bosses)
            {
                boss.desc = boss.desc.Replace(Environment.NewLine, "<br />");
                boss.desc_bel = boss.desc_bel.Replace(Environment.NewLine, "<br />");
                boss.desc_eng = boss.desc_eng.Replace("\n", "<br />");
            }
            if (cookie != null && cookie.Value == "be")
            {
                foreach (Bosses boss in bosses)
                {
                    boss.name = boss.name_bel;
                    boss.post = boss.post_bel;
                    boss.desc = boss.desc_bel;

                }
            }
            if (cookie != null && cookie.Value == "en")
            {
                foreach (Bosses boss in bosses)
                {
                    boss.name = boss.name_eng;
                    boss.post = boss.post_eng;
                    boss.desc = boss.desc_eng;

                }
            }
            return View(bosses);
        }



    }
}