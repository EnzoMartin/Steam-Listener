using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Steam_Listener.Models
{
    public class User
    {
        public string userName { get; set; }
        public string userPass { get; set; }
        public string authCode { get; set; }

        public bool authPass(string password) {
           return (password == this.userPass); 
        }
    }
}