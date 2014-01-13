using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;
using Steam_Listener.Models;
using Steam_Listener.utils;
using System.IO;
using Newtonsoft.Json;

namespace Steam_Listener.lib
{
    public class SteamHttpClient
    {
        public static string URL;
        private static string secret;
        private static int max;


        public SteamHttpClient()
        {
            URL = HttpSettings.URL;
            secret = HttpSettings.secret;
            max = HttpSettings.AppsPerRequest;
        }


        public void process(List<App> apps)
        {
            var total = apps.Count / max;

            for (int i = 0; i < total; i++)
            {
                var appReq = new AppRequest(secret, apps.Skip(i * total).Take(max));
                sendData(JsonConvert.SerializeObject((appReq)));
            }


        }

        public void sendData(string data)
        {
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(URL);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";

            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                // var preString = "{\"secret\":\"" + secret + "\",\"data\":\"";

                //var dataa = data.Remove(0, 1);
                //var datadone = dataa.Remove(dataa.Length - 1);


                streamWriter.Write(data);
                streamWriter.Flush();
                streamWriter.Close();

                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();
                }
            }


        }
    }


}