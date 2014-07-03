using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace CamelDotNet.Models
{
    public class QualityPassRecord
    {
        public int Id { get; set; }
        [DisplayName("缆号")]
        public int VnaRecordId { get; set; }
        [DisplayName("质量工程师")]
        public string QeId { get; set; }
        [DisplayName("质量工程师意见")]
        public string QeSuggestion { get; set; }
        [DisplayName("责任部门")]
        public int? DepartmentId { get; set; }
        [DisplayName("技术工程师")]
        public string TechnologistId { get; set; }
        [DisplayName("技术部评审")]
        public string TechnologistSuggestion { get; set; }
        [DisplayName("质量经理")]
        public string QmId { get; set; }
        [DisplayName("质量经理审核")]
        public string QmSuggestion { get; set; }
        [DisplayName("总工")]
        public string HmId { get; set; }
        [DisplayName("总工审核")]
        public string HmSuggestion { get; set; }
        [DisplayName("需要总工审核")]
        public bool NeedHmApprove { get; set; }
        [DisplayName("放行")]//false is do no change to PASS, true is change to PASS
        public bool ChangePass { get; set; }
        public virtual VnaRecord VnaRecord { get; set; }
        public virtual CamelDotNetUser Qe { get; set;}
        public virtual CamelDotNetUser Technologist { get; set; }
        public virtual CamelDotNetUser Qm { get; set; }
        public virtual CamelDotNetUser Hm { get; set; }
        public virtual Department Department { get; set; }
    }
}