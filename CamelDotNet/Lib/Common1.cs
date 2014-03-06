using CamelDotNet.Models.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.Identity;

namespace CamelDotNet.Lib
{
    public class Common1
    {
        public static void CheckLogin()
        {
            if (!HttpContext.Current.User.Identity.IsAuthenticated)
            {
                throw new Exception("没有登陆!");
            }
        }

        public static string GetCurUserId()
        {
            CheckLogin();
            if (HttpContext.Current.Session["UserId"] == null)
            {
                HttpContext.Current.Session["UserId"] = HttpContext.Current.User.Identity.GetUserId();
            }
            return (string)(HttpContext.Current.Session["UserId"]);
        }

        public static string GetCurSystemRole()
        {
            CheckLogin();
            if (HttpContext.Current.Session["CamelRole"] == null)
            {
                using (var db = new CamelDotNetDBContext())
                {
                    HttpContext.Current.Session["CamelRole"] = db.UserManager.FindById(GetCurUserId()).CamelDotNetRole.Name;
                }
            }

            return (string)HttpContext.Current.Session["CamelRole"];
        }

        public static List<string> GetCurPermissionList()
        {
            CheckLogin();
            if (HttpContext.Current.Session["PermissionList"] == null)
            {
                using (var db = new CamelDotNetDBContext())
                {
                    var camleUser = db.UserManager.FindById(GetCurUserId());
                    var results = camleUser.CamelDotNetRole.Permissions.Select(a => a.ControllerName + "_" + a.ActionName).ToList();
                    HttpContext.Current.Session["PermissionList"] = results;
                }
            }

            return (List<string>)HttpContext.Current.Session["PermissionList"];
        }
    }
}