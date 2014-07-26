using CamelDotNet.Models.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CamelDotNet.Models.ViewModels
{
    public class CopyBat
    {
        public CopyBat(int clientId) 
        {
            CamelDotNetDBContext db = new CamelDotNetDBContext();
            var existProduct = db.TestConfig.Where(a => a.ClientId == clientId).Select(a => a.ProductTypeId).ToList();
            ProductTypes = db.ProductType.Where(a => a.Knumber.StartsWith("2.1") || a.Knumber.StartsWith("3.1") || a.Knumber == "000000")
                .Where(a => !existProduct.Contains(a.Id)).OrderBy(a => a.Name).ToList();
        }
        public int TestConfigId { get; set; }
        public virtual ICollection<ProductType> ProductTypes { get; set; }
    }
}