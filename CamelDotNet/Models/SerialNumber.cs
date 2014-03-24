using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CamelDotNet.Models
{
    public class SerialNumber
    {
        public SerialNumber() 
        {
            VnaRecords = new List<VnaRecord> { };
        }
        public int Id { get; set; }
        [Required]
        [DisplayName("序列号")]
        public string Number { get; set; }
        public virtual ICollection<VnaRecord> VnaRecords { get; set; }
    }
}