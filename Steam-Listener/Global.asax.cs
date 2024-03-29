﻿using System;
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

                int sendInterval;
                if (!int.TryParse(CloudConfigurationManager.GetSetting("SEND_INTERVAL"), out sendInterval))
                {
                    sendInterval = 50;
                }
                Settings.SendInterval = sendInterval;

                int timerInterval;
                if (!int.TryParse(CloudConfigurationManager.GetSetting("TIMER_INTERVAL"), out timerInterval))
                {
                    timerInterval = 20000;
                }
                Settings.TimerInterval = timerInterval;
           } else {
                // Manually Fill out this data for testing  
                listenUser.userName = "";
                listenUser.userPass = "";
                listenUser.authCode = "";  // sent by email on first SteamGuard protected logon

                HttpSettings.URL = "";
                HttpSettings.featureClearURL = ""; // api endpoint for the Feature Clear/Update URL
                HttpSettings.secret = "";
                HttpSettings.AppsPerRequest = 50; // default

                Settings.SendInterval = 1000; //default
                Settings.TimerInterval = 15000; // default
                Settings.startRevision = 494063; // Change/Revision number to start from (uses Steam internal revision numbers) i.e. 494063

            }

            SteamListener.user = listenUser;
            SteamListener.init();
        }
    }
}