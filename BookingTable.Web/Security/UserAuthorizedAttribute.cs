using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using BookingTable.Entities.Entities;

namespace BookingTable.Web.Security
{
    public class UserAuthorizedAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var isLogin = HttpContext.Current.Session["User"] != null;
            if (isLogin) return;
            HttpContext.Current.Session["ReturnToUrl"] = HttpContext.Current.Request.RawUrl;
            filterContext.Result = new RedirectToRouteResult(
                new RouteValueDictionary { { "Controller", "Login" }, { "Action", "Index" }}
            );
        }
    }
}