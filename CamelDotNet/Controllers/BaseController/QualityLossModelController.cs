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
using System.Data.Entity.Infrastructure;
using AutoMapper;

namespace CamelDotNet.Controllers.BaseController
{
    public class QualityLossModelController<Model> : Controller where Model : QualityLoss
    {
        public UnitOfWork UW;
        public GenericRepository<Model> GR;
        public List<string> Path;
        public string ViewPath1 = "~/Views/";
        public string ViewPath = "QualityLoss";
        public string ViewPathBase = "QualityLoss";
        public string ViewPathEditorTempletes = "EditorTemplates";
        public string ViewPath2 = "/";

        public QualityLossModelController()
        {
            UW = new UnitOfWork();
            GR = (GenericRepository<Model>)(typeof(UnitOfWork).GetProperty(typeof(Model).Name + "Repository").GetValue(UW));
        }

        public virtual ActionResult Index(int page = 1, string filter = null)
        {
            ViewBag.RV = new RouteValueDictionary { { "tickTime", DateTime.Now.ToLongTimeString() }, { "returnRoot", "Index" }, { "actionAjax", "Get" }, { "page", page }, { "filter", filter } };
            return View(ViewPath1 + ViewPath + ViewPath2 + "Index.cshtml");
        }

        public virtual ActionResult Get(string returnRoot, string actionAjax = "", int page = 1, string filter = null, bool export = false)
        {
            var results = QualityLossCommon<QualityLoss>.GetQuery(UW, filter);

            results = results.OrderByDescending(a => a.Id);

            var rv = new RouteValueDictionary { { "tickTime", DateTime.Now.ToLongTimeString() }, { "returnRoot", returnRoot }, { "actionAjax", actionAjax }, { "page", page }, { "filter", filter } };
            return PartialView(ViewPath1 + ViewPath + ViewPath2 + "Get.cshtml", Common<QualityLoss>.Page(this, rv, results));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual ActionResult Create(string returnUrl = "Index") 
        {
            ViewBag.ReturnUrl = returnUrl;

            return View(ViewPath1 + ViewPath + ViewPath2 + "Create.cshtml");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual ActionResult CreateSave(QualityLossEdit qualityLossEdit, string returnUrl = "Index")
        {
            ViewBag.ReturnUrl = returnUrl;
            if (ModelState.IsValid)
            {
                try
                {
                    QualityLoss qualityLoss = new QualityLoss();
                    qualityLoss.ProcessId = qualityLossEdit.ProcessId;
                    qualityLoss.TestItemId = qualityLossEdit.TestItemId;
                    List<QualityLossPercent> qualityLossPercentList = new List<QualityLossPercent>();
                    foreach (var qualityLossPercentEditNotDelete in qualityLossEdit.QualityLossPercentEdits.Where(a => a.Delete == false))
                    {
                        Mapper.CreateMap<QualityLossPercentEdit, QualityLossPercent>();
                        QualityLossPercent qualityLossPercent = new QualityLossPercent();
                        Mapper.Map(qualityLossPercentEditNotDelete, qualityLossPercent);
                        qualityLossPercentList.Add(qualityLossPercent);
                    }
                    qualityLoss.QualityLossPercents = qualityLossPercentList;
                    UW.context.QualityLoss.Add(qualityLoss);
                    UW.DbSave();
                    return Redirect(Url.Content(returnUrl));
                }
                catch (DbUpdateException e)
                {
                    var DbUpdateExceptionMsg = e.InnerException.InnerException.Message;
                    if (DbUpdateExceptionMsg.Contains("Cannot insert duplicate key row"))
                    {
                        if (DbUpdateExceptionMsg.Contains("index_LossValue"))
                        {
                            ModelState.AddModelError(string.Empty, "损失比已存在!");
                        }
                        else if (DbUpdateExceptionMsg.Contains("index_TestItemProcess"))
                        {
                            ModelState.AddModelError(string.Empty, "该测试项的该工序已定义损失比!");
                        }
                        else
                        {
                            ModelState.AddModelError(string.Empty, "相同名称的记录已存在,保存失败!");
                        }
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "新建记录失败!" + e.ToString());
                    }
                }
                catch (Exception e)
                {
                    ModelState.AddModelError(string.Empty, "新建记录失败!" + e.ToString());
                }
            }

            return View(ViewPath1 + ViewPath + ViewPath2 + "Create.cshtml", qualityLossEdit);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual ActionResult Edit(int id = 0, string returnUrl = "Index") 
        {
            var result = QualityLossCommon<Model>.GetQuery(UW).Where(a => a.Id == id).SingleOrDefault();
            if(result == null)
            {
                Common.RMError(this);
                return Redirect(Url.Content(returnUrl));
            }

            /* map QualityLoss to QualityLossEdit */
            Mapper.CreateMap<QualityLoss, QualityLossEdit>();
            QualityLossEdit qualityLossEdit = new QualityLossEdit();
            Mapper.Map(result, qualityLossEdit);
            foreach(var qualityLossPercent in result.QualityLossPercents)
            {
                Mapper.CreateMap<QualityLossPercent, QualityLossPercentEdit>();
                QualityLossPercentEdit qualityLossPercentEdit = new QualityLossPercentEdit();
                Mapper.Map(qualityLossPercent,qualityLossPercentEdit);
                qualityLossEdit.QualityLossPercentEdits.Add(qualityLossPercentEdit);
            }
            /*end here*/

            ViewBag.ReturnUrl = returnUrl;

            return View(ViewPath1 + ViewPath + ViewPath2 + "Edit.cshtml", qualityLossEdit);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual ActionResult EditSave(QualityLossEdit qualityLossEdit, string returnUrl = "Index")
        {
            var result = QualityLossCommon<QualityLoss>.GetQuery(UW).Where(a => a.Id == qualityLossEdit.Id).SingleOrDefault();
            if(result == null)
            {
                return Redirect(Url.Content(returnUrl));
            }

            if(ModelState.IsValid)
            {
                try 
                {
                    result.ProcessId = qualityLossEdit.ProcessId;
                    result.TestItemId = qualityLossEdit.TestItemId;
                    foreach(QualityLossPercentEdit qualityLossPercentEdit in qualityLossEdit.QualityLossPercentEdits)
                    {
                        if (qualityLossPercentEdit.Delete == true)//is deleted QualityLossPercentEdit
                        {
                            if (qualityLossPercentEdit.Id != 0)//is deleted QualityLossPercent, has QualityLossPercent record -> remove record in db
                            {
                                Mapper.CreateMap<QualityLossPercentEdit, QualityLossPercent>();
                                QualityLossPercent qualityLossPercent = new QualityLossPercent();
                                Mapper.Map(qualityLossPercentEdit, qualityLossPercent);
                                try
                                {
                                    QualityLossPercent qualityLossPercentOld = UW.context.QualityLossPercent.Where(a => a.Id == qualityLossPercent.Id).Single();
                                    UW.context.QualityLossPercent.Remove(qualityLossPercentOld);
                                }
                                catch(Exception e)
                                {
                                    ModelState.AddModelError(string.Empty, "编辑记录失败!" + e.ToString());
                                }
                            }
                            else
                            {
                                //is deleted QualityLossPercent, has no QualityLossPercent record -> do nothing
                            }
                        }
                        else//is not deleted QualityLossPercentEdit
                        {
                            Mapper.CreateMap<QualityLossPercentEdit, QualityLossPercent>();
                            QualityLossPercent qualityLossPercent = new QualityLossPercent();
                            Mapper.Map(qualityLossPercentEdit, qualityLossPercent);
                            if (qualityLossPercentEdit.Id == 0)//is not deleted QualityLossPercent, has no QualityLossPercent record -> add record to db
                            {
                                result.QualityLossPercents.Add(qualityLossPercent);
                            }
                            else//is not deleted QualityLossPercent, has QualityLossPercent record -> add record to db
                            {
                                try 
                                {
                                    var qualityLossPercentOld = result.QualityLossPercents.Where(a => a.Id == qualityLossPercent.Id).Single();
                                    qualityLossPercentOld.QualityLossFreq = qualityLossPercent.QualityLossFreq;
                                    qualityLossPercentOld.QualityLossRef = qualityLossPercent.QualityLossRef;
                                    qualityLossPercentOld.LossValue = qualityLossPercent.LossValue;
                                }
                                catch(Exception e)
                                {
                                    ModelState.AddModelError(string.Empty, "编辑记录失败!" + e.ToString());
                                }
                            }
                        }
                    }

                    UW.DbSave();

                    Common.RMOk(this, "记录:" + result + "保存成功!");
                    return Redirect(Url.Content(returnUrl));
                }
                catch(DbUpdateException e)
                {
                    var DbUpdateExceptionMsg = e.InnerException.InnerException.Message;
                    if (DbUpdateExceptionMsg.Contains("Cannot insert duplicate key row"))
                    {
                        if (DbUpdateExceptionMsg.Contains("index_LossValue"))
                        {
                            ModelState.AddModelError(string.Empty, "该质量损失比已存在!");
                        }
                        else if (DbUpdateExceptionMsg.Contains("index_TestItemProcess"))
                        {
                            ModelState.AddModelError(string.Empty, "该测试项的该工序已定义损失比!");
                        }
                        else
                        {
                            ModelState.AddModelError(string.Empty, "相同名称的记录已存在,保存失败!");
                        }
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "编辑记录失败!" + e.ToString());
                    }
                }
                catch(Exception e)
                {
                    ModelState.AddModelError(string.Empty, "编辑记录失败!" + e.ToString());
                }
            }

            ViewBag.ReturnUrl = returnUrl;

            return View(ViewPath1 + ViewPath + ViewPath2 + "Edit.cshtml", qualityLossEdit);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual ActionResult Delete(int id = 0, string returnUrl = "Index") 
        {
            var result = QualityLossCommon<Model>.GetQuery(UW).Where(a => a.Id == id).SingleOrDefault();
            if (result == null)
            {
                Common.RMError(this);
                return Redirect(Url.Content(returnUrl));
            }

            ViewBag.ReturnUrl = returnUrl;

            return View(ViewPath1 + ViewPath + ViewPath2 + "Delete.cshtml", result);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual ActionResult DeleteSave(int id, string returnUrl = "Index") 
        {
            var result = UW.context.QualityLoss.Include(a => a.QualityLossPercents).Where(a => a.Id == id).SingleOrDefault();
            if (result == null)
            {
                Common.RMError(this);
                return Redirect(Url.Content(returnUrl));
            }

            try
            {
                List<QualityLossPercent> qualityLossPercentList = result.QualityLossPercents.ToList();
                foreach (QualityLossPercent qualityLossPercent in qualityLossPercentList)
                {
                    UW.context.QualityLossPercent.Remove(qualityLossPercent);
                }
                UW.context.QualityLoss.Remove(result);
                UW.DbSave();
                Common.RMOk(this, "记录删除成功!");
                return Redirect(Url.Content(returnUrl));
            }
            catch (Exception e)
            {
                Common.RMError(this, "记录删除失败!" + e.ToString());
            }

            return Redirect(Url.Content(returnUrl));
        }

        [ChildActionOnly]
        public virtual PartialViewResult Abstract(int id)
        {
            var result = GR.GetByID(id);
            return PartialView(ViewPath1 + ViewPath + ViewPath2 + "Abstract.cshtml", result);
        }

        [ChildActionOnly]
        public virtual PartialViewResult GetQualityLossPercentEdit(QualityLossPercentEdit item)
        {
            return PartialView(ViewPath1 + ViewPath + ViewPath2 + ViewPathEditorTempletes + ViewPath2 + "QualityLossPercentEdit.cshtml", item);
        }
	}
}