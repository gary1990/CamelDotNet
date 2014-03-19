using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CamelDotNet.Models.Common
{
    public class AllowPermisson
    {
        public AllowPermisson() 
        {
            this.Allowed = new List<string> 
            {
                "TestConfig_GetTestConfig",
                "Api_TestStation",
            };
        }
        public List<string> Allowed { get; set; }
    }
}