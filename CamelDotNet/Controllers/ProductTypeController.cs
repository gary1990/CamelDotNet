using CamelDotNet.Lib;
using CamelDotNet.Models;
using CamelDotNet.Models.DAL;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace CamelDotNet.Controllers
{
    public class ProductTypeController : BaseModelController<ProductType>
    {
        private CamelDotNetDBContext db = new CamelDotNetDBContext();
        List<string> path = new List<string>();
        public ProductTypeController()
        {
            path.Add("测试管理");
            path.Add("产品型号");
            ViewBag.path = path;
            ViewBag.Name = "产品型号";
            ViewBag.Title = "产品型号";
            ViewBag.Controller = "ProductType";
            ViewPath = "ProductType";
        }

        [HttpPost]
        public virtual PartialViewResult SyncProductType(string returnRoot, string actionAjax = "", int page = 1, bool includeSoftDeleted = false, string filter = null)
        {
            using (var scope = new TransactionScope())
            {
                try
                {
                    var existRecords = db.ProductType.Where(a => a.isLocal == false);
                    if (existRecords != null)
                    {
                        foreach (var record in existRecords)
                        {
                            db.ProductType.Remove(record);
                        }
                    }
                    
                    using (var connection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["CamelDotNetK3"].ConnectionString))
                    {
                        using (SqlCommand command = connection.CreateCommand())
                        {
                            command.CommandText = "SELECT DISTINCT a.FModel AS Name,a.FDeleted AS IsDeleted FROM vICItem a WHERE a.FModel IS NOT null GROUP BY a.FModel, a.FDeleted";
                            connection.Open();
                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    bool isDeleted = false;
                                    if (Convert.ToInt32(reader["IsDeleted"]) == 1)
                                    {
                                        isDeleted = true;
                                    }
                                    ProductType pt = new ProductType { Name = Convert.ToString(reader["Name"]), isLocal = false, IsDeleted = isDeleted };
                                    db.ProductType.Add(pt);  
                                }
                                reader.Close();
                            }
                            connection.Close();
                        }
                    }

                    db.SaveChanges();
                }
                catch (Exception e)
                {
                    throw e;
                }
                scope.Complete();  
            } 

            var results = BaseCommon<ProductType>.GetQuery(UW, includeSoftDeleted, filter);

            if (!includeSoftDeleted)
            {
                results = results.Where(a => a.IsDeleted == false);
            }

            results = results.OrderBy(a => a.Name);

            var rv = new RouteValueDictionary { { "tickTime", DateTime.Now.ToLongTimeString() }, { "returnRoot", returnRoot }, { "actionAjax", actionAjax }, { "page", page }, { "includeSoftDeleted", includeSoftDeleted }, { "filter", filter } };
            return PartialView(ViewPath1 + ViewPath + ViewPath2 + "Get.cshtml", Common<ProductType>.Page(this, rv, results));
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
	}
}