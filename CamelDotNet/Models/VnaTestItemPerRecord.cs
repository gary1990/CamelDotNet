using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
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
        [DisplayFormat(DataFormatString = "{0:0.#####}", ApplyFormatInEditMode = true)]
        public decimal? XValue { get; set; }
        [DisplayName("测试值")]
        [DisplayFormat(DataFormatString = "{0:0.#####}", ApplyFormatInEditMode = true)]
        public decimal? YValue { get; set; }
        [DisplayName("计算值")]
        [DisplayFormat(DataFormatString = "{0:0.#####}", ApplyFormatInEditMode = true)]
        public decimal? RValue { get; set; }
        [DisplayName("测试结果")]
        public bool TestitemPerResult { get; set; }
        public virtual VnaTestItemRecord VnaTestItemRecord { get; set; }
    }
}