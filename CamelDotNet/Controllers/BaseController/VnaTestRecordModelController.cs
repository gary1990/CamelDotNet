using AutoMapper;
using CamelDotNet.Controllers.Lib;
using CamelDotNet.Lib;
using CamelDotNet.Models;
using CamelDotNet.Models.Base;
using CamelDotNet.Models.DAL;
using CamelDotNet.Models.Interfaces;
using CamelDotNet.Models.ViewModels;
using iTextSharp.text;
using iTextSharp.text.html.simpleparser;
using iTextSharp.text.pdf;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Core;
using System.IO;
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

        public virtual PartialViewResult Get(string returnRoot, string actionAjax = "", int page = 1, bool includeSoftDeleted = false, string filter = null, bool export = false,bool exportReport = false, string testitems = null)
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
                if (!exportReport)
                {
                    var rv = new RouteValueDictionary { { "tickTime", DateTime.Now.ToLongTimeString() }, { "returnRoot", returnRoot }, { "actionAjax", actionAjax }, { "page", page }, { "includeSoftDeleted", includeSoftDeleted }, { "filter", filter } };
                    return PartialView(ViewPath1 + ViewPath + ViewPath2 + "Get.cshtml", Common<Model>.Page(this, rv, results));
                }
                else 
                {
                    string[] filesPath = GenrateReprotPdf(results, testitems);
                    return new ZipResult(filesPath) { FileName = DateTime.Now.ToString("yyyyMMddHHmmss")+".zip"};
                } 
            }
        }
        public ActionResult Edit(int? id, string returnUrl = "Index") 
        {
            if (id == null)
            {
                Common.RMError(this);
                return Redirect(Url.Content(returnUrl));
            }
            var result = VnaRecordCommon<Model>.GetQuery(UW)
                .Where(a => a.Id == id).SingleOrDefault();
            if(result == null)
            {
                Common.RMError(this);
                return Redirect(Url.Content(returnUrl));
            }
            ViewBag.ReturnUrl = returnUrl;
            return View(ViewPath1 + ViewPath + ViewPath2 + "Edit.cshtml", result);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual ActionResult EditSave(Model model, string returnUrl = "Index") 
        {
            //检查记录在权限范围内
            var result = VnaRecordCommon<Model>.GetQuery(UW).Where(a => a.Id == model.Id).SingleOrDefault();
            if (result == null)
            {
                Common.RMError(this);
                return Redirect(Url.Content(returnUrl));
            }
            if (ModelState.IsValid)
            {
                try
                {
                    result.Edit(model);
                    UW.CamelSave();
                    Common.RMOk(this, "记录:" + result + "保存成功!");
                    return Redirect(Url.Content(returnUrl));
                }
                catch (Exception e)
                {
                    ModelState.AddModelError(string.Empty, "编辑记录失败!" + e.ToString());
                }
            }
            //end 检查记录在权限范围内
            ViewBag.ReturnUrl = returnUrl;
            return View(ViewPath1 + ViewPath + ViewPath2 + "Edit.cshtml", model);
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

        public ActionResult Show(int? id, string returnUrl = "Index") 
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

        private string[] GenrateReprotPdf(IQueryable<Model> results, string testitems)
        {
            //filenames of each vna record pdf
            List<string> filenamesStrList = new List<string> { };
            //split testitems string to string array
            string[] stringSeparators = new string[] { "," };
            string[] testitemIds = testitems.Split(stringSeparators, StringSplitOptions.None);
            //uploadFolder path
            string uploadFolderPath = AppDomain.CurrentDomain.BaseDirectory + "Content/UploadedFolder/VnaRecord";
            //donwload folder path
            string folderPath = "";
            //producter info folder
            string producterInfoPath = "";
            string producterName = "请填写生产厂家";
            string currentTime = DateTime.Now.ToString("yyyyMMddHHmmss");
            string currentTimeInReport = DateTime.Now.ToString("yyyy年MM月dd日HH时mm分ss秒");
            folderPath = AppDomain.CurrentDomain.BaseDirectory + "Content/downloadfolder/" + currentTime;
            producterInfoPath = AppDomain.CurrentDomain.BaseDirectory + "Content/prodcuterinfo";
            //create downloadfolder if not exist
            bool folderPathExists = System.IO.Directory.Exists(folderPath);
            if (!folderPathExists)
                System.IO.Directory.CreateDirectory(folderPath);
            //create producterinfofolder if not exist
            bool producterInfoPathExists = System.IO.Directory.Exists(producterInfoPath);
            if (!producterInfoPathExists)
                System.IO.Directory.CreateDirectory(producterInfoPath);
            //create productername.txt if not exist
            FileInfo producterNameFilePath = new FileInfo(producterInfoPath + "/" + "productername.txt");
            if (!producterNameFilePath.Exists)
            {
                StreamWriter producterNameFileSW = new StreamWriter(producterInfoPath + "/" + "productername.txt");
                producterNameFileSW.WriteLine("请填写生产厂家");
                producterNameFileSW.Close();
            }
            else
            {
                StreamReader producterNameFileSR = new StreamReader(producterInfoPath + "/" + "productername.txt");
                producterName = producterNameFileSR.ReadLine();
                producterNameFileSR.Close();
            }
            //create producter.png if not exists
            FileInfo producterLogoFilePath = new FileInfo(producterInfoPath + "/" + "producter.png");
            if (!producterLogoFilePath.Exists)
            {
                StreamWriter producterNameFileSW = new StreamWriter(producterInfoPath + "/" + "producter.png");
                producterNameFileSW.Close();
            }

            //baseFont use "黑体"
            BaseFont baseFont = BaseFont.CreateFont("C:/Windows/Fonts/simhei.TTF", BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED);
            Font font = new Font(baseFont, 10f);
            Font font12 = new Font(baseFont, 12f);
            Font link = FontFactory.GetFont("Arial", 10, Font.NORMAL, new BaseColor(0, 0, 255));
            //create newline Phrase
            Phrase newLinePhrase = new Phrase(Environment.NewLine);
            //per vna record
            foreach (var perResult in results)
            {
                string filename = folderPath + "/" + perResult.ReelNumber + "_" + perResult.TestTime.ToString("yyyyMMddHHmmss") + "_" + perResult.Id + ".pdf";
                Document doc = new Document();
                PdfWriter pdfWriter = PdfWriter.GetInstance(doc, new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.None));
                doc.Open();
                System.Drawing.Image LogoImage = System.Drawing.Image.FromFile(producterInfoPath + "/" + "producter.png");
                iTextSharp.text.Image logo = iTextSharp.text.Image.GetInstance(LogoImage, System.Drawing.Imaging.ImageFormat.Png);
                logo.ScalePercent(24f);
                logo.Alignment = Element.ALIGN_RIGHT;
                doc.Add(logo);
                doc.Add(newLinePhrase);
                //add detail match index click
                Phrase title = new Phrase("VNA测试：详情", font12);
                doc.Add(title);
                //create perrecord table
                PdfPTable perRecordTable = new PdfPTable(4);
                perRecordTable.AddCell(new Phrase("产品型号", font));
                perRecordTable.AddCell(new Phrase("序列号", font));
                perRecordTable.AddCell(new Phrase("盘号", font));
                perRecordTable.AddCell(new Phrase("制造长度", font));
                perRecordTable.AddCell(new Phrase(perResult.ProductType.Name, font));
                perRecordTable.AddCell(new Phrase(perResult.SerialNumber.Number, font));
                perRecordTable.AddCell(new Phrase(perResult.ReelNumber, font));
                perRecordTable.AddCell((Math.Abs(perResult.InnerLength - perResult.OuterLength)).ToString());
                doc.Add(perRecordTable);
                foreach (var testitem in perResult.VnaTestItemRecords)
                {
                    PdfPTable perRecordTestItemTable = new PdfPTable(1);
                    perRecordTestItemTable.DefaultCell.Border = Rectangle.NO_BORDER;
                    //if testedrecord in selected testitem
                    bool isSelected = testitemIds.Contains(testitem.TestItemId.ToString());
                    if (isSelected)
                    {
                        perRecordTestItemTable.AddCell(new Phrase("测试项：" + testitem.TestItem.Name, font));
                        doc.Add(newLinePhrase);
                        //uploadFolder image path
                        FileInfo itemTestImagePath = new FileInfo(uploadFolderPath + "/" + testitem.ImagePath);
                        if (itemTestImagePath.Exists)
                        {
                            System.Drawing.Image testitemImage = System.Drawing.Image.FromFile(uploadFolderPath + "/" + testitem.ImagePath);
                            iTextSharp.text.Image testitemImagePng = iTextSharp.text.Image.GetInstance(testitemImage, System.Drawing.Imaging.ImageFormat.Png);
                            testitemImagePng.ScalePercent(25f);
                            perRecordTestItemTable.AddCell(testitemImagePng);
                        }
                    }
                    doc.Add(perRecordTestItemTable);
                }
                doc.Close();
                filenamesStrList.Add(filename);
            }

            return filenamesStrList.ToArray();
        }

        private string GenerateReport(IQueryable<Model> results, string testitems) 
        {
            string[] stringSeparators = new string[] { "," };
            string[] testitemIds = testitems.Split(stringSeparators, StringSplitOptions.None);
            //uploadFolder path
            string uploadFolderPath = AppDomain.CurrentDomain.BaseDirectory + "Content/UploadedFolder/VnaRecord";
            //donwload folder path
            string folderPath = "";
            //producter info folder
            string producterInfoPath = "";
            string producterName = "请填写生产厂家";
            string currentTime = DateTime.Now.ToString("yyyyMMddHHmmss");
            string currentTimeInReport = DateTime.Now.ToString("yyyy年MM月dd日HH时mm分ss秒");
            folderPath = AppDomain.CurrentDomain.BaseDirectory + "Content/downloadfolder/" + currentTime;
            producterInfoPath = AppDomain.CurrentDomain.BaseDirectory + "Content/prodcuterinfo";
            //create downloadfolder if not exist
            bool folderPathExists = System.IO.Directory.Exists(folderPath);
            if (!folderPathExists)
                System.IO.Directory.CreateDirectory(folderPath);
            //create producterinfofolder if not exist
            bool producterInfoPathExists = System.IO.Directory.Exists(producterInfoPath);
            if (!producterInfoPathExists)
                System.IO.Directory.CreateDirectory(producterInfoPath);
            //create productername.txt if not exist
            FileInfo producterNameFilePath = new FileInfo(producterInfoPath + "/" + "productername.txt");
            if (!producterNameFilePath.Exists)
            {
                StreamWriter producterNameFileSW = new StreamWriter(producterInfoPath + "/" + "productername.txt");
                producterNameFileSW.WriteLine("请填写生产厂家");
                producterNameFileSW.Close();
            }
            else
            {
                StreamReader producterNameFileSR = new StreamReader(producterInfoPath + "/" + "productername.txt");
                producterName = producterNameFileSR.ReadLine();
                producterNameFileSR.Close();
            }
            //create producter.png if not exists
            FileInfo producterLogoFilePath = new FileInfo(producterInfoPath + "/" + "producter.png");
            if (!producterLogoFilePath.Exists)
            {
                StreamWriter producterNameFileSW = new StreamWriter(producterInfoPath + "/" + "producter.png");
                producterNameFileSW.Close();
            }
            //copy producter.png to downloadFolder
            producterLogoFilePath.CopyTo(folderPath + "/" + "producter.png", true);

            System.IO.StreamWriter indexHtml = new System.IO.StreamWriter(folderPath + "/" + "index.html");
            
            StringBuilder indexStringBuilder = new StringBuilder();
            indexStringBuilder.Append("<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Strict//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd\"><html xmlns=\"http://www.w3.org/1999/xhtml\"><head><meta http-equiv=\"content-type\" content=\"text/html;charset=utf-8\">");
            indexStringBuilder.Append("<style type=\"text/css\">");
            indexStringBuilder.Append("body{border:0px;margin:0px}a{text-decoration:none;}.container{width:1024px;margin:0px auto;border:1px solid black;padding:15px;}img{width:60px;height:30px;}table{border-collapse:collapse;}table, td, th{border:1px solid black;}");
            indexStringBuilder.Append("</style></head><body>");
            indexStringBuilder.Append("<div class=\"container\">");
            indexStringBuilder.Append("<div class=\"top\">");
            indexStringBuilder.Append("<div style=\"float:left;width:45%;\"><img src=\"./producter.png\"/></div>");
            indexStringBuilder.Append("<div style=\"font-weight:bold;font-size:28px;text-align:left\">质量报告</div>");
            indexStringBuilder.Append("</div>");
            indexStringBuilder.Append("<div style=\"margin-top:30px;margin-bottom:10px;\">");
            indexStringBuilder.Append("<div style=\"text-align:left;padding-left:70%;\">");
            indexStringBuilder.Append("<span>生产厂家：" + producterName + "</span>");
            indexStringBuilder.Append("</div>");
            indexStringBuilder.Append("<div style=\"text-align:left;padding-left:70%;\">");
            indexStringBuilder.Append("<span>报告日期：" + currentTimeInReport + "</span>");
            indexStringBuilder.Append("</div></div>");
            indexStringBuilder.Append("<div class=\"content\" style=\"padding-left:10px;padding-right:10px;font-size:13px;\">");
            indexStringBuilder.Append("<table style=\"width:100%;\"><tr><th>产品型号</th><th>产品序列号</th><th>检测结果</th>");
            indexStringBuilder.Append("</tr>");
            
            foreach(var result in results)
            {
                string resultStr = "";
                if (result.TestResult)
                {
                    resultStr = "<span style=\"color:red;\"><b>不合格</b></span>";
                }
                else 
                {
                    resultStr = "<span style=\"color:green;\"><b>合格</b></span>";
                }
                indexStringBuilder.Append("<tr><td>" + result.ProductType.Name + "</td><td><a href=\"./"+ result.Id +".html\">" + result.SerialNumber.Number + "</a></td><td>" + resultStr + "</td></tr>");
                //create perrecord image folder
                string recordImagefolderPath = folderPath + "/" + result.Id;
                bool recordImagefolderPathExists = System.IO.Directory.Exists(recordImagefolderPath);
                if (!recordImagefolderPathExists)
                    System.IO.Directory.CreateDirectory(recordImagefolderPath);
                //get all testeditem in this testrecord
                foreach(var item in result.VnaTestItemRecords)
                {
                    //if testedrecord in selected testitem
                    foreach (var selectedItem in testitemIds)
                    {
                        if (selectedItem == item.TestItemId.ToString())
                        {
                            //uploadFolder
                            FileInfo itemTestImagePath = new FileInfo(uploadFolderPath + "/" + item.ImagePath);
                            if (itemTestImagePath.Exists)
                            {
                                //copy producter.png to downloadFolder
                                itemTestImagePath.CopyTo(recordImagefolderPath + "/" + selectedItem + ".png", true);
                            }
                        }
                    }
                }
                ////create perrecord html
                //System.IO.StreamWriter deailHtml = new System.IO.StreamWriter(folderPath + "/" + result.Id + ".html");
                
                //deailHtml.Close();
            }

            indexStringBuilder.Append("</table></div></div></body></html>");

            indexHtml.Write(indexStringBuilder);

            indexHtml.Close();


            return folderPath;
        }

        protected override void Dispose(bool disposing)
        {
            UW.Dispose();
            base.Dispose(disposing);
        }
    }
}