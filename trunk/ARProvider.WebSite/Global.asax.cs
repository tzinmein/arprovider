using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Castle.ActiveRecord;
using Castle.ActiveRecord.Framework.Config;

namespace BeechtreeTech.ARProviderExample.WebSite
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                "Default",                                              // Route name
                "{controller}/{action}/{id}",                           // URL with parameters
                new { controller = "Home", action = "Index", id = "" }  // Parameter defaults
            );

        }

        protected void Application_Start()
        {
            RegisterRoutes(RouteTable.Routes);

            //FileInfo logFile = new FileInfo(Server.MapPath("Log4Net.Config"));
            //log4net.Config.XmlConfigurator.Configure(logFile);

            XmlConfigurationSource source = new XmlConfigurationSource(Server.MapPath("~/ar.config"));
            ActiveRecordStarter.Initialize(Assembly.Load("BeechtreeTech.ARProviderExample.BLL"), source);

            ActiveRecordStarter.SetSchemaDelimiter("\n GO \n");
            ActiveRecordStarter.GenerateCreationScripts(Server.MapPath("~/BeechtreeTech.ARProviderExample.sql"));
        }
    }
}