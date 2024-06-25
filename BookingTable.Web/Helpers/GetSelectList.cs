using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using BookingTable.Business.IRepository;
using BookingTable.Business.Repository;
using BookingTable.Entities.Entities;
using BookingTable.Entities.Enum;
using BookingTable.Web.Business;
using BookingTable.Web.Security;

namespace BookingTable.Web.Helpers
{
    public static class GetSelectList
    {
        public static SelectList GetFloorSelectList()
        {
            IFloorRepository floorRepository = new FloorRepository();

            var items = floorRepository.GetFloors();
            var selectListItems = items.Select(item => new SelectListItem
            {
                Value = item.Id.ToString(),
                Text = item.Name
            });

            return new SelectList(selectListItems, "Value", "Text");
        }

        public static SelectList GetTableTypeSelectList()
        {
            ITableTypeRepository tableTypeRepository = new TableTypeRepository();

            var items = tableTypeRepository.GetTableTypes();

            var selectListItems = items.Select(item => new SelectListItem
            {
                Value = item.Id.ToString(),
                Text = item.Name
            }).ToList();
            return new SelectList(selectListItems, "Value", "Text");
        }
        [AdminAuthorized]
        public static SelectList GetPermissionsSelectList()
        {
            var selectListItems = (from object item in typeof(PermissionEnum).GetEnumValues()
                select new SelectListItem
                {
                    Value = item.ToString(),
                    Text = item.ToString()
                }).ToList();

            return new SelectList(selectListItems, "Value", "Text");
        }
        [AdminAuthorized]
        public static SelectList GetRoleSelectList()
        {
            IRoleRepository roleRepository = new RoleRepository();

            var items = roleRepository.GetRoles();

            var selectListItems = items.Select(item => new SelectListItem
            {
                Value = item.Id.ToString(),
                Text = item.Name
            }).ToList();
            return new SelectList(selectListItems, "Value", "Text");
        }

        public static SelectList GetTableSelectList()
        {
            ITableRepository tableRepository = new TableRepository();

            var items = tableRepository.GetTables().Where(x=>x.Active == null || x.Active.Value);
            var selectListItems = items.Select(item => new SelectListItem
            {
                Value = item.Id.ToString(),
                Text = item.Name
            }).ToList();

            return new SelectList(selectListItems, "Value", "Text");
        }

        public static List<Food> GetFoodSelectList()
        {
            IFoodRepository foodRepository = new FoodRepository();
            return foodRepository.GetValidFoods();
        }
    }
}

