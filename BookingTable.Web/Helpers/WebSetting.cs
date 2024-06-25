using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BookingTable.Business.IRepository;
using BookingTable.Business.Repository;
using BookingTable.Entities.Enum;
using BookingTable.Entities.Models;

namespace BookingTable.Web.Helpers
{
    public static class WebSetting
    {
        public static int GetTimeDistanceSetting()
        {
            ISettingRepository settingRepository = new SettingRepository();
            var entity = settingRepository.GetSettingByKey(SystemSettingEnum.TimeDistance.ToString());
            return entity == null ? 0 : int.Parse(entity.Value);
        }

        public static WebContentSettingModel GetWebContent()
        {
            try
            {
                ISettingRepository settingRepository = new SettingRepository();
                var websetting = settingRepository.GetSettings(SettingTypeEnum.WebContent.ToString());
                if (websetting == null) return null;

                var model = new WebContentSettingModel
                {
                    WebLongName = websetting.FirstOrDefault(x => x.Key == WebContentSettingEnum.WebLongName.ToString()).Value,
                    WebShortName = websetting.FirstOrDefault(x => x.Key == WebContentSettingEnum.WebShortName.ToString()).Value,
                    Phone = websetting.FirstOrDefault(x => x.Key == WebContentSettingEnum.Phone.ToString()).Value,
                    Email = websetting.FirstOrDefault(x => x.Key == WebContentSettingEnum.Email.ToString()).Value,
                    Address = websetting.FirstOrDefault(x => x.Key == WebContentSettingEnum.Address.ToString()).Value,
                };
                return model;
            }
            catch (Exception)
            {

                return new WebContentSettingModel
                {
                    WebLongName = "Waiting...",
                    WebShortName = "Waiting...",
                    Phone = "Waiting...",
                    Email = "Waiting...",
                    Address = "Waiting..."
                };
            }
           

        }

        public static string GetPayPalAccount()
        {
            ISettingRepository settingRepository = new SettingRepository();
            return settingRepository.GetSettingByKey(PaypalSettingEnum.PaypalEmail.ToString()).Value;
        }

        public static string GetBookingLimit()
        {
            ISettingRepository settingRepository = new SettingRepository();
            return settingRepository.GetSettingByKey(SystemSettingEnum.BookingLimit.ToString()).Value;
        }
    }
}