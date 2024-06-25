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
    [Permission("Table")]
    public class TableTypeController : Controller
    {
        private readonly ITableTypeRepository _tableRepository = new TableTypeRepository();

        // GET
        public ActionResult Index()
        {
            var data = _tableRepository.GetTableTypes();

            return View(data);
        }
        public ActionResult Enter(int id = 0)
        {
            var entity = new TableType();

            if (id > 0)
            {
                entity = _tableRepository.Find(id);
            }

            return PartialView("_EnterTableType", entity);
        }
        public ActionResult Delete(int id)
        {
            var entity = _tableRepository.Find(id);

            return PartialView("_DeleteTableType", entity);
        }

        //POST
        [HttpPost]
        public ActionResult Enter(TableType model)
        {
            var message = new MessageModel
            {
                Content = Resources.Resources.Message_Success_Insert,
                Title = Resources.Resources.Content_Success,
                Type = MessageTypeEnum.SuccessReload.ToString(),
                ClosePopup = true
            };

            //validate
            if (!Validator.Validate(model))
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
            model.LastUpdate = DateTime.Now;
            model.UpdateByAdminId = Support.GetCurrentAdmin().Id;
            model.Deleted = false;

            //Save
            if (!_tableRepository.Save(model))
            {
                message = new MessageModel
                {
                    Content = Resources.Resources.Message_Error_System,
                    Title = Resources.Resources.Content_Error,
                    Type = MessageTypeEnum.Error.ToString()
                };

                return Json(message, JsonRequestBehavior.AllowGet);
            }

            //return
            return Json(message, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult Delete(TableType model)
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
            var entity = _tableRepository.Find(model.Id);
            model.LastUpdate = DateTime.Now;
            model.UpdateByAdminId = Support.GetCurrentAdmin().Id;

            //check
            if (entity.Tables.Any(x => x.OrderDetails.Any(y => y.Completed != true)))
            {
                message = new MessageModel
                {
                    Content = Resources.Resources.Message_Error_TableType_Foreign,
                    Title = Resources.Resources.Content_Error,
                    Type = MessageTypeEnum.Error.ToString(),
                    ClosePopup = true
                };
                return Json(message, JsonRequestBehavior.AllowGet);
            }
                //delete
                if (!_tableRepository.Delete(entity))
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