using CamelDotNet.Models;
using CamelDotNet.Models.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using CamelDotNet.Models.Common;
using System.Data.SqlClient;
using System.Data;
using System.Transactions;

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

        public ActionResult Client() 
        {
            List<Client> result = db.Client.Where(a => a.IsDeleted == false).ToList();
            ClientListXml clientXmlList = new ClientListXml();
            if (result.Count() != 0)
            {
                foreach (var client in result)
                {
                    ClientXml clientXml = new ClientXml();
                    clientXml.Id = client.Id;
                    clientXml.Name = client.Name;
                    clientXmlList.ClientXmls.Add(clientXml);
                }
            }

            return new XmlResult<ClientListXml>()
            {
                Data = clientXmlList
            };
        }

        public ActionResult BarCodeUsed(string serialNumber = null)
        {
            
            SingleResultXml result = new SingleResultXml()
            {
                Message = "true"
            };
            serialNumber = serialNumber.ToString().Trim();
            if (serialNumber != null && serialNumber != "")
            {
                try 
                {
                    var serialNumberRecord = db.SerialNumber.Where(a => a.Number == serialNumber).SingleOrDefault();
                    if (serialNumberRecord != null)
                    {
                        using(var scope = new TransactionScope())
                        {
                            try
                            {
                                var vnaRecords = db.VnaRecord.Where(a => a.SerialNumberId == serialNumberRecord.Id).ToList();
                                foreach(var item in vnaRecords)
                                {
                                    item.BarCodeUsed = true;
                                }
                                db.SaveChanges();
                            }
                            catch(Exception)
                            {
                                result.Message = "更新BarCodeUsed失败";
                            }
                            scope.Complete();
                        } 
                    }
                    else
                    {
                        result.Message = "数据库中无此缆号";
                    }
                }
                catch(Exception)
                {
                    result.Message = "数据库查询缆号失败";
                }
            }
            else 
            {
                result.Message = "缆号不能为空";
            }

            return new XmlResult<SingleResultXml>()
            {
                Data = result
            };
        }

        public void TestBarCode() 
        {
            DataTable table = new DataTable();

            using (SqlConnection connection = new SqlConnection("Data Source=localhost;Initial Catalog=CamelDotNet;User ID=BarCode;Password=BarCode; Integrated Security=True; MultipleActiveResultSets=True"))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT * from TestResult_View";

                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Response.Write(reader[0].ToString()+"=="+reader[1].ToString()+"<br/>");
                        }
                    }
                    connection.Close();
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
	}
}