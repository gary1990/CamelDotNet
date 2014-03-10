using CamelDotNet.Models.Base;
using CamelDotNet.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CamelDotNet.Models
{
    public class ProductType : BaseModel,IEditable<ProductType>
    {
        public void Edit(ProductType model)
        {
            this.Name = model.Name;
        }
    }
}