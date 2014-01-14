using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SteamKit2;
using Steam_Listener.Models;
using Steam_Listener.utils;
using System.IO;
using System.Threading;

namespace Steam_Listener.lib
{
    public class SteamAuthHandler
    {
        static SteamClient steamClient { get; set; }
        static CallbackManager manager;
        static SteamUser steamUser { get; set; }
        static bool isRunning;
        static User user { get; set; }
        static bool isConnected { get; set; }

        public SteamAuthHandler(SteamClient client, User userModel)
        {
            steamClient = client;
            user = userModel;
            init();
        }

        public static void init()
        {
            manager = new CallbackManager(steamClient);

            // get the steamuser handler, which is used for logging on after successfully connecting
            steamUser = steamClient.GetHandler<SteamUser>();

            // register a few callbacks we're interested in
            // these are registered upon creation to a callback manager, which will then route the callbacks
            // to the functions specified
            new Callback<SteamClient.ConnectedCallback>(OnConnected, manager);
            new Callback<SteamClient.DisconnectedCallback>(OnDisconnected, manager);

            new Callback<SteamUser.LoggedOnCallback>(OnLoggedOn, manager);
            new Callback<SteamUser.LoggedOffCallback>(OnLoggedOff, manager);

            // this callback is triggered when the steam servers wish for the client to store the sentry file
            new JobCallback<SteamUser.UpdateMachineAuthCallback>(OnMachineAuth, manager);
        }

        public bool ConnState()
        {

            return isConnected;
        }

        public void connect()
        {

            isRunning = true;

            Logs.Log("SteamAuth", "Connecting to Steam...");

            // initiate the connection
            steamClient.Connect();

            // create our callback handling loop
            while (isRunning)
            {
                // in order for the callbacks to get routed, they need to be handled by the manager
                manager.RunWaitCallbacks(TimeSpan.FromSeconds(1));
            }
        }


        static void OnConnected(SteamClient.ConnectedCallback callback)
        {
            if (callback.Result != EResult.OK)
            {
                Logs.Log("SteamAuth", "Unable to connect to Steam: " + callback.Result);

                isRunning = false;
                return;
            }

            Logs.Log("SteamAuth", "Connected to Steam! Logging in " + user.userName + "...");

            byte[] sentryHash = null;
            if (File.Exists("sentry.bin"))
            {
                // if we have a saved sentry file, read and sha-1 hash it
                byte[] sentryFile = File.ReadAllBytes("sentry.bin");
                sentryHash = CryptoHelper.SHAHash(sentryFile);
            }

            steamUser.LogOn(new SteamUser.LogOnDetails
            {
                Username = user.userName,
                Password = user.userPass,

                // in this sample, we pass in an additional authcode
                // this value will be null (which is the default) for our first logon attempt
                AuthCode = user.authCode,

                // our subsequent logons use the hash of the sentry file as proof of ownership of the file
                // this will also be null for our first (no authcode) and second (authcode only) logon attempts
                SentryFileHash = sentryHash,
            });
        }

        static void OnDisconnected(SteamClient.DisconnectedCallback callback)
        {
            // after recieving an AccountLogonDenied, we'll be disconnected from steam
            // so after we read an authcode from the user, we need to reconnect to begin the logon flow again

            Logs.Log("SteamAuth","Disconnected from Steam, reconnecting in 5...");

            Thread.Sleep(TimeSpan.FromSeconds(5));

            steamClient.Connect();
        }

        static void OnLoggedOn(SteamUser.LoggedOnCallback callback)
        {
            if (callback.Result == EResult.AccountLogonDenied)
            {
                Logs.Log("Steam", "Unable to logon to Steam: This account is SteamGuard protected.");
                Logs.Log("SteamAuth","Auth code sent to the email at: ", callback.EmailDomain);
                return;
            }

            if (callback.Result != EResult.OK)
            {
                Logs.Log("SteamAuth", "Unable to logon to Steam: " + callback.ExtendedResult + " - This account may be SteamGuard protected");

                isRunning = false;
                return;
            }

            Logs.Log("SteamAuth", "Successfully logged on!");
            isConnected = true;
            isRunning = false;

            // at this point, we'd be able to perform actions on Steam
        }

        static void OnLoggedOff(SteamUser.LoggedOffCallback callback)
        {
            Logs.Log("SteamAuth", "Logged off of Steam: " + callback.Result);
            isRunning = false;
        }

        static void OnMachineAuth(SteamUser.UpdateMachineAuthCallback callback, JobID jobId)
        {
            Logs.Log("SteamAuth","Updating sentryfile...");

            byte[] sentryHash = CryptoHelper.SHAHash(callback.Data);

            // write out our sentry file
            // ideally we'd want to write to the filename specified in the callback
            // but then this sample would require more code to find the correct sentry file to read during logon
            // for the sake of simplicity, we'll just use "sentry.bin"
            File.WriteAllBytes("sentry.bin", callback.Data);

            // inform the steam servers that we're accepting this sentry file
            steamUser.SendMachineAuthResponse(new SteamUser.MachineAuthDetails
            {
                JobID = jobId,

                FileName = callback.FileName,

                BytesWritten = callback.BytesToWrite,
                FileSize = callback.Data.Length,
                Offset = callback.Offset,

                Result = EResult.OK,
                LastError = 0,

                OneTimePassword = callback.OneTimePassword,

                SentryFileHash = sentryHash,
            });

            isRunning = false;
        }
    }
}
