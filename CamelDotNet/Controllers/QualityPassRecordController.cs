using CamelDotNet.Controllers.BaseController;
using CamelDotNet.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CamelDotNet.Controllers
{
    public class QualityPassRecordController : QualityPassRecordModelController<QualityPassRecord>
    {
        public List<string> path = new List<string>();
        public QualityPassRecordController()
        {
            path.Add("报表管理");
            path.Add("质量放行");
            ViewBag.path = path;
            ViewBag.Name = "质量放行";
            ViewBag.Title = "质量放行";
            ViewBag.Controller = "QualityPassRecord";  
        }
	}
}