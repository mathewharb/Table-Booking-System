﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BookingTable.Entities.Entities;

namespace BookingTable.Business.IRepository
{
    public interface IFoodRepository
    {
        Food Find(int id);
        List<Food> GetFoods();
        bool Save(Food entity);
        List<Food> GetValidFoods();
    }
}
