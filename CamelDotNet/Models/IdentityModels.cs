using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace CamelDotNet.Models
{
    public class CamelDotNetUser : IdentityUser
    {
        [DisplayName("角色")]
        public int CamelDotNetRoleId { get; set; }
        public virtual CamelDotNetRole CamelDotNetRole { get; set; }
    }
}