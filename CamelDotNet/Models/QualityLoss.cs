using CamelDotNet.Models.DAL;
using CamelDotNet.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CamelDotNet.Models
{
    public class QualityLoss : IValidatableObject, IEditable<QualityLoss>
    {
        public QualityLoss() 
        {
            QualityLossPercents = new List<QualityLossPercent>() { };
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
        public virtual ICollection<QualityLossPercent> QualityLossPercents { get; set; }
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (QualityLossPercents == null || QualityLossPercents.Count() == 0)
            {
                yield return new ValidationResult("质量损失比不能为空", new[] { "QualityLossPercents" });
            }
            else
            {
                CamelDotNetDBContext db = new CamelDotNetDBContext();
                var testItem = db.TestItem.Where(a => a.Id == TestItemId).SingleOrDefault();
                if (testItem.TestItemCategory.Name.Contains("非"))
                {
                    //非电气性能，质量损失比个数大于1
                    if (QualityLossPercents.Count() > 1)
                    {
                        yield return new ValidationResult("非电气性能测试项只能有一个质量损失比", new[] { "QualityLossPercents" });
                    }
                    else
                    {
                        if (QualityLossPercents.First().QualityLossFreq != null || QualityLossPercents.First().QualityLossRef != null)
                        {
                            yield return new ValidationResult("非电气性能测试项频率/值范围必须为空", new[] { "QualityLossPercents" });
                        }
                    }
                }
                else
                {

                }
            }
        }

        public void Edit(QualityLoss model)
        {
            TestItemId = model.TestItemId;
            ProcessId = model.ProcessId;
            QualityLossPercents = model.QualityLossPercents;
        }
    }
}