using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CamelDotNet.Controllers.Lib
{
    public class PdfResult : PartialViewResult
    {
        private string _fileName;
        
        public string FileName
        {
            get
            {
                return DateTime.Now.ToString("yyyyMMddHHmmss") + ".pdf" ?? "file.pdf";
            }
            set { _fileName = value; }
        }

        public PdfResult(string file)
        {
            this._fileName = file;
        }

        public override void ExecuteResult(ControllerContext context)
        {
            context.HttpContext.Response.ContentType = "application/pdf";
            context.HttpContext.Response.AppendHeader("Content-Disposition", "attachment;filename=" + FileName);
            context.HttpContext.Response.TransmitFile(_fileName);
            context.HttpContext.Response.End();
        }

    }
}