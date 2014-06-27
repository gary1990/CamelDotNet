using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using CamelDotNet.Models;
using CamelDotNet.Models.DAL;
using CamelDotNet.Controllers.BaseController;

namespace CamelDotNet.Controllers
{
    public class QualityLossController : QualityLossModelController<QualityLoss>
    {
        public List<string> path = new List<string>();
        public QualityLossController()
        {
            path.Add("报表管理");
            path.Add("质量损失比");
            ViewBag.path = path;
            ViewBag.Name = "质量损失比";
            ViewBag.Title = "质量损失比";
            ViewBag.Controller = "QualityLoss";  
        }
    }
}
