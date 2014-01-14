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

            var testUser = new User();

           switch(CloudConfigurationManager.GetSetting("ENVIRONMENT")) {

                case "azure": 
                // Get the configuration data from Windows Azure
                testUser.userName = CloudConfigurationManager.GetSetting("STEAM_USER");
                testUser.userPass = CloudConfigurationManager.GetSetting("STEAM_PASSWORD");
                testUser.authCode = "";  // sent by email on first SteamGuard protected logon

                HttpSettings.URL = CloudConfigurationManager.GetSetting("ENDPOINT_URL");
                HttpSettings.secret = CloudConfigurationManager.GetSetting("LISTENER_SECRET");
                HttpSettings.AppsPerRequest = int.Parse(CloudConfigurationManager.GetSetting("APPS_PER_REQUEST")); // default: 50

                break;

                default:
                // Manually Fill out this data for testing  
                testUser.userName = "";
                testUser.userPass = "";
                testUser.authCode = "";  // sent by email on first SteamGuard protected logon

                HttpSettings.URL = "";
                HttpSettings.secret = "";
                HttpSettings.AppsPerRequest = 50; // default

                break;

            }


            SteamListener.user = testUser;
            SteamListener.init();


        }
    }
}