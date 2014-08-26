using CamelDotNet.Models.DAL;
using CamelDotNet.Models.ViewModels;
using DotNet.Highcharts;
using DotNet.Highcharts.Enums;
using DotNet.Highcharts.Helpers;
using DotNet.Highcharts.Options;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CamelDotNet.Controllers
{
    public class CoaxialQualityStatController : Controller
    {
        private CamelDotNetDBContext db = new CamelDotNetDBContext();

        List<string> path = new List<string>();
        public CoaxialQualityStatController()
        {
            path.Add("报表管理");
            path.Add("同轴质量统计");
            ViewBag.path = path;
            ViewBag.Name = "同轴质量统计";
            ViewBag.Title = "同轴质量统计";
        }
        //
        // GET: /CoaxialQualityStat/
        public ActionResult Index()
        {
            List<SelectListItem> items = new List<SelectListItem>();

            items.Add(new SelectListItem { Text = "00", Value = "00" });
            items.Add(new SelectListItem { Text = "01", Value = "01" });
            items.Add(new SelectListItem { Text = "02", Value = "02"});
            items.Add(new SelectListItem { Text = "03", Value = "03" });
            items.Add(new SelectListItem { Text = "04", Value = "04" });
            items.Add(new SelectListItem { Text = "05", Value = "05" });
            items.Add(new SelectListItem { Text = "06", Value = "06" });
            items.Add(new SelectListItem { Text = "07", Value = "07" });
            items.Add(new SelectListItem { Text = "08", Value = "08" });
            items.Add(new SelectListItem { Text = "09", Value = "09" });
            for (int i = 10; i <= 23; i++ )
            {
                items.Add(new SelectListItem { Text = i.ToString(), Value = i.ToString() });
            }
            ViewBag.Hours = items;

            return View();
        }
        
        public ActionResult StatByFailItem(string TestTimeStartDay = null, string TestTimeStartHour = null, string TestTimeStopDay = null, string TestTimeStopHour = null, string DrillingCrew = null, string WorkGroup = null, int ProductTypeId = 0)
        {
            ViewBag.path = path;
            string errorMsg = null;
            DateTime testTimeStart = DateTime.Now.Date;
            DateTime testTimeStop = DateTime.Now;
            string testTimeStarStr;
            string testTimeStopStr;
            if (TestTimeStartHour == "")
            {
                TestTimeStartHour = "00";
            }
            if (TestTimeStopHour == "")
            {
                TestTimeStopHour = "23";
            }
            if (TestTimeStartDay != "")
            {
                testTimeStarStr = TestTimeStartDay + " " + TestTimeStartHour;
                if (!DateTime.TryParseExact(testTimeStarStr, "yyyy-MM-dd HH", CultureInfo.InvariantCulture, DateTimeStyles.None, out testTimeStart))
                {
                }
            }
            if (TestTimeStopDay != "")
            {
                testTimeStopStr = TestTimeStopDay + " " + TestTimeStopHour;
                if (!DateTime.TryParseExact(testTimeStopStr, "yyyy-MM-dd HH", CultureInfo.InvariantCulture, DateTimeStyles.None, out testTimeStop))
                {
                }
            }
            string sql = "exec p_coaxialqualitystatTab_Rp @testtimestart, @testtimestop, @drillingcrew, @workgroup";
            SqlParameter[] param = new SqlParameter[4];
            param[0] = new SqlParameter("@testtimestart", SqlDbType.DateTime2);
            param[0].Value = testTimeStart;
            param[1] = new SqlParameter("@testtimestop", SqlDbType.DateTime2);
            param[1].Value = testTimeStop;
            param[2] = new SqlParameter("@drillingcrew", SqlDbType.NVarChar);
            param[2].Value = DrillingCrew;
            param[3] = new SqlParameter("@workgroup", SqlDbType.NVarChar);
            param[3].Value = WorkGroup;

            DataTable dt = CommonController.GetDateTable(sql, param);
            //vna total result list
            List<VnaTotalResult> vnaTotalResultList = new List<VnaTotalResult>();
            //vna total result length
            decimal totalLength = 0;
            //vna fail result length
            decimal failLength = 0;
            //initial chart
            DotNet.Highcharts.Highcharts chart = new DotNet.Highcharts.Highcharts("chart");
            if(dt.Rows.Count > 0)
            {
                //auto map dt to VnaTotalResult
                DataTableReader dr = dt.CreateDataReader();
                vnaTotalResultList = AutoMapper.Mapper.DynamicMap<IDataReader, List<VnaTotalResult>>(dr);
                //fail list
                List<VnaTotalResult> vnaFailResultList = vnaTotalResultList.Where(a => a.TestResult == 1).ToList();
                // no defined qulity loss
                var noDefQualityLossList = vnaFailResultList
                    .Where(a => a.LossPercent_Result == null)
                    .GroupBy(p => new { p.TestItemName_Fail, p.ProcessName_Fail })
                    .Select(y => y.First()).ToList();
                if (noDefQualityLossList.Count > 0)
                {
                    errorMsg += "<b style='color:red;'>未定义质量损失比</b>：<br/>";
                    foreach (var noDefQulityLoss in noDefQualityLossList)
                    {
                        errorMsg += "测试项：" + noDefQulityLoss.TestItemName_Fail + ", 工序：" + noDefQulityLoss.ProcessName_Fail + "<br/>";
                    }
                    ViewBag.ErrorMsg = errorMsg;
                }
                else 
                {
                    //get total length, unit m to km
                    totalLength = vnaTotalResultList.Sum(a => a.Lengths) / 1000;
                    //get total length, unit m to km
                    failLength = vnaFailResultList.Sum(a => a.Lengths) / 1000;
                    ViewBag.TotalLength = totalLength;
                    ViewBag.FailLength = failLength;
                    //get fail group list
                    var vnaFailGroupList = vnaFailResultList
                        .GroupBy(a => new
                        {
                            a.ProcessName_Fail,
                            a.TestItemName_Fail,
                            a.FreqFormularR,
                            a.ValueFormularR
                        })
                        .Select(ac => new
                        {
                            ProcessName = ac.Key.ProcessName_Fail,
                            TestItemName = ac.Key.TestItemName_Fail,
                            FreqFormularR = ac.Key.FreqFormularR,
                            ValueFormularR = ac.Key.ValueFormularR,
                            Length = ac.Sum(acs => acs.Lengths)
                        }).ToList();
                    vnaFailGroupList = vnaFailGroupList.OrderBy(a => a.ProcessName).ThenBy(a => a.TestItemName).ThenBy(a => a.FreqFormularR).ThenBy(a => a.ValueFormularR).ToList();
                    //totalXObj for X(X list), totalYObj for Y(Y is fail lenght list)
                    var totalXObj = new string[vnaFailGroupList.Count()];
                    var totalYObj = new object[vnaFailGroupList.Count()];
                    for (int i = 0; i < vnaFailGroupList.Count(); i++)
                    {
                        var vnaFailGroup = vnaFailGroupList[i];
                        //formart FreqFormularR/ValueFormularR, (-∞, +∞) to null
                        var freqFormularR = vnaFailGroup.FreqFormularR;
                        var valueFormularR = vnaFailGroup.ValueFormularR;
                        if (freqFormularR.Contains("+"))
                        {
                            freqFormularR = null;
                        }
                        if (valueFormularR.Contains("+"))
                        {
                            valueFormularR = null;
                        }
                        totalXObj[i] = valueFormularR + "<br/>" + freqFormularR + "<br/>" + vnaFailGroup.TestItemName + "<br/>" + vnaFailGroup.ProcessName + "<br/>　　　<br/>";
                        //vnaFailGroup length, unit m to km
                        totalYObj[i] = Convert.ToDecimal(vnaFailGroup.Length / 1000);
                    }
                    chart
                    .SetTitle(new Title { Text = "不合格项不合格量统计" })
                    .SetXAxis(new XAxis
                    {
                        Categories = totalXObj
                    })
                    .SetYAxis(new YAxis
                    {
                        Title = new YAxisTitle { Text = "不合格量(km)" }
                    })
                    .SetSeries(new Series
                    {
                        Name = "工序-测试项-点-值",
                        Type = ChartTypes.Column,
                        Data = new Data(totalYObj)
                    })
                    .SetTooltip(new Tooltip
                    {
                        Formatter = @"function(){return '<b>不合格量</b>:' + this.y + 'km<br/>' + '<b>不合格比</b>: ' + Highcharts.numberFormat((this.y/" + failLength + ")*100) + '%'}"
                    });
                    var s = JsonSerializer.Serialize<object>(totalXObj);
                    
                }
            }
            
            List<Highcharts> chartList = new List<Highcharts> { };
            chartList.Add(chart);
            return PartialView(chartList);
        }

        public ActionResult StatByProductType(string TestTimeStartDay = null, string TestTimeStartHour = null, string TestTimeStopDay = null, string TestTimeStopHour = null, string DrillingCrew = null, string WorkGroup = null, int ProductTypeId = 0) 
        {
            ViewBag.path = path;
            DateTime testTimeStart = DateTime.Now.Date;
            DateTime testTimeStop = DateTime.Now;
            string testTimeStarStr;
            string testTimeStopStr;
            if (TestTimeStartHour == "")
            {
                TestTimeStartHour = "00";
            }
            if (TestTimeStopHour == "")
            {
                TestTimeStopHour = "23";
            }
            if (TestTimeStartDay != "")
            {
                testTimeStarStr = TestTimeStartDay + " " + TestTimeStartHour;
                if (!DateTime.TryParseExact(testTimeStarStr, "yyyy-MM-dd HH", CultureInfo.InvariantCulture, DateTimeStyles.None, out testTimeStart))
                {
                }
            }
            if (TestTimeStopDay != "")
            {
                testTimeStopStr = TestTimeStopDay + " " + TestTimeStopHour;
                if (!DateTime.TryParseExact(testTimeStopStr, "yyyy-MM-dd HH", CultureInfo.InvariantCulture, DateTimeStyles.None, out testTimeStop))
                {
                }
            }

            var totalRecord = db.VnaRecord.Where(a => a.NoStatistics == false).Where(a => a.TestTime <= testTimeStop && a.TestTime >= testTimeStart);
            if(DrillingCrew != "")
            {
                totalRecord = totalRecord.Where(a => a.DrillingCrew.Contains(DrillingCrew));
            }
            if(WorkGroup != "")
            {
                totalRecord = totalRecord.Where(a => a.WorkGroup.Contains(WorkGroup));
            }
            if(ProductTypeId != 0)
            {
                totalRecord = totalRecord.Where(a => a.ProductTypeId == ProductTypeId);
            }
            //total record, length/1000 because InnerLength/OuterLength unit in database is m
            var totalRecordList = totalRecord.GroupBy(a => a.ProductTypeId).Select(b => new { 
                id = b.Key,
                name = b.Select(c => c.ProductType.Name +"#"+ c.ProductType.ModelName).Distinct().FirstOrDefault(), 
                count = b.Count(), 
                length = b.Sum(c => Math.Abs(c.InnerLength-c.OuterLength)/1000)}).OrderByDescending(d => d.length).ToList();
            //fail record, length/1000 because InnerLength/OuterLength unit in database is m
            var failRecordList = totalRecord.Where(a => a.TestResult == true).GroupBy(a => a.ProductTypeId).Select(b => new {
                                    id = b.Key,
                                    name = b.Select(c => c.ProductType.Name + "#" + c.ProductType.ModelName).Distinct().FirstOrDefault(),
                                    count = b.Count(),
                                    length = b.Sum(c => Math.Abs(c.InnerLength - c.OuterLength)/1000)
                                }).ToList();
            decimal totalLength = 0;
            decimal totalFailLength = 0;
            //get total length
            foreach (var item in totalRecordList)
            {
                totalLength += item.length;
            }
            //get total fail length
            foreach (var item in failRecordList)
            {
                totalFailLength += item.length;
            }

            //totalProductObj for X(product list), totalFailObj for Y(fail lenght list)
            var totalProductObj = new string[totalRecordList.Count()];
            var totalFailObj = new object[totalRecordList.Count()];
            for (int i = 0; i < totalProductObj.Count(); i++)
            {
                totalProductObj[i] = totalRecordList[i].name;
                var currentFail = failRecordList.Where(a => a.name == totalRecordList[i].name).SingleOrDefault();
                if (currentFail == null)
                {
                    totalFailObj[i] = 0;
                }
                else 
                {
                    totalFailObj[i] = Convert.ToDecimal(currentFail.length);
                }
            }

            DotNet.Highcharts.Highcharts chart = new DotNet.Highcharts.Highcharts("chart")
            .SetTitle(new Title { Text = "物料名称不合格量统计" })
            .SetXAxis(new XAxis
            {
                Categories = totalProductObj
            })
            .SetYAxis(new YAxis
            {
                Title = new YAxisTitle { Text = "不合格量(km)" }
            })
            .SetSeries(new Series
            {
                Name = "产品型号",
                Type = ChartTypes.Column,
                Data = new Data(totalFailObj)
            })
            .SetTooltip(new Tooltip
            {
                Formatter = @"function(){return '<b>不合格量</b>:' + this.y + 'km<br/>' + '<b>不合格比</b>: ' + Highcharts.numberFormat((this.y/" + totalFailLength + ")*100) + '%'}"
            })
            .SetPlotOptions(new PlotOptions() {
                Pie = new PlotOptionsPie
                {
                    AllowPointSelect = true,
                    Cursor = Cursors.Pointer,
                    DataLabels = new PlotOptionsPieDataLabels
                    {
                        Color = ColorTranslator.FromHtml("#000000"),
                        ConnectorColor = ColorTranslator.FromHtml("#000000"),
                        Formatter = @"function(){return '<b>不合格量</b>:' + this.y + '<br/>' + '<b>不合格比</b>: ' + Highcharts.numberFormat((this.y/" + totalFailLength + ")*100) + '%'}"
                    }
                }
            });
            
            ViewBag.TotalLength = totalLength;
            ViewBag.TotalFailLength = totalFailLength;

            List<Highcharts> chartList = new List<Highcharts> { };
            chartList.Add(chart);
            return PartialView(chartList);
        }
        
        public ActionResult StatByDrillingCrew(string TestTimeStartDay = null, string TestTimeStartHour = null, string TestTimeStopDay = null, string TestTimeStopHour = null, string DrillingCrew = null, string WorkGroup = null, int ProductTypeId = 0)
        {
            DateTime testTimeStart = DateTime.Now.Date;
            DateTime testTimeStop = DateTime.Now;
            string testTimeStarStr;
            string testTimeStopStr;
            if (TestTimeStartHour == "")
            {
                TestTimeStartHour = "00";
            }
            if (TestTimeStopHour == "")
            {
                TestTimeStopHour = "23";
            }
            if (TestTimeStartDay != "")
            {
                testTimeStarStr = TestTimeStartDay + " " + TestTimeStartHour;
                if (!DateTime.TryParseExact(testTimeStarStr, "yyyy-MM-dd HH", CultureInfo.InvariantCulture, DateTimeStyles.None, out testTimeStart))
                {
                }
            }
            if (TestTimeStopDay != "")
            {
                testTimeStopStr = TestTimeStopDay + " " + TestTimeStopHour;
                if (!DateTime.TryParseExact(testTimeStopStr, "yyyy-MM-dd HH", CultureInfo.InvariantCulture, DateTimeStyles.None, out testTimeStop))
                {
                }
            }

            var totalRecord = db.VnaRecord.Where(a => a.NoStatistics == false).Where(a => a.TestTime <= testTimeStop && a.TestTime >= testTimeStart);
            if (DrillingCrew != "")
            {
                totalRecord = totalRecord.Where(a => a.DrillingCrew.Contains(DrillingCrew));
            }
            if (WorkGroup != "")
            {
                totalRecord = totalRecord.Where(a => a.WorkGroup.Contains(WorkGroup));
            }
            if (ProductTypeId != 0)
            {
                totalRecord = totalRecord.Where(a => a.ProductTypeId == ProductTypeId);
            }
            //total record, length/1000 because InnerLength/OuterLength unit in database is m
            var totalRecordList = totalRecord.GroupBy(a => a.DrillingCrew).Select(b => new
            {
                id = b.Key,
                length = b.Sum(c => Math.Abs(c.InnerLength - c.OuterLength)/1000)
            }).OrderByDescending(d => d.length).ToList();
            //fail record, length/1000 because InnerLength/OuterLength unit in database is m
            var failRecordList = totalRecord.Where(a => a.TestResult == true).GroupBy(a => a.DrillingCrew).Select(b => new
            {
                id = b.Key,
                length = b.Sum(c => Math.Abs(c.InnerLength - c.OuterLength)/1000)
            }).ToList();
            decimal totalLength = 0;
            decimal totalFailLength = 0;
            //get total length
            foreach (var item in totalRecordList)
            {
                totalLength += item.length;
            }
            //get total fail length
            foreach (var item in failRecordList)
            {
                totalFailLength += item.length;
            }

            //totalDrillingCrewObj for X(product list), totalFailObj for Y(fail lenght list)
            var totalDrillingCrewObj = new string[totalRecordList.Count()];
            var totalFailObj = new object[totalRecordList.Count()];
            for (int i = 0; i < totalDrillingCrewObj.Count(); i++)
            {
                totalDrillingCrewObj[i] = totalRecordList[i].id;
                var currentFail = failRecordList.Where(a => a.id == totalRecordList[i].id).SingleOrDefault();
                if (currentFail == null)
                {
                    totalFailObj[i] = 0;
                }
                else
                {
                    totalFailObj[i] = Convert.ToDecimal(currentFail.length);
                }
            }

            DotNet.Highcharts.Highcharts chart = new DotNet.Highcharts.Highcharts("chart")
            .SetTitle(new Title { Text = "机台不合格量统计" })
            .SetXAxis(new XAxis
            {
                Categories = totalDrillingCrewObj
            })
            .SetYAxis(new YAxis
            {
                Title = new YAxisTitle { Text = "不合格量(km)" }
            })
            .SetSeries(new Series
            {
                Name = "机台",
                Type = ChartTypes.Column,
                Data = new Data(totalFailObj)
            })
            .SetTooltip(new Tooltip
            {
                Formatter = @"function(){return '<b>不合格量</b>:' + this.y + 'km<br/>' + '<b>不合格比</b>: ' + Highcharts.numberFormat((this.y/" + totalFailLength + ")*100) + '%'}"
            })
            .SetPlotOptions(new PlotOptions()
            {
                Pie = new PlotOptionsPie
                {
                    AllowPointSelect = true,
                    Cursor = Cursors.Pointer,
                    DataLabels = new PlotOptionsPieDataLabels
                    {
                        Color = ColorTranslator.FromHtml("#000000"),
                        ConnectorColor = ColorTranslator.FromHtml("#000000"),
                        Formatter = @"function(){return '<b>不合格量</b>:' + this.y + '<br/>' + '<b>不合格比</b>: ' + Highcharts.numberFormat((this.y/" + totalFailLength + ")*100) + '%'}"
                    }
                }
            });

            ViewBag.TotalLength = totalLength;
            ViewBag.TotalFailLength = totalFailLength;

            List<Highcharts> chartList = new List<Highcharts> { };
            chartList.Add(chart);
            return PartialView(chartList);
        }

        public ActionResult StatByWorkGroup(string TestTimeStartDay = null, string TestTimeStartHour = null, string TestTimeStopDay = null, string TestTimeStopHour = null, string DrillingCrew = null, string WorkGroup = null, int ProductTypeId = 0)
        {
            DateTime testTimeStart = DateTime.Now.Date;
            DateTime testTimeStop = DateTime.Now;
            string testTimeStarStr;
            string testTimeStopStr;
            if (TestTimeStartHour == "")
            {
                TestTimeStartHour = "00";
            }
            if (TestTimeStopHour == "")
            {
                TestTimeStopHour = "23";
            }
            if (TestTimeStartDay != "")
            {
                testTimeStarStr = TestTimeStartDay + " " + TestTimeStartHour;
                if (!DateTime.TryParseExact(testTimeStarStr, "yyyy-MM-dd HH", CultureInfo.InvariantCulture, DateTimeStyles.None, out testTimeStart))
                {
                }
            }
            if (TestTimeStopDay != "")
            {
                testTimeStopStr = TestTimeStopDay + " " + TestTimeStopHour;
                if (!DateTime.TryParseExact(testTimeStopStr, "yyyy-MM-dd HH", CultureInfo.InvariantCulture, DateTimeStyles.None, out testTimeStop))
                {
                }
            }

            var totalRecord = db.VnaRecord.Where(a => a.NoStatistics == false).Where(a => a.TestTime <= testTimeStop && a.TestTime >= testTimeStart);
            if (DrillingCrew != "")
            {
                totalRecord = totalRecord.Where(a => a.DrillingCrew.Contains(DrillingCrew));
            }
            if (WorkGroup != "")
            {
                totalRecord = totalRecord.Where(a => a.WorkGroup.Contains(WorkGroup));
            }
            if (ProductTypeId != 0)
            {
                totalRecord = totalRecord.Where(a => a.ProductTypeId == ProductTypeId);
            }
            //total record, length/1000 because InnerLength/OuterLength unit in database is m
            var totalRecordList = totalRecord.GroupBy(a => a.WorkGroup).Select(b => new
            {
                id = b.Key,
                length = b.Sum(c => Math.Abs(c.InnerLength - c.OuterLength)/1000)
            }).OrderByDescending(d => d.length).ToList();
            //fail record, length/1000 because InnerLength/OuterLength unit in database is m
            var failRecordList = totalRecord.Where(a => a.TestResult == true).GroupBy(a => a.WorkGroup).Select(b => new
            {
                id = b.Key,
                length = b.Sum(c => Math.Abs(c.InnerLength - c.OuterLength)/1000)
            }).ToList();
            decimal totalLength = 0;
            decimal totalFailLength = 0;
            //get total length
            foreach (var item in totalRecordList)
            {
                totalLength += item.length;
            }
            //get total fail length
            foreach (var item in failRecordList)
            {
                totalFailLength += item.length;
            }

            //totalWorkGroupObj for X(product list), totalFailObj for Y(fail lenght list)
            var totalWorkGroupObj = new string[totalRecordList.Count()];
            var totalFailObj = new object[totalRecordList.Count()];
            for (int i = 0; i < totalWorkGroupObj.Count(); i++)
            {
                totalWorkGroupObj[i] = totalRecordList[i].id;
                var currentFail = failRecordList.Where(a => a.id == totalRecordList[i].id).SingleOrDefault();
                if (currentFail == null)
                {
                    totalFailObj[i] = 0;
                }
                else
                {
                    totalFailObj[i] = Convert.ToDecimal(currentFail.length);
                }
            }

            DotNet.Highcharts.Highcharts chart = new DotNet.Highcharts.Highcharts("chart")
            .SetTitle(new Title { Text = "班组不合格量统计" })
            .SetXAxis(new XAxis
            {
                Categories = totalWorkGroupObj
            })
            .SetYAxis(new YAxis
            {
                Title = new YAxisTitle { Text = "不合格量(km)" }
            })
            .SetSeries(new Series
            {
                Name = "班组",
                Type = ChartTypes.Column,
                Data = new Data(totalFailObj)
            })
            .SetTooltip(new Tooltip
            {
                Formatter = @"function(){return '<b>不合格量</b>:' + this.y + 'km<br/>' + '<b>不合格比</b>: ' + Highcharts.numberFormat((this.y/" + totalFailLength + ")*100) + '%'}"
            })
            .SetPlotOptions(new PlotOptions()
            {
                Pie = new PlotOptionsPie
                {
                    AllowPointSelect = true,
                    Cursor = Cursors.Pointer,
                    DataLabels = new PlotOptionsPieDataLabels
                    {
                        Color = ColorTranslator.FromHtml("#000000"),
                        ConnectorColor = ColorTranslator.FromHtml("#000000"),
                        Formatter = @"function(){return '<b>不合格量</b>:' + this.y + '<br/>' + '<b>不合格比</b>: ' + Highcharts.numberFormat((this.y/" + totalFailLength + ")*100) + '%'}"
                    }
                }
            });

            ViewBag.TotalLength = totalLength;
            ViewBag.TotalFailLength = totalFailLength;

            List<Highcharts> chartList = new List<Highcharts> { };
            chartList.Add(chart);
            return PartialView(chartList);
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
	}
}