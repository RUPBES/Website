using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using rupbes.Jobs.TendersLoader;
namespace rupbes
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            //запуск триггера, добавляющего тендеры в базу
            UpdateSheduler.Start();
        }
        private const int MyMaxContentLength = 2097152; //Wathever you want to accept as max file.
        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            if ((Request.FilePath=="/OneWindow/Person" || Request.FilePath== "/OneWindow/Legal") && Request.HttpMethod == "POST"
               && Request.ContentLength > MyMaxContentLength)
            {
                Response.Redirect(Request.FilePath+"?message=Error");
            }
        }
    }
}
