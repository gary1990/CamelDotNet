using AutoMapper;
using CamelDotNet.Lib;
using CamelDotNet.Models;
using CamelDotNet.Models.Base;
using CamelDotNet.Models.DAL;
using CamelDotNet.Models.Interfaces;
using CamelDotNet.Models.ViewModels;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Core;
using System.Linq;
using System.Text;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace CamelDotNet.Controllers
{
    public class VnaTestRecordModelController<Model> : Controller where Model : VnaRecord
    {
        public UnitOfWork UW;
        public GenericRepository<Model> GR;
        public List<string> Path;
        public string ViewPath1 = "~/Views/";
        public string ViewPath = "VnaRecord";
        public string ViewPathBase = "VnaRecord";
        public string ViewPathEditorTempletes = "EditorTemplates";
        public string ViewPath2 = "/";

        public VnaTestRecordModelController()
        {
            UW = new UnitOfWork();
            GR = (GenericRepository<Model>)(typeof(UnitOfWork).GetProperty(typeof(Model).Name + "Repository").GetValue(UW));
        }

        public virtual ActionResult Index(int page = 1, bool includeSoftDeleted = false, string filter = null)
        {
            ViewBag.RV = new RouteValueDictionary { { "tickTime", DateTime.Now.ToLongTimeString() }, { "returnRoot", "Index" }, { "actionAjax", "Get" }, { "page", page }, { "includeSoftDeleted", includeSoftDeleted }, { "filter", filter } };
            return View(ViewPath1 + ViewPath + ViewPath2 + "Index.cshtml");
        }

        public virtual PartialViewResult Get(string returnRoot, string actionAjax = "", int page = 1, bool includeSoftDeleted = false, string filter = null, bool export = false)
        {
            var results = VnaRecordCommon<Model>.GetQuery(UW, includeSoftDeleted, filter)
                .Include(a => a.SerialNumber)
                .Include(a => a.TestEquipment).Include(a => a.TestStation).Include(a => a.CamelDotNetUser)
                .Include(a => a.ProductType)
                .Include(a => a.VnaTestItemRecords)
                .Include(a => a.VnaTestItemRecords.Select(b => b.TestItem))
                .Include(a => a.VnaTestItemRecords.Select(b => b.VnaTestItemPerRecords));

            var totalResultCount = results.Count();
            if (totalResultCount != 0)
            {
                var passResultCount = results.Where(a => a.TestResult == false && a.NoStatistics == false).Count();
                var passPercent = ((decimal)passResultCount / (decimal)totalResultCount) * 100;
                ViewBag.PassPercent = passPercent;
            }

            results = results.OrderByDescending(a => a.TestTime).ThenBy(a => a.ProductType.Name);

            if (export)
            {
                ExportToExcel();
                return PartialView(ViewPath1 + ViewPath + ViewPath2 + "Export.cshtml", results.ToList());
            }
            else
            {
                var rv = new RouteValueDictionary { { "tickTime", DateTime.Now.ToLongTimeString() }, { "returnRoot", returnRoot }, { "actionAjax", actionAjax }, { "page", page }, { "includeSoftDeleted", includeSoftDeleted }, { "filter", filter } };
                return PartialView(ViewPath1 + ViewPath + ViewPath2 + "Get.cshtml", Common<Model>.Page(this, rv, results));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual ActionResult Details(int id = 0, string returnUrl = "Index")
        {
            var result = VnaRecordCommon<Model>.GetQuery(UW)
                .Include(a => a.SerialNumber)
                .Include(a => a.TestEquipment).Include(a => a.TestStation).Include(a => a.CamelDotNetUser)
                .Include(a => a.ProductType)
                .Include(a => a.VnaTestItemRecords)
                .Include(a => a.VnaTestItemRecords.Select(b => b.TestItem))
                .Include(a => a.VnaTestItemRecords.Select(b => b.VnaTestItemPerRecords))
                .Where(a => a.Id == id).SingleOrDefault();
            if (result == null)
            {
                Common.RMError(this);
                return Redirect(Url.Content(returnUrl));
            }

            ViewBag.ReturnUrl = returnUrl;

            return View(ViewPath1 + ViewPath + ViewPath2 + "Details.cshtml", result);
        }
        public ActionResult ExportToExcel()
        {
            StringBuilder str = new StringBuilder();
            HttpContext.Response.AddHeader("content-disposition", "attachment; filename=VnaResult" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xls");
            this.Response.ContentType = "application/vnd.ms-excel";
            byte[] temp = Encoding.UTF8.GetBytes(str.ToString());
            return File(temp, "application/vnd.ms-excel");
        }
        protected override void Dispose(bool disposing)
        {
            UW.Dispose();
            base.Dispose(disposing);
        }
    }
}