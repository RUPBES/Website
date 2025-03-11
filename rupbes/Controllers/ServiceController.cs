using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using rupbes.Models;
using rupbes.Models.ViewModels;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using rupbes.Classes;
using rupbes.Models.Products;

namespace rupbes.Controllers
{    
    [Filters.Culture]
    public class ServiceController : Controller
    {
        Models.Database db = new Models.Database();
        private Models.DatabaseBes.Database db1 = new Models.DatabaseBes.Database();

        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }

        //УСЛУГИ
        [HttpGet]
        public ActionResult Services()
        {
            HttpCookie cookie = Request.Cookies["lang"];
            List<Services> services = db.Services.ToList();
            services.Sort((x, y) => x.title.CompareTo(y.title));
            if (cookie != null && cookie.Value == "be")
            {
                foreach (Services service in services)
                {
                    service.title = service.title_bel;
                }
            }

            return View(services);
        }
        [HttpPost]
        public ActionResult GetService(int id)
        {
            HttpCookie cookie = Request.Cookies["lang"];
            Services service = db.Services.Find(id);
            service.desc = service.desc.Replace(Environment.NewLine, "<br />");
            service.desc_bel = service.desc_bel.Replace(Environment.NewLine, "<br />");
            if (cookie != null && cookie.Value == "be")
            {
                service.title = service.title_bel;
                service.desc = service.desc_bel;
            }
            return PartialView("_GetService", service);
        }

        //Аренда
        [HttpGet]
        public ActionResult Realty()
        {
            HttpCookie cookie = Request.Cookies["lang"];
            List<Realty> realtys = db.Realty.ToList();
            realtys.Sort((x, y) => x.title.CompareTo(y.title));
            if (cookie != null && cookie.Value == "be")
            {
                foreach (Realty realty in realtys)
                {
                    realty.title = realty.title_bel;
                }
            }

            return View(realtys);
        }
        [HttpPost]
        public ActionResult GetRealty(int id)
        {
            HttpCookie cookie = Request.Cookies["lang"];
            Realty realty = db.Realty.Find(id);
            realty.desc = realty.desc.Replace(Environment.NewLine, "<br />");
            realty.desc_bel = realty.desc_bel.Replace(Environment.NewLine, "<br />");
            if (cookie != null && cookie.Value == "be")
            {
                realty.title = realty.title_bel;
                realty.desc = realty.desc_bel;
                realty.adress = realty.adress_bel;
            }
            return PartialView("_GetRealty", realty);
        }

        //Продажа имущества
        [HttpGet]
        public ActionResult Sale()
        {
            HttpCookie cookie = Request.Cookies["lang"];
            List<Sale> sales = db.Sale.ToList();
            sales.Sort((x, y) => x.title.CompareTo(y.title));
            if (cookie != null && cookie.Value == "be")
            {
                foreach (Sale sale in sales)
                {
                    sale.title = sale.title_bel;
                }
            }

            return View(sales);
        }
        [HttpPost]
        public ActionResult GetSale(int id)
        {
            HttpCookie cookie = Request.Cookies["lang"];
            Sale sale = db.Sale.Find(id);
            sale.desc = sale.desc.Replace(Environment.NewLine, "<br />");
            sale.desc_bel = sale.desc_bel.Replace(Environment.NewLine, "<br />");
            if (cookie != null && cookie.Value == "be")
            {
                sale.title = sale.title_bel;
                sale.desc = sale.desc_bel;
                sale.adress = sale.adress_bel;
            }
            return PartialView("_GetSale", sale);
        }

        //Техника
        [HttpGet]
        public ActionResult Mechs()
        {
            HttpCookie cookie = Request.Cookies["lang"];
            List<Mechanisms> mechs = db.Mechanisms.ToList();
            mechs.Sort((x, y) => x.title.CompareTo(y.title));
            if (cookie != null && cookie.Value == "be")
            {
                foreach (Mechanisms mech in mechs)
                {
                    mech.title = mech.title_bel;
                }
            }

            return View(mechs);
        }
        [HttpPost]
        public ActionResult GetMech(int id)
        {
            HttpCookie cookie = Request.Cookies["lang"];
            Mechanisms mech = db.Mechanisms.Find(id);
            mech.desc = mech.desc.Replace(Environment.NewLine, "<br />");
            mech.desc_bel = mech.desc_bel.Replace(Environment.NewLine, "<br />");
            if (cookie != null && cookie.Value == "be")
            {
                mech.title = mech.title_bel;
                mech.desc = mech.desc_bel;
            }
            return PartialView("_GetMech", mech);
        }

        //Товары
        [HttpGet]
        public ActionResult Products()
        {
            return View(db.GroupProducts.ToList());
        }

        [HttpGet]
        public ActionResult SelectGroupProduct(int id)
        {
            return PartialView("_SelectGroupProduct", db.SubGroupProducts.Where(x => x.GroupProductId == id).ToList());
        }

        [HttpGet]
        public ActionResult SelectSubGroupProduct(int id)
        {
            var productsView = new List<ProductViewModel>();
            var products = db.Products.Where(x => x.SubGroupProductId == id).ToList();
            foreach (var item in products)
            {
                var itemView = new ProductViewModel();
                itemView.id = item.id;
                itemView.name = item.name;
                itemView.codeTNVD = item.codeTNVD;
                itemView.unitName = item.Unit.name;
                itemView.note = item.note;
                itemView.departmentName = item.Departments.short_name_ru;
                itemView.properties = new List<PropertyViewModel>();
                foreach (var prop in db.PropertyProducts.Where(x => x.ProductId == item.id))
                {
                    var property = db.Properties.Where(x => x.id == prop.PropertyId).FirstOrDefault();
                    itemView.properties.Add(new PropertyViewModel { name = property.name, value = prop.value });
                }         
                itemView.components = new List<ComponentViewModel>();
                foreach (var components in db.ComponentProducts.Where(x => x.ProductId == item.id))
                {
                    var component = db.Components.Where(x => x.id == components.ComponentId).FirstOrDefault();
                    itemView.components.Add(new ComponentViewModel { name = component.name});
                }              
                productsView.Add(itemView);
            }
            return PartialView("_SelectSubGroupProduct", productsView);
        }

        [HttpGet]
        public ActionResult ShowVersionProduct(int id)
        {
            var modelView = new List<VersionProductViewModel>();
            var model = db.VersionProducts.Where(x => x.ProductId == id && x.isSale).ToList();
            foreach (var item in model)
            {
                var itemView = new VersionProductViewModel();
                itemView.name = item.name;
                itemView.note = item.note;
                itemView.properties = new List<PropertyViewModel>();
                foreach (var prop in db.PropertyVersions.Where(x => x.VersionId == item.id))
                {
                    var property = db.Properties.Where(x => x.id == prop.PropertyId).FirstOrDefault();
                    itemView.properties.Add(new PropertyViewModel { name = property.name, value = prop.value });
                }
                itemView.Imgs = new List<Imgs>();
                foreach (var imgToVersion in db.ImgsVersionProduct.Where(x => x.VersionProductId == item.id))
                {
                    var img = db.Imgs.Where(x => x.id == imgToVersion.ImgsId).FirstOrDefault();
                    itemView.Imgs.Add(img);
                }
                modelView.Add(itemView);
            }
            var product = db.Products.Where(x => x.id == id).FirstOrDefault();
            var imgs = new List<Imgs>();
            foreach (var imgsToProduct in db.ImgsProduct.Where(x => x.ProductId == id))
            {
                var img = db.Imgs.Where(x => x.id == imgsToProduct.ImgsId).FirstOrDefault();
                imgs.Add(img);
            }
            ViewBag.imgs = imgs;
            ViewData["productName"] = product.name;
            ViewData["codeTNVED"] = product.codeTNVD;
            ViewData["unitName"] = product.Unit.name;
            ViewData["departmentName"] = product.Departments.short_name_ru;
            var productProperties = new List<PropertyViewModel>();
            foreach (var prop in db.PropertyProducts.Where(x => x.ProductId == product.id))
            {
                var property = db.Properties.Where(x => x.id == prop.PropertyId).FirstOrDefault();
                productProperties.Add(new PropertyViewModel { name = property.name, value = prop.value });
            }
            ViewBag.properties = productProperties;
            var components = new List<Component>();
            foreach (var compProduct in db.ComponentProducts.Where(x => x.ProductId == product.id))
            {
                var comp = db.Components.Where(x => x.id == compProduct.ComponentId).FirstOrDefault();
                components.Add(new Component { name = comp.name });
            }
            ViewBag.components = components;            
            return PartialView("_ShowVersionProduct", modelView);
        }

        [HttpGet]
        public ActionResult GetProductByNameOrCode(string search)
        {
            List<ProductViewModel> answer = new List<ProductViewModel>();
            List<List<ProductViewModel>> list = new List<List<ProductViewModel>>();
            var productsView = new List<ProductViewModel>();
            #region По названию
            /// Убирает пробелы лишние между словами
            Regex space = new Regex("  ");
            string sRegex = search.TrimStart();
            while (true)
            {
                if (sRegex.Contains("  "))
                {
                    sRegex = space.Replace(sRegex, " ");
                }
                else { break; }
            }
            //делит по спец знаками строку
            List<string> masWord = sRegex.Split(' ', '-', '+', '.', '/', '(', ')').ToList();

            for (int i = 0; i < masWord.Count(); i++)
            {
                var masWordElement = masWord[i].ToLower();
                var ek = db.Products.Where(p => p.name.ToLower().Contains(masWordElement)).ToList();
                productsView = new List<ProductViewModel>();
                foreach (var item in ek)
                {
                    var itemView = new ProductViewModel();
                    itemView.id = item.id;
                    itemView.name = item.name;
                    itemView.codeTNVD = item.codeTNVD;
                    itemView.unitName = item.Unit.name;
                    itemView.note = item.note;
                    itemView.departmentName = item.Departments.short_name_ru;
                    itemView.properties = new List<PropertyViewModel>();
                    foreach (var prop in db.PropertyProducts.Where(x => x.ProductId == item.id))
                    {
                        var property = db.Properties.Where(x => x.id == prop.PropertyId).FirstOrDefault();
                        itemView.properties.Add(new PropertyViewModel { name = property.name, value = prop.value });
                    }
                    itemView.components = new List<ComponentViewModel>();
                    foreach (var components in db.ComponentProducts.Where(x => x.ProductId == item.id))
                    {
                        var component = db.Components.Where(x => x.id == components.ComponentId).FirstOrDefault();
                        itemView.components.Add(new ComponentViewModel { name = component.name });
                    }
                    productsView.Add(itemView);
                }
                list.Add(productsView);
            }

            // Ищет совпадения между 1/2 подстроками и остальными
            for (int i = 1; i < masWord.Count(); i++)
            {
                answer = list[0];
                answer = answer.Intersect(list[i]).ToList();   
            }
            #endregion

            #region Код ТН ВЕД
            var listCodeTNVED = db.Products.Where(p => p.codeTNVD.Contains(sRegex)).ToList();
            productsView = new List<ProductViewModel>();
            foreach (var item in listCodeTNVED)
            {
                var itemView = new ProductViewModel();
                itemView.id = item.id;
                itemView.name = item.name;
                itemView.codeTNVD = item.codeTNVD;
                itemView.unitName = item.Unit.name;
                itemView.note = item.note;
                itemView.departmentName = item.Departments.short_name_ru;
                itemView.properties = new List<PropertyViewModel>();
                foreach (var prop in db.PropertyProducts.Where(x => x.ProductId == item.id))
                {
                    var property = db.Properties.Where(x => x.id == prop.PropertyId).FirstOrDefault();
                    itemView.properties.Add(new PropertyViewModel { name = property.name, value = prop.value });
                }
                itemView.components = new List<ComponentViewModel>();
                foreach (var components in db.ComponentProducts.Where(x => x.ProductId == item.id))
                {
                    var component = db.Components.Where(x => x.id == components.ComponentId).FirstOrDefault();
                    itemView.components.Add(new ComponentViewModel { name = component.name });
                }
                productsView.Add(itemView);
            }
            list.Add(productsView);
            #endregion

            //Добавляет остальные возможные варианты
            for (int i = 0; i < list.Count(); i++)
            {
                answer = answer.Union(list[i]).ToList();
            }
            return PartialView("_SelectSubGroupProduct", answer);
        }
    }
}