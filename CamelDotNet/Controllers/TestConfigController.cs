using AutoMapper;
using CamelDotNet.Models;
using CamelDotNet.Models.Common;
using CamelDotNet.Models.DAL;
using CamelDotNet.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml.Serialization;
using System.Data.Entity;
using CamelDotNet.Models.Base;
using Microsoft.AspNet.Identity;

namespace CamelDotNet.Controllers
{
    public class TestConfigController : TestConfigModelController<TestConfig>
    {
        private CamelDotNetDBContext db = new CamelDotNetDBContext();

        public List<string> path = new List<string>();
        public TestConfigController() 
        {
            path.Add("测试管理");
            path.Add("测试方案");
            ViewBag.path = path;
            ViewBag.Name = "测试方案";
            ViewBag.Title = "测试方案";
            ViewBag.Controller = "TestConfig";  
        }

        [ChildActionOnly]
        public virtual PartialViewResult GetTestItemConfigEdit(TestItemConfig item, int parentOrder)
        {
            Mapper.CreateMap<TestItemConfig, TestItemConfigEdit>();
            TestItemConfigEdit testItemConfigEdit = new TestItemConfigEdit();
            Mapper.Map(item, testItemConfigEdit);
            ViewBag.ParentOrder = parentOrder;
            return PartialView(ViewPath1 + ViewPath + ViewPath2 + ViewPathEditorTempletes + ViewPath2 + "TestItemConfigEdit.cshtml", testItemConfigEdit);
        }

        [ChildActionOnly]
        public virtual PartialViewResult GetTestItemConfigEditFeedBack(TestItemConfigEdit item, int parentOrder)
        {
            ViewBag.ParentOrder = parentOrder;
            return PartialView(ViewPath1 + ViewPath + ViewPath2 + ViewPathEditorTempletes + ViewPath2 + "TestItemConfigEdit.cshtml", item);
        }

        [ChildActionOnly]
        public virtual PartialViewResult GetPerConfigEdit(PerConfig item, string tick)
        {
            Mapper.CreateMap<PerConfig, PerConfigEdit>();
            PerConfigEdit perConfigEdit = new PerConfigEdit();
            Mapper.Map(item, perConfigEdit);
            ViewBag.PerConfigEditType = typeof(PerConfig);
            ViewBag.Tick = tick.ToString();
            return PartialView(ViewPath1 + ViewPath + ViewPath2 + ViewPathEditorTempletes + ViewPath2+"PerConfigEdit.cshtml", perConfigEdit);
        }

        [ChildActionOnly]
        public virtual PartialViewResult GetPerConfigEditFeedBack(PerConfigEdit item, string tick)
        {
            ViewBag.PerConfigEditType = typeof(PerConfig);
            ViewBag.Tick = tick.ToString();
            return PartialView(ViewPath1 + ViewPath + ViewPath2 + ViewPathEditorTempletes + ViewPath2 + "PerConfigEdit.cshtml", item);
        }

        [ChildActionOnly]
        public virtual PartialViewResult GetPerConfigEdit1()
        {
            var nestedObject = Activator.CreateInstance(typeof(PerConfigEdit));
            return PartialView("~/Views/TestConfig/EditorTemplates/PerConfigEdit.cshtml", nestedObject);
        }

        public ActionResult ClientUserCheck(string userName = null, string passWord = null) 
        {
            SingleResultXml result = new SingleResultXml()
            {
                Message = "true"
            };

            if (userName == null || passWord == null)
            {
                result.Message = "用户名或密码不能为空";
            }
            else
            {
                var user = db.UserManager.Find(userName, passWord);
                if (user == null)
                {
                    result.Message = "用户名或密码错误";
                }
                else
                {
                    if (user.IsDeleted != false)
                    {
                        result.Message = "该用户被禁用";
                    }
                }
            }

            return new XmlResult<SingleResultXml>()
            {
                Data = result
            };
        }

        private bool CheckUser(string userName = null, string passWord = null) 
        {
            bool result = true;
            if (userName != null && passWord != null)
            {
                var user = db.UserManager.Find(userName, passWord);
                if (user == null)
                {
                    result = false;
                }
                else
                {
                    if (user.IsDeleted != false)
                    {
                        return false;
                    }
                    else 
                    {
                        if (user.CamelDotNetRole.Name != "VnaTester")
                        {
                            result = false;
                        }
                    }
                }
            }
            else 
            {
                result = false;
            }

            return result;
        }

        private bool CheckEquipment(string equipmentSn)
        {
            bool result = true;
            if (equipmentSn == null)
            {
                result = false;
            }
            else 
            {
                equipmentSn = equipmentSn.ToString().Trim();
                if (equipmentSn != null && equipmentSn != "")
                {
                    var equipment = db.TestEquipment.Where(a => a.Serialnumber == equipmentSn && a.IsDeleted == false).SingleOrDefault();
                    if (equipment == null)
                    {
                        result = false;
                    }
                }
            } 

            return result;
        }

        public ActionResult GetTestConfig(string userName = null, string passWord = null, string equipmentSn = null) 
        {
            if (!CheckUser(userName, passWord)) 
            {
                SingleResultXml result = new SingleResultXml()
                {
                    Message = "用户名或密码错误"
                };

                return new XmlResult<SingleResultXml>()
                {
                    Data = result
                };
            }

            if(!CheckEquipment(equipmentSn))
            {
                SingleResultXml result = new SingleResultXml()
                {
                    Message = "仪器验证失败"
                };

                return new XmlResult<SingleResultXml>()
                {
                    Data = result
                };
            }

            List<TestConfig> testConfigs = db.TestConfig
                .Where(a => a.IsDeleted == false)
                .Where(a => a.ProductType.IsDeleted == false)
                .Where(a => a.Client.IsDeleted == false)
                .Include(a => a.ProductType)
                .Include(a => a.Client)
                .Include(a => a.TestItemConfigs.Select(b => b.TestItem).Select(c => c.TestItemCategory))
                .Include(a => a.TestItemConfigs.Select(b => b.PerConfigs))
                .ToList();

            TestConfigListXml testConfigXmlList = new TestConfigListXml();

            foreach(var testConfigItem in testConfigs)
            {
                TestConfigXml testConfigXml = new TestConfigXml();

                ProductTypeXml productTypeXml = new ProductTypeXml 
                {
                    Id = testConfigItem.ProductTypeId,
                    Name = testConfigItem.ProductType.Name
                };
                testConfigXml.ProductTypeXml = productTypeXml;
                ClientXml clientXml = new ClientXml 
                { 
                    Id = testConfigItem.ClientId,
                    Name = testConfigItem.Client.Name
                };
                testConfigXml.ClientXml = clientXml;

                foreach(var testItemConfig in testConfigItem.TestItemConfigs)
                {
                    TestItemConfigXml testItemConfigXml = new TestItemConfigXml
                    {
                        TestItemXml = new TestItemXml
                        {
                            Id = testItemConfig.TestItemId,
                            Name = testItemConfig.TestItem.Name,
                            Category = testItemConfig.TestItem.TestItemCategory.Name
                        },
                        VersionDate = testItemConfig.VersionDate.ToString("yyyyMMddHHmmss"),
                        PerConfigXmls = new List<PerConfigXml> { }
                    };

                    foreach(var perConfig in testItemConfig.PerConfigs)
                    {
                        PerConfigXml perConfigXml = new PerConfigXml() 
                        { 
                            Channel = perConfig.Channel,
                            Trace = perConfig.Trace,
                            StartF = perConfig.StartF,
                            StopF = perConfig.StopF,
                            ScanPoint = perConfig.ScanPoint,
                            ScanTime = perConfig.ScanTime,
                            TransportSpeed = perConfig.TransportSpeed,
                            FreqPoint = perConfig.FreqPoint,
                            LimitLine = perConfig.LimitLine
                        };

                        testItemConfigXml.PerConfigXmls.Add(perConfigXml);
                    }
                    testConfigXml.TestItemConfigXmls.Add(testItemConfigXml);
                }

                testConfigXmlList.TestConfigXmls.Add(testConfigXml);
            }

            return new XmlResult<TestConfigListXml>() 
            {
                Data = testConfigXmlList
            };
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
	}
}