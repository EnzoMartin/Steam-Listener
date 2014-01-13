using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Steam_Listener.Models
{
    public class AppRequest
    {
        public string secret { get; set; }
        public IEnumerable<App> data { get; set; }

        public AppRequest(string s, IEnumerable<App> d)
        {
            this.secret = s;
            this.data = d;
        }
    }
}