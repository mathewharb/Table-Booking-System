using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BookingTable.Business.IRepository;
using BookingTable.Business.Repository;
using BookingTable.Entities.Entities;
using BookingTable.Entities.Enum;
using BookingTable.Entities.Models;
using BookingTable.Web.Business;
using BookingTable.Web.Helpers;
using BookingTable.Web.Security;

namespace BookingTable.Web.Areas.Admin.Controllers
{
    [AdminAuthorized]
    public class TableController : Controller
    {
        private readonly ITableRepository _tableRepository = new TableRepository();
        // GET
        [Permission("Table")]
        public ActionResult Index()
        {
            var data = _tableRepository.GetTables();

            return View(data);
        }
        [Permission("Table")]
        public ActionResult Enter(int id = 0)
        {
            var entity = new Table();

            if (id > 0)
            {
                entity = _tableRepository.Find(id);
            }

            return PartialView("_EnterTable", entity);
        }
        [Permission("Table")]
        public ActionResult Delete(int id)
        {
            var entity = _tableRepository.Find(id);

            return PartialView("_DeleteTable", entity);
        }
        [Permission("Orders")]
        public ActionResult GetValidTables(string timeString, int floorId, int typeId)
        {
            var data = _tableRepository.GetActivedTablesByFloorAndType(floorId,typeId);

            var time = DateTime.ParseExact(timeString, "MM/dd/yyyy hh:mm tt", CultureInfo.InvariantCulture);

            var tableSelectModels = new TableBll().ConvertTableToTableModelByTime(data, time);

            return Json(tableSelectModels, JsonRequestBehavior.AllowGet);

        }

        //POST
        [HttpPost]
        [Permission("Table")]
        public ActionResult Enter(Table model)
        {
            var message = new MessageModel
            {
                Content = Resources.Resources.Message_Success_Delete,
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

            //save
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
        [Permission("Table")]
        public ActionResult Delete(Table model)
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
            var entity = _tableRepository.Find(model.Id);
            entity.Deleted = true;
            entity.UpdateByAdminId = Support.GetCurrentAdmin().Id;
            entity.LastUpdate = DateTime.Now;

            //Check
            if (entity.OrderDetails.Any(x => x.Completed != true))
            {
                message = new MessageModel
                {
                    Content = Resources.Resources.Message_Error_Table_Foreign,
                    Title = Resources.Resources.Content_Error,
                    Type = MessageTypeEnum.Error.ToString(),
                    ClosePopup = true
                };
                return Json(message, JsonRequestBehavior.AllowGet);
            }
            //delete
            if (!_tableRepository.Save(entity))
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