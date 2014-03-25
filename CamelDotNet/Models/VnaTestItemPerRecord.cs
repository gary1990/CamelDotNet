using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace CamelDotNet.Models
{
    public class VnaTestItemPerRecord
    {
        public VnaTestItemPerRecord() 
        {
            TestitemPerResult = false;
        }
        public int Id { get; set; }
        public int VnaTestItemRecordId { get; set; }
        [DisplayName("频点")]
        public string XValue { get; set; }
        [DisplayName("测试值")]
        public string YValue { get; set; }
        [DisplayName("计算值")]
        public string RValue { get; set; }
        [DisplayName("测试结果")]
        public bool TestitemPerResult { get; set; }
        public virtual VnaTestItemRecord VnaTestItemRecord { get; set; }
    }
}