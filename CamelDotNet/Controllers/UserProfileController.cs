using CamelDotNet.Lib;
using CamelDotNet.Models;
using CamelDotNet.Models.DAL;
using CamelDotNet.Models.Interfaces;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace CamelDotNet.Controllers
{
    public class UserProfileController : UserModelController<CamelDotNetUser>
    {
        public List<string> path = new List<string>();
        public UserProfileController() 
        {
            path.Add("系统管理");
            path.Add("用户管理");
            ViewBag.path = path;
            ViewBag.Name = "用户管理";
            ViewBag.Title = "用户管理";
            ViewBag.Controller = "UserProfile";  
        }
	}
}