using CamelDotNet.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CamelDotNet.Controllers
{
    public class TestConfigController : TestConfigModelController<TestConfig>
    {
        public List<string> path = new List<string>();
        public TestConfigController() 
        {
            path.Add("测试管理");
            path.Add("测试方案");
            ViewBag.path = path;
            ViewBag.Name = "测试方案";
            ViewBag.Title = "测试方案";
            ViewBag.Controller = "TestConfig";  
        }
	}
}