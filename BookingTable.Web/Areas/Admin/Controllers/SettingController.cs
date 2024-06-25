using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Caching;
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
    [Permission("Setting")]
    public class SettingController : Controller
    {
        private readonly ISettingRepository _settingRepository = new SettingRepository();
        // GET: Admin/Setting
        public ActionResult WebContent()
        {

            var data = _settingRepository.GetSettings(SettingTypeEnum.WebContent.ToString());

            var webLongName = data.SingleOrDefault(x => x.Key == WebContentSettingEnum.WebLongName.ToString());
            var webShortName = data.SingleOrDefault(x => x.Key == WebContentSettingEnum.WebShortName.ToString());
            var address = data.SingleOrDefault(x => x.Key == WebContentSettingEnum.Address.ToString());
            var phone = data.SingleOrDefault(x => x.Key == WebContentSettingEnum.Phone.ToString());
            var email = data.SingleOrDefault(x => x.Key == WebContentSettingEnum.Email.ToString());
            var tableImage = data.SingleOrDefault(x => x.Key == WebContentSettingEnum.TableImage.ToString());
            var tablesString = data.SingleOrDefault(x => x.Key == WebContentSettingEnum.TablesString.ToString());
            var model = new WebContentSettingModel
            {
                WebLongName = webLongName.Value,
                WebShortName = webShortName.Value,
                Address = address.Value,
                Phone = phone.Value,
                Email = email.Value,
                TableImage = tableImage.Value,
                TablesString = tablesString.Value
            };

            return View(model);
        }

        public ActionResult System()
        {
            var data = _settingRepository.GetSettings(SettingTypeEnum.System.ToString());

            var timeDistance = data.SingleOrDefault(x => x.Key == SystemSettingEnum.TimeDistance.ToString());
            var bookingLimit = data.SingleOrDefault(x => x.Key == SystemSettingEnum.BookingLimit.ToString());
            var tax = data.SingleOrDefault(x => x.Key == SystemSettingEnum.Tax.ToString());
            var discount = data.SingleOrDefault(x => x.Key == SystemSettingEnum.Discount.ToString());
            var model = new SystemSettingModel
            {
                TimeDistance = timeDistance.Value,
                BookingLimit = bookingLimit.Value,
                Tax = tax.Value,
                Discount = discount.Value
            };
            return View(model);
        }

        public ActionResult Paypal()
        {
            var data = _settingRepository.GetSettings(SettingTypeEnum.Paypal.ToString());
            var paypalEmail = data.SingleOrDefault(x => x.Key == PaypalSettingEnum.PaypalEmail.ToString());
            var ptdToken = data.SingleOrDefault(x => x.Key == PaypalSettingEnum.PDTToken.ToString());
            var mode = data.SingleOrDefault(x => x.Key == PaypalSettingEnum.Mode.ToString());
            var model = new PaypalSettingModel
            {
                PaypalEmail = paypalEmail.Value,
                PDTToken = ptdToken.Value,
                Live = mode.Value == "Live"
            };
            return View(model);
        }

        [HttpPost]
        public ActionResult SaveWebContentSetting(string WebLongName, string Address, string WebShortName, string Phone, string Email, HttpPostedFileBase fileImage, string TablesString)   
        {
            //save image
            if (fileImage != null)
            {
                Support.SavePhoto(fileImage, "table", Server.MapPath("~/Content/Uploads/Photos"));
            }
            var model = new WebContentSettingModel
            {
                WebLongName = WebLongName,
                Address = Address,
                WebShortName = WebShortName,
                Phone = Phone,
                Email = Email,
                TableImage = "table",
                TablesString = TablesString
            };
            var message = new MessageModel
            {
                Content = Resources.Resources.Message_Success_Insert,
                Title = Resources.Resources.Content_Success,
                Type = MessageTypeEnum.SuccessReload.ToString(),
                ClosePopup = true
            };

            var webLongName = new Entities.Entities.Setting
            {
                Key = WebContentSettingEnum.WebLongName.ToString(),
                Value = model.WebLongName,
                Type = SettingTypeEnum.WebContent.ToString(),
                LastUpdate = DateTime.Now,
                UpdateByAdminId = Support.GetCurrentAdmin().Id
            };
            var webShortName = new Entities.Entities.Setting
            {
                Key = WebContentSettingEnum.WebShortName.ToString(),
                Value = model.WebShortName,
                Type = SettingTypeEnum.WebContent.ToString(),
                LastUpdate = DateTime.Now,
                UpdateByAdminId = Support.GetCurrentAdmin().Id
            };
            var address = new Entities.Entities.Setting
            {
                Key = WebContentSettingEnum.Address.ToString(),
                Value = model.Address,
                Type = SettingTypeEnum.WebContent.ToString(),
                LastUpdate = DateTime.Now,
                UpdateByAdminId = Support.GetCurrentAdmin().Id
            };
            var phone = new Entities.Entities.Setting
            {
                Key = WebContentSettingEnum.Phone.ToString(),
                Value = model.Phone,
                Type = SettingTypeEnum.WebContent.ToString(),
                LastUpdate = DateTime.Now,
                UpdateByAdminId = Support.GetCurrentAdmin().Id
            };
            var email = new Entities.Entities.Setting
            {
                Key = WebContentSettingEnum.Email.ToString(),
                Value = model.Email,
                Type = SettingTypeEnum.WebContent.ToString(),
                LastUpdate = DateTime.Now,
                UpdateByAdminId = Support.GetCurrentAdmin().Id
            };
            var tableImage = new Entities.Entities.Setting
            {
                Key = WebContentSettingEnum.TableImage.ToString(),
                Value = model.TableImage,
                Type = SettingTypeEnum.WebContent.ToString(),
                LastUpdate = DateTime.Now,
                UpdateByAdminId = Support.GetCurrentAdmin().Id
            };
            var tablesString = new Entities.Entities.Setting
            {
                Key = WebContentSettingEnum.TablesString.ToString(),
                Value = string.IsNullOrEmpty(model.TablesString)?",": model.TablesString,
                Type = SettingTypeEnum.WebContent.ToString(),
                LastUpdate = DateTime.Now,
                UpdateByAdminId = Support.GetCurrentAdmin().Id
            };
            var setting = new List<Setting> { webShortName, webLongName, address, phone, email, tableImage,tablesString};

            if (!_settingRepository.SaveSetting(setting))
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

        [HttpPost]
        public ActionResult SaveSystemSetting(SystemSettingModel model)
        {
            if (int.Parse(model.BookingLimit) < 0 || int.Parse(model.Discount) < 0 || int.Parse(model.Tax) < 0 || int.Parse(model.TimeDistance) < 0)
            {
                return RedirectToAction("Index", "Home");
            }
            var message = new MessageModel
            {
                Content = Resources.Resources.Message_Success_Insert,
                Title = Resources.Resources.Content_Success,
                Type = MessageTypeEnum.Success.ToString(),
                ClosePopup = true
            };
            var timeDistance = new Entities.Entities.Setting
            {
                Key = SystemSettingEnum.TimeDistance.ToString(),
                Value = model.TimeDistance,
                Type = SettingTypeEnum.System.ToString(),
                LastUpdate = DateTime.Now,
                UpdateByAdminId = Support.GetCurrentAdmin().Id
            };
            var bookingLimit = new Entities.Entities.Setting
            {
                Key = SystemSettingEnum.BookingLimit.ToString(),
                Value = model.BookingLimit,
                Type = SettingTypeEnum.System.ToString(),
                LastUpdate = DateTime.Now,
                UpdateByAdminId = Support.GetCurrentAdmin().Id
            };
            var tax = new Entities.Entities.Setting
            {
                Key = SystemSettingEnum.Tax.ToString(),
                Value = model.Tax,
                Type = SettingTypeEnum.System.ToString(),
                LastUpdate = DateTime.Now,
                UpdateByAdminId = Support.GetCurrentAdmin().Id
            };
            var discount = new Entities.Entities.Setting
            {
                Key = SystemSettingEnum.Discount.ToString(),
                Value = model.Discount,
                Type = SettingTypeEnum.System.ToString(),
                LastUpdate = DateTime.Now,
                UpdateByAdminId = Support.GetCurrentAdmin().Id
            };

            var setting = new List<Setting> { timeDistance, bookingLimit, tax, discount };

            if (!_settingRepository.SaveSetting(setting))
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

        [HttpPost]
        public ActionResult SavePaypalSetting(PaypalSettingModel model)
        {
            var message = new MessageModel
            {
                Content = Resources.Resources.Message_Success_Insert,
                Title = Resources.Resources.Content_Success,
                Type = MessageTypeEnum.Success.ToString(),
                ClosePopup = true
            };
            var paypalEmail = new Entities.Entities.Setting
            {
                Key = PaypalSettingEnum.PaypalEmail.ToString(),
                Value = model.PaypalEmail,
                Type = SettingTypeEnum.Paypal.ToString(),
                LastUpdate = DateTime.Now,
                UpdateByAdminId = Support.GetCurrentAdmin().Id
            };
            var pdtToken  = new Entities.Entities.Setting
            {
                Key = PaypalSettingEnum.PDTToken.ToString(),
                Value = model.PDTToken,
                Type = SettingTypeEnum.Paypal.ToString(),
                LastUpdate = DateTime.Now,
                UpdateByAdminId = Support.GetCurrentAdmin().Id
            };
            var mode = new Entities.Entities.Setting
            {
                Key = PaypalSettingEnum.Mode.ToString(),
                Value = model.Live?PaypalSettingEnum.Live.ToString():PaypalSettingEnum.Sandbox.ToString(),
                Type = SettingTypeEnum.Paypal.ToString(),
                LastUpdate = DateTime.Now,
                UpdateByAdminId = Support.GetCurrentAdmin().Id
            };

            var setting = new List<Setting> { paypalEmail,pdtToken,mode };

            if (!_settingRepository.SaveSetting(setting))
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