using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
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
    [Permission("Admin")]
    public class RoleController : Controller
    {
        private readonly IRoleRepository _roleRepository = new RoleRepository();
        private readonly IPermissionRepository _permissionRepository = new PermissionRepository();
        
        // GET
        public ActionResult Index()
        {
           var data = _roleRepository.GetRoles();

            return View(data);
        }
        public ActionResult Enter(int id = 0)
        {

            var model = new RoleModel();

            if (id <= 0) return PartialView("_EnterRole", model);

            var entity = _roleRepository.Find(id);

            model.Id = entity.Id;
            model.Name = entity.Name;
            model.Permissions = entity.Permissions;

            foreach (var permission in entity.Permissions)
            {
                model.PermissionString += permission.Code + ",";
            }

            return PartialView("_EnterRole", model);
        }
        public ActionResult Delete(int id)
        {
            var entity = _roleRepository.Find(id);

            return PartialView("_DeleteRole", entity);
        }

        [HttpPost]
        public ActionResult Enter(RoleModel model)
        {
            var message = new MessageModel
            {
                Content = Resources.Resources.Message_Success_Insert,
                Title = Resources.Resources.Content_Success,
                Type = MessageTypeEnum.SuccessReload.ToString(),
                ClosePopup = true
            };

            //Parse
            var entity = new Role
            {
                Id = model.Id,
                Name = model.Name,
                Deleted = false,
                LastUpdate = DateTime.Now,
                UpdateByAdminId = Support.GetCurrentAdmin().Id
            };

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

            //Change Permisstion
            if (!string.IsNullOrEmpty(model.PermissionString))
            {
                var permissions = model.PermissionString.Split(',').ToList();

                _permissionRepository.DeleteByRoleId(entity.Id);

                foreach (var p in permissions)
                {
                    if (!string.IsNullOrEmpty(p))
                    {
                        if (GetSelectList.GetPermissionsSelectList().Any(x => x.Value == p))
                        {
                            var r = new Permission
                            {
                                Code = p,
                                RoleId = entity.Id,
                                Deleted = false,
                                LastUpdate = DateTime.Now,
                                UpdateByAdminId = Support.GetCurrentAdmin().Id
                            };
                            _permissionRepository.Save(r);
                            entity.Permissions.Add(r);
                        }
                        else
                        {
                            message = new MessageModel
                            {
                                Content = Resources.Resources.Message_Error_PermissionNoExist,
                                Title = Resources.Resources.Content_Error,
                                Type = MessageTypeEnum.Error.ToString()
                            };

                            return Json(message, JsonRequestBehavior.AllowGet);
                        }
                    }
                }
            }

            //Save
            if (!_roleRepository.Save(entity))
            {

                message = new MessageModel
                {
                    Content = Resources.Resources.Message_Error_System,
                    Title = Resources.Resources.Content_Error,
                    Type = MessageTypeEnum.Error.ToString()
                };

                return Json(message, JsonRequestBehavior.AllowGet);
            }

            //Return
            return Json(message, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult Delete(Role model)
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

            //action
            var entity = _roleRepository.Find(model.Id);
            entity.LastUpdate = DateTime.Now;
            entity.UpdateByAdminId = Support.GetCurrentAdmin().Id;

            //check
            if (entity.Admins.Any(x => x.Deleted != true))
            {
                message = new MessageModel
                {
                    Content = Resources.Resources.Message_Error_Floor_Foreign,
                    Title = Resources.Resources.Content_Error,
                    Type = MessageTypeEnum.Error.ToString(),
                    ClosePopup = true
                };
                return Json(message, JsonRequestBehavior.AllowGet);
            }

            //Delete
            if (!_roleRepository.Delete(entity))
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

            //Return
            return Json(message, JsonRequestBehavior.AllowGet);
        }
    }
}