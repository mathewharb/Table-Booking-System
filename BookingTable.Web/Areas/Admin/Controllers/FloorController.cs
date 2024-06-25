using System;
using System.Linq;
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
    [Permission("Floor")]
    public class FloorController : Controller
    {
        private readonly IFloorRepository _floorRepository = new FloorRepository();
        private readonly ITableRepository _tableRepository = new TableRepository();

        // GET
        public ActionResult Index()
        {
            var data = _floorRepository.GetFloors();

            return View(data);
        }
        public ActionResult Enter(int id = 0)
        {

            var entity = new Floor();

            if (id > 0)
            {
                entity = _floorRepository.Find(id);
            }

            return PartialView("_EnterFloor", entity);
        }
        public ActionResult AddTable(int id)
        {
            var entity = new Table { Floor = _floorRepository.Find(id) };

            return PartialView("_AddTable", entity);
        }

        public ActionResult ViewTables(int id)
        {
            var data = _tableRepository.GetTablesByFloorId(id).OrderBy(x => x.Name);

            return PartialView("_ViewTables", data);
        }
        public ActionResult Delete(int id)
        {
            var entity = _floorRepository.Find(id);

            return PartialView("_DeleteFloor", entity);
        }

        //POST
        [HttpPost]
        public ActionResult Enter(Floor model)
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
            if (!_floorRepository.Save(model))
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
        public ActionResult Delete(Floor model)
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
            var entity = _floorRepository.Find(model.Id);
            entity.LastUpdate = DateTime.Now;
            entity.UpdateByAdminId = Support.GetCurrentAdmin().Id;

            //check
            if (entity.Tables.Any(x => x.OrderDetails.Any(y => y.Completed != true)))
            {
                message = new MessageModel
                {
                    Content = Resources.Resources.Message_Error_Food_Foreign,
                    Title = Resources.Resources.Content_Error,
                    Type = MessageTypeEnum.Error.ToString(),
                    ClosePopup = true
                };
                return Json(message, JsonRequestBehavior.AllowGet);
            }

            //Save
            if (!_floorRepository.Delete(entity))
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