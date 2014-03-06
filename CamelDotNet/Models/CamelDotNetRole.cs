using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CamelDotNet.Models
{
    public class CamelDotNetRole
    {
        public CamelDotNetRole() 
        {
            this.Permissions = new List<Permission>() { };
        }
        public int Id { get; set; }
        [Required]
        [DisplayName("名称")]
        public string Name { get; set; }
        [DisplayName("权限")]
        public virtual ICollection<Permission> Permissions { get; set; }
    }
}