﻿using CamelDotNet.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CamelDotNet.Controllers
{
    public class TestEquipmentController : BaseModelController<TestEquipment>
    {
        List<string> path = new List<string>();
        public TestEquipmentController()
        {
            path.Add("测试管理");
            path.Add("测试设备");
            ViewBag.path = path;
            ViewBag.Name = "测试设备";
            ViewBag.Title = "测试设备";
            ViewBag.Controller = "TestEquipment";
            ViewPath = "TestEquipment";
        }
	}
}