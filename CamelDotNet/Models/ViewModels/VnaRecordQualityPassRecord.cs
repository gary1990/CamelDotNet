using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CamelDotNet.Models.ViewModels
{
    public class VnaRecordQualityPassRecord
    {
        public virtual VnaRecord VnaRecord { get; set; }
        public virtual QualityPassRecord QualityPassRecord { get; set; }
    }
}