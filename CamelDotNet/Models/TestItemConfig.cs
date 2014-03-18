using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Xml.Serialization;

namespace CamelDotNet.Models
{
    public class TestItemConfig
    {
        public TestItemConfig()
        {
            this.VersionDate = DateTime.Now;
            this.PerConfigs = new List<PerConfig>() { };
        }
        public int Id { get; set; }
        public int TestConfigId { get; set; }
        [Required]
        public int TestItemId { get; set; }
        public DateTime VersionDate { get; set; }
        public virtual TestConfig TestConfig { get; set; }
        public virtual TestItem TestItem { get; set; }
        public virtual ICollection<PerConfig> PerConfigs { get; set; }
    }
}