using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Xml.Serialization;

namespace CamelDotNet.Models.Base
{
    public class BaseModel
    {
        public BaseModel() 
        {
            this.IsDeleted = false;
        }
        public int Id { get; set; }
        [DisplayName("名称")]
        [Required]
        [MaxLength(255)]
        public virtual string Name { get; set; }
        [DisplayName("已删除")]
        public bool IsDeleted { get; set; }
    }
}