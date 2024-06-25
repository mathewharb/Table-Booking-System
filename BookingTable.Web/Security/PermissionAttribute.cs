using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using BookingTable.Business;
using BookingTable.Business.IRepository;
using BookingTable.Business.Repository;
using BookingTable.Entities;
using BookingTable.Entities.Entities;

namespace BookingTable.Web.Security
{
    public class PermissionAttribute : ActionFilterAttribute
    {
        private readonly string _permission;
        private readonly IAdminRepository _adminRepository = new AdminRepository();
        public PermissionAttribute(string permission)
        {
            _permission = permission;
        }

        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            if (string.IsNullOrEmpty(_permission)) return;
            if (HttpContext.Current.Session["Admin"] == null) return;
            var entity = _adminRepository.Find(((Admin)HttpContext.Current.Session["Admin"]).Id);
            if (entity.Role.Permissions.All(x => x.Code != _permission))
                filterContext.Result = new RedirectToRouteResult(
                    new RouteValueDictionary { { "Controller", "Home" }, { "Action", "NoPermission" } });

        }
    }
}