﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.AspNet.SignalR; 

namespace Steam_Listener.utils
{
    public class Logs
    {
        /// <summary>
        /// The StreamWriter handling all the IO writing to the Log file
        /// </summary>
        private static StreamWriter stream;

        /// <summary>
        /// lock object for thread safety
        /// </summary>
        private static object lockObj = new object();


        private static List<string> loghistory;

        /// <summary>
        /// Creates the Log.txt file if it does not exist, effectively instantiating the Log-class
        /// </summary>
        public static void initialize()
        {

            loghistory = new List<string>();
          /*  if (File.Exists("Log.txt"))
            {
                try
                {
                    Log("---------- New Session ----------");
                    return;
                }
                catch (IOException e)
                {
                    Console.Write(e.Message);
                }
            }
            try
            {
                FileStream fs = File.Create("Log.txt");
                fs.Close();
                Log("---------- New Session ----------");
            }
            catch (IOException e)
            {
                Console.Write(e.Message);
            } */
        }

        /// <summary>
        /// Writes a simple msg to the Logfile
        /// </summary>
        /// <param name="msg">The msg to be written to the Logfile</param>
        public static void Log(string msg)
        {
            var logmsg = "[" + DateTime.Now.ToLongTimeString() + "] " + msg;
            loghistory.Add(logmsg);
            var context = GlobalHost.ConnectionManager.GetHubContext<LogConsole>();
            context.Clients.All.broadcastMessage(logmsg); 
        }

        /// <summary>
        /// Likely the most commong Log overload, writes the msg to the Logfile and includes the sender class
        /// </summary>
        /// <param name="className">The class generating the Log entry</param>
        /// <param name="msg">The message to be sent to the Logfile</param>
        public static void Log(string className, string msg)
        {
            Log(className + ": " + msg);
        }

        /// <summary>
        /// Overload of Log, including a function for precise debugging
        /// </summary>
        /// <param name="className">The class generating the Log-event</param>
        /// <param name="func">The function within the above class responsible</param>
        /// <param name="msg">The message to be logged</param>
        public static void Log(string className, string func, string msg)
        {
            Log("" + className + "-" + func + ": " + msg);
        }


        public static void send_history(Microsoft.AspNet.SignalR.Hubs.HubCallerContext con)
        {
            var history = loghistory;

            if (history.Count > 25)
            {
                history = history.GetRange(history.Count-26,25);
            } 
            var context = GlobalHost.ConnectionManager.GetHubContext<LogConsole>();
            foreach (string msg in history)
            {
                context.Clients.Client(con.ConnectionId).broadcastmessage(msg);
            }
        
        }
    }
    }
