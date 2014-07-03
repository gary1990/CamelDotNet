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
            Highcharts chart = new Highcharts("chart")
                 .SetTitle(new Title { Text = "Chart inside JavaScript function" })
                 .SetXAxis(new XAxis { Categories = new[] { "Apples", "Oranges", "Pears", "Bananas", "Plums" } })
                 .SetLabels(new Labels
                 {
                     Items = new[]
                                       {
                                           new LabelsItems
                                           {
                                               Html = "Total fruit consumption",
                                               Style = "left: '40px', top: '8px', color: 'black'"
                                           }
                                       }
                 })
                 .SetPlotOptions(new PlotOptions
                 {
                     Pie = new PlotOptionsPie
                     {
                         Center = new[] { new PercentageOrPixel(100), new PercentageOrPixel(80) },
                         Size = new PercentageOrPixel(100),
                         ShowInLegend = false,
                         DataLabels = new PlotOptionsPieDataLabels { Enabled = false }
                     }
                 })
                 .SetSeries(new[]
                           {
                               new Series
                               {
                                   Type = ChartTypes.Column,
                                   Name = "Jane",
                                   Data = new Data(new object[] { 3, 2, 1, 3, 4 })
                               },
                               new Series
                               {
                                   Type = ChartTypes.Column,
                                   Name = "John",
                                   Data = new Data(new object[] { 2, 3, 5, 7, 6 })
                               },
                               new Series
                               {
                                   Type = ChartTypes.Column,
                                   Name = "Joe",
                                   Data = new Data(new object[] { 4, 3, 3, 9, 0 })
                               },
                               new Series
                               {
                                   Type = ChartTypes.Spline,
                                   Name = "Average",
                                   Data = new Data(new object[] { 3, 2.67, 3, 6.33, 3.33 })
                               },
                               new Series
                               {
                                   Type = ChartTypes.Pie,
                                   Name = "Total consumption",
                                   Data = new Data(new[]
                                                   {
                                                       new DotNet.Highcharts.Options.Point
                                                       {
                                                           Name = "Jane",
                                                           Y = 13,
                                                           Color = Color.FromName("red")
                                                       },
                                                       new DotNet.Highcharts.Options.Point
                                                       {
                                                           Name = "John",
                                                           Y = 23,
                                                           Color = Color.FromName("blue")
                                                       },
                                                       new DotNet.Highcharts.Options.Point
                                                       {
                                                           Name = "Joe",
                                                           Y = 19,
                                                           Color = Color.FromName("red")
                                                       }
                                                   }
                                       )
                               }
                           });
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