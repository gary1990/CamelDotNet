using CamelDotNet.Models.Base;
using CamelDotNet.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace CamelDotNet.Models
{
    public class TestStation : BaseModel, IEditable<TestStation>
    {
        [Required]
        [DisplayName("工序")]
        public int ProcessId { get; set; }

        public virtual Process Process { get; set; }
        public void Edit(TestStation model)
        {
            this.Name = model.Name;
            ProcessId = model.ProcessId;
        }
    }
}