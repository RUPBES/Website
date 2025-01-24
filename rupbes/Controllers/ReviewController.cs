using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using BotDetect.Web.Mvc;
using rupbes.Models.ViewModels.Review;
using rupbes.Models.DatabaseBes;
using System.Linq;

namespace rupbes.Controllers
{
    [Filters.Culture]
    public class ReviewController : Controller
    {
        private Database db = new Database();
        public IndexModel indexModel { get; set; }
        private static int IdCompany { get; set; }
        private static string ReturnURL { get; set; }


        [HttpGet]
        public ActionResult Reviews()
        {
            HttpCookie cookie = Request.Cookies["lang"];
            if (cookie != null && cookie.Value == "be")
            {
                ViewBag.Title = Resources.Resource.ReviewTitle;
            }
            else
            {
                ViewBag.Title = Resources.Resource.ReviewTitle;
            }

            List<CompanyReview> reviews = db.CompanyReviews.Where(x => x.Confirm == true).ToList();
            List<ReviewsModel> reviewModels = new List<ReviewsModel>();

            foreach (CompanyReview item in reviews)
            {
                ReviewsModel model = new ReviewsModel();
                model.ReviewText = item.ReviewText;       
                model.DateReview = item.DateReview.ToShortDateString();
                model.Filial = db.Companies.Find(item.IdCompany);
                                
                switch (item.IdCompany)
                {
                    case 9:
                        {
                            model.LinkToFilialSite = "https://rupbes.by/";
                            break;
                        }
                    case 10:
                        {
                            model.LinkToFilialSite = "https://bem.rupbes.by/";
                            break;
                        }
                    case 11:
                        {
                            model.LinkToFilialSite = "https://besi.rupbes.by/";
                            break;
                        }
                    case 12:
                        {
                            model.LinkToFilialSite = "https://besm.rupbes.by/";
                            break;
                        }
                    case 13:
                        {
                            model.LinkToFilialSite = "https://betss.rupbes.by/";
                            break;
                        }
                    case 14:
                        {
                            model.LinkToFilialSite = "https://ges.rupbes.by/";
                            break;
                        }
                    case 15:
                        {
                            model.LinkToFilialSite = "https://sutec2.rupbes.by/";
                            break;
                        }
                    case 16:
                        {
                            model.LinkToFilialSite = "https://ustec5.rupbes.by/";
                            break;
                        }
                    case 17:
                        {
                            model.LinkToFilialSite = "https://usbelaes.rupbes.by/";
                            break;
                        }
                }
                    reviewModels.Add(model);
            }
            //Сортировка списка отзывов по дате (новые будут в самом верху)
            List<ReviewsModel> list = reviewModels.OrderByDescending(x => DateTime.Parse(x.DateReview)).ToList();
            return View(list);
        }


        [HttpGet]
        public ActionResult Index(int? id, string returnUrl = "")
        {
            indexModel = new IndexModel();

            if(id != null || returnUrl != "")
            {
                if(id == 9)
                {
                    IdCompany = 9;
                    ReturnURL = "https://rupbes.by/Review/Reviews";
                }
                else
                {
                    IdCompany = (int)id;
                    ReturnURL = "https://" + returnUrl + "/Review/Reviews";
                }                
            }
            else
            {
                IdCompany = 9;
                ReturnURL = "https://rupbes.by/Review/Reviews";
            }            

            HttpCookie cookie = Request.Cookies["lang"];
            if (cookie != null && cookie.Value == "be")
            {
                ViewBag.Title = Resources.Resource.ReviewTitle;
            }
            else if (cookie != null && cookie.Value == "en")
            {
                ViewBag.Title = Resources.Resource.ReviewTitle;
            }
            else
            {
                ViewBag.Title = Resources.Resource.ReviewTitle;
            }

            return View(indexModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [CaptchaValidation("CaptchaCode", "Captcha", "Incorrect CAPTCHA code!")]
        public ActionResult Index(IndexModel model)
        {            
            ViewBag.CaptchaMessage = "";                    // Сообщение о валидности каптчи          
            
            if (!ModelState.IsValid)
            {
                MvcCaptcha.ResetCaptcha("Captcha");

                HttpCookie cookie = Request.Cookies["lang"];
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
                return View(model);
            }
            else
            {                
                MvcCaptcha.ResetCaptcha("Captcha");
                
                CompanyReview companyReview = new CompanyReview();
                try
                {
                    companyReview.Company = db.Companies.Find(IdCompany);
                }
                catch(Exception e)
                {
                    return RedirectToAction("Reviews", "Home");
                }                              

                companyReview.ReviewText = model.ReviewText;
                DateTime dateNow = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
                companyReview.DateReview = dateNow;

                db.CompanyReviews.Add(companyReview);
                db.SaveChanges();
                
            }

            return View("_SendReviewMessage");
        }

        //Перенаправление по указанной ссылке
        public ActionResult RedirectToURL()
        {
            return Redirect(ReturnURL);
        }

    }
}