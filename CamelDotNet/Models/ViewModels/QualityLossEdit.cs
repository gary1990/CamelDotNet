using CamelDotNet.Models.DAL;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CamelDotNet.Models.ViewModels
{
    public class QualityLossEdit : IValidatableObject
    {
        public QualityLossEdit() 
        {
            QualityLossPercentEdits = new List<QualityLossPercentEdit>() { };
        }
        public int Id { get; set; }
        [Required]
        [DisplayName("测试项")]
        public int TestItemId { get; set; }
        [DisplayName("工序")]
        public int ProcessId { get; set; }
        public virtual Process Process { get; set; }
        public virtual TestItem TestItem { get; set; }
        [DisplayName("质量损失比")]
        public virtual ICollection<QualityLossPercentEdit> QualityLossPercentEdits { get; set; }
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            //质量损失比个数为0,去除页面上删除的
            if (QualityLossPercentEdits == null || QualityLossPercentEdits.Where(a => a.Delete == false).Count() == 0)
            {
                yield return new ValidationResult("质量损失比不能为空", new[] { "QualityLossPercentEdits" });
            }
            else
            {
                CamelDotNetDBContext db = new CamelDotNetDBContext();
                var testItem = db.TestItem.Where(a => a.Id == TestItemId).SingleOrDefault();
                if (testItem.TestItemCategory.Name.Contains("非"))
                {
                    //非电气性能，质量损失比个数大于1,去除页面上删除的
                    if (QualityLossPercentEdits.Where(a => a.Delete == false).Count() > 1)
                    {
                        yield return new ValidationResult("非电气性能测试项只能有一个质量损失比", new[] { "QualityLossPercentEdits" });
                    }
                    else
                    {
                        if (QualityLossPercentEdits.Where(a => a.Delete == false).First().QualityLossFreq != null || QualityLossPercentEdits.First().QualityLossRef != null)
                        {
                            yield return new ValidationResult("非电气性能测试项频率/值范围必须为空", new[] { "QualityLossPercentEdits" });
                        }
                    }
                }
                else
                {

                }
            }
        }
    }
}