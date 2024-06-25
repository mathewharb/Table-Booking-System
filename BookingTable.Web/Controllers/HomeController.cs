using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BookingTable.Business.IRepository;
using BookingTable.Business.Repository;
using BookingTable.Entities.Enum;
using BookingTable.Web.Business;
using BookingTable.Web.Helpers;

namespace BookingTable.Web.Controllers
{
    public class HomeController : Controller
    {
        // GET: Home
        public ActionResult Index()
        {
            //string timeString, int floorId, int typeId
            ITableRepository tableRepository = new TableRepository();
            ISettingRepository settingRepository = new SettingRepository();

            var listIdString = settingRepository.GetSettingByKey(WebContentSettingEnum.TablesString.ToString()).Value.Split(',').ToList();
            listIdString.Remove(listIdString.Last());
            if (listIdString.Count > 0)
            {
                var listId = listIdString.Select(x => int.Parse("0"+x)).ToList();

                var data = tableRepository.GetTablesByListId(listId);

                var time = DateTime.Now;
                var tableSelectModels = new TableBll().ConvertTableToTableModelByTime(data, time);
                return View(tableSelectModels);
            }
            return null;

        }
        public ActionResult Error()
        {
            return View();
        }
    }
}