using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CamelDotNet.Models.ViewModels
{
    public class Msg
    {
        public MsgType MsgType { get; set; }
        public string Content { get; set; }
    }
}