using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CamelDotNet.Models
{
    public class Permission
    {
        public Permission() 
        {
            this.CamelDotNetRoles = new List<CamelDotNetRole>() { };
        }
        public int Id { get; set; }
        [DisplayName("权限名称")]
        [Required,StringLength(256)]
        public string Name { get; set; }
        [DisplayName("Controller")]
        [Required, StringLength(256)]
        public string ControllerName { get; set; }
        [DisplayName("Action")]
        [Required, StringLength(256)]
        public string ActionName { get; set; }
        public virtual ICollection<CamelDotNetRole> CamelDotNetRoles { get; set; }
    }
}