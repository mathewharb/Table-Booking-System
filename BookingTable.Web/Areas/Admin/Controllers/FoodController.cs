using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Helpers;
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
    [Permission("Food")]
    public class FoodController : Controller
    {
        private readonly IFoodRepository _foodRepository = new FoodRepository();

        //GET
        public ActionResult Index()
        {
            var data = _foodRepository.GetFoods();

            return View(data);
        }
        public ActionResult Enter(int id = 0)
        {
            var entity = new Food();

            if (id > 0) entity = _foodRepository.Find(id);

            return PartialView("_EnterFood", entity);
        }
        public ActionResult Details(int id)
        {
            var entity = _foodRepository.Find(id);

            return PartialView("_DetailsFood", entity);
        }
        public ActionResult Delete(int id)
        {
            var entity = _foodRepository.Find(id);

            return PartialView("_DeleteFood", entity);
        }

        //POST
        [HttpPost]
        public ActionResult Enter(int id, string name, string description, string unit, decimal price, int quantity, bool active, HttpPostedFileBase fileImage)
        {
            var message = new MessageModel
            {
                Content = Resources.Resources.Message_Success_Insert,
                Title = Resources.Resources.Content_Success,
                Type = MessageTypeEnum.SuccessReload.ToString(),
                ClosePopup = true
            };

            //Parse
            var model = new Food()
            {
                Id = id,
                Name = name,
                Description = description,
                Unit = unit,
                Price = price,
                Quantity = quantity,
                Active = active,
                LastUpdate = DateTime.Now,
                UpdateByAdminId = Support.GetCurrentAdmin().Id,
                Deleted = false
            };

            //Validate
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

            //Check image and get old image if image is null
            if (fileImage == null && id > 0) model.Image = _foodRepository.Find(id).Image;

            //Save
            if (!_foodRepository.Save(model))
            {
                message = new MessageModel
                {
                    Content = Resources.Resources.Message_Error_System,
                    Title = Resources.Resources.Content_Error,
                    Type = MessageTypeEnum.Error.ToString()
                };

                return Json(message, JsonRequestBehavior.AllowGet);
            }

            //save image
            if (fileImage != null)
            {
                model.Image = Support.SavePhoto(fileImage, "food_" + model.Id, Server.MapPath("~/Content/Uploads/Photos"));
                _foodRepository.Save(model);
            }
            else
            {
                if (String.IsNullOrEmpty(model.Image))
                {
                    model.Image = "noImage.png";
                    _foodRepository.Save(model);
                }
            }

            //Return
            return Json(message, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Delete(Food model)
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
            var entity = _foodRepository.Find(model.Id);
            entity.Deleted = true;
            entity.LastUpdate = DateTime.Now;
            entity.UpdateByAdminId = Support.GetCurrentAdmin().Id;
            
            //Check
            if (entity.OrderDetails.Any(x => x.Completed != true))
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
            if (!_foodRepository.Save(entity))
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