using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SteamKit2;
using Steam_Listener.Models;
using Steam_Listener.utils;
using System.Threading;

namespace Steam_Listener.lib
{
    public static class SteamListener
    {
        public static SteamClient Client { get; private set; }
        public static SteamAppHandler steamApps;
        public static User user { get; set; }
        public static SteamAuthHandler steamAuth;

        public static void init()
        {
            Client = new SteamClient();
            steamAuth = new SteamAuthHandler(Client, user);
            steamApps = new SteamAppHandler(Client, user);


            if (Connect())
            {
                Logs.Log("SteamListener", "Connected.");
                Thread t = new Thread(new ThreadStart(getApps));
                t.Start();
            }
            else { Logs.Log("SteamListener", "Not Connected."); }

        }

        public static bool Connect()
        {
            steamAuth.connect();

            return steamAuth.ConnState();
        }

        public static void getApps()
        {

            steamApps.start();
            steamApps.GetPICSChanges();

        }

    }
}