using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BookingTable.Business;
using BookingTable.Business.IRepository;
using BookingTable.Business.Repository;
using BookingTable.Entities.Entities;
using BookingTable.Entities.Models;

namespace BookingTable.Web.Areas.Admin.Controllers
{
    public class LoginController : Controller
    {
        private readonly IAdminRepository _adminRepository = new AdminRepository();
        // GET: Admin/Login
        public ActionResult Index(string tracking)
        {
            var model = new LoginModel { RedirectToUrl = tracking };
            return View(model);
        }

        public ActionResult LogOut()
        {
            Session.Remove("Admin");
            return Redirect("Index");
        }

        //POST
        [HttpPost]
        public ActionResult Login(LoginModel model)
        {
            if (model != null)
            {
                var entity = _adminRepository.Login(model.Username, Utils.ToMd5Hash(model.Password));

                if (entity != null)
                {
                    Session["Admin"] = entity;
                    if (!string.IsNullOrEmpty(model.RedirectToUrl))
                    {
                        if(!model.RedirectToUrl.Equals(Url.Action("Index","Home")))
                            return Redirect(model.RedirectToUrl);
                    }
                    return RedirectToAction("BookingList", "Orders");
                }
                if (!string.IsNullOrEmpty(model.RedirectToUrl))
                    return RedirectToAction("Index", "Login", new { tracking = model.RedirectToUrl });
            }
            return RedirectToAction("Index", "Login");
        }
    }
}