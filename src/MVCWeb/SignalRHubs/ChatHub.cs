using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

namespace MVCWeb.SignalRHubs
{
    [HubName("ChatHub")]
    public class ChatHub : Hub
    {
        public void Send(string msg)
        {
            Clients.All.addNewMessage(msg);
        }
    }
}