using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CamelDotNet.Models.ViewModels
{
    public class CreateTestItemConfig
    {
        public CreateTestItemConfig() 
        {
            this.VersionNo = DateTime.Now;
        }
        public DateTime VersionNo { get; set; }
        public virtual TestConfig TestConfig { get; set; }
        public virtual ICollection<CreatePerCofig> PerConfigs { get; set; }
    }
}