using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Steam_Listener.Models
{
    public class AppRequest
    {
        public string secret { get; set; }
        public int current { get; set; }
        public int total { get; set; }
        public IEnumerable<App> data { get; set; }

        public AppRequest(string s, int current, int total, IEnumerable<App> d)
        {
            this.secret = s;
            this.current = current;
            this.total = total;
            this.data = d;
        }
    }
}