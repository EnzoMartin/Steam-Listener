using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Steam_Listener.Models;
using Steam_Listener.lib;
using Steam_Listener.utils;
using Microsoft.WindowsAzure;

namespace Steam_Listener
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);


            Logs.initialize();

            var listenUser = new User();

           if(!String.IsNullOrEmpty(CloudConfigurationManager.GetSetting("ENVIRONMENT"))) {
                // Get the configuration data from Windows Azure
                listenUser.userName = CloudConfigurationManager.GetSetting("STEAM_USER");
                listenUser.userPass = CloudConfigurationManager.GetSetting("STEAM_PASSWORD");
                listenUser.authCode = CloudConfigurationManager.GetSetting("STEAM_GUARD");

                HttpSettings.URL = CloudConfigurationManager.GetSetting("ENDPOINT_URL");
                HttpSettings.secret = CloudConfigurationManager.GetSetting("LISTENER_SECRET");

                int appsPerRequest;
                if(!int.TryParse(CloudConfigurationManager.GetSetting("APPS_PER_REQUEST"), out appsPerRequest))
                {
                    appsPerRequest = 50;
                }
                HttpSettings.AppsPerRequest = appsPerRequest;

                Settings.TimerInterval = int.Parse(CloudConfigurationManager.GetSetting("TIMER_INTERVAL"));
 
           } else {
                // Manually Fill out this data for testing  
                listenUser.userName = "";
                listenUser.userPass = "";
                listenUser.authCode = "";  // sent by email on first SteamGuard protected logon

                HttpSettings.URL = "";
                HttpSettings.secret = "";
                HttpSettings.AppsPerRequest = 50; // default

                Settings.TimerInterval = 30000; // default

            }

            SteamListener.user = listenUser;
            SteamListener.init();
        }
    }
}