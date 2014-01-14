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
    public class SteamAppHandler
    {
        static SteamClient steamClient { get; set; }
        static CallbackManager manager;
        static bool isRunning;
        static User user { get; set; }
        public static SteamApps Apps { get; private set; }
        public static uint PreviousChange { get; set; }
        public System.Timers.Timer Timer { get; private set; }
        public static List<App> ChangedApps { get; private set; }
        public static int currChangedAppKeys { get; private set; }
        public static int currChangedPackageKeys { get; private set; }
        private static int lastProc { get; set; }
        private static int AIcount { get; set; }
        private static int currProc { get; set; }
        public static int timerInterval { get; set; }

        public SteamAppHandler(SteamClient client, User userModel)
        {
            steamClient = client;
            user = userModel;
            PreviousChange = 1;
            init();

            Timer = new System.Timers.Timer();
            Timer.Elapsed += OnTimer;
            Timer.Interval = Settings.TimerInterval;
        }

        public static void init()
        {

            manager = new CallbackManager(steamClient);
            Apps = steamClient.GetHandler<SteamApps>();
            ChangedApps = new List<App>();

            manager.Register(new Callback<SteamApps.LicenseListCallback>(OnLicenseListCallback));
            manager.Register(new JobCallback<SteamApps.PICSProductInfoCallback>(OnPICSProductInfo));
            manager.Register(new JobCallback<SteamApps.PICSChangesCallback>(OnPICSChanges));
            manager.Register(new JobCallback<SteamApps.AppInfoCallback>(OnAppInfoCallback));
        }

        public void start()
        {
            Logs.Log("SteamApps", "Starting cycle..");
            isRunning = true;

            Timer.Start();

            while (isRunning)
            {
                manager.RunWaitCallbacks(TimeSpan.FromSeconds(1));
            }
        }

        private static void OnLicenseListCallback(SteamApps.LicenseListCallback licenseList)
        {
            if (licenseList.Result != EResult.OK)
            {
                Logs.Log("Steam", "Unable to get license list: " + licenseList.Result);

                return;
            }

            Logs.Log("SteamApps", "Licenses Count: " + licenseList.LicenseList.Count + " " + string.Join(", ", licenseList.LicenseList.Select(lic => lic.PackageID)));
        }

        public void GetPICSChanges()
        {
            Apps.PICSGetChangesSince(PreviousChange, true, true);
        }

        private void OnTimer(object sender, System.Timers.ElapsedEventArgs e)
        {
            var total = currChangedAppKeys + currChangedPackageKeys;
            currProc = ChangedApps.Count;

            if (currChangedAppKeys == 0 && currProc == 0)
            {
                lastProc = ChangedApps.Count;
                Logs.Log("SteamApps", "Running update cycle..");
                GetPICSChanges();

            }

            else if (currProc + currChangedPackageKeys == total)
            {
                Logs.Log("SteamApps", "Completed Processing AppCycle.");
                var apps = new List<App>(ChangedApps);
                ChangedApps.Clear();
                var http = new SteamHttpClient();
                http.process(apps);
                currChangedAppKeys = 0;
                currChangedPackageKeys = 0;
                AIcount = 0;
                currProc = 0;
                lastProc = 0;
            }
            else
            {
                if (currProc == lastProc && currProc > 0 && lastProc > 0)
                {
                    AIcount++;
                    Logs.Log("SteamApps", "App processing queue appears to be stuck at " + ChangedApps.Count + " out of " + currChangedAppKeys + " (" + AIcount + ")");

                }

                if (AIcount == 3)
                {

                    currChangedAppKeys = 0;
                    currChangedPackageKeys = 0;
                    AIcount = 0;
                    currProc = 0;
                    lastProc = 0;
                    Logs.Log("SteamApps", "AppCycle seems to have reached maximum, preparing to send gathered data.");
                    var apps = new List<App>(ChangedApps);
                    ChangedApps.Clear();
                    var http = new SteamHttpClient();
                    http.process(apps);
                    
                    return;
                }
                // Cycle isn't done yet
                Logs.Log("SteamApps", "Processing.. " + currProc + " out of " + currChangedAppKeys);
                lastProc = currProc;
            }
        }

        private static void OnPICSChanges(SteamApps.PICSChangesCallback callback, JobID job)
        {
            if (PreviousChange == callback.CurrentChangeNumber)
            {
                Logs.Log("SteamApps", "All titles are up to date.");
                return;
            }

            Logs.Log("SteamApps", "Changelist " + PreviousChange + " -> " + callback.CurrentChangeNumber + " . Changed Apps: " + callback.AppChanges.Count + " Changed Packages: " + callback.PackageChanges.Count);
            Logs.Log("SteamApps", "Processing apps..");
            PreviousChange = callback.CurrentChangeNumber;

            Apps.PICSGetProductInfo(callback.AppChanges.Keys, callback.PackageChanges.Keys, false, false);

            if (callback.AppChanges.Count > 0)
            {
                // changes
                currChangedAppKeys = callback.AppChanges.Count;
            }

            if (callback.PackageChanges.Count > 0)
            {
                currChangedPackageKeys = callback.PackageChanges.Count;
            }
        }

        private static void OnPICSProductInfo(SteamApps.PICSProductInfoCallback callback, JobID jobID)
        {

            foreach (var app in callback.Apps)
            {
                // Logs.Log("SteamApps", "App Info: " + app.Key);
                Apps.GetAppInfo(app.Key, false);

            }

            foreach (var package in callback.Packages)
            {
                //  Logs.Log("SteamApps", "SubID: " + package.Key);

            }

        }

        private static void OnAppInfoCallback(SteamApps.AppInfoCallback callback, JobID jobID)
        {
            if (callback.Apps.Count > 0 && callback.Apps[0].Sections != null)
            {
                var info = new Dictionary<string, string>();
                foreach (var kvp in callback.Apps[0].Sections.Values)
                {
                    var temp = kvp.Children.ToArray()[0];
                    info.Add(temp.Name, temp.Value);
                }
                ChangedApps.Add(new App(callback.Apps[0].AppID, callback.Apps[0].ChangeNumber, info));
            }
            else
            {
                ChangedApps.Add(new App(callback.Apps[0].AppID, callback.Apps[0].ChangeNumber, null));
            }

        }


    }
}