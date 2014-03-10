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
    public class Permission : BaseModel,IEditable<Permission>
    {
        public Permission() 
        {
            this.CamelDotNetRoles = new List<CamelDotNetRole>() { };
        }
        [DisplayName("Controller")]
        [Required, StringLength(256)]
        public string ControllerName { get; set; }
        [DisplayName("Action")]
        [Required, StringLength(256)]
        public string ActionName { get; set; }
        public virtual ICollection<CamelDotNetRole> CamelDotNetRoles { get; set; }

        public void Edit(Permission model)
        {
            Name = model.Name;
            ControllerName = model.ControllerName;
            ActionName = model.ActionName;
        }
    }
}