using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using Steam_Listener.utils;

namespace Steam_Listener
{
    public class LogConsole : Hub
    {

        public void Send(string message)
        {
            // Call the broadcastMessage method to update clients. 
            Clients.All.broadcastMessage(message);
            

        }

        public override System.Threading.Tasks.Task OnConnected()
        {    
            Logs.send_history(Context);        
            return base.OnConnected();
        }
    }
}