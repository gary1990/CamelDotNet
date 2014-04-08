using CamelDotNet.Models.Interfaces;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CamelDotNet.Models
{
    public class CamelDotNetUser : IdentityUser,IEditable<CamelDotNetUser>
    {
        public CamelDotNetUser() 
        {
            IsDeleted = false;
        }
        [Required]
        [DisplayName("工号")]
        [MaxLength(20)]
        public string JobNumber { get; set; }
        [DisplayName("已删除")]
        public bool IsDeleted { get; set; }
        [DisplayName("角色")]
        public int CamelDotNetRoleId { get; set; }
        [DisplayName("部门")]
        public int DepartmentId { get; set; }
        public virtual CamelDotNetRole CamelDotNetRole { get; set; }
        public virtual Department Department { get; set; }
        public void Edit(CamelDotNetUser model)
        {
            this.JobNumber = model.JobNumber;
            this.UserName = model.UserName;
            this.CamelDotNetRoleId = model.CamelDotNetRoleId;
        }
    }
}