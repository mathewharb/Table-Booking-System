using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using BookingTable.Entities.Entities;

namespace BookingTable.Web.Security
{
    public class AdminAuthorizedAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var isLogin = HttpContext.Current.Session["Admin"] != null;
            var tracking = HttpContext.Current.Request.RawUrl.ToString();
            if (isLogin) return;
            if (!string.IsNullOrEmpty(tracking))
            {
                filterContext.Result = new RedirectToRouteResult(
                    new RouteValueDictionary {{"Controller", "Login"}, {"Action", "Index"}, {"tracking", tracking}}
                );
            }
            else
            {
                filterContext.Result = new RedirectToRouteResult(
                    new RouteValueDictionary { { "Controller", "Login" }, { "Action", "Index" }}
                );
            }
        }
    }
}