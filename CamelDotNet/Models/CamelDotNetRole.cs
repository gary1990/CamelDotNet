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
    public class CamelDotNetRole : BaseModel, IEditable<CamelDotNetRole>
    {
        public CamelDotNetRole()
        {
            this.Permissions = new List<Permission>() { };
        }
        [DisplayName("权限")]
        public virtual ICollection<Permission> Permissions { get; set; }

        public void Edit(CamelDotNetRole model)
        {
            Permissions = model.Permissions;
        }
    }
}