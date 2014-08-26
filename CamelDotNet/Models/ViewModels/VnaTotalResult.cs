using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CamelDotNet.Models.ViewModels
{
    public class VnaTotalResult
    {
        public int VnaRecordId { get; set; }
        public DateTime TestTime { get; set; }
        public DateTime TestDate { get; set; }
        public string DrillingCrew { get; set; }
        public string WorkGroup { get; set; }
        public int TestResult { get; set; }
        public decimal InnerLength { get; set; }
        public decimal Lengths { get; set; }
        public decimal OuterLength { get; set; }
        public int TestStationId { get; set; }
        public string TestStaion { get; set; }
        public int TestStationProcessId { get; set; }
        public string TestStationProcess { get; set; }
        public int ProductTypeId { get; set; }
        public decimal Price { get; set; }
        public string ProductFullName { get; set; }
        public string SerialNum { get; set; }
        public int? VnaRecordId_Result { get; set; }
        public decimal? LossPercent_Result { get; set; }
        public int? TestItemId_Fail { get; set; }
        public string TestItemName_Fail { get; set; }
        public int? ProcessId_Fail { get; set; }
        public string ProcessName_Fail { get; set; }
        public int? QualityLossId_Result { get; set; }
        public int? QualityLossPercentId_Result { get; set; }
        public string FreqFormularR { get; set; }
        public string ValueFormularR { get; set; }
    }
}