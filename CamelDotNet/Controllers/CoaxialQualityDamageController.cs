using CamelDotNet.Models.DAL;
using DotNet.Highcharts;
using DotNet.Highcharts.Enums;
using DotNet.Highcharts.Helpers;
using DotNet.Highcharts.Options;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CamelDotNet.Controllers
{
    public class CoaxialQualityDamageController : Controller
    {
        private CamelDotNetDBContext db = new CamelDotNetDBContext();

        List<string> path = new List<string>();
        public CoaxialQualityDamageController() 
        {
            path.Add("报表管理");
            path.Add("同轴质量损失");
            ViewBag.path = path;
            ViewBag.Name = "同轴质量损失";
            ViewBag.Title = "同轴质量损失";
        }


        public ActionResult Index()
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

            return View();
        }

        //质量损失-部门
        public ActionResult StatByDepartment(string TestTimeStartDay = null, string TestTimeStartHour = null, string TestTimeStopDay = null, string TestTimeStopHour = null, string DrillingCrew = null, string WorkGroup = null, int ProductTypeId = 0)
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
            //fail record, damage/1000 because InnerLength/OuterLength unit in database are m
            var failRecordList = totalRecord.Where(a => a.TestResult == true).GroupBy(a => a.DrillingCrew).Select(b => new
            {
                id = b.Key,
                length = b.Sum(c => Math.Abs(c.InnerLength - c.OuterLength)),
                damage = b.Sum(c => Math.Abs(c.InnerLength - c.OuterLength) * c.ProductType.Price / 1000)
            }).OrderByDescending(d => d.damage).ToList();
            decimal totalDamage = 0;
            //get total damage
            foreach (var item in failRecordList)
            {
                totalDamage += item.damage;
            }
            //totalDrillingCrewObj for X(product list), totalFailDamageObj for Y(fail damage list)
            var totalDrillingCrewObj = new string[failRecordList.Count()];
            var totalFailDamageObj = new object[failRecordList.Count()];
            for (int i = 0; i < totalDrillingCrewObj.Count(); i++)
            {
                totalDrillingCrewObj[i] = failRecordList[i].id;
                var currentFail = failRecordList.Where(a => a.id == failRecordList[i].id).SingleOrDefault();
                if (currentFail == null)
                {
                    totalFailDamageObj[i] = 0;
                }
                else
                {
                    totalFailDamageObj[i] = Convert.ToDecimal(currentFail.damage);
                }
            }

            DotNet.Highcharts.Highcharts chart = new DotNet.Highcharts.Highcharts("chart")
            .SetTitle(new Title { Text = "各机台同轴质量损失表（元）" })
            .SetXAxis(new XAxis
            {
                Categories = totalDrillingCrewObj
            })
            .SetYAxis(new YAxis
            {
                Title = new YAxisTitle { Text = "损失金额" }
            })
            .SetSeries(new Series
            {
                Name = "机台",
                Type = ChartTypes.Column,
                Data = new Data(totalFailDamageObj)
            })
            .SetTooltip(new Tooltip
            {
                Formatter = @"function(){return '<b>损失金额</b>:' + this.y + '<br/>' + '<b>所占比例</b>: ' + Highcharts.numberFormat((this.y/" + totalDamage + ")*100) + '%'}"
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
                        Formatter = @"function(){return '<b>损失金额</b>:' + this.y + '<br/>' + '<b>所占比例</b>: ' + Highcharts.numberFormat((this.y/" + totalDamage + ")*100) + '%'}"
                    }
                }
            });

            ViewBag.TotalFailDamage = totalDamage;

            List<Highcharts> chartList = new List<Highcharts> { };
            chartList.Add(chart);
            return PartialView(chartList);
        }

        //质量损失-机台
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
            //fail record, damage/1000 because InnerLength/OuterLength unit in database are m
            var failRecordList = totalRecord.Where(a => a.TestResult == true).GroupBy(a => a.DrillingCrew).Select(b => new
            {
                id = b.Key,
                length = b.Sum(c => Math.Abs(c.InnerLength - c.OuterLength)),
                damage = b.Sum(c => Math.Abs(c.InnerLength - c.OuterLength)*c.ProductType.Price/1000)
            }).OrderByDescending(d => d.damage).ToList();
            decimal totalDamage = 0;
            //get total damage
            foreach (var item in failRecordList)
            {
                totalDamage += item.damage;
            }
            //totalDrillingCrewObj for X(product list), totalFailDamageObj for Y(fail damage list)
            var totalDrillingCrewObj = new string[failRecordList.Count()];
            var totalFailDamageObj = new object[failRecordList.Count()];
            for (int i = 0; i < totalDrillingCrewObj.Count(); i++)
            {
                totalDrillingCrewObj[i] = failRecordList[i].id;
                var currentFail = failRecordList.Where(a => a.id == failRecordList[i].id).SingleOrDefault();
                if (currentFail == null)
                {
                    totalFailDamageObj[i] = 0;
                }
                else
                {
                    totalFailDamageObj[i] = Convert.ToDecimal(currentFail.damage);
                }
            }

            DotNet.Highcharts.Highcharts chart = new DotNet.Highcharts.Highcharts("chart")
            .SetTitle(new Title { Text = "各机台同轴质量损失表（元）" })
            .SetXAxis(new XAxis
            {
                Categories = totalDrillingCrewObj
            })
            .SetYAxis(new YAxis
            {
                Title = new YAxisTitle { Text = "损失金额" }
            })
            .SetSeries(new Series
            {
                Name = "机台",
                Type = ChartTypes.Column,
                Data = new Data(totalFailDamageObj)
            })
            .SetTooltip(new Tooltip
            {
                Formatter = @"function(){return '<b>损失金额</b>:' + this.y + '<br/>' + '<b>所占比例</b>: ' + Highcharts.numberFormat((this.y/" + totalDamage + ")*100) + '%'}"
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
                        Formatter = @"function(){return '<b>损失金额</b>:' + this.y + '<br/>' + '<b>所占比例</b>: ' + Highcharts.numberFormat((this.y/" + totalDamage + ")*100) + '%'}"
                    }
                }
            });

            ViewBag.TotalFailDamage = totalDamage;

            List<Highcharts> chartList = new List<Highcharts> { };
            chartList.Add(chart);
            return PartialView(chartList);
        }

        //质量损失-班组
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
            //fail record, damage/1000 because InnerLength/OuterLength unit in database are m
            var failRecordList = totalRecord.Where(a => a.TestResult == true).GroupBy(a => a.WorkGroup).Select(b => new
            {
                id = b.Key,
                length = b.Sum(c => Math.Abs(c.InnerLength - c.OuterLength)),
                damage = b.Sum(c => Math.Abs(c.InnerLength - c.OuterLength) * c.ProductType.Price / 1000)
            }).OrderByDescending(d => d.damage).ToList();
            decimal totalDamage = 0;
            //get total damage
            foreach (var item in failRecordList)
            {
                totalDamage += item.damage;
            }
            //totalDrillingCrewObj for X(WorkGroup list), totalFailDamageObj for Y(fail damage list)
            var totalWorkGroupObj = new string[failRecordList.Count()];
            var totalFailDamageObj = new object[failRecordList.Count()];
            for (int i = 0; i < totalWorkGroupObj.Count(); i++)
            {
                totalWorkGroupObj[i] = failRecordList[i].id;
                var currentFail = failRecordList.Where(a => a.id == failRecordList[i].id).SingleOrDefault();
                if (currentFail == null)
                {
                    totalFailDamageObj[i] = 0;
                }
                else
                {
                    totalFailDamageObj[i] = Convert.ToDecimal(currentFail.damage);
                }
            }

            DotNet.Highcharts.Highcharts chart = new DotNet.Highcharts.Highcharts("chart")
            .SetTitle(new Title { Text = "各班组同轴质量损失表（元）" })
            .SetXAxis(new XAxis
            {
                Categories = totalWorkGroupObj
            })
            .SetYAxis(new YAxis
            {
                Title = new YAxisTitle { Text = "损失金额" }
            })
            .SetSeries(new Series
            {
                Name = "机台",
                Type = ChartTypes.Column,
                Data = new Data(totalFailDamageObj)
            })
            .SetTooltip(new Tooltip
            {
                Formatter = @"function(){return '<b>损失金额</b>:' + this.y + '<br/>' + '<b>所占比例</b>: ' + Highcharts.numberFormat((this.y/" + totalDamage + ")*100) + '%'}"
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
                        Formatter = @"function(){return '<b>损失金额</b>:' + this.y + '<br/>' + '<b>所占比例</b>: ' + Highcharts.numberFormat((this.y/" + totalDamage + ")*100) + '%'}"
                    }
                }
            });

            ViewBag.TotalFailDamage = totalDamage;

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