using CamelDotNet.Models;
using CamelDotNet.Models.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using CamelDotNet.Models.Common;

namespace CamelDotNet.Controllers
{
    public class ApiController : Controller
    {
        private CamelDotNetDBContext db = new CamelDotNetDBContext();

        public ActionResult TestStation()
        {
            List<TestStation> result = db.TestStation.Where(a => a.IsDeleted == false).Include(a => a.Process).ToList();
            TestStationListXml testStaitonXmlList = new TestStationListXml();
            if(result.Count() != 0)
            {
                foreach(var testStation in result)
                {
                    TestStationXml testStationXml = new TestStationXml();
                    testStationXml.Id = testStation.Id;
                    testStationXml.Name = testStation.Name;
                    testStationXml.Process = testStation.Process.Name;
                    testStaitonXmlList.TestStationXmls.Add(testStationXml);
                }
            }

            return new XmlResult<TestStationListXml>()
            {
                Data = testStaitonXmlList
            };
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
	}
}