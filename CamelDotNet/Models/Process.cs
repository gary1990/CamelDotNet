using CamelDotNet.Models.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CamelDotNet.Models
{
    public class Process:BaseModel
    {
        public virtual ICollection<TestStation> TestStations { get; set; }
    }
}