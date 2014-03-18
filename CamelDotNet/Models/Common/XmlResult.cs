using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace CamelDotNet.Models.Common
{
    public class XmlResult<T> : ActionResult
    {
        public T Data { private get; set; }

        public override void ExecuteResult(ControllerContext context)
        {
            HttpContextBase httpContextBase = context.HttpContext;
            httpContextBase.Response.Buffer = true;
            httpContextBase.Response.Clear();

            string fileName = DateTime.Now.ToString("yyyymmddhhss") + ".xml";

            httpContextBase.Response.AddHeader("content-disposition", "attachment; filename="+fileName);
            httpContextBase.Response.ContentType = "text/xml";

            using(StringWriter writer = new StringWriter())
            {
                XmlSerializer xml = new XmlSerializer(typeof(T));
                xml.Serialize(writer,Data);
                httpContextBase.Response.Write(writer);
            }
        }
    }
}