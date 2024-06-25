using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BookingTable.Business;
using BookingTable.Business.IRepository;
using BookingTable.Business.Repository;
using BookingTable.Entities.Entities;
using BookingTable.Entities.Enum;
using BookingTable.Entities.Models;
using BookingTable.Web.Helpers;
using BookingTable.Web.Security;

namespace BookingTable.Web.Areas.Admin.Controllers
{
    [AdminAuthorized]
    [Permission("Customer")]
    public class CustomerController : Controller
    {
        private readonly ICustomerRepository _customerRepository = new CustomerRepository();
        
        // GET
        public ActionResult Index()
        {
            var data = _customerRepository.GetCustomers();

            return View(data);
        }
        public ActionResult Enter(int id = 0)
        {
            var model = new UserModel();

            if (id <= 0) return View("Enter", model);

            var entity = _customerRepository.Find(id);

            model = new UserModel
            {
                Id = entity.Id,
                DateOfBirth = entity.DateOfBirth,
                Active = entity.Active,
                Username = entity.Username,
                Password = entity.Password,
                Phone = entity.Phone,
                Email = entity.Email,
                FullName = entity.FullName,
                Address = entity.Address,
                PasswordConfirm = entity.Password,
                ImageName = entity.Image
            };

            return View("Enter", model);
        }
        public ActionResult Details(int id)
        {
            var entity = _customerRepository.Find(id);

            return PartialView("_DetailsCustomer", entity);
        }
        public ActionResult Delete(int id)
        {
            var entity = _customerRepository.Find(id);

            return PartialView("_DeleteCustomer", entity);
        }

        //POST
        [HttpPost]
        public ActionResult Enter(int id, DateTime dateOfBirth,
            bool active, string username, string password, string passwordConfirm,
            string phone, string email, string fullName, string address,
            HttpPostedFileBase image){
            var message = new MessageModel
            {
                Content = Resources.Resources.Message_Success_Insert,
                Title = Resources.Resources.Content_Success,
                Type = MessageTypeEnum.SuccessReload.ToString(),
                ClosePopup = true
            };

            //Parse
            var entity = new Customer()
            {
                Id = id,
                DateOfBirth = dateOfBirth,
                Active = active,
                Username = username,
                Password = password,
                Phone = phone,
                Email = email,
                FullName = fullName,
                Address = address,
                Deleted = false,
                LastUpdate = DateTime.Now,
                UpdateByAdminId = Support.GetCurrentAdmin().Id
            };

            //Check Password = Password confirm
            if (password != passwordConfirm)
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

            //Get Old Password, Old Image or create
            if (entity.Id > 0 && string.IsNullOrEmpty(entity.Password))
            {
                var temp = _customerRepository.Find(entity.Id);

                entity.Password = temp.Password;

                if (image == null) entity.Image = temp.Image;
            }
            else
            {
                entity.Password = Utils.ToMd5Hash(entity.Password);
            }

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

            //Action
            entity.LastUpdate = DateTime.Now;
            entity.UpdateByAdminId = Support.GetCurrentAdmin().Id;

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

            //Save image
            if (image != null)
            {
                entity.Image = Support.SavePhoto(image, "customer_" + entity.Id, Server.MapPath("~/Content/Uploads/Photos"));
                _customerRepository.Save(entity);
            }
            else
            {
                if (String.IsNullOrEmpty(entity.Image))
                {
                    entity.Image = "noUserImage.png";
                    _customerRepository.Save(entity);
                }
            }

            //Return
            return Json(message, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult Delete(UserModel model)
        {
            var message = new MessageModel
            {
                Content = Resources.Resources.Message_Success_Delete,
                Title = Resources.Resources.Content_Success,
                Type = MessageTypeEnum.SuccessReload.ToString(),
                ClosePopup = true
            };

            //Check
            if (model == null)
            {
                message = new MessageModel
                {
                    Content = Resources.Resources.Message_Error_Delete,
                    Title = Resources.Resources.Content_Error,
                    Type = MessageTypeEnum.Error.ToString(),
                    ClosePopup = true
                };
                return Json(message, JsonRequestBehavior.AllowGet);
            }

            //Action
            var entity = _customerRepository.Find(model.Id);
            entity.Deleted = true;
            entity.LastUpdate = DateTime.Now;
            entity.UpdateByAdminId = Support.GetCurrentAdmin().Id;

            //Check
            if (entity.Orders != null && entity.Orders.Any(x => x.Completed != true))
            {
                message = new MessageModel
                {
                    Content = Resources.Resources.Message_Error_Customer_Foreign,
                    Title = Resources.Resources.Content_Error,
                    Type = MessageTypeEnum.Error.ToString(),
                    ClosePopup = true
                };
                return Json(message, JsonRequestBehavior.AllowGet);

            }

            //Delete
            if (!_customerRepository.Save(entity))
            {
                message = new MessageModel
                {
                    Content = Resources.Resources.Message_Error_System,
                    Title = Resources.Resources.Content_Error,
                    Type = MessageTypeEnum.Error.ToString(),
                    ClosePopup = true
                };
                return Json(message, JsonRequestBehavior.AllowGet);
            }

            //return
            return Json(message, JsonRequestBehavior.AllowGet);
        }
    }
}