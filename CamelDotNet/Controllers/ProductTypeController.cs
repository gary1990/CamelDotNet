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
            try
            {
                var connection = new SqlConnection("Data Source=10.10.2.150,1433;Network Library=DBMSSOCN;Initial Catalog=AIS20121207075920;User ID=sa;Password=Hengxin8454;");
                connection.Open();
                SqlCommand command = connection.CreateCommand();
                command.CommandText = "SELECT FNumber AS KNumber,FName AS Name,FModel AS ModelName,FDeleted AS IsDeleted FROM vICItem WHERE FItemClassID = 4 AND FDetail = 1 AND FDeleted = 0 AND (FNumber LIKE '3%' OR FNumber LIKE '2%')";
                SqlDataReader reader = command.ExecuteReader();

                using (var scope = new TransactionScope())
                {
                    try
                    {
                        while (reader.Read())
                        {
                            bool isDeleted = false;
                            if (Convert.ToInt32(reader["IsDeleted"]) == 1)
                            {
                                isDeleted = true;
                            }
                            var kNumber = Convert.ToString(reader["KNumber"]);
                            var name = Convert.ToString(reader["Name"]);
                            var modelName = Convert.ToString(reader["ModelName"]);
                            var existProductType = db.ProductType.Where(a => a.Knumber == kNumber).SingleOrDefault();
                            if (existProductType == null)
                            {
                                ProductType pt = new ProductType
                                {
                                    Knumber = kNumber,
                                    Name = name,
                                    ModelName = modelName,
                                    isLocal = false,
                                    IsDeleted = isDeleted,
                                    Price = 0
                                };
                                db.ProductType.Add(pt);
                            }
                            else
                            {
                                //如果信息有变动，更新
                                if (existProductType.Name != name || existProductType.ModelName != modelName || existProductType.isLocal != false || existProductType.IsDeleted != isDeleted)
                                {
                                    existProductType.Name = name;
                                    existProductType.ModelName = modelName;
                                    existProductType.isLocal = false;
                                    existProductType.IsDeleted = isDeleted;
                                }
                            }
                        }
                        reader.Close();
                        
                        db.SaveChanges();
                    }
                    catch (Exception e)
                    {
                        throw e;
                    }

                    scope.Complete();
                }
            }
            catch (Exception ex) 
            {
                throw ex;
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

        private bool IsChinese(string text) 
        {
            return text.Any(a => a >= 0x20000 && a <= 0xFA2D);
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
	}
}