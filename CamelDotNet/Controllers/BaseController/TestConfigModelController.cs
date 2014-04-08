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
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace CamelDotNet.Controllers
{
    public class TestConfigModelController<Model> : Controller where Model : TestConfig
    {
        public UnitOfWork UW;
        public GenericRepository<Model> GR;
        public List<string> Path;
        public string ViewPath1 = "~/Views/";
        public string ViewPath = "TestConfig";
        public string ViewPathBase = "TestConfig";
        public string ViewPathEditorTempletes = "EditorTemplates";
        public string ViewPath2 = "/";

        public TestConfigModelController()
        {
            UW = new UnitOfWork();
            GR = (GenericRepository<Model>)(typeof(UnitOfWork).GetProperty(typeof(Model).Name + "Repository").GetValue(UW));
        }

        public virtual ActionResult Index(int page = 1, bool includeSoftDeleted = false, string filter = null)
        {
            ViewBag.RV = new RouteValueDictionary { { "tickTime", DateTime.Now.ToLongTimeString() }, { "returnRoot", "Index" }, { "actionAjax", "Get" }, { "page", page }, { "includeSoftDeleted", includeSoftDeleted }, { "filter", filter } };
            return View(ViewPath1 + ViewPath + ViewPath2 + "Index.cshtml");
        }

        public virtual PartialViewResult Get(string returnRoot, string actionAjax = "", int page = 1, bool includeSoftDeleted = false, string filter = null)
        {
            var results = TestConfigCommon<Model>.GetQuery(UW, includeSoftDeleted, filter);

            if (!includeSoftDeleted)
            {
                results = results.Where(a => a.IsDeleted == false);
            }

            results = results.OrderBy(a => a.Client.Name).ThenBy(a => a.ProductType.Name);

            var rv = new RouteValueDictionary { { "tickTime", DateTime.Now.ToLongTimeString() }, { "returnRoot", returnRoot }, { "actionAjax", actionAjax }, { "page", page }, { "includeSoftDeleted", includeSoftDeleted }, { "filter", filter } };
            return PartialView(ViewPath1 + ViewPath + ViewPath2 + "Get.cshtml", Common<Model>.Page(this, rv, results));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual ActionResult Copy(int id, string returnUrl = "Index")
        {
            ViewBag.ReturnUrl = returnUrl;
            ViewBag.Title = "复制需编辑";

            var result = UW.context.TestConfig.Where(a => a.Id == id).Include(a => a.TestItemConfigs.Select(b => b.PerConfigs)).SingleOrDefault();
            if (result == null)
            {
                Common.RMError(this);
                return Redirect(Url.Content(returnUrl));
            }
            try
            {
                TestConfig newItem = new TestConfig();
                newItem.Copy(result);
                newItem = CreateNewTestConfig(newItem, result);

                Mapper.CreateMap<TestConfig, TestConfigEdit>();
                TestConfigEdit testConfigEdit = new TestConfigEdit();
                Mapper.Map(newItem, testConfigEdit);

                return View(ViewPath1 + ViewPath + ViewPath2 + "CreateOrCopy.cshtml", testConfigEdit);
            }
            catch (Exception e)
            {
                Common.RMError(this, "复制失败!" + e.ToString());
            }

            return Redirect(Url.Content(returnUrl));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual ActionResult CreateOrCopySave(TestConfigEdit testConfigEidt, string returnUrl = "Index", string title = "编辑")
        {
            ViewBag.ReturnUrl = returnUrl;
            ViewBag.Title = title;

            var oldRecord = TestConfigCommon<Model>.GetQuery(UW).Where(a => a.Id != testConfigEidt.Id && a.ClientId == testConfigEidt.ClientId && a.ProductTypeId == testConfigEidt.ProductTypeId && a.IsDeleted == false).SingleOrDefault();
            if (oldRecord != null)
            {
                ModelState.AddModelError(string.Empty, "存在客户、产品型号相同的记录");
                return View(ViewPath1 + ViewPath + ViewPath2 + "CreateOrCopy.cshtml", testConfigEidt);
            }

            if (!(testConfigEidt.TestItemConfigEdits == null || testConfigEidt.TestItemConfigEdits.Where(a => a.Delete == false).Count() == 0))
            {
                using (var scope = new TransactionScope())
                {
                    try
                    {
                        TestConfig model = new TestConfig() 
                        {
                            ClientId = testConfigEidt.ClientId,
                            ProductTypeId = testConfigEidt.ProductTypeId
                        };
                        UW.context.TestConfig.Add(model);
                        UW.CamelSave();

                        foreach (var testItemConfigEdit in testConfigEidt.TestItemConfigEdits)
                        {
                            if (testItemConfigEdit.Delete == true)//delete
                            {
                                //do nothing
                            }
                            else //do not delete
                            {
                                if (!(testItemConfigEdit.PerConfigEdits == null || testItemConfigEdit.PerConfigEdits.Where(a => a.Delete == false).Count() == 0))
                                {
                                    TestItemConfig testItemConfigAdd = new TestItemConfig
                                    {
                                        TestConfigId = model.Id,
                                        TestItemId = testItemConfigEdit.TestItemId,
                                        VersionDate = DateTime.Now
                                    };
                                    UW.context.TestItemConfig.Add(testItemConfigAdd);
                                    UW.CamelSave();

                                    foreach (var perconfigAdd in testItemConfigEdit.PerConfigEdits)
                                    {
                                        if (perconfigAdd.Delete != true)
                                        {
                                            PerConfig perConfigAdd = new PerConfig
                                            {
                                                Channel = perconfigAdd.Channel,
                                                Trace = perconfigAdd.Trace,
                                                StartF = perconfigAdd.StartF,
                                                StartUnitId = perconfigAdd.StartUnitId,
                                                StopF = perconfigAdd.StopF,
                                                StopUnitId = perconfigAdd.StopUnitId,
                                                ScanPoint = perconfigAdd.ScanPoint,
                                                ScanTime = perconfigAdd.ScanTime,
                                                TransportSpeed = perconfigAdd.TransportSpeed,
                                                FreqPoint = perconfigAdd.FreqPoint,
                                                LimitLine = perconfigAdd.LimitLine,
                                                TestItemConfigId = testItemConfigAdd.Id
                                            };

                                            UW.context.PerConfig.Add(perConfigAdd);
                                        }
                                    }   
                                }
                                else
                                {
                                    ModelState.AddModelError(string.Empty, "当前测试项的指标不能全空");
                                    return View(ViewPath1 + ViewPath + ViewPath2 + "CreateOrCopy.cshtml", testConfigEidt);
                                }
                            }
                        }

                        UW.CamelSave();
                    }
                    catch (DataException)
                    {
                        ModelState.AddModelError(string.Empty, "记录更新失败");
                        return View(ViewPath1 + ViewPath + ViewPath2 + "CreateOrCopy.cshtml", testConfigEidt);
                    }
                    scope.Complete();
                }

                Common.RMOk(this, "保存成功!");
                return Redirect(Url.Content(returnUrl));
            }
            else
            {
                ModelState.AddModelError(string.Empty, "所选客户当前型号产品的测试不能全空");
                return View(ViewPath1 + ViewPath + ViewPath2 + "CreateOrCopy.cshtml", testConfigEidt);
            }
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
        public virtual ActionResult CreateSave(TestConfig model, string returnUrl = "Index")
        {
            ViewBag.ReturnUrl = returnUrl;
            ViewBag.Title = "新增需编辑";

            if (ModelState.IsValid)
            {
                try
                {
                    var result = UW.context.TestConfig.Where(a => a.Client.Name.ToUpper() == "GENERAL" || a.ClientId == 1)
                        .Where(a => a.ProductTypeId == model.ProductTypeId).Include(a => a.TestItemConfigs.Select(b => b.PerConfigs)).SingleOrDefault();
                    if (result != null)
                    {
                        model = CreateNewTestConfig(model, result);
                        UW.context.TestConfig.Add(model);
                    }

                    Mapper.CreateMap<TestConfig, TestConfigEdit>();
                    TestConfigEdit testConfigEdit = new TestConfigEdit();
                    Mapper.Map(model, testConfigEdit);

                    return View(ViewPath1 + ViewPath + ViewPath2 + "CreateOrCopy.cshtml", testConfigEdit);
                }
                catch (UpdateException e)
                {
                    if (e.InnerException.Message.Contains("Cannot insert duplicate key row"))
                    {
                        ModelState.AddModelError(string.Empty, "相同名称的记录已存在,保存失败!");
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
            
            return View(ViewPath1 + ViewPath + ViewPath2 + "Create.cshtml", model);
        }

        private TestConfig CreateNewTestConfig(TestConfig newTestConfig, TestConfig testConfigSelected)
        {
            foreach(var testItemConfigSelected in testConfigSelected.TestItemConfigs)
            {
                TestItemConfig testItemConfigNew = new TestItemConfig();
                testItemConfigNew.TestConfigId = 0;
                testItemConfigNew.TestItemId = testItemConfigSelected.TestItemId;
                testItemConfigNew.VersionDate = DateTime.Now;
                newTestConfig.TestItemConfigs.Add(testItemConfigNew);
                foreach (var perConfigSeleted in testItemConfigSelected.PerConfigs)
                {
                    Mapper.CreateMap<PerConfig, PerConfig>();
                    PerConfig perConfigNew = new PerConfig();
                    Mapper.Map(perConfigSeleted, perConfigNew);
                    perConfigNew.Id = 0;
                    perConfigNew.TestItemConfigId = 0;
                    perConfigNew.TestItemConfig = null;
                    testItemConfigNew.PerConfigs.Add(perConfigNew);
                }
            }

            return newTestConfig;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual ActionResult Edit(int id, string returnUrl = "Index")
        {
            var testConfig = TestConfigCommon<Model>.GetQuery(UW).Where(a => a.Id == id).Include(a => a.TestItemConfigs.Select(b => b.PerConfigs)).SingleOrDefault();

            if (testConfig == null)
            {
                Common.RMError(this);
                return Redirect(Url.Content(returnUrl));
            }

            Mapper.CreateMap<TestConfig, TestConfigEdit>();
            TestConfigEdit testConfigEdit = new TestConfigEdit();
            Mapper.Map(testConfig, testConfigEdit);

            ViewBag.ReturnUrl = returnUrl;

            return View(ViewPath1 + ViewPath + ViewPath2 + "Edit.cshtml", testConfigEdit);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual ActionResult EditSave(TestConfigEdit testConfigEdit, string returnUrl = "Index")
        {
            ViewBag.ReturnUrl = returnUrl;

            var oldRecord = TestConfigCommon<Model>.GetQuery(UW).Where(a => a.Id != testConfigEdit.Id && a.ClientId == testConfigEdit.ClientId && a.ProductTypeId == testConfigEdit.ProductTypeId && a.IsDeleted == false).SingleOrDefault();
            if (oldRecord != null)
            {
                ModelState.AddModelError(string.Empty, "存在客户、产品型号相同的记录");
                return View(ViewPath1 + ViewPath + ViewPath2 + "Edit.cshtml", testConfigEdit);
            }
            if (!(testConfigEdit.TestItemConfigEdits.Where(a => a.Delete == false).Count() == 0))
            {
                var result = TestConfigCommon<Model>.GetQuery(UW)
                .Include(a => a.TestItemConfigs.Select(b => b.PerConfigs))
                .Where(a => a.Id == testConfigEdit.Id && a.IsDeleted == false)
                .SingleOrDefault();
                if (result != null)
                {
                    using (var scope = new TransactionScope())
                    {
                        try
                        {
                            result.ClientId = testConfigEdit.ClientId;
                            result.ProductTypeId = testConfigEdit.ProductTypeId;
                            foreach (var testItemConfigEdit in testConfigEdit.TestItemConfigEdits)
                            {
                                if (testItemConfigEdit.Delete == true)//delete
                                {
                                    if (testItemConfigEdit.Id != 0)//exist and delete
                                    {
                                        TestItemConfig testItemConfigRemoveItem = result.TestItemConfigs.Where(a => a.Id == testItemConfigEdit.Id && a.TestConfigId == testItemConfigEdit.TestConfigId).Single();
                                        if (testItemConfigRemoveItem != null)
                                        {
                                            List<PerConfig> perConfigRemoveItems = UW.context.PerConfig.Where(a => a.TestItemConfigId == testItemConfigEdit.Id).ToList();
                                            foreach (var item in perConfigRemoveItems)
                                            {
                                                UW.context.PerConfig.Remove(item);
                                            }
                                            UW.context.TestItemConfig.Remove(testItemConfigRemoveItem);
                                        }
                                        else
                                        {
                                            ModelState.AddModelError(string.Empty, "未找到对应测试项配置记录,测试项ID：" + testItemConfigEdit.TestItemId);
                                            return View(ViewPath1 + ViewPath + ViewPath2 + "Edit.cshtml", testConfigEdit);
                                        }
                                    }
                                }
                                else //do not delete
                                {
                                    if (!(testItemConfigEdit.PerConfigEdits.Where(a => a.Delete == false).Count() == 0))
                                    {
                                        if (testItemConfigEdit.Id != 0)//edit
                                        {
                                            TestItemConfig editTestItemConfig = result.TestItemConfigs.Where(a => a.Id == testItemConfigEdit.Id && a.TestConfigId == testItemConfigEdit.TestConfigId).Single();
                                            if (editTestItemConfig != null)
                                            {
                                                var modifyTime = DateTime.Now;
                                                if (editTestItemConfig.TestItemId != testItemConfigEdit.TestItemId)
                                                {
                                                    editTestItemConfig.TestItemId = testItemConfigEdit.TestItemId;
                                                    editTestItemConfig.VersionDate = modifyTime;
                                                }
                                                foreach (var perConfigEdit in testItemConfigEdit.PerConfigEdits)
                                                {
                                                    if (perConfigEdit.Delete == true)//perconfig delete
                                                    {
                                                        if (perConfigEdit.Id != 0)//exist
                                                        {
                                                            PerConfig perConfig = UW.context.PerConfig.Where(a => a.Id == perConfigEdit.Id).Single();
                                                            if (perConfig != null)
                                                            {
                                                                UW.context.PerConfig.Remove(perConfig);
                                                            }
                                                            else
                                                            {
                                                                ModelState.AddModelError(string.Empty, "未找到对应指标记录,指标ID：" + perConfigEdit.Id);
                                                                return View(ViewPath1 + ViewPath + ViewPath2 + "Edit.cshtml", testConfigEdit);
                                                            }
                                                        }
                                                    }
                                                    else //do not delete
                                                    {
                                                        if (perConfigEdit.Id != 0)//modify perconfig
                                                        {
                                                            PerConfig perConfig = UW.context.PerConfig.Where(a => a.Id == perConfigEdit.Id).Single();
                                                            if (perConfig != null)
                                                            {
                                                                if (perConfig.Channel != perConfigEdit.Channel ||
                                                                   perConfig.Trace != perConfigEdit.Trace ||
                                                                   perConfig.StartF != perConfigEdit.StartF ||
                                                                   perConfig.StartUnitId != perConfigEdit.StartUnitId ||
                                                                   perConfig.StopF != perConfigEdit.StopF ||
                                                                   perConfig.StopUnitId != perConfigEdit.StopUnitId ||
                                                                   perConfig.ScanPoint != perConfigEdit.ScanPoint ||
                                                                   perConfig.ScanTime != perConfigEdit.ScanTime ||
                                                                   perConfig.TransportSpeed != perConfigEdit.TransportSpeed ||
                                                                   perConfig.FreqPoint != perConfigEdit.FreqPoint ||
                                                                   perConfig.LimitLine != perConfigEdit.LimitLine)
                                                                {
                                                                    perConfig.Channel = perConfigEdit.Channel;
                                                                    perConfig.Trace = perConfigEdit.Trace;
                                                                    perConfig.StartF = perConfigEdit.StartF;
                                                                    perConfig.StartUnitId = perConfigEdit.StartUnitId;
                                                                    perConfig.StopF = perConfigEdit.StopF;
                                                                    perConfig.StopUnitId = perConfigEdit.StopUnitId;
                                                                    perConfig.ScanPoint = perConfigEdit.ScanPoint;
                                                                    perConfig.ScanTime = perConfigEdit.ScanTime;
                                                                    perConfig.TransportSpeed = perConfigEdit.TransportSpeed;
                                                                    perConfig.FreqPoint = perConfigEdit.FreqPoint;
                                                                    perConfig.LimitLine = perConfigEdit.LimitLine;
                                                                    editTestItemConfig.VersionDate = modifyTime;
                                                                }
                                                            }
                                                            else
                                                            {
                                                                ModelState.AddModelError(string.Empty, "未找到对应指标记录,指标ID：" + perConfigEdit.Id);
                                                                return View(ViewPath1 + ViewPath + ViewPath2 + "Edit.cshtml", testConfigEdit);
                                                            }
                                                        }
                                                        else//add
                                                        {
                                                            PerConfig perConfigAdd = new PerConfig
                                                            {
                                                                Channel = perConfigEdit.Channel,
                                                                Trace = perConfigEdit.Trace,
                                                                StartF = perConfigEdit.StartF,
                                                                StartUnitId = perConfigEdit.StartUnitId,
                                                                StopF = perConfigEdit.StopF,
                                                                StopUnitId = perConfigEdit.StopUnitId,
                                                                ScanPoint = perConfigEdit.ScanPoint,
                                                                ScanTime = perConfigEdit.ScanTime,
                                                                TransportSpeed = perConfigEdit.TransportSpeed,
                                                                FreqPoint = perConfigEdit.FreqPoint,
                                                                LimitLine = perConfigEdit.LimitLine,
                                                                TestItemConfigId = testItemConfigEdit.Id
                                                            };
                                                            UW.context.PerConfig.Add(perConfigAdd);
                                                            editTestItemConfig.VersionDate = modifyTime;
                                                        }
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                ModelState.AddModelError(string.Empty, "未找到对应测试项配置记录,测试项ID：" + testItemConfigEdit.TestItemId);
                                                return View(ViewPath1 + ViewPath + ViewPath2 + "Edit.cshtml", testConfigEdit);
                                            }
                                        }
                                        else //add TestItemConfig And PerConfig
                                        {
                                            TestItemConfig testItemConfigAdd = new TestItemConfig
                                            {
                                                TestConfigId = testConfigEdit.Id,
                                                TestItemId = testItemConfigEdit.TestItemId,
                                                VersionDate = DateTime.Now
                                            };
                                            UW.context.TestItemConfig.Add(testItemConfigAdd);
                                            UW.CamelSave();
                                            
                                            foreach (var perconfigAdd in testItemConfigEdit.PerConfigEdits)
                                            {
                                                if (perconfigAdd.Delete != true)
                                                {
                                                    PerConfig perConfigAdd = new PerConfig
                                                    {
                                                        Channel = perconfigAdd.Channel,
                                                        Trace = perconfigAdd.Trace,
                                                        StartF = perconfigAdd.StartF,
                                                        StartUnitId = perconfigAdd.StartUnitId,
                                                        StopF = perconfigAdd.StopF,
                                                        StopUnitId = perconfigAdd.StopUnitId,
                                                        ScanPoint = perconfigAdd.ScanPoint,
                                                        ScanTime = perconfigAdd.ScanTime,
                                                        TransportSpeed = perconfigAdd.TransportSpeed,
                                                        FreqPoint = perconfigAdd.FreqPoint,
                                                        LimitLine = perconfigAdd.LimitLine,
                                                        TestItemConfigId = testItemConfigAdd.Id
                                                    };
                                                    
                                                    UW.context.PerConfig.Add(perConfigAdd);
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        ModelState.AddModelError(string.Empty, "当前测试项的指标不能全空");
                                        return View(ViewPath1 + ViewPath + ViewPath2 + "Edit.cshtml", testConfigEdit);
                                    }
                                }
                            }

                            UW.CamelSave();
                        }
                        catch (DataException)
                        {
                            ModelState.AddModelError(string.Empty, "记录更新失败");
                            return View(ViewPath1 + ViewPath + ViewPath2 + "Edit.cshtml", testConfigEdit);
                        }
                        scope.Complete();
                    }

                    Common.RMOk(this, "保存成功!");
                    return Redirect(Url.Content(returnUrl));
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "不存在此测试方案或已停用");
                    return View(ViewPath1 + ViewPath + ViewPath2 + "Edit.cshtml", testConfigEdit);
                }
            }
            else
            {
                ModelState.AddModelError(string.Empty, "所选客户当前型号产品的测试不能全空");
                return View(ViewPath1 + ViewPath + ViewPath2 + "Edit.cshtml", testConfigEdit);
            }

            //return View(ViewPath1 + ViewPath + ViewPath2 + "Edit.cshtml", testConfigEdit);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual ActionResult Delete(int id, string returnUrl = "Index")
        {
            //检查记录在权限范围内
            var result = TestConfigCommon<Model>.GetQuery(UW).Where(a => a.Id == id).SingleOrDefault();
            if (result == null)
            {
                Common.RMError(this);
                return Redirect(Url.Content(returnUrl));
            }
            //end 检查记录在权限范围内

            ViewBag.ReturnUrl = returnUrl;

            return View(ViewPath1 + ViewPath + ViewPath2 + "Delete.cshtml", result);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual ActionResult DeleteSave(int id, string returnUrl = "Index")
        {
            //检查记录在权限范围内
            var result = TestConfigCommon<Model>.GetQuery(UW).Where(a => a.Id == id).SingleOrDefault();
            if (result == null)
            {
                Common.RMError(this);
                return Redirect(Url.Content(returnUrl));
            }
            //end 检查记录在权限范围内
            var removeName = result.ToString();
            try
            {
                GR.Delete(id);
                UW.CamelTestConfigSave();
                Common.RMOk(this, "记录:" + removeName + "删除成功!");
                return Redirect(Url.Content(returnUrl));
            }
            catch (Exception e)
            {
                Common.RMError(this, "记录" + removeName + "删除失败!" + e.ToString());
            }
            return Redirect(Url.Content(returnUrl));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual ActionResult Restore(int id, string returnUrl = "Index")
        {
            var result = TestConfigCommon<Model>.GetQuery(UW, true).Where(a => a.Id == id && a.IsDeleted == true).SingleOrDefault();
            if (result == null)
            {
                Common.RMError(this);
                return Redirect(Url.Content(returnUrl));
            }
            ViewBag.ReturnUrl = returnUrl;

            return View(ViewPath1 + ViewPath + ViewPath2 + "Restore.cshtml", result);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual ActionResult RestoreSave(Model model, string returnUrl = "Index")
        {
            var result = TestConfigCommon<Model>.GetQuery(UW, true).Where(a => a.Id == model.Id && a.IsDeleted == true).SingleOrDefault();
            if (result == null)
            {
                Common.RMError(this);
                return Redirect(Url.Content(returnUrl));
            }

            try
            {
                result.IsDeleted = false;
                UW.CamelTestConfigSave();
                Common.RMOk(this, "记录:" + result + "恢复成功!");
                return Redirect(Url.Content(returnUrl));
            }
            catch (Exception e)
            {
                Common.RMOk(this, "记录" + result + "恢复失败!" + e.ToString());
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
        public virtual PartialViewResult AbstractEdit(int id)
        {
            var result = GR.GetByID(id);
            return PartialView(ViewPath1 + ViewPathBase + ViewPath2 + "AbstractEdit.cshtml", result);
        }

        public virtual PartialViewResult AbstractEditCopyOrSave(TestConfigEdit result)
        {
            return PartialView(ViewPath1 + ViewPathBase + ViewPath2 + "AbstractEditCopyOrSave.cshtml", result);
        }

        protected override void Dispose(bool disposing)
        {
            UW.Dispose();
            base.Dispose(disposing);
        }
    }
}