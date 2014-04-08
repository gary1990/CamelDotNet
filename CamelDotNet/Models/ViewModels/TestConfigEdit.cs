using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace CamelDotNet.Models.ViewModels
{
    public class TestConfigEdit
    {
        public int Id { get; set; }
        [DisplayName("产品型号")]
        public int ProductTypeId { get; set; }
        [DisplayName("测试方案")]
        public int ClientId { get; set; }

        public bool IsDeleted { get; set; }
        public virtual ICollection<TestItemConfig> TestItemConfigs { get; set; }

        [DisplayName("测试项配置")]
        public virtual ICollection<TestItemConfigEdit> TestItemConfigEdits { get; set; }
    }
}