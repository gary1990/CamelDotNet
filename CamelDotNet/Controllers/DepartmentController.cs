using CamelDotNet.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CamelDotNet.Controllers
{
    public class DepartmentController : BaseModelController<Department>
    {
        List<string> path = new List<string>();
        public DepartmentController()
        {
            path.Add("系统管理");
            path.Add("部门");
            ViewBag.path = path;
            ViewBag.Name = "部门";
            ViewBag.Title = "部门";
            ViewBag.Controller = "Department";
        }
    }
}