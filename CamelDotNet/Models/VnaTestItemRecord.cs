using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace CamelDotNet.Models
{
    public class VnaTestItemRecord
    {
        public int Id { get; set; }
        public int VnaRecordId { get; set; }
        [DisplayName("测试项")]
        public int TestItemId { get; set; }
        [DisplayName("测试结果")]
        public bool TestItemResult { get; set; }
        [DisplayName("测试截图")]
        public string ImagePath { get; set; }
        public virtual ICollection<VnaTestItemPerRecord> VnaTestItemPerRecords { get; set; }
        public virtual VnaRecord VnaRecord { get; set; }
        public virtual TestItem TestItem { get; set;}
    }
}