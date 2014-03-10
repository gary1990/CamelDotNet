using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace CamelDotNet.Models.ViewModels
{
    public class CreatePerCofig
    {
        [DisplayName("Channel")]
        public int? Channel { get; set; }
        [DisplayName("Trace")]
        public int? Trace { get; set; }
        [DisplayName("开始频率")]
        public decimal StartF { get; set; }
        [DisplayName("截止频率")]
        public decimal StopF { get; set; }
        [DisplayName("扫描点数")]
        public decimal ScanPoint { get; set; }
        [DisplayName("扫描时间")]
        public decimal ScanTime { get; set; }
        [DisplayName("传输速率")]
        public decimal TransportSpeed { get; set; }
        [DisplayName("极限值")]
        public decimal LimitLine { get; set; }
    }
}