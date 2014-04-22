using CamelDotNet.Models;
using CamelDotNet.Models.Common;
using CamelDotNet.Models.DAL;
using Ionic.Zip;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Transactions;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;

namespace CamelDotNet.Controllers
{
    public class VnaTestRecordController : VnaTestRecordModelController<VnaRecord>
    {
        private CamelDotNetDBContext db = new CamelDotNetDBContext();
        public List<string> path = new List<string>();
        public VnaTestRecordController() 
        {
            path.Add("质量追溯");
            path.Add("VNA测试");
            ViewBag.path = path;
            ViewBag.Name = "VNA测试";
            ViewBag.Title = "VNA测试";
            ViewBag.Controller = "VnaTestRecord";
        }

        public void TestZip() 
        {
            var dt = DateTime.Now;

            Response.Write(dt.ToString("yyyy/MM/dd HH:mm:ss"));
            //using (var zip = new ZipFile(Encoding.Default))
            //{
            //    zip.AddDirectory("c:\\test");
            //    zip.Save("c:\\zip\\archive.zip");
            //}
        }

        public ActionResult UploadVnaRecord()
        {
            SingleResultXml result = new SingleResultXml() 
            {
                Message = "true"
            };

            HttpPostedFileBase file = Request.Files["file"];
            string fileFullName;
            string fileEx;
            string fileNameWithoutEx;
            string uploadPath;
            string savePath;
            string uploadTime = DateTime.Now.ToString("yyyyMMdd");
            string slash = "/";

            if (file == null || file.ContentLength <= 0)
            {
                result.Message = "文件不能为空";
                return new XmlResult<SingleResultXml>()
                {
                    Data = result
                };
            }
            else 
            {
                fileFullName = System.IO.Path.GetFileName(file.FileName);
                fileEx = System.IO.Path.GetExtension(fileFullName);
                fileNameWithoutEx = System.IO.Path.GetFileNameWithoutExtension(fileFullName);
                if(fileEx != ".zip")
                {
                    result.Message = "文件类型不正确，只能上传zip格式的文件";
                    return new XmlResult<SingleResultXml>()
                    {
                        Data = result
                    };
                }
                
                uploadPath = AppDomain.CurrentDomain.BaseDirectory + "Content/UploadedFolder/VnaRecord/" + uploadTime;
                bool isExists = System.IO.Directory.Exists(uploadPath);
                if(!isExists)
                {
                    try 
                    {
                        System.IO.Directory.CreateDirectory(uploadPath);
                    }
                    catch(Exception)
                    {
                        result.Message = "创建上传目录失败";
                        return new XmlResult<SingleResultXml>()
                        {
                            Data = result
                        }; 
                    } 
                }
                savePath = System.IO.Path.Combine(uploadPath, fileFullName);
                try 
                { 
                    file.SaveAs(savePath);
                }
                catch(Exception)
                {
                    result.Message = "保存文件失败";
                    return new XmlResult<SingleResultXml>()
                    {
                        Data = result
                    }; 
                }
            }

            ZipFile zip = ZipFile.Read(savePath, new ReadOptions { Encoding = Encoding.Default });
            try 
            {
                //解压缩文件
                zip.AlternateEncoding = Encoding.Default;
                zip.ExtractAll(uploadPath, ExtractExistingFileAction.OverwriteSilently);
            }
            catch(Exception)
            {
                zip.Dispose();
                result.Message = "解压缩文件失败";
                return new XmlResult<SingleResultXml>()
                {
                    Data = result
                }; 
            }
            zip.Dispose();

            var serialNumber = fileNameWithoutEx.Substring(fileNameWithoutEx.IndexOf("_") + 1);
            var serialNumberRecord = db.SerialNumber.Where(a => a.Number == serialNumber).SingleOrDefault();
            //解析文件
            using (var scope = new TransactionScope())
            {
                //解析General.csv文件
                try
                {
                    //是否重测
                    bool isRetest = false;
                    //产品序列号
                    int serialNumberId;
                    int vnaRecordId = 0;
                    if (serialNumberRecord != null)
                    {
                        //reTest
                        isRetest = true;
                        serialNumberId = serialNumberRecord.Id;
                    }
                    else
                    {
                        try
                        {
                            SerialNumber serialNumberAdd = new SerialNumber()
                            {
                                Number = serialNumber
                            };
                            db.SerialNumber.Add(serialNumberAdd);
                            db.SaveChanges();
                            serialNumberId = serialNumberAdd.Id;
                        }
                        catch (Exception)
                        {
                            result.Message = "插入产品序列号失败";
                            return new XmlResult<SingleResultXml>()
                            {
                                Data = result
                            };
                        }
                    }

                    StreamReader sr = new StreamReader(uploadPath + slash + fileNameWithoutEx + slash + "General.csv");
                    string line = string.Empty;
                    string[] lineArr = null;
                    int i = 0;
                    while ((line = sr.ReadLine()) != null)
                    {
                        i++;
					    if (i == 1)
					    {
                            lineArr = line.Split(',');
						    continue;
					    }
                        lineArr = line.Split(',');
                        //测试时间
                        string testTimeStr = lineArr[0];
                        DateTime testTime;
                        if (!DateTime.TryParseExact(testTimeStr, "yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out testTime)) 
                        {
                            if (!DateTime.TryParseExact(testTimeStr, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out testTime))
                            {
                                sr.Close();
                                result.Message = "测试时间转换失败";
                                return new XmlResult<SingleResultXml>()
                                {
                                    Data = result
                                };
                            }  
                        }
                        
                        //测试站点
                        string testStationName = lineArr[1];
                        int testStationId;
                        var testStationRecord = UW.context.TestStation.Where(a => a.Name == testStationName && a.IsDeleted == false).SingleOrDefault();
                        if (testStationRecord != null)
                        {
                            testStationId = testStationRecord.Id;
                        }
                        else 
                        {
                            sr.Close();
                            result.Message = "General.csv中测试站点不存在";
                            return new XmlResult<SingleResultXml>()
                            {
                                Data = result
                            };
                        }
                        //测试设备序列号
                        string testEquipmentSn = lineArr[2];
                        int testEquipmentId;
                        var testEquipmentRecord = UW.context.TestEquipment.Where(a => a.Serialnumber == testEquipmentSn && a.IsDeleted == false).SingleOrDefault();
                        if (testEquipmentRecord != null)
                        {
                            testEquipmentId = testEquipmentRecord.Id;
                        }
                        else 
                        {
                            sr.Close();
                            result.Message = "General.csv中测试设备不存在";
                            return new XmlResult<SingleResultXml>()
                            {
                                Data = result
                            };
                        }
                        //测试员工号
                        string testerJobNum = lineArr[3];
                        string testerId;
                        var testerRecord = UW.CamelDotNetUserRepository.dbSet.Where(a => a.JobNumber == testerJobNum && a.IsDeleted == false).SingleOrDefault();
                        if (testerRecord != null)
                        {
                            testerId = testerRecord.Id;
                        }
                        else 
                        {
                            sr.Close();
                            result.Message = "General.csv中测试员不存在";
                            return new XmlResult<SingleResultXml>()
                            {
                                Data = result
                            };
                        }
                        //产品型号
                        string productTypeName = lineArr[4];
                        //物料名称
                        string modelName = lineArr[18];
                        if (modelName == "" || modelName == null)
                        {
                            modelName = null;
                        }
                        int productTypeId;
                        var productTypeRecord = UW.context.ProductType.Where(a => a.Name == productTypeName && a.ModelName == modelName && a.IsDeleted == false).SingleOrDefault();
                        if (productTypeRecord != null)
                        {
                            productTypeId = productTypeRecord.Id;
                        }
                        else 
                        {
                            sr.Close();
                            result.Message = "General.csv中产品型号不存在";
                            return new XmlResult<SingleResultXml>()
                            {
                                Data = result
                            };
                        }
                        //取得无需校验项
                        //测试结果
                        string testResultStr = lineArr[6];
                        bool testResult = false;
                        if (testResultStr.ToUpper() == "FAIL")
                        {
                            testResult = true;
                        }
                        //温度
                        string temperatureStr = lineArr[7];
                        decimal temperature;
                        if (!Decimal.TryParse(temperatureStr, out temperature))
                        {
                            sr.Close();
                            result.Message = "General.csv中温度转换失败";
                            return new XmlResult<SingleResultXml>()
                            {
                                Data = result
                            };
                        }
                        //盘号
                        string reelNumber = lineArr[8];
                        //机台
                        string drillingCrew = lineArr[9];
                        //内端计米
                        string innerLengthStr = lineArr[10];
                        decimal innerLength;
                        if (!Decimal.TryParse(innerLengthStr, out innerLength))
                        {
                            sr.Close();
                            result.Message = "General.csv中内端计米转换失败";
                            return new XmlResult<SingleResultXml>()
                            {
                                Data = result
                            };
                        }
                        //外端计米
                        string outerLengthStr = lineArr[11];
                        decimal outerLength;
                        if (!Decimal.TryParse(outerLengthStr, out outerLength))
                        {
                            sr.Close();
                            result.Message = "General.csv中外端计米转换失败";
                            return new XmlResult<SingleResultXml>()
                            {
                                Data = result
                            };
                        }
                        //工单号
                        string orderNumber = lineArr[12];
                        //班组
                        string workGroup = lineArr[13];
                        //不统计
                        string noStatisticsStr = lineArr[14];
                        bool noStatistics = false;
                        if (noStatisticsStr.ToUpper() == "TRUE")
                        {
                            noStatistics = true;
                        }
                        //备注
                        string remark = lineArr[15];
                        //客户
                        string clientName = lineArr[16];
                        int clientId;
                        var clientRecord = UW.context.Client.Where(a => a.Name == clientName && a.IsDeleted == false).SingleOrDefault();
                        if (clientRecord != null)
                        {
                            clientId = clientRecord.Id;
                        }
                        else
                        {
                            sr.Close();
                            result.Message = "General.csv中客户不存在";
                            return new XmlResult<SingleResultXml>()
                            {
                                Data = result
                            };
                        }
                        //订单编号
                        string orderNo = lineArr[17];
                        //插入General.csv中数据
                        try 
                        {
                            VnaRecord vnaRecord = new VnaRecord()
                            {
                                SerialNumberId = serialNumberId,
                                ProductTypeId = productTypeId,
                                CamelDotNetUserId = testerId,
                                TestEquipmentId = testEquipmentId,
                                TestStationId = testStationId,
                                OrderNumber = orderNumber,
                                OrderNo = orderNo,
                                DrillingCrew = drillingCrew,
                                ReelNumber = reelNumber,
                                WorkGroup = workGroup,
                                TestTime = testTime,
                                TestResult = testResult,
                                Temperature = temperature,
                                InnerLength = innerLength,
                                OuterLength = outerLength,
                                reTest = isRetest,
                                NoStatistics = noStatistics,
                                Remark = remark,
                                ClientId = clientId,
                            };
                            db.VnaRecord.Add(vnaRecord);
                            db.SaveChanges();
                            vnaRecordId = vnaRecord.Id;
                        }
                        catch(Exception)
                        {
                            sr.Close();
                            result.Message = "插入General.csv信息失败";
                            return new XmlResult<SingleResultXml>()
                            {
                                Data = result
                            };
                        } 
                    }
                    sr.Close();

                    //非电气性能，读取nep.txt
                    try
                    {
                        StreamReader nepSr = new StreamReader(uploadPath + slash + fileNameWithoutEx + slash + "nep.txt");
                        string nepIdString = nepSr.ReadToEnd();
                        if (nepIdString.Length != 0)
                        {
                            string[] nepIdArr = nepIdString.Split(',');
                            foreach(var item in nepIdArr)
                            {
                                try
                                {
                                    int testItemId = Int32.Parse(item);
                                    var nepTestItemRecord = UW.context.TestItem.Where(a => a.Id == testItemId && a.IsDeleted == false).SingleOrDefault();
                                    if (nepTestItemRecord != null)
                                    {
                                        VnaTestItemRecord vnaTestItemRecord = new VnaTestItemRecord()
                                        {
                                            VnaRecordId = vnaRecordId,
                                            TestItemId = nepTestItemRecord.Id,
                                            TestItemResult = true,
                                        };
                                        db.VnaTestItemRecord.Add(vnaTestItemRecord);
                                        db.SaveChanges();
                                    }
                                    else
                                    {
                                        nepSr.Close();
                                        result.Message = "nep.txt中测试项Id:" + item + "不存在";
                                        return new XmlResult<SingleResultXml>()
                                        {
                                            Data = result
                                        };
                                    }
                                }
                                catch(Exception)
                                {
                                    nepSr.Close();
                                    result.Message = "非电气性能不合格项Id:" + item + "插入失败";
                                    return new XmlResult<SingleResultXml>()
                                    {
                                        Data = result
                                    };
                                }
                            }
                        }
                        nepSr.Close();
                    }
                    catch (Exception)
                    {
                        result.Message = "非电气性能不合格项nep.txt读取失败";
                        return new XmlResult<SingleResultXml>()
                        {
                            Data = result
                        };
                    }

                    //读取各测试项csv
                    string[] testItemFiles = Directory.GetFiles(uploadPath + slash + fileNameWithoutEx,"*.csv");
                    foreach (var testItemFile in testItemFiles)
                    {
                        if (!testItemFile.Contains("General"))
                        {
                            var testItemFileArr = testItemFile.Split(new char[] {'-','.'});
                            var testItemName = testItemFileArr[0].Substring(testItemFileArr[0].LastIndexOf('\\') + 1);
                            var testItemRecord = UW.context.TestItem.Where(a => a.Name == testItemName && a.IsDeleted == false).SingleOrDefault();
                            if (testItemRecord != null)
                            {
                                var TestItemResultStr = testItemFileArr[1];
                                bool testItemResult = false;
                                if (TestItemResultStr.ToUpper() == "FAIL")
                                {
                                    testItemResult = true;
                                }
                                var imagePath = uploadTime + slash + fileNameWithoutEx + slash + testItemName + "-img.png";
                                int testItemRecordId;
                                try 
                                {
                                    VnaTestItemRecord vnaTestItemRecord = new VnaTestItemRecord()
                                    {
                                        VnaRecordId = vnaRecordId,
                                        TestItemId = testItemRecord.Id,
                                        TestItemResult = testItemResult,
                                        ImagePath = imagePath,
                                    };
                                    db.VnaTestItemRecord.Add(vnaTestItemRecord);
                                    db.SaveChanges();
                                    testItemRecordId = vnaTestItemRecord.Id;
                                }
                                catch(Exception)
                                {
                                    result.Message = "测试项:" + testItemName + "插入失败";
                                    return new XmlResult<SingleResultXml>()
                                    {
                                        Data = result
                                    };
                                }
                                //插入VnaTestItemPerRecord
                                try
                                {
                                    StreamReader testItemSr = new StreamReader(testItemFile);
                                    string perTestItemLine = string.Empty;
                                    string[] perTestItemLineArr = null;
                                    int j = 0;
                                    while ((perTestItemLine = testItemSr.ReadLine()) != null)
                                    {
                                        j++;
                                        if (j == 1)
                                        {
                                            perTestItemLineArr = perTestItemLine.Split(',');
                                            continue;
                                        }
                                        perTestItemLineArr = perTestItemLine.Split(',');
                                        //XVlaue
                                        string xValueStr = perTestItemLineArr[0];
                                        decimal xValue = 0M;
                                        if (xValueStr != null && xValueStr.Trim() != "")
                                        {
                                            if (!Decimal.TryParse(xValueStr, out xValue))
                                            {
                                                result.Message = "测试项:" + testItemName + "xValue转换失败";
                                                return new XmlResult<SingleResultXml>()
                                                {
                                                    Data = result
                                                };
                                            }
                                        }
                                        //YValue
                                        string yValueStr = perTestItemLineArr[1];
                                        decimal yValue = 0M;
                                        var ss = yValueStr.Trim();
                                        if (yValueStr != null && yValueStr.Trim() != "")
                                        {
                                            if (!Decimal.TryParse(yValueStr, out yValue))
                                            {
                                                result.Message = "测试项:" + testItemName + "yValue转换失败";
                                                return new XmlResult<SingleResultXml>()
                                                {
                                                    Data = result
                                                };
                                            }
                                        }
                                        //RValue
                                        string rValueStr = perTestItemLineArr[2];
                                        decimal rValue = 0M;
                                        if (rValueStr != null && rValueStr.Trim() != "")
                                        {
                                            if (!Decimal.TryParse(rValueStr, out rValue))
                                            {
                                                result.Message = "测试项:" + testItemName + "rValue转换失败";
                                                return new XmlResult<SingleResultXml>()
                                                {
                                                    Data = result
                                                };
                                            }
                                        }
                                        //TestitemPerResult
                                        var testitemPerResultStr = perTestItemLineArr[3];
                                        bool testitemPerResult = false;
                                        if (testitemPerResultStr.ToUpper() == "FAIL")
                                        {
                                            testitemPerResult = true;
                                        }

                                        try 
                                        {
                                            VnaTestItemPerRecord vnaTestItemPerRecord = new VnaTestItemPerRecord()
                                            {
                                                VnaTestItemRecordId = testItemRecordId,
                                                XValue = xValue,
                                                YValue = yValue,
                                                RValue = rValue,
                                                TestitemPerResult = testitemPerResult,
                                            };
                                            db.VnaTestItemPerRecord.Add(vnaTestItemPerRecord);
                                            db.SaveChanges();
                                        }
                                        catch(Exception)
                                        {
                                            result.Message = "测试项:" + testItemName + "csv文件内容插入失败";
                                            return new XmlResult<SingleResultXml>()
                                            {
                                                Data = result
                                            };
                                        }
                                    }
                                    testItemSr.Close();
                                }
                                catch (Exception)
                                {
                                    result.Message = "测试项:" + testItemName + "csv文件打开失败";
                                    return new XmlResult<SingleResultXml>()
                                    {
                                        Data = result
                                    };
                                }
                            }
                            else 
                            {
                                result.Message = "对相应测试项：'" + testItemName + "'没有找到";
                                return new XmlResult<SingleResultXml>()
                                {
                                    Data = result
                                };
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    result.Message = "General.csv打开失败";
                    return new XmlResult<SingleResultXml>()
                    {
                        Data = result
                    };
                } 
                scope.Complete();
            }
            

            return new XmlResult<SingleResultXml>()
            {
                Data = result
            };
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
	}
}