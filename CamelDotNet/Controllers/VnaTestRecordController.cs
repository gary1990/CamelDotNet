using CamelDotNet.Models;
using CamelDotNet.Models.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;

namespace CamelDotNet.Controllers
{
    public class VnaTestRecordController : VnaTestRecordModelController<VnaRecord>
    {
        public List<string> path = new List<string>();
        public VnaTestRecordController() 
        {
            path.Add("质量追溯");
            path.Add("VNA测试");
            ViewBag.path = path;
            ViewBag.Name = "VNA测试";
            ViewBag.Title = "VNA测试";
            ViewBag.Controller = "VnaRecord";  
        }

        public ActionResult UploadVnaRecord()
        {
            SingleResultXml result = new SingleResultXml() 
            {
                Message = "true"
            };

            HttpPostedFileBase file = Request.Files["files"];
            string savePath;

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
                string fileFullName = System.IO.Path.GetFileName(file.FileName);
                string fileEx = System.IO.Path.GetExtension(fileFullName);
                string fileNameWithoutEx = System.IO.Path.GetFileNameWithoutExtension(fileFullName);
                if(fileEx != ".zip")
                {
                    result.Message = "文件类型不正确，只能上传zip格式的文件";
                    return new XmlResult<SingleResultXml>()
                    {
                        Data = result
                    };
                }
                string uploadTime = DateTime.Now.ToString("yyyyMMdd");
                string uploadPath = HostingEnvironment.ApplicationPhysicalPath + "Content/UploadedFolder/VnaRecord/" + uploadTime;
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

            //7zip解压缩文件
            
            string osVersion = Environment.OSVersion.ToString();
            if (osVersion.ToUpper().Contains("WIN"))
            {
                if (Environment.Is64BitProcess)
                {
                    
                }
            }
            else 
            {
                result.Message = "未能识别的操作系统";
                return new XmlResult<SingleResultXml>()
                {
                    Data = result
                };  
            }
            //if (Environment.Is64BitOperatingSystem) 
            //{
            
            //}
            //else if(Environment.OSVersion)
            //{
            
            //}

            return new XmlResult<SingleResultXml>()
            {
                Data = result
            };
        }
	}
}