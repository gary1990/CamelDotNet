using CamelDotNet.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CamelDotNet.Controllers
{
    public class ClientController : BaseModelController<Client>
    {
        List<string> path = new List<string>();
        public ClientController()
        {
            path.Add("测试管理");
            path.Add("客户");
            ViewBag.path = path;
            ViewBag.Name = "客户";
            ViewBag.Title = "客户";
            ViewBag.Controller = "Client";
        }
	}
}