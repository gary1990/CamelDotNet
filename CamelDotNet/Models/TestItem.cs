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
    public class TestItem : BaseModel, IEditable<TestItem>
    {
        [DisplayName("计算公式")]
        public string Formular { get; set; }
        public void Edit(TestItem model)
        {
            this.Name = model.Name;
        }
    }
}