using CamelDotNet.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CamelDotNet.Controllers
{
    public class ProductTypeController : BaseModelController<ProductType>
    {
        List<string> path = new List<string>();
        public ProductTypeController()
        {
            path.Add("测试管理");
            path.Add("产品型号");
            ViewBag.path = path;
            ViewBag.Name = "产品型号";
            ViewBag.Title = "产品型号";
            ViewBag.Controller = "ProductType";
        }
	}
}