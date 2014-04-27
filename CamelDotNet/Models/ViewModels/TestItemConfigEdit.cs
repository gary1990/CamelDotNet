using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CamelDotNet.Models.ViewModels
{
    public class TestItemConfigEdit
    {
        public TestItemConfigEdit()
        {
            PerConfigEdits = new List<PerConfigEdit> { };
            this.VersionDate = DateTime.Now;
        }
        public int Id { get; set; }
        public int TestConfigId { get; set; }
        [Required]
        [DisplayName("测试项")]
        public int TestItemId { get; set; }
        [DisplayName("状态文件名")]
        [MaxLength(100)]
        [RegularExpression("^[a-zA-Z0-9]+$", ErrorMessage = "请输入英文或数字")]
        public string StateFileName { get; set; }
        [DisplayName("修改日期")]
        public DateTime VersionDate { get; set; }
        public virtual ICollection<PerConfig> PerConfigs { get; set; }
        public bool Delete { get; set; }
        [DisplayName("指标配置")]
        public virtual ICollection<PerConfigEdit> PerConfigEdits { get; set; }
    }
}