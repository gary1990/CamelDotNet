using CamelDotNet.Models.Base;
using CamelDotNet.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CamelDotNet.Models
{
    public class ProductType : BaseModel,IEditable<ProductType>
    {
        public ProductType() 
        {
            isLocal = true;
        }
        [DisplayName("本地")]
        public bool isLocal { get; set; }
        public void Edit(ProductType model)
        {
            this.Name = model.Name;
        }
    }
}