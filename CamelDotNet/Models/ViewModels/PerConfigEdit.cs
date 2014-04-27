using CamelDotNet.Models.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CamelDotNet.Models.ViewModels
{
    public class PerConfigEdit
    {
        public int Id { get; set; }
        public int TestItemConfigId { get; set; }
        [DisplayName("Channel")]
        [Required(ErrorMessage = "Channel不能为空")]
        [RegularExpression("^[1-4]{1}$", ErrorMessage = "请输入1/2/3/4中一个")]
        public int Channel { get; set; }
        [DisplayName("Trace")]
        [Required(ErrorMessage = "Trace不能为空")]
        [RegularExpression("^[1-2]{1}$", ErrorMessage = "请输入1/2中一个")]
        public int Trace { get; set; }
        [DisplayName("Start")]
        [Required(ErrorMessage = "Start不能为空")]
        [DisplayFormat(DataFormatString = "{0:0.#####}", ApplyFormatInEditMode = true)]
        public decimal StartF { get; set; }
        [DisplayName("单位")]
        public int StartUnitId { get; set; }
        [DisplayName("Stop")]
        [Required(ErrorMessage = "Stop不能为空")]
        [DisplayFormat(DataFormatString = "{0:0.#####}", ApplyFormatInEditMode = true)]
        public decimal StopF { get; set; }
        [DisplayName("单位")]
        public int StopUnitId { get; set; }
        [DisplayName("扫描点数")]
        [Required(ErrorMessage = "扫描点数不能为空")]
        [RegularExpression("^[1-9][0-9]*$", ErrorMessage = "请输入正整数")]
        public int ScanPoint { get; set; }
        [DisplayName("扫描时间")]
        [Required(ErrorMessage = "扫描时间不能为空")]
        [RegularExpression("(?=.*[1-9])\\d+(\\.\\d+)?", ErrorMessage = "请输入正数")]
        [DisplayFormat(DataFormatString = "{0:0.#####}", ApplyFormatInEditMode = true)]
        public decimal ScanTime { get; set; }
        [DisplayName("传输速率")]
        [RegularExpression("(?=.*[1-9])\\d+(\\.\\d+)?", ErrorMessage = "请输入正数")]
        [DisplayFormat(DataFormatString = "{0:0.#####}", ApplyFormatInEditMode = true)]
        public decimal? TransportSpeed { get; set; }
        [DisplayName("频点(MHz)")]
        [RegularExpression("(?=.*[1-9])\\d+(\\.\\d+)?", ErrorMessage = "请输入正数")]
        [DisplayFormat(DataFormatString = "{0:0.#####}", ApplyFormatInEditMode = true)]
        public decimal? FreqPoint { get; set; }
        [DisplayName("最小值")]
        [DisplayFormat(DataFormatString = "{0:0.#####}", ApplyFormatInEditMode = true)]
        public decimal? LimitLine { get; set; }
        [DisplayName("最大值")]
        [DisplayFormat(DataFormatString = "{0:0.#####}", ApplyFormatInEditMode = true)]
        public decimal? LimitLineMax { get; set; }
        public bool Delete { get; set; }
    }
}