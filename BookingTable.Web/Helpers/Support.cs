using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using BookingTable.Business.IRepository;
using BookingTable.Business.Repository;
using BookingTable.Entities.Entities;
using BookingTable.Entities.Enum;
using BookingTable.Web.Security;
using System.IO;
using System.Web.Caching;

namespace BookingTable.Web.Helpers
{
    public static class Support
    {
        [AdminAuthorized]
        public static string SavePhoto(HttpPostedFileBase file, string name, string path)
        {
            name += ".png";
            file.SaveAs(path+"/"+name);

            return name;
        }
        [AdminAuthorized]
        public static string GetPathPhotoFolder()
        {
            return "/Content/Uploads/Photos";
        }
        [AdminAuthorized]
        public static Admin GetCurrentAdmin()
        {
            return (Admin) HttpContext.Current.Session["Admin"];
        }
        [UserAuthorized]
        public static Customer GetCurrentUser()
        {
            return (Customer)HttpContext.Current.Session["User"];
        }
        public static void ClearApplicationCache()
        {
            HttpContext.Current.Response.Cache.SetExpires(DateTime.UtcNow.AddDays(-1));
            HttpContext.Current.Response.Cache.SetValidUntilExpires(false);
            HttpContext.Current.Response.Cache.SetRevalidation(HttpCacheRevalidation.AllCaches);
            HttpContext.Current.Response.Cache.SetCacheability(HttpCacheability.NoCache);
            HttpContext.Current.Response.Cache.SetNoStore();

        }
    }
}