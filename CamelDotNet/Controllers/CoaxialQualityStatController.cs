using CamelDotNet.Models.DAL;
using CamelDotNet.Models.ViewModels;
using DotNet.Highcharts;
using DotNet.Highcharts.Enums;
using DotNet.Highcharts.Helpers;
using DotNet.Highcharts.Options;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
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
        
        public ActionResult StatByFailItem(string TestTimeStartDay = null, string TestTimeStartHour = null, string TestTimeStopDay = null, string TestTimeStopHour = null, string DrillingCrew = null, string WorkGroup = null, int ProductTypeId = 0, bool export = false)
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
            //initial chart
            DotNet.Highcharts.Highcharts chart = new DotNet.Highcharts.Highcharts("chart");
            List<Highcharts> chartList = new List<Highcharts> { };
            if (!export)//do not export, normal search
            {
                //call procedure
                string sql = "exec p_coaxialqualitystatTab_Rp @testtimestart, @testtimestop, @drillingcrew, @workgroup, @producttypeId";
                SqlParameter[] param = new SqlParameter[5];
                param[0] = new SqlParameter("@testtimestart", SqlDbType.DateTime2);
                param[0].Value = testTimeStart;
                param[1] = new SqlParameter("@testtimestop", SqlDbType.DateTime2);
                param[1].Value = testTimeStop;
                param[2] = new SqlParameter("@drillingcrew", SqlDbType.NVarChar);
                param[2].Value = DrillingCrew;
                param[3] = new SqlParameter("@workgroup", SqlDbType.NVarChar);
                param[3].Value = WorkGroup;
                param[4] = new SqlParameter("@producttypeId", SqlDbType.Int);
                param[4].Value = ProductTypeId;
                //get procedure result
                DataTable dt = CommonController.GetDateTable(sql, param);
                //init vna total result list, use VnaTotalResult ViewModel
                List<VnaTotalResult> vnaTotalResultList = new List<VnaTotalResult>();
                //vna total result length
                decimal totalLength = 0;
                //vna fail result length
                decimal failLength = 0;
                if (dt.Rows.Count > 0)
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
                        //vnaFailGroupList = vnaFailGroupList.OrderBy(a => a.ProcessName).ThenBy(a => a.TestItemName).ThenBy(a => a.FreqFormularR).ThenBy(a => a.ValueFormularR).ToList();
                        vnaFailGroupList = vnaFailGroupList.OrderByDescending(a => a.Length).ToList();
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
                        })
                        .SetPlotOptions(new PlotOptions
                        {
                            Column = new PlotOptionsColumn
                            {
                                Cursor = Cursors.Pointer,
                                DataLabels = new PlotOptionsColumnDataLabels
                                {
                                    Enabled = true,
                                    Color = Color.FromName("colors[0]"),
                                    Formatter = "function() { return this.y + ' (' + Highcharts.numberFormat((this.y/" + failLength + ")*100) + '%)'; }",
                                    Style = "fontWeight: 'bold'"
                                }
                            }
                        })
                        .AddJavascripVariable("colors", "Highcharts.getOptions().colors");
                    }
                }
            }
            else //export
            {
                string sql = "exec p_coaxialqualitystatexcelTab_Rp @testtimestart, @testtimestop, @drillingcrew, @workgroup, @producttypeId";
                SqlParameter[] param = new SqlParameter[5];
                param[0] = new SqlParameter("@testtimestart", SqlDbType.DateTime2);
                param[0].Value = testTimeStart;
                param[1] = new SqlParameter("@testtimestop", SqlDbType.DateTime2);
                param[1].Value = testTimeStop;
                param[2] = new SqlParameter("@drillingcrew", SqlDbType.NVarChar);
                param[2].Value = DrillingCrew;
                param[3] = new SqlParameter("@workgroup", SqlDbType.NVarChar);
                param[3].Value = WorkGroup;
                param[4] = new SqlParameter("@producttypeId", SqlDbType.Int);
                param[4].Value = ProductTypeId;

                DataTable dt = CommonController.GetDateTable(sql, param);
                //init vna total result list, use VnaTotalResult ViewModel
                List<VnaTotalResultExcel> vnaTotalResultExcelList = new List<VnaTotalResultExcel>();
                //initailize excel name
                string excelName = DateTime.Now.ToString("yyyyMMddHHmmss") + ".xls";
                if (dt.Rows.Count > 0)
                {
                    //auto map dt to VnaTotalResult
                    DataTableReader dr = dt.CreateDataReader();
                    vnaTotalResultExcelList = AutoMapper.Mapper.DynamicMap<IDataReader, List<VnaTotalResultExcel>>(dr);
                    // no defined qulity loss
                    var noDefQualityLossList = vnaTotalResultExcelList
                        .Where(a => a.QualityLossId_Result == 0 || a.ValueFormularR == null)
                        .GroupBy(p => new { p.TestItemName_Fail, p.ProcessName_Fail })
                        .Select(y => y.First()).ToList();
                    if (noDefQualityLossList.Count() == 0)//undifined qualityloss is null
                    {
                        MemoryStream stream = new MemoryStream();
                        HSSFWorkbook workbook = new HSSFWorkbook();
                        workbook.CreateSheet("sheet1");
                        ISheet worksheet = workbook.GetSheet("sheet1");
                        //processRow in row 1
                        IRow processRow = worksheet.CreateRow(0);
                        //testitemRow in row 2
                        IRow testitemRow = worksheet.CreateRow(1);
                        //freqFormularRow for row 3
                        IRow freqFormularRow = worksheet.CreateRow(2);
                        //title Row start from 4, also used for valueFormular
                        IRow titleRow = worksheet.CreateRow(3);
                        titleRow.CreateCell(0).SetCellValue("机台");
                        titleRow.CreateCell(1).SetCellValue("班组");
                        titleRow.CreateCell(2).SetCellValue("物料名称");
                        titleRow.CreateCell(3).SetCellValue("总产量（KM）");
                        titleRow.CreateCell(4).SetCellValue("总不合格量（KM）");
                        titleRow.CreateCell(5).SetCellValue("合格率");
                        titleRow.CreateCell(6).SetCellValue("单价");
                        //get QualityLoss List
                        var qualityLossList = vnaTotalResultExcelList
                            .GroupBy(a => new
                            {
                                a.ProcessName_Fail,
                                a.TestItemName_Fail,
                                a.QualityLossId_Result,
                                a.QualityLossPercentId_Result,
                                a.FreqFormularR,
                                a.ValueFormularR
                            })
                            .Select(ac => new
                            {
                                ProcessName = ac.Key.ProcessName_Fail,
                                TestItemName = ac.Key.TestItemName_Fail,
                                QualityLossId = ac.Key.QualityLossId_Result,
                                QualityLossPercentId = ac.Key.QualityLossPercentId_Result,
                                FreqFormularR = ac.Key.FreqFormularR,
                                ValueFormularR = ac.Key.ValueFormularR
                            }).OrderBy(p => p.ProcessName).ThenBy(p => p.TestItemName).ThenBy(p => p.QualityLossId).ThenBy(p => p.QualityLossPercentId).ToList();
                        //initialize qualityLossGroupList, use QualityLossGroup ViewModel to add row and cell for each qualityLoss
                        List<QualityLossGroup> qualityLossGroupList = new List<QualityLossGroup> { };
                        //cell start from cell 8
                        int cellPostionStart = 7;
                        foreach(var qualityLoss in qualityLossList)
                        {
                            QualityLossGroup qualityLossGroup = new QualityLossGroup
                            {
                                ProcessName = qualityLoss.ProcessName,
                                TestItemName = qualityLoss.TestItemName,
                                QualityLossId = qualityLoss.QualityLossId,
                                QualityLossPercentId = qualityLoss.QualityLossPercentId,
                                FreqFormularR = qualityLoss.FreqFormularR,
                                ValueFormularR = qualityLoss.ValueFormularR,
                                CellNum = cellPostionStart
                            };
                            qualityLossGroupList.Add(qualityLossGroup);
                            cellPostionStart = cellPostionStart + 1;
                        }
                        //write qualityLossGroupList to excel
                        foreach (var qualityLossGroup in qualityLossGroupList)
                        {
                            int cellNum = qualityLossGroup.CellNum;
                            processRow.CreateCell(cellNum).SetCellValue(qualityLossGroup.ProcessName);
                            testitemRow.CreateCell(cellNum).SetCellValue(qualityLossGroup.TestItemName);
                            freqFormularRow.CreateCell(cellNum).SetCellValue(qualityLossGroup.FreqFormularR);
                            titleRow.CreateCell(cellNum).SetCellValue(qualityLossGroup.ValueFormularR);
                        }
                        //write data to excel
                        //initialize a tempGroup to store TempGroups
                        var tempGroupList = new List<string>() { };
                        //initialize total Length
                        decimal totalLength = 0;
                        decimal failTotalLength = 0;
                        //start from row 5
                        int startRow = 4;
                        foreach (var vnaTotalResultExcel in vnaTotalResultExcelList)
                        {
                            //get cell from qualityLossGroupList
                            var qualityLossGroup = qualityLossGroupList
                                .Where(a => a.QualityLossId == vnaTotalResultExcel.QualityLossId_Result && a.QualityLossPercentId == vnaTotalResultExcel.QualityLossPercentId_Result)
                                .SingleOrDefault();
                            //add PerFailLength to current qualityLossGroup
                            qualityLossGroup.PerFailLengthTotal = qualityLossGroup.PerFailLengthTotal + vnaTotalResultExcel.PerFailLength;

                            string drillingCrew = vnaTotalResultExcel.DrillingCrew;
                            string workGroup = vnaTotalResultExcel.WorkGroup;
                            string productFullName = vnaTotalResultExcel.ProductFullName;
                            int qualityLossId = vnaTotalResultExcel.QualityLossId_Result;
                            int qualityLossPecentId = vnaTotalResultExcel.QualityLossPercentId_Result;
                            //new tempGroup, stored distinct(drillingCrew + workGroup + productFullName)
                            string tempGroup =  drillingCrew + workGroup + productFullName;
                            //if not contains in tempGroupList
                            if (!tempGroupList.Contains(tempGroup))
                            {
                                //add current TotalLength to toalLength, add current PerFailLength to totalFailLength
                                totalLength = totalLength + vnaTotalResultExcel.TotalLength;
                                failTotalLength = failTotalLength + vnaTotalResultExcel.TotalFailLength;
                                //create new Row
                                IRow newRow = worksheet.CreateRow(startRow);
                                //write general info, from cell 1 to cell 7
                                newRow.CreateCell(0).SetCellValue(drillingCrew);
                                newRow.CreateCell(1).SetCellValue(workGroup);
                                newRow.CreateCell(2).SetCellValue(productFullName);
                                newRow.CreateCell(3).SetCellValue(vnaTotalResultExcel.TotalLength.ToString());
                                newRow.CreateCell(4).SetCellValue(vnaTotalResultExcel.TotalFailLength.ToString());
                                newRow.CreateCell(5).SetCellValue(vnaTotalResultExcel.PassPercent.ToString() + "%");
                                newRow.CreateCell(6).SetCellValue(vnaTotalResultExcel.Price.ToString());
                                //write Length to selected cell
                                newRow.CreateCell(qualityLossGroup.CellNum).SetCellValue(vnaTotalResultExcel.PerFailLength.ToString());
                                //add current group to tempGroupList
                                tempGroupList.Add(tempGroup);
                                startRow = startRow + 1;
                            }
                            else //if contains in tempGroupList， do not write general info
                            {
                                //get pre Row because Group is Ordered
                                IRow currentRow = worksheet.GetRow(startRow-1);
                                //write fail length
                                currentRow.CreateCell(qualityLossGroup.CellNum).SetCellValue(vnaTotalResultExcel.PerFailLength.ToString());
                            }
                        }
                        //write totalLength, totalFailLength, totalPercent to Excel, startRow + 1 is to add a blank row
                        IRow totalRow = worksheet.CreateRow(startRow + 1);
                        totalRow.CreateCell(0).SetCellValue("汇总");
                        //write totalLength
                        totalRow.CreateCell(3).SetCellValue(totalLength.ToString());
                        //write totalFailLength
                        totalRow.CreateCell(4).SetCellValue(failTotalLength.ToString());
                        //write totalPercent
                        totalRow.CreateCell(5).SetCellValue(Math.Round(((totalLength - failTotalLength) / totalLength) * 100, 2, MidpointRounding.ToEven).ToString() + "%");
                        //write totalPerFailLength
                        foreach (var qualityLossGroup in qualityLossGroupList)
                        {
                            int cellNum = qualityLossGroup.CellNum;
                            totalRow.CreateCell(cellNum).SetCellValue(qualityLossGroup.PerFailLengthTotal.ToString());
                        }

                        if (!workbook.IsWriteProtected)
                        {
                            workbook.Write(stream);
                        }
                        return File(stream.ToArray(), "application/vnd.ms-excel", excelName);
                    }
                    else//undifined qualityloss is not null
                    {
                        MemoryStream stream = new MemoryStream();
                        HSSFWorkbook workbook = new HSSFWorkbook();
                        workbook.CreateSheet("sheet1");
                        ISheet worksheet = workbook.GetSheet("sheet1");
                        IRow firstRow = worksheet.CreateRow(0);
                        ICell firstCell = firstRow.CreateCell(0);
                        firstCell.SetCellValue("未定义质量损失比");
                        //start insert undefined qulityloss from second row
                        int startRow = 1;
                        foreach (var noDefQulityLoss in noDefQualityLossList)
                        {
                            IRow newRow = worksheet.CreateRow(startRow);
                            string processName = noDefQulityLoss.ProcessName_Fail;
                            newRow.CreateCell(0).SetCellValue(processName);
                            string testItemName = noDefQulityLoss.TestItemName_Fail;
                            newRow.CreateCell(1).SetCellValue(testItemName);
                            startRow = startRow + 1;
                        }
                        if (!workbook.IsWriteProtected)
                        {
                            workbook.Write(stream);
                        }
                        return File(stream.ToArray(), "application/vnd.ms-excel", excelName);
                    }
                }
                else 
                {
                    try
                    {
                        MemoryStream stream = new MemoryStream();
                        HSSFWorkbook workbook = new HSSFWorkbook();
                        workbook.CreateSheet("sheet1");
                        ISheet worksheet = workbook.GetSheet("sheet1");
                        IRow newRow = worksheet.CreateRow(0);
                        ICell newCell = newRow.CreateCell(0);
                        newCell.SetCellValue("未查到相关记录");
                        //worksheet.AddMergedRegion(new CellRangeAddress(0,0,1,0));
                        if (!workbook.IsWriteProtected)
                        {
                            workbook.Write(stream);
                        }
                        return File(stream.ToArray(), "application/vnd.ms-excel", excelName);
                    }
                    catch (Exception ex)
                    {
                        errorMsg = ex.Message;
                        MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(errorMsg));
                        return File(stream.ToArray(), "application/vnd.ms-excel", excelName);
                    }
                }
            }
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

            //call procedure
            string sql = "exec p_coaxialqualitystatTab_Rp @testtimestart, @testtimestop, @drillingcrew, @workgroup, @producttypeId";
            SqlParameter[] param = new SqlParameter[5];
            param[0] = new SqlParameter("@testtimestart", SqlDbType.DateTime2);
            param[0].Value = testTimeStart;
            param[1] = new SqlParameter("@testtimestop", SqlDbType.DateTime2);
            param[1].Value = testTimeStop;
            param[2] = new SqlParameter("@drillingcrew", SqlDbType.NVarChar);
            param[2].Value = DrillingCrew;
            param[3] = new SqlParameter("@workgroup", SqlDbType.NVarChar);
            param[3].Value = WorkGroup;
            param[4] = new SqlParameter("@producttypeId", SqlDbType.Int);
            param[4].Value = ProductTypeId;
            //get procedure result
            DataTable dt = CommonController.GetDateTable(sql, param);
            //init vna total result list, use VnaTotalResult ViewModel
            List<VnaTotalResult> vnaTotalResultList = new List<VnaTotalResult>();
            //vna total result length
            decimal totalLength = 0;
            //vna fail result length
            decimal totalFailLength = 0;
            //initialize chart
            DotNet.Highcharts.Highcharts chart = new DotNet.Highcharts.Highcharts("chart");
            if (dt.Rows.Count > 0)
            {
                //auto map dt to VnaTotalResult
                DataTableReader dr = dt.CreateDataReader();
                vnaTotalResultList = AutoMapper.Mapper.DynamicMap<IDataReader, List<VnaTotalResult>>(dr);
                var totalResultList = vnaTotalResultList.Select(a => new
                {
                    TestTime = a.TestTime,
                    ProductFullName = a.ProductFullName,
                    DrillingCrew = a.DrillingCrew,
                    WorkGroup = a.WorkGroup,
                    Lengths = a.Lengths,
                    TestResult = a.TestResult
                }).Distinct().ToList();
                //fail list
                var failResultList = totalResultList.Where(a => a.TestResult == 1).ToList();
                //get total length, unit m to km
                totalLength = totalResultList.Sum(a => a.Lengths) / 1000;
                //get total length, unit m to km
                totalFailLength = failResultList.Sum(a => a.Lengths) / 1000;
                var vnaFailGroupList = failResultList.GroupBy(a => a.ProductFullName).Select(b => new
                {
                    id = b.Key,
                    length = b.Sum(c => Math.Abs(c.Lengths) / 1000)
                }).OrderByDescending(d => d.length).ToList();
                //totalXObj for X(X list), totalYObj for Y(Y is fail lenght list)
                var totalXObj = new string[vnaFailGroupList.Count()];
                var totalYObj = new object[vnaFailGroupList.Count()];
                for (int i = 0; i < vnaFailGroupList.Count(); i++)
                {
                    totalXObj[i] = vnaFailGroupList[i].id;
                    //vnaFailGroup length, unit m to km
                    totalYObj[i] = Convert.ToDecimal(vnaFailGroupList[i].length);
                }
                chart
                .SetTitle(new Title { Text = "物料名称不合格量统计" })
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
                    Name = "产品型号",
                    Type = ChartTypes.Column,
                    Data = new Data(totalYObj)
                })
                .SetTooltip(new Tooltip
                {
                    Formatter = @"function(){return '<b>不合格量</b>:' + this.y + 'km<br/>' + '<b>不合格比</b>: ' + Highcharts.numberFormat((this.y/" + totalFailLength + ")*100) + '%'}"
                })
                .SetPlotOptions(new PlotOptions
                {
                    Column = new PlotOptionsColumn
                    {
                        Cursor = Cursors.Pointer,
                        DataLabels = new PlotOptionsColumnDataLabels
                        {
                            Enabled = true,
                            Color = Color.FromName("colors[0]"),
                            Formatter = "function() { return this.y + ' (' + Highcharts.numberFormat((this.y/" + totalFailLength + ")*100) + '%)'; }",
                            Style = "fontWeight: 'bold'"
                        }
                    }
                })
                .AddJavascripVariable("colors", "Highcharts.getOptions().colors");
            }
            
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

            //call procedure
            string sql = "exec p_coaxialqualitystatTab_Rp @testtimestart, @testtimestop, @drillingcrew, @workgroup, @producttypeId";
            SqlParameter[] param = new SqlParameter[5];
            param[0] = new SqlParameter("@testtimestart", SqlDbType.DateTime2);
            param[0].Value = testTimeStart;
            param[1] = new SqlParameter("@testtimestop", SqlDbType.DateTime2);
            param[1].Value = testTimeStop;
            param[2] = new SqlParameter("@drillingcrew", SqlDbType.NVarChar);
            param[2].Value = DrillingCrew;
            param[3] = new SqlParameter("@workgroup", SqlDbType.NVarChar);
            param[3].Value = WorkGroup;
            param[4] = new SqlParameter("@producttypeId", SqlDbType.Int);
            param[4].Value = ProductTypeId;
            //get procedure result
            DataTable dt = CommonController.GetDateTable(sql, param);
            //init vna total result list, use VnaTotalResult ViewModel
            List<VnaTotalResult> vnaTotalResultList = new List<VnaTotalResult>();
            //vna total result length
            decimal totalLength = 0;
            //vna fail result length
            decimal totalFailLength = 0;
            //initialize chart
            DotNet.Highcharts.Highcharts chart = new DotNet.Highcharts.Highcharts("chart");
            if (dt.Rows.Count > 0)
            {
                //auto map dt to VnaTotalResult
                DataTableReader dr = dt.CreateDataReader();
                vnaTotalResultList = AutoMapper.Mapper.DynamicMap<IDataReader, List<VnaTotalResult>>(dr);
                var totalResultList = vnaTotalResultList.Select(a => new
                {
                    TestTime = a.TestTime,
                    ProductFullName = a.ProductFullName,
                    DrillingCrew = a.DrillingCrew,
                    WorkGroup = a.WorkGroup,
                    Lengths = a.Lengths,
                    TestResult = a.TestResult
                }).Distinct().ToList();
                //fail list
                var failResultList = totalResultList.Where(a => a.TestResult == 1).ToList();
                //get total length, unit m to km
                totalLength = totalResultList.Sum(a => a.Lengths) / 1000;
                //get total length, unit m to km
                totalFailLength = failResultList.Sum(a => a.Lengths) / 1000;
                var vnaFailGroupList = failResultList.GroupBy(a => a.DrillingCrew).Select(b => new
                {
                    id = b.Key,
                    length = b.Sum(c => Math.Abs(c.Lengths) / 1000)
                }).OrderByDescending(d => d.length).ToList();
                //totalXObj for X(X list), totalYObj for Y(Y is fail lenght list)
                var totalXObj = new string[vnaFailGroupList.Count()];
                var totalYObj = new object[vnaFailGroupList.Count()];
                for (int i = 0; i < vnaFailGroupList.Count(); i++)
                {
                    //vnaFailGroup length, unit m to km
                    totalYObj[i] = Convert.ToDecimal(vnaFailGroupList[i].length);
                    //get current DrillingCrew's total length
                    var currGroupTotalLength = totalResultList.Where(a => a.DrillingCrew == vnaFailGroupList[i].id).Sum(b => Math.Abs(b.Lengths) / 1000);
                    totalXObj[i] = vnaFailGroupList[i].id + "<br/>机台不合格率：" + (Convert.ToDecimal(vnaFailGroupList[i].length) / currGroupTotalLength * 100).ToString("#.##") + "%";
                }
                chart
                .SetTitle(new Title { Text = "机台不合格量统计" })
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
                    Name = "机台",
                    Type = ChartTypes.Column,
                    Data = new Data(totalYObj)
                })
                .SetTooltip(new Tooltip
                {
                    Formatter = @"function(){return '<b>不合格量</b>:' + this.y + 'km<br/>' + '<b>总不合格比</b>: ' + Highcharts.numberFormat((this.y/" + totalFailLength + ")*100) + '%'}"
                })
                .SetPlotOptions(new PlotOptions
                {
                    Column = new PlotOptionsColumn
                    {
                        Cursor = Cursors.Pointer,
                        DataLabels = new PlotOptionsColumnDataLabels
                        {
                            Enabled = true,
                            Color = Color.FromName("colors[0]"),
                            Formatter = "function() { return this.y + ' (' + Highcharts.numberFormat((this.y/" + totalFailLength + ")*100) + '%)'; }",
                            Style = "fontWeight: 'bold'"
                        }
                    }
                })
                .AddJavascripVariable("colors", "Highcharts.getOptions().colors");
            }

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

            //call procedure
            string sql = "exec p_coaxialqualitystatTab_Rp @testtimestart, @testtimestop, @drillingcrew, @workgroup, @producttypeId";
            SqlParameter[] param = new SqlParameter[5];
            param[0] = new SqlParameter("@testtimestart", SqlDbType.DateTime2);
            param[0].Value = testTimeStart;
            param[1] = new SqlParameter("@testtimestop", SqlDbType.DateTime2);
            param[1].Value = testTimeStop;
            param[2] = new SqlParameter("@drillingcrew", SqlDbType.NVarChar);
            param[2].Value = DrillingCrew;
            param[3] = new SqlParameter("@workgroup", SqlDbType.NVarChar);
            param[3].Value = WorkGroup;
            param[4] = new SqlParameter("@producttypeId", SqlDbType.Int);
            param[4].Value = ProductTypeId;
            //get procedure result
            DataTable dt = CommonController.GetDateTable(sql, param);
            //init vna total result list, use VnaTotalResult ViewModel
            List<VnaTotalResult> vnaTotalResultList = new List<VnaTotalResult>();
            //vna total result length
            decimal totalLength = 0;
            //vna fail result length
            decimal totalFailLength = 0;
            //initialize chart
            DotNet.Highcharts.Highcharts chart = new DotNet.Highcharts.Highcharts("chart");
            if (dt.Rows.Count > 0)
            {
                //auto map dt to VnaTotalResult
                DataTableReader dr = dt.CreateDataReader();
                vnaTotalResultList = AutoMapper.Mapper.DynamicMap<IDataReader, List<VnaTotalResult>>(dr);
                var totalResultList = vnaTotalResultList.Select(a => new
                {
                    TestTime = a.TestTime,
                    ProductFullName = a.ProductFullName,
                    DrillingCrew = a.DrillingCrew,
                    WorkGroup = a.WorkGroup,
                    Lengths = a.Lengths,
                    TestResult = a.TestResult
                }).Distinct().ToList();
                //fail list
                var failResultList = totalResultList.Where(a => a.TestResult == 1).ToList();
                //get total length, unit m to km
                totalLength = totalResultList.Sum(a => a.Lengths) / 1000;
                //get total length, unit m to km
                totalFailLength = failResultList.Sum(a => a.Lengths) / 1000;
                var vnaFailGroupList = failResultList.GroupBy(a => a.WorkGroup).Select(b => new
                {
                    id = b.Key,
                    length = b.Sum(c => Math.Abs(c.Lengths) / 1000)
                }).OrderByDescending(d => d.length).ToList();
                //totalXObj for X(X list), totalYObj for Y(Y is fail lenght list)
                var totalXObj = new string[vnaFailGroupList.Count()];
                var totalYObj = new object[vnaFailGroupList.Count()];
                for (int i = 0; i < vnaFailGroupList.Count(); i++)
                {
                    //vnaFailGroup length, unit m to km
                    totalYObj[i] = Convert.ToDecimal(vnaFailGroupList[i].length);
                    //get current WorkGroup's total length
                    var currGroupTotalLength = totalResultList.Where(a => a.WorkGroup == vnaFailGroupList[i].id).Sum(b => Math.Abs(b.Lengths) / 1000);
                    totalXObj[i] = vnaFailGroupList[i].id + "<br/>班组不合格率：" + (Convert.ToDecimal(vnaFailGroupList[i].length) / currGroupTotalLength * 100).ToString("#.##") + "%";
                }
                chart
                .SetTitle(new Title { Text = "班组不合格量统计" })
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
                    Name = "班组",
                    Type = ChartTypes.Column,
                    Data = new Data(totalYObj)
                })
                .SetTooltip(new Tooltip
                {
                    Formatter = @"function(){return '<b>不合格量</b>:' + this.y + 'km<br/>' + '<b>总不合格比</b>: ' + Highcharts.numberFormat((this.y/" + totalFailLength + ")*100) + '%'}"
                })
                .SetPlotOptions(new PlotOptions
                {
                    Column = new PlotOptionsColumn
                    {
                        Cursor = Cursors.Pointer,
                        DataLabels = new PlotOptionsColumnDataLabels
                        {
                            Enabled = true,
                            Color = Color.FromName("colors[0]"),
                            Formatter = "function() { return this.y + ' (' + Highcharts.numberFormat((this.y/" + totalFailLength + ")*100) + '%)'; }",
                            Style = "fontWeight: 'bold'"
                        }
                    }
                })
                .AddJavascripVariable("colors", "Highcharts.getOptions().colors");
            }

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