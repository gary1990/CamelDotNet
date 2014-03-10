using CamelDotNet.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CamelDotNet.Controllers
{
    public class PermissionController : BaseModelController<Permission>
    {
        List<string> path = new List<string>();
        public PermissionController()
        {
            path.Add("系统管理");
            path.Add("权限管理");
            ViewBag.path = path;
            ViewBag.Name = "权限管理";
            ViewBag.Title = "权限管理";
            ViewBag.Controller = "Permission";
            ViewPath = "Permission";
        }
	}
}