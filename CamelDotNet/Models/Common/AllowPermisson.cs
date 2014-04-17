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
                "Api_TestBarCode",
                "Api_BarCodeUsed",
                "Api_Client",
                "VnaTestRecord_UploadVnaRecord",
                "QualityPassRecord_ExportToExcel",
                "VnaTestRecord_TestZip",
            };
        }
        public List<string> Allowed { get; set; }
    }
}