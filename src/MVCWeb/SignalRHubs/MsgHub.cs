using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

namespace MVCWeb.SignalRHubs
{
    [HubName("MsgHub")]
    public class MsgHub : Hub
    {
        public void Send(string msg)
        {
            Clients.All.addNewMessage(msg);
        }

        //发送给指定用户
        public void SendToUser(string userID, string msg)
        {
            Clients.User(userID).addNewMessage(msg);
        }
    }
}