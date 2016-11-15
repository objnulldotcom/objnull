using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace MVCWeb
{
    public abstract class BaseController : Controller
    {
        public CurrentUser CurrentUser
        {
            get
            {
                if (HttpContext.User != null && HttpContext.User is CurrentUser)
                {
                    return HttpContext.User as CurrentUser;
                }
                else
                {
                    return null;
                }
            }
        }

        public CurrentManager CurrentManager
        {
            get
            {
                if (HttpContext.User != null && HttpContext.User is CurrentManager)
                {
                    return HttpContext.User as CurrentManager;
                }
                else
                {
                    return null;
                }
            }
        }
    }
}