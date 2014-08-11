using CamelDotNet.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CamelDotNet.Models
{
    public class VnaRecord : IEditable<VnaRecord>
    {
        public VnaRecord() 
        {
            VnaTestItemRecords = new List<VnaTestItemRecord> { };
            TestResult = false;
            BarCodeUsed = false;
            NoStatistics = false;
            isGreenLight = false;
            reTest = false;
        }
        public int Id { get; set; }
        [Required]
        [DisplayName("序列号")]
        public int SerialNumberId { get; set; }
        [Required]
        [DisplayName("产品型号")]
        public int ProductTypeId { get; set; }
        [Required]
        [DisplayName("测试员")]
        public string CamelDotNetUserId { get; set; }
        [Required]
        [DisplayName("测试设备")]
        public int TestEquipmentId { get; set; }
        [Required]
        [DisplayName("测试站")]
        public int TestStationId { get; set; }
        [Required]
        [DisplayName("工单号")]
        public string OrderNumber { get; set; }
        [Required]
        [DisplayName("订单编号")]
        public string OrderNo { get; set; }
        [Required]
        [DisplayName("机台")]
        public string DrillingCrew { get; set; }
        [Required]
        [DisplayName("盘号")]
        public string ReelNumber { get; set; }
        [Required]
        [DisplayName("班组")]
        public string WorkGroup { get; set; }
        [Required]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd HH:mm:ss}", ApplyFormatInEditMode = true)]
        [DisplayName("测试时间")]
        public DateTime TestTime { get; set; }
        [DisplayName("测试结果")]
        public bool TestResult { get; set; }
        [DisplayName("温度")]
        public decimal Temperature { get; set; }
        [DisplayName("内端计米")]
        public decimal InnerLength { get; set; }
        [DisplayName("外端计米")]
        public decimal OuterLength { get; set; }
        [DisplayName("客户")]
        public int ClientId { get; set; }
        [DisplayName("放行")]
        public bool isGreenLight { get; set; }
        [DisplayName("不统计")]
        public bool NoStatistics { get; set; }
        [DisplayName("重测")]
        public bool reTest { get; set; }
        [DisplayName("备注")]
        public string Remark { get; set; }
        [DisplayName("条码使用与否")]
        public bool BarCodeUsed { get; set; }

        public virtual ICollection<VnaTestItemRecord> VnaTestItemRecords { get; set; }

        public virtual SerialNumber SerialNumber { get; set; }
        public virtual ProductType ProductType { get; set; }
        public virtual CamelDotNetUser CamelDotNetUser { get; set; }
        public virtual TestStation TestStation { get; set; }
        public virtual TestEquipment TestEquipment { get; set; }
        public virtual Client Client { get; set; }

        public void Edit(VnaRecord model)
        {
            this.ReelNumber = model.ReelNumber;
        }
    }
}