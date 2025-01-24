using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using rupbes.Models.DatabaseBes;
using rupbes.Classes;

namespace rupbes.Controllers
{
    [Filters.Culture]
    public class PurchasesController : Controller
    {
        private static Database _db = new Database();

        [HttpGet]
        public ActionResult Purchases(int? page1, int? page2, int? page3)
        {
            var tenders = from tender in _db.Tenders select tender;

            tenders = tenders.OrderBy(t => t.icetrade_id);

            int pageSize = 5;            

            PurchasesViewModel purchasesViewModel = new PurchasesViewModel();            
            purchasesViewModel.TendersList = new PaginatedList<Tenders>(tenders, page2 ?? 1, pageSize);            

            return View(purchasesViewModel);
        }
    }
}