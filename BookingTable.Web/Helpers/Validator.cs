using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using BookingTable.Web.Security;

namespace BookingTable.Web.Helpers
{
    [AdminAuthorized]
    public static class Validator
    {
        public static bool Validate(Object obj)
        {
            try
            {
                var results = new ValidationContext(obj);
                System.ComponentModel.DataAnnotations.Validator.ValidateObject(obj, results, true);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
            
        }
    }
}