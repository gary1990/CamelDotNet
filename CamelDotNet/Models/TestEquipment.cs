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
    public class TestEquipment : BaseModel, IEditable<TestEquipment>
    {
        [Required]
        [DisplayName("序列号")]
        [MaxLength(50)]
        public string Serialnumber { get; set; }
        public void Edit(TestEquipment model)
        {
            Name = model.Name;
            Serialnumber = model.Serialnumber;
        }
    }
}