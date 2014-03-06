using CamelDotNet.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CamelDotNet.Controllers
{
    public class TestItemController : BaseModelController<TestItem>
    {
        List<string> path = new List<string>();
        public TestItemController()
        {
            path.Add("测试管理");
            path.Add("测试项");
            ViewBag.path = path;
            ViewBag.Name = "测试项";
            ViewBag.Title = "测试项";
            ViewBag.Controller = "TestItem";
            ViewPath = "TestItem";
        }
	}
}