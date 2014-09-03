using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CamelDotNet.Models.ViewModels
{
    public class VnaTotalResultDamageDepartment
    {
        public int ProductTypeId { get; set; }
        public string ProductFullName { get; set; }
        public decimal ProductLossMoney { get; set; }
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; }
        public decimal DepartmentLossMoney { get; set; }
    }
}