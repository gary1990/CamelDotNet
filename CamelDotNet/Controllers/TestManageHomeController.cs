using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CamelDotNet.Controllers
{
    public class TestManageHomeController : Controller
    {
        List<string> path = new List<string>();
        public TestManageHomeController() 
        {
            path.Add("测试管理");
        }
        public ActionResult Index()
        {
            ViewBag.path = path;
            return View();
        }
	}
}