using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BookingTable.Business.IRepository;
using BookingTable.Entities.Entities;

namespace BookingTable.Business.Repository
{
    public class FoodRepository : IFoodRepository
    {
        private readonly BookingTableEntities _entities;

        public FoodRepository()
        {
            _entities = new BookingTableEntities();
        }

        //GET
        public Food Find(int id)
        {
            return _entities.Foods.FirstOrDefault(x => x.Id == id && x.Deleted != true);
        }
        public List<Food> GetFoods()
        {
            return _entities.Foods.Where(x => x.Deleted != true).ToList();
        }

        //SET
        public bool Save(Food entity)
        {
            try
            {
                _entities.Foods.AddOrUpdate(entity);

                _entities.SaveChanges();

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public List<Food> GetValidFoods()
        {
            try
            {
                return _entities.Foods.Where(x => x.Quantity > 0 && x.Active != false && x.Deleted != true).ToList();
            }
            catch (Exception)
            {

                return null;
            }
        }
    }
}
