using CamelDotNet.Models.Base;
using CamelDotNet.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Xml.Serialization;

namespace CamelDotNet.Models
{
    public class TestItem : BaseModel, IEditable<TestItem>
    {
        [DisplayName("类别")]
        public int TestItemCategoryId { get; set; }
        [DisplayName("计算公式")]
        public string Formular { get; set; }
        public virtual TestItemCategory TestItemCategory { get; set; }
        public void Edit(TestItem model)
        {
            this.TestItemCategoryId = model.TestItemCategoryId;
            this.Name = model.Name;
        }
    }
}