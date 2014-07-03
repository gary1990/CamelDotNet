using CamelDotNet.Models.Base;
using CamelDotNet.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CamelDotNet.Models
{
    public class ProductType : BaseModel,IEditable<ProductType>
    {
        public ProductType() 
        {
            isLocal = true;
        }
        [Required(ErrorMessage = "K3代码不能为空")]
        [RegularExpression("[0-9\\.]+", ErrorMessage = "只能输入数字和点")]
        [DisplayName("代码")]
        [MaxLength(80)]
        public virtual string Knumber { get; set; }
        [DisplayName("物料名称")]
        [MaxLength(255)]
        public virtual string ModelName { get; set; }
        [DisplayName("全称")]
        public string FullName { get { return Name + "#" + ModelName; }}
        [DisplayName("本地")]//false is in K3,true is Local
        public bool isLocal { get; set; }
        [Required(ErrorMessage = "价格不能为空")]
        [DisplayName("价格（元/km）")]
        public decimal Price { get; set; }
        public void Edit(ProductType model)
        {
            this.Name = model.Name;
            this.ModelName = model.ModelName;
            this.Knumber = model.Knumber;
            this.Price = model.Price;
        }
    }
}