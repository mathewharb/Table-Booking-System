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
    public class AdminController : Controller
    {
        private readonly IAdminRepository _adminRepository = new AdminRepository();
        // GET
        [Permission("Admin")]
        public ActionResult Index()
        {
            var data = _adminRepository.GetAdmins();

            return View(data);
        }
        
        [Permission("Admin")]
        public ActionResult Enter(int id = 0)
        {
            var model = new UserModel();
            if (id <= 0) return View("Enter", model);
            var entity = _adminRepository.Find(id);
            model = new UserModel
            {
                Id = entity.Id,
                RoleId = entity.RoleId,
                DateOfBirth = entity.DateOfBirth,
                Active = entity.Active,
                Username = entity.Username,
                Password = entity.Password,
                Phone = entity.Phone,
                Email = entity.Email,
                FullName = entity.FullName,
                Address = entity.Address,
                PasswordConfirm = entity.Password,
                IdentityCard = entity.IdentityCard,
                ImageName = entity.Image
            };
            return View("Enter", model);
        }
        [Permission("Admin")]
        public ActionResult Details(int id)
        {
            var entity = _adminRepository.Find(id);

            return PartialView("_DetailsAdmin", entity);
        }
        [Permission("Admin")]
        public ActionResult Delete(int id)
        {
            var entity = _adminRepository.Find(id);

            return PartialView("_DeleteAdmin", entity);
        }
        public ActionResult ChangePassword()
        {
            var model = new ChangePasswordModel();

            return View(model);
        }

        //POST
        [Permission("Admin")]
        [HttpPost]
        public ActionResult Enter(int id, int roleId, DateTime dateOfBirth,
            bool active, string username, string password, string passwordConfirm,
            string phone, string email, string fullName, string address, string identityCard,
            HttpPostedFileBase image)
        {
            var message = new MessageModel
            {
                Content = Resources.Resources.Message_Success_Insert,
                Title = Resources.Resources.Content_Success,
                Type = MessageTypeEnum.SuccessReload.ToString(),
                ClosePopup = true
            };

            //Parse
            var entity = new Entities.Entities.Admin()
            {
                Id = id,
                RoleId = roleId,
                DateOfBirth = dateOfBirth,
                Active = active,
                Username = username,
                Password = password,
                Phone = phone,
                Email = email,
                FullName = fullName,
                Address = address,
                IdentityCard = identityCard,
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
            if (!_adminRepository.IsValid(entity))
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
                var temp = _adminRepository.Find(entity.Id);

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

            //action
            entity.LastUpdate = DateTime.Now;
            entity.UpdateByAdminId = Support.GetCurrentAdmin().Id;

            //Save
            if (!_adminRepository.Save(entity))
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
                entity.Image = Support.SavePhoto(image, "admin_" + entity.Id, Server.MapPath("~/Content/Uploads/Photos"));
                _adminRepository.Save(entity);
            }else
            {
                if (String.IsNullOrEmpty(entity.Image))
                {
                    entity.Image = "noUserImage.png";
                    _adminRepository.Save(entity);
                }
            }

            //Return
            return Json(message, JsonRequestBehavior.AllowGet);
        }

        [Permission("Admin")]
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
            var entity = _adminRepository.Find(model.Id);
            if (entity.Id != Support.GetCurrentAdmin().Id)
            {
                entity.Deleted = true;
                entity.LastUpdate = DateTime.Now;
                entity.UpdateByAdminId = Support.GetCurrentAdmin().Id;
                //Save
                if (!_adminRepository.Save(entity))
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
            }else
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
            return Json(message, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult ChangePassword(ChangePasswordModel model)
        {
            var message = new MessageModel
            {
                Content = Resources.Resources.Message_Success_Insert,
                Title = Resources.Resources.Content_Success,
                Type = MessageTypeEnum.SuccessReload.ToString(),
                ClosePopup = true
            };
            if (model.New != model.Confirm)
            {
                message = new MessageModel
                {
                    Content = Resources.Resources.Message_Error_PasswordConfirm,
                    Title = Resources.Resources.Content_Error,
                    Type = "error"
                };
                return Json(message, JsonRequestBehavior.AllowGet);
            }

            var entity = _adminRepository.Find(Support.GetCurrentAdmin().Id);
            if (entity == null)
            {
                message = new MessageModel
                {
                    Content = Resources.Resources.Message_Error_Validate,
                    Title = Resources.Resources.Content_Error,
                    Type = MessageTypeEnum.Error.ToString()
                };
                return Json(message, JsonRequestBehavior.AllowGet);
            }
            if (entity.Password != Utils.ToMd5Hash(model.Old))
            {
                message = new MessageModel
                {
                    Content = Resources.Resources.Massage_Error_PasswordNotMatch,
                    Title = Resources.Resources.Content_Error,
                    Type = MessageTypeEnum.Error.ToString()
                };
                return Json(message, JsonRequestBehavior.AllowGet);
            }

            entity.Password = Utils.ToMd5Hash(model.New);
            entity.UpdateByAdminId = Support.GetCurrentAdmin().Id;
            entity.LastUpdate = DateTime.Now;

            if (!_adminRepository.Save(entity))
            {
                message = new MessageModel
                {
                    Content = Resources.Resources.Message_Error_System,
                    Title = Resources.Resources.Content_Error,
                    Type = MessageTypeEnum.Error.ToString()
                };
                return Json(message, JsonRequestBehavior.AllowGet);
            }


            return Json(message, JsonRequestBehavior.AllowGet);
        }
    }
}