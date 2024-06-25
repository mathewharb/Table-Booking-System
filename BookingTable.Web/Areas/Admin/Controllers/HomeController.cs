using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BookingTable.Web.Security;

namespace BookingTable.Web.Areas.Admin.Controllers
{
    [AdminAuthorized]
    public class HomeController : Controller
    {
        // GET: Admin/Home
        public ActionResult Index()
        {
            return RedirectToAction("BookingList", "Orders");
        }

        public ActionResult NoPermission()
        {
            return View();
        }

        public ActionResult Error()
        {
            return View();
        }
    }
}