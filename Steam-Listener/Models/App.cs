using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Steam_Listener.Models
{
    public class App
    {
        public uint AppID { get; set; }
        public uint ChangeID { get; set; }
        public Dictionary<string, string> data { get; set; }


        public App(uint ID, uint cID, Dictionary<string, string> info)
        {
            this.AppID = ID;
            this.ChangeID = cID;
            this.data = info;
        }
    }
}