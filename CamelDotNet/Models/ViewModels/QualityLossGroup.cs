using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CamelDotNet.Models.ViewModels
{
    public class QualityLossGroup
    {
        public string ProcessName{ get; set; }
        public string TestItemName{ get; set; }
        public int QualityLossId{ get; set; }
        public int QualityLossPercentId{ get; set; }
        public string FreqFormularR{ get; set; }
        public string ValueFormularR { get; set; }
        public int RowNum { get; set; }
        public int CellNum { get; set; }
        public decimal PerFailLengthTotal { get; set; }
        public QualityLossGroup() {
            //because row in excel is alwals 4
            this.RowNum = 3;
        }
    }
}