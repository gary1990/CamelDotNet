using CamelDotNet.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace CamelDotNet.Models
{
    public class TestConfig : IEditable<TestConfig>
    {
        public TestConfig() 
        {
            this.IsDeleted = false;
            this.TestItemConfigs = new List<TestItemConfig>() { };
        }
        public int Id { get; set; }
        [DisplayName("产品型号")]
        public int ProductTypeId { get; set; }
        public virtual ProductType ProductType { get; set; }
        [DisplayName("已删除")]
        public bool IsDeleted { get; set; }
        [DisplayName("客户")]
        public int ClientId { get; set; }
        public virtual Client Client { get; set; }
        public virtual ICollection<TestItemConfig> TestItemConfigs { get; set; }
        public void Edit(TestConfig model)
        {
            this.ProductTypeId = model.ProductTypeId;
            this.ClientId = model.ClientId;
        }

        public void Copy(TestConfig testConfig) 
        {
            this.ClientId = testConfig.ClientId;
            this.ProductTypeId = testConfig.ProductTypeId;
            this.TestItemConfigs = testConfig.TestItemConfigs;
        }
    }
}