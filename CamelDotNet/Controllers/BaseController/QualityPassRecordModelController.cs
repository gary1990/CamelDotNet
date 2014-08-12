using CamelDotNet.Lib;
using CamelDotNet.Models;
using CamelDotNet.Models.DAL;
using CamelDotNet.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Data.Entity;
using Microsoft.AspNet.Identity;
using System.Text;
using System.Web.UI.WebControls;
using System.IO;
using System.Web.UI;

namespace CamelDotNet.Controllers.BaseController
{
    public class QualityPassRecordModelController<Model> : Controller where Model : QualityPassRecord
    {
        public UnitOfWork UW;
        public GenericRepository<Model> GR;
        public List<string> Path;
        public string ViewPath1 = "~/Views/";
        public string ViewPath = "QualityPassRecord";
        public string ViewPathBase = "QualityPassRecord";
        public string ViewPath2 = "/";

        public QualityPassRecordModelController()
        {
            UW = new UnitOfWork();
            GR = (GenericRepository<Model>)(typeof(UnitOfWork).GetProperty(typeof(Model).Name + "Repository").GetValue(UW));
        }

        public virtual ActionResult Index(int page = 1, bool includeSoftDeleted = false, string filter = null)
        {
            List<SelectListItem> items = new List<SelectListItem>();

            items.Add(new SelectListItem { Text = "00", Value = "00" });
            items.Add(new SelectListItem { Text = "01", Value = "01" });
            items.Add(new SelectListItem { Text = "02", Value = "02" });
            items.Add(new SelectListItem { Text = "03", Value = "03" });
            items.Add(new SelectListItem { Text = "04", Value = "04" });
            items.Add(new SelectListItem { Text = "05", Value = "05" });
            items.Add(new SelectListItem { Text = "06", Value = "06" });
            items.Add(new SelectListItem { Text = "07", Value = "07" });
            items.Add(new SelectListItem { Text = "08", Value = "08" });
            items.Add(new SelectListItem { Text = "09", Value = "09" });
            for (int i = 10; i <= 23; i++)
            {
                items.Add(new SelectListItem { Text = i.ToString(), Value = i.ToString() });
            }
            ViewBag.Hours = items;

            ViewBag.RV = new RouteValueDictionary { { "tickTime", DateTime.Now.ToLongTimeString() }, { "returnRoot", "Index" }, { "actionAjax", "Get" }, { "page", page }, { "includeSoftDeleted", includeSoftDeleted }, { "filter", filter } };
            return View(ViewPath1 + ViewPath + ViewPath2 + "Index.cshtml");
        }

        public virtual ActionResult Get(string returnRoot, string actionAjax = "", int page = 1, bool includeSoftDeleted = false, string filter = null, bool export = false)
        {
            var vnaResults = VnaRecordCommon<VnaRecord>.GetQuery(UW, includeSoftDeleted, filter)
                .Where(a => a.NoStatistics == false)
                .Where(a => a.TestResult == true || (a.TestResult == false && a.isGreenLight == true))
                .Include(a => a.Client)
                .Include(a => a.SerialNumber)
                .Include(a => a.TestEquipment).Include(a => a.TestStation).Include(a => a.CamelDotNetUser)
                .Include(a => a.ProductType)
                .Include(a => a.VnaTestItemRecords)
                .Include(a => a.VnaTestItemRecords.Select(b => b.TestItem))
                .Include(a => a.VnaTestItemRecords.Select(b => b.TestItem.TestItemCategory))
                .Include(a => a.VnaTestItemRecords.Select(b => b.VnaTestItemPerRecords));

            var curUserId = User.Identity.GetUserId();
            var curUserRole = UW.context.UserManager.FindById(curUserId).CamelDotNetRole.Name;
            ViewBag.CurUserRole = curUserRole;

            var s = vnaResults.ToList();

            var results = from a in vnaResults
                          from b in UW.context.QualityPassRecord
                          .Where(o => a.Id == o.VnaRecordId)
                          .DefaultIfEmpty()
                          select new VnaRecordQualityPassRecord { VnaRecord = a, QualityPassRecord = b };

            results = results.OrderByDescending(a => a.VnaRecord.TestTime);
            //if (export)
            //{
            //    string patial = PartialView(ViewPath1 + ViewPath + ViewPath2 + "Export.cshtml", results).ToString();
            //    StringBuilder str = new StringBuilder();
            //    str.Append(patial);
            //    HttpContext.Response.AddHeader("content-disposition", "attachment; filename=QualityPass" + DateTime.Now.Year.ToString() + ".xls");
            //    this.Response.ContentType = "application/vnd.ms-excel";
            //    byte[] temp = Encoding.UTF8.GetBytes(str.ToString());
            //    return File(temp, "application/vnd.ms-excel");
            //    //var exportResult = ExportToExcel();
            //    //return exportResult;
            //}
            //else
            //{
            //    var rv = new RouteValueDictionary { { "tickTime", DateTime.Now.ToLongTimeString() }, { "returnRoot", returnRoot }, { "actionAjax", actionAjax }, { "page", page }, { "includeSoftDeleted", includeSoftDeleted }, { "filter", filter } };
            //    return PartialView(ViewPath1 + ViewPath + ViewPath2 + "Get.cshtml", Common<VnaRecordQualityPassRecord>.Page(this, rv, results)); 
            //}

            if (export)
            {
                ExportToExcel();
                return PartialView(ViewPath1 + ViewPath + ViewPath2 + "Export.cshtml", results);
            }
            else 
            {
                var rv = new RouteValueDictionary { { "tickTime", DateTime.Now.ToLongTimeString() }, { "returnRoot", returnRoot }, { "actionAjax", actionAjax }, { "page", page }, { "includeSoftDeleted", includeSoftDeleted }, { "filter", filter } };
                return PartialView(ViewPath1 + ViewPath + ViewPath2 + "Get.cshtml", Common<VnaRecordQualityPassRecord>.Page(this, rv, results));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual ActionResult Create(int vnaRecordId = 0, string returnUrl = "Index") 
        {
            ViewBag.ReturnUrl = returnUrl;
            var curUserRole = UW.context.UserManager.FindById(User.Identity.GetUserId()).CamelDotNetRole.Name;
            ViewBag.CurUserRole = curUserRole;

            //get record from QualityPassRecord table
            var result = UW.context.QualityPassRecord
                .Include(a => a.VnaRecord)
                .Include(a => a.Department)
                .Where(a => a.VnaRecordId == vnaRecordId)
                .SingleOrDefault();
            if(result == null)
            {
                result = new QualityPassRecord { Id = 0, VnaRecordId = vnaRecordId };
            }

            return View(ViewPath1 + ViewPath + ViewPath2 + "Create.cshtml",result);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual ActionResult CreateSave(QualityPassRecord qualityPassRecord, string returnUrl = "Index")
        {
            ViewBag.ReturnUrl = returnUrl;
            var curUserId = User.Identity.GetUserId();
            var curUserRole = UW.context.UserManager.FindById(curUserId).CamelDotNetRole.Name;
            
            ViewBag.CurUserRole = curUserRole;
            
            if(ModelState.IsValid)
            {
                try
                {
                    //get record from QualityPassRecord table
                    var result = UW.context.QualityPassRecord
                        .Include(a => a.VnaRecord)
                        .Include(a => a.Department)
                        .Where(a => a.Id == qualityPassRecord.Id && a.VnaRecordId == qualityPassRecord.VnaRecordId)
                        .SingleOrDefault();
                    var vnaRecord = UW.context.VnaRecord.Where(a => a.Id == qualityPassRecord.VnaRecordId).SingleOrDefault();
                    if (result != null)
                    {
                        result = UpdateQualityPassRecord(curUserRole, curUserId, result, qualityPassRecord, vnaRecord);
                    }
                    else
                    {
                        result = UpdateQualityPassRecord(curUserRole, curUserId, qualityPassRecord, qualityPassRecord, vnaRecord);
                    }
                    if (result.Id == 0)
                    {
                        UW.context.QualityPassRecord.Add(qualityPassRecord);
                    }
                    UW.CamelSave();
                    Common.RMOk(this, "记录编辑成功!");
                    return Redirect(Url.Content(returnUrl));
                }
                catch (Exception e)
                {
                    ModelState.AddModelError(string.Empty, "编辑记录失败!" + e.ToString());
                    return View(ViewPath1 + ViewPath + ViewPath2 + "Create.cshtml", qualityPassRecord);
                }
            }

            return View(ViewPath1 + ViewPath + ViewPath2 + "Create.cshtml", qualityPassRecord);
        }

        public ActionResult ExportToExcel()
        {

            StringBuilder str = new StringBuilder();
            HttpContext.Response.AddHeader("content-disposition", "attachment; filename=QualityPass" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xls");
            this.Response.ContentType = "application/vnd.ms-excel";
            byte[] temp = Encoding.UTF8.GetBytes(str.ToString());
            return File(temp, "application/vnd.ms-excel");

            //StringBuilder stringBuilder = new StringBuilder();

            //stringBuilder.Append("<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n");

            //stringBuilder.Append("<Workbook xmlns=\"urn:schemas-microsoft-com:office:spreadsheet\"\n");

            //stringBuilder.Append("xmlns:x=\"urn:schemas-microsoft-com:office:excel\"\n");

            //stringBuilder.Append("xmlns:ss=\"urn:schemas-microsoft-com:office:spreadsheet\"\n");

            //stringBuilder.Append("xmlns:html=\"http://www.w3.org/TR/REC-html40\">\n");

            //stringBuilder.Append("<Worksheet ss:Name=\"Table1\">\n");

            //stringBuilder.Append("<Table>\n");

            //stringBuilder.Append("<Column ss:Index=\"1\" ss:AutoFitWidth=\"0\" ss:Width=\"110\"/>\n");

            //stringBuilder.Append("<Row>\n");

            //stringBuilder.Append("<Cell><Data ss:Type=\"String\">所属客户</Data></Cell>\n");

            //stringBuilder.Append("<Cell><Data ss:Type=\"String\">外派客户</Data></Cell>\n");

            //stringBuilder.Append("</Row>\n");
            //stringBuilder.Append("</Table>\n");
            //stringBuilder.Append("</Worksheet>\n");
            //stringBuilder.Append("</Workbook>\n");
            //Response.Clear();
            //Response.AppendHeader("Content-Disposition", "attachment;filename=xueda" + System.DateTime.Now.ToString("_yyMMdd_hhmm") + ".xls");
            //Response.Charset = "gb2312"; Response.ContentType = "application/ms-excel";
            //Response.Write(stringBuilder.ToString());
            //Response.End();
        }

        private QualityPassRecord UpdateQualityPassRecord(string userRole, string userId, QualityPassRecord qualityPassRecord, QualityPassRecord qualityPassRecordEdit,VnaRecord vnaRecord) 
        {
            switch (userRole)
            {
                case "质量工程师":
                    qualityPassRecord.QeId = userId;
                    qualityPassRecord.QeSuggestion = qualityPassRecordEdit.QeSuggestion;
                    qualityPassRecord.DepartmentId = qualityPassRecordEdit.DepartmentId;
                    break;
                case "技术工程师":
                    qualityPassRecord.TechnologistId = userId;
                    qualityPassRecord.DepartmentId = qualityPassRecordEdit.DepartmentId;
                    qualityPassRecord.TechnologistSuggestion = qualityPassRecordEdit.TechnologistSuggestion;
                    break;
                case "质量经理":
                    qualityPassRecord.QmId = userId;
                    qualityPassRecord.QmSuggestion = qualityPassRecordEdit.QmSuggestion;
                    qualityPassRecord.NeedHmApprove = qualityPassRecordEdit.NeedHmApprove;
                    qualityPassRecord.ChangePass = qualityPassRecordEdit.ChangePass;
                    //do not need Header Engineer Approve
                    if (!qualityPassRecordEdit.NeedHmApprove)
                    {
                        vnaRecord.isGreenLight = qualityPassRecordEdit.ChangePass;
                        if (qualityPassRecordEdit.ChangePass)
                        {
                            //change result to pass
                            vnaRecord.TestResult = false;
                        }
                        else 
                        {
                            //change result to fail
                            vnaRecord.TestResult = true;
                        }
                    }
                    break;
                case "总工":
                    qualityPassRecord.HmId = userId;
                    qualityPassRecord.HmSuggestion = qualityPassRecordEdit.HmSuggestion;
                    qualityPassRecord.ChangePass = qualityPassRecordEdit.ChangePass;
                    qualityPassRecord.NeedHmApprove = false;
                    vnaRecord.isGreenLight = qualityPassRecordEdit.ChangePass;
                    if (qualityPassRecordEdit.ChangePass)
                    {
                        //change result to pass
                        vnaRecord.TestResult = false;
                    }
                    else 
                    {
                        //change result to fail
                        vnaRecord.TestResult = true;
                    }
                    break;
                default:
                    break;
            }
            return qualityPassRecord;
        }
	}
}