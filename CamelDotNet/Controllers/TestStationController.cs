using CamelDotNet.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CamelDotNet.Controllers
{
    public class TestStationController : BaseModelController<TestStation>
    {
        List<string> path = new List<string>();
        public TestStationController()
        {
            path.Add("测试管理");
            path.Add("测试站点");
            ViewBag.path = path;
            ViewBag.Name = "测试站点";
            ViewBag.Title = "测试站点";
            ViewBag.Controller = "TestStation";
            ViewPath = "TestStation";
        }
	}
}