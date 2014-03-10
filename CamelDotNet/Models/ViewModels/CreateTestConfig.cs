using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace CamelDotNet.Models.ViewModels
{
    public class CreateTestConfig
    {
        [DisplayName("产品型号")]
        public int ProductTypeId { get; set; }
        [DisplayName("客户")]
        public int ClientId { get; set; }
        public virtual ProductType ProductType { get; set; }
        public virtual Client Client { get; set; }
        public virtual ICollection<CreateTestItemConfig> CreateTestItemConfigs { get; set; }
    }
}