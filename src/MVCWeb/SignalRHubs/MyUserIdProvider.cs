using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using MVCWeb;

namespace MVCWeb.SignalRHubs
{
    public class MyUserIdProvider : IUserIdProvider
    {
        //使用cookie中的用户ID作为signalr客户端标识
        public string GetUserId(IRequest request)
        {
            return request.Cookies["UID"].Value;
        }
    }
}