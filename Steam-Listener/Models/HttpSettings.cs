using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Steam_Listener.Models
{
    public static class HttpSettings
    {
        public static string URL { get; set; }
        public static string secret { get; set; }
        public static int AppsPerRequest { get; set; }
    }
}