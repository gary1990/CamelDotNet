using CamelDotNet.Models.Base;
using CamelDotNet.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CamelDotNet.Models
{
    public class Department : BaseModel, IEditable<Department>
    {
        public void Edit(Department model)
        {
            Name = model.Name;
        }
    }
}