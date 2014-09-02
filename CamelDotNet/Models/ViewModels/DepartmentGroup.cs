using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CamelDotNet.Models.ViewModels
{
    public class DepartmentGroup
    {
        public int? DepartmentId { get; set; }
        public string DepartmentName { get; set; }
        public int RowNum { get; set; }
        public int CellNum { get; set; }
        public decimal TotalLossMoney { get; set; }
        public DepartmentGroup() 
        {
            //because row in excel is alwals 4
            this.RowNum = 3;
        }
    }
}