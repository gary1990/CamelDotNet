using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CamelDotNet.Models
{
    public class QualityLossPercent
    {
        public int Id { get; set; }
        public int QualityLossId { get; set; }
        [RegularExpression("^(\\(([0-9]+(\\.[0-9]+)?)?\\,([0-9]+(\\.[0-9]+)?)?\\))+$", ErrorMessage = "格式为(a,b)或(a,)或(,b)中的一组或多组")]
        [DisplayName("频点范围")]
        public string QualityLossFreq { get; set; }
        [RegularExpression("^\\(([0-9]+(\\.[0-9]+)?)?\\,([0-9]+(\\.[0-9]+)?)?\\)$", ErrorMessage = "输入格式为(a,b)或(a,)或(,b)")]
        [DisplayName("值范围")]
        public string QualityLossRef { get; set; }
        [DisplayName("质量损失比")]
        public decimal LossValue { get; set; }
        public virtual QualityLoss QualityLoss { get; set; }
    }
}