using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CamelDotNet.Controllers
{
    public class QualityTracingHomeController : Controller
    {
        List<string> path = new List<string>();

        public QualityTracingHomeController() 
        {
            path.Add("质量追溯");
        }
        public ActionResult Index()
        {
            ViewBag.path = path;
            return View();
        }
	}
}