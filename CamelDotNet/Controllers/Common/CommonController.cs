using CamelDotNet.Models;
using CamelDotNet.Models.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CamelDotNet.Controllers
{
    public class CommonController : Controller
    {
        [ChildActionOnly]
        public ActionResult TestConfigAbstract(int id)
        {
            CamelDotNetDBContext db = new CamelDotNetDBContext();
            TestConfig testConfig = db.TestConfig.Where(a => a.Id == id).SingleOrDefault();
            return View("_TestConfigAbstract", testConfig);
        }

        public static SelectList getTestItem() 
        {
            using (CamelDotNetDBContext db = new CamelDotNetDBContext())
            {
                Dictionary<int, string> cd = (from a in db.TestItem 
                                              where a.IsDeleted == false 
                                              select a).ToDictionary(a => a.Id, a => a.Name);
                return new SelectList(cd,"Key","Value");
            }
        }
	}
}