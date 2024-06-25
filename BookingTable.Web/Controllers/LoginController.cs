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
using BookingTable.Entities.Enum;
using BookingTable.Web.Helpers;

namespace BookingTable.Web.Controllers
{
    public class LoginController : Controller
    {
        private readonly ICustomerRepository _customerRepository = new CustomerRepository();

        //GET
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Logout()
        {
            Session.Remove("User");
            return RedirectToAction("Index", "Home");
        }

        //POST
        [HttpPost]
        public ActionResult Login(LoginModel model)
        {
            var message = new MessageModel
            {
                Content = Resources.Resources.Message_Success_Login,
                Title = Resources.Resources.Content_Success,
                Type = MessageTypeEnum.SuccessReload.ToString(),
                ClosePopup = true
            };
            if (model != null)
            {
                var entity = _customerRepository.Login(model.Username, Utils.ToMd5Hash(model.Password));

                if (entity != null)
                {
                    Session["User"] = entity;
                    message.Content = string.Format(Resources.Resources.Content_HelloOldMember, entity.FullName);
                    return Json(message, JsonRequestBehavior.AllowGet);
                }
                message = new MessageModel
                {
                    Content = Resources.Resources.Message_Error_UsernameNotExist,
                    Title = Resources.Resources.Content_Error,
                    Type = MessageTypeEnum.Error.ToString(),
                };
                return Json(message, JsonRequestBehavior.AllowGet);
            }
            message = new MessageModel
            {
                Content = Resources.Resources.Message_Error_Validate,
                Title = Resources.Resources.Content_Error,
                Type = MessageTypeEnum.Error.ToString()
            };

            return Json(message, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult Register(UserModel model)
        {

            var message = new MessageModel
            {
                Content = Resources.Resources.Message_Success_Register,
                Title = Resources.Resources.Content_Success,
                Type = MessageTypeEnum.SuccessReload.ToString(),
                ClosePopup = true
            };

            //Parse
            var entity = new Customer()
            {
                Id = model.Id,
                DateOfBirth = model.DateOfBirth,
                Username = model.Username,
                Password = model.Password,
                Phone = model.Phone,
                Email = model.Email,
                FullName = model.FullName,
                Address = model.Address,
                Active = true,
                Deleted = false,
                LastUpdate = DateTime.Now,
            };

            //Check Password = Password confirm
            if (model.Password != model.PasswordConfirm)
            {
                message = new MessageModel
                {
                    Content = Resources.Resources.Message_Error_PasswordConfirm,
                    Title = Resources.Resources.Content_Error,
                    Type = MessageTypeEnum.Error.ToString()
                };

                return Json(message, JsonRequestBehavior.AllowGet);
            }

            //Check username Exist
            if (!_customerRepository.IsValid(entity))
            {
                message = new MessageModel
                {
                    Content = Resources.Resources.Message_Error_UserExisted,
                    Title = Resources.Resources.Content_Error,
                    Type = MessageTypeEnum.Error.ToString()
                };
                return Json(message, JsonRequestBehavior.AllowGet);
            }
            entity.Password = Utils.ToMd5Hash(entity.Password);

            //Validate
            if (!Validator.Validate(entity))
            {
                message = new MessageModel
                {
                    Content = Resources.Resources.Message_Error_Validate,
                    Title = Resources.Resources.Content_Error,
                    Type = MessageTypeEnum.Error.ToString()
                };
                return Json(message, JsonRequestBehavior.AllowGet);
            }

            //Save
            if (!_customerRepository.Save(entity))
            {
                message = new MessageModel
                {
                    Content = Resources.Resources.Message_Error_System,
                    Title = Resources.Resources.Content_Error,
                    Type = MessageTypeEnum.Error.ToString()
                };
                return Json(message, JsonRequestBehavior.AllowGet);
            }

            Session["User"] = entity;
            message.Content = string.Format(Resources.Resources.Content_HelloNewMember, entity.FullName,
                WebSetting.GetWebContent().WebLongName);
            return Json(message, JsonRequestBehavior.AllowGet);
        }

    }
}