using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CamelDotNet.Models.ViewModels
{
    public class VnaTotalResultExcel
    {
        public DateTime TestDate{ get; set; }
		public string DrillingCrew{ get; set; }
		public string WorkGroup{ get; set; }
		public string ProductFullName{ get; set; }
		public decimal TotalLength{ get; set; }
		public decimal TotalFailLength{ get; set; }
		public decimal PassPercent{ get; set; }
		public decimal Price{ get; set; }
		public string TestItemName_Fail{ get; set; }
		public string ProcessName_Fail{ get; set; }
		public int QualityLossId_Result{ get; set; }
		public int QualityLossPercentId_Result{ get; set; }
		public string FreqFormularR{ get; set; }
		public string ValueFormularR{ get; set; }
        public decimal PerFailLength { get; set; }
    }
}