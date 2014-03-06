using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CamelDotNet.Controllers
{
    public class SystemHomeController : Controller
    {
        List<string> path = new List<string>();
        public SystemHomeController() 
        {
            path.Add("系统管理");
        }
        public ActionResult Index()
        {
            ViewBag.path = path;
            return View();
        }
	}
}