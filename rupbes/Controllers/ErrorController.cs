using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace rupbes.Controllers
{
    public class ErrorController : Controller
    {
        public ActionResult PageNotFound()
        {
            Response.StatusCode = 404;
            ViewBag.Title = "Ошибка 404";
            ViewBag.Text = "Запрашиваемая вами страница не найдена!";
            return View("Index");
        }

        public ActionResult Index()
        {
            Response.StatusCode = 500;
            ViewBag.Title = "Ошибка 500";
            ViewBag.Text = "Произошла ошибка сервера!";
            return View("Index");
        }
    }
}