using CamelDotNet.Models.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Xml.Serialization;

namespace CamelDotNet.Models
{
    public class PerConfig
    {
        public int Id { get; set; }
        public int TestItemConfigId { get; set; }
        [DisplayName("Channel")]
        public int Channel { get; set; }
        [DisplayName("Trace")]
        public int Trace { get; set; }
        [DisplayName("Start")]
        public decimal StartF { get; set; }
        public int StartUnitId { get; set; }
        [DisplayName("Stop")]
        public decimal StopF { get; set; }
        public int StopUnitId { get; set; }
        [DisplayName("扫描点数")]
        public int ScanPoint { get; set; }
        [DisplayName("扫描时间")]
        public decimal ScanTime { get; set; }
        [DisplayName("传输速率")]
        public decimal? TransportSpeed { get; set; }
        [DisplayName("频点")]
        public decimal? FreqPoint { get; set; }
        [DisplayName("极限值")]
        public decimal? LimitLine { get; set; }
        public virtual TestItemConfig TestItemConfig { get; set; }
        public virtual Unit StartUnit { get; set; }
        public virtual Unit StopUnit { get; set; }
    }
}