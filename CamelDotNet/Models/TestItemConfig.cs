using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CamelDotNet.Models
{
    public class TestItemConfig
    {
        public TestItemConfig()
        {
            this.version = DateTime.Now;
            this.PerConfigs = new List<PerConfig>() { };
        }
        public int Id { get; set; }
        public int TestConfigId { get; set; }
        public DateTime version { get; set; }
        public virtual TestConfig TestConfig { get; set; }
        public virtual ICollection<PerConfig> PerConfigs { get; set; }
    }
}