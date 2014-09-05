using CamelDotNet.Models.DAL;
using CamelDotNet.Models.ViewModels;
using DotNet.Highcharts;
using DotNet.Highcharts.Enums;
using DotNet.Highcharts.Helpers;
using DotNet.Highcharts.Options;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
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

        //质量损失-不合格项
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
            //vna total damage
            decimal? totalDamage = 0;
            if (!export)//do not export, normal search
            {
                //call procedure
                string sql = "exec p_coaxialqualitydamageTab_Rp @testtimestart, @testtimestop, @drillingcrew, @workgroup, @producttypeId";
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
                //init vna total result damage list, use VnaTotalResultDamage ViewModel
                List<VnaTotalResultDamage> vnaTotalResultDamageList = new List<VnaTotalResultDamage>();
                if (dt.Rows.Count > 0)
                {
                    //auto map dt to VnaTotalResultDamage
                    DataTableReader dr = dt.CreateDataReader();
                    vnaTotalResultDamageList = AutoMapper.Mapper.DynamicMap<IDataReader, List<VnaTotalResultDamage>>(dr);
                    //fail（不合格） or IsGreenLight（放行） list,
                    List<VnaTotalResultDamage> vnaFailResultDamageList = vnaTotalResultDamageList.Where(a => a.TestResult == 1 || (a.TestResult == 0 && a.IsGreenLight == 1)).ToList();
                    // no defined qulity loss
                    var noDefQualityLossList = vnaFailResultDamageList
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
                        //get totalDamage
                        totalDamage = vnaFailResultDamageList.Sum(a => a.LossMoney);
                        //get fail group list
                        var vnaFailGroupList = vnaFailResultDamageList
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
                                Demage = ac.Sum(acs => acs.LossMoney)
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
                            //vnaFailGroup demage
                            totalYObj[i] = Convert.ToDecimal(vnaFailGroup.Demage);
                        }
                        chart
                        .SetTitle(new Title { Text = "不合格项损失金额统计" })
                        .SetXAxis(new XAxis
                        {
                            Categories = totalXObj
                        })
                        .SetYAxis(new YAxis
                        {
                            Title = new YAxisTitle { Text = "损失金额(元)" }
                        })
                        .SetSeries(new Series
                        {
                            Name = "工序-测试项-点-值",
                            Type = ChartTypes.Column,
                            Data = new Data(totalYObj)
                        })
                        .SetTooltip(new Tooltip
                        {
                            Formatter = @"function(){return '<b>损失金额</b>:' + this.y + '元<br/>' + '<b>损失比例</b>: ' + Highcharts.numberFormat((this.y/" + totalDamage + ")*100) + '%'}"
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
                                    Formatter = "function() { return this.y + ' (' + Highcharts.numberFormat((this.y/" + totalDamage + ")*100) + '%)'; }",
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
                string sql = "exec p_coaxialqualitydamageexcelTab_Rp @testtimestart, @testtimestop, @drillingcrew, @workgroup,@producttypeId";
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
                //init vna total result list, use VnaTotalResultDamageExcel ViewModel
                List<VnaTotalResultDamageExcel> vnaTotalResultDamageExcelList = new List<VnaTotalResultDamageExcel>();
                //initailize excel name
                string excelName = DateTime.Now.ToString("yyyyMMddHHmmss") + ".xls";
                if (dt.Rows.Count > 0)
                {
                    //auto map dt to VnaTotalResult
                    DataTableReader dr = dt.CreateDataReader();
                    vnaTotalResultDamageExcelList = AutoMapper.Mapper.DynamicMap<IDataReader, List<VnaTotalResultDamageExcel>>(dr);
                    // no defined qulity loss
                    var noDefQualityLossList = vnaTotalResultDamageExcelList
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
                        titleRow.CreateCell(0).SetCellValue("日期");
                        titleRow.CreateCell(1).SetCellValue("机台");
                        titleRow.CreateCell(2).SetCellValue("班组");
                        titleRow.CreateCell(3).SetCellValue("物料名称");
                        //get Department List
                        var departmentList = vnaTotalResultDamageExcelList.Where(a => a.DepartmentName != null)
                            .GroupBy(a => new
                            {
                                a.DepartmentId,
                                a.DepartmentName
                            })
                            .Select(ac => new {
                                DepartmentId = ac.Key.DepartmentId,
                                DepartmentName = ac.Key.DepartmentName
                            }).OrderBy(p => p.DepartmentId).ToList();
                        //initialize departmentGroupList, use DepartmentGroup ViewModel to add row and cell for each department
                        List<DepartmentGroup> departmentGroupList = new List<DepartmentGroup>() { };
                        //cell start from 5
                        int departmentGroupCellStart = 4;
                        foreach (var department in departmentList)
                        {
                            DepartmentGroup departmentGroup = new DepartmentGroup()
                            {
                                DepartmentId = department.DepartmentId,
                                DepartmentName = department.DepartmentName,
                                CellNum = departmentGroupCellStart
                            };
                            departmentGroupList.Add(departmentGroup);
                            departmentGroupCellStart = departmentGroupCellStart + 1;
                        }
                        //write departmentGroupList to excel
                        foreach (var departmentGroup in departmentGroupList)
                        {
                            int cellNum = departmentGroup.CellNum;
                            titleRow.CreateCell(cellNum).SetCellValue(departmentGroup.DepartmentName);
                        }
                        //get departmentGroupList count, used to add this count while write other titles after departmentGroup
                        int departmentGroupListCount = departmentGroupList.Count();
                        //write 总产量（KM）,总不合格量（KM）,合格率,单价 title, after depaetmentGroupList
                        titleRow.CreateCell(4 + departmentGroupListCount).SetCellValue("总产量（KM）");
                        titleRow.CreateCell(5 + departmentGroupListCount).SetCellValue("总不合格量（KM）");
                        titleRow.CreateCell(6 + departmentGroupListCount).SetCellValue("合格率");
                        titleRow.CreateCell(7 + departmentGroupListCount).SetCellValue("单价");
                        //get QualityLoss List
                        var qualityLossList = vnaTotalResultDamageExcelList
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
                        List<QualityLossGroup> qualityLossGroupList = new List<QualityLossGroup>() { };
                        //cell start from cell 9 + departmentGroup.Count()
                        int qualityLossGroupStart = 8 + departmentGroupList.Count();
                        foreach (var qualityLoss in qualityLossList)
                        {
                            QualityLossGroup qualityLossGroup = new QualityLossGroup
                            {
                                ProcessName = qualityLoss.ProcessName,
                                TestItemName = qualityLoss.TestItemName,
                                QualityLossId = qualityLoss.QualityLossId,
                                QualityLossPercentId = qualityLoss.QualityLossPercentId,
                                FreqFormularR = qualityLoss.FreqFormularR,
                                ValueFormularR = qualityLoss.ValueFormularR,
                                CellNum = qualityLossGroupStart
                            };
                            qualityLossGroupList.Add(qualityLossGroup);
                            qualityLossGroupStart = qualityLossGroupStart + 1;
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
                        //initialize total loss money
                        decimal totalLossMoney = 0;
                        //initialize department group list total loss money 
                        decimal departmentGrouplistTotalLossMoney = 0;
                        //start from row 5
                        int startRow = 4;
                        foreach (var vnaTotalResultDamageExcel in vnaTotalResultDamageExcelList)
                        {
                            //add current Loss money to totalLossMoney
                            totalLossMoney = totalLossMoney + vnaTotalResultDamageExcel.PerLossMoney;
                            //get cell from qualityLossGroupList
                            var qualityLossGroup = qualityLossGroupList
                                .Where(a => a.QualityLossId == vnaTotalResultDamageExcel.QualityLossId_Result && a.QualityLossPercentId == vnaTotalResultDamageExcel.QualityLossPercentId_Result)
                                .SingleOrDefault();
                            //get cell from departmentGroupList
                            var departmentGroup = departmentGroupList
                                .Where(a => a.DepartmentId == vnaTotalResultDamageExcel.DepartmentId && a.DepartmentName == vnaTotalResultDamageExcel.DepartmentName)
                                .SingleOrDefault();
                            //add PerFailLength(this is PerLossMoney) to current qualityLossGroup
                            qualityLossGroup.PerFailLengthTotal = qualityLossGroup.PerFailLengthTotal + vnaTotalResultDamageExcel.PerLossMoney;
                            //add PerLossMoney to departmentGroup
                            if (departmentGroup != null)
                            {
                                departmentGrouplistTotalLossMoney = departmentGrouplistTotalLossMoney + vnaTotalResultDamageExcel.PerLossMoney;
                                departmentGroup.TotalLossMoney = departmentGroup.TotalLossMoney + vnaTotalResultDamageExcel.PerLossMoney;
                            }

                            string testDate = vnaTotalResultDamageExcel.TestDate.ToString();
                            string drillingCrew = vnaTotalResultDamageExcel.DrillingCrew;
                            string workGroup = vnaTotalResultDamageExcel.WorkGroup;
                            string productFullName = vnaTotalResultDamageExcel.ProductFullName;
                            int qualityLossId = vnaTotalResultDamageExcel.QualityLossId_Result;
                            int qualityLossPecentId = vnaTotalResultDamageExcel.QualityLossPercentId_Result;
                            //new tempGroup, stored distinct(testDate + drillingCrew + workGroup + productFullName)
                            string tempGroup = testDate + drillingCrew + workGroup + productFullName;
                            //if not contains in tempGroupList
                            if (!tempGroupList.Contains(tempGroup))
                            {
                                //add current TotalLength to toalLength, add current PerFailLength to totalFailLength
                                totalLength = totalLength + vnaTotalResultDamageExcel.TotalLength;
                                failTotalLength = failTotalLength + vnaTotalResultDamageExcel.TotalFailLength;
                                //create new Row
                                IRow newRow = worksheet.CreateRow(startRow);
                                //write general info, from cell 1-3 is 日期、机台、班组、物料名称
                                newRow.CreateCell(0).SetCellValue(testDate);
                                newRow.CreateCell(1).SetCellValue(drillingCrew);
                                newRow.CreateCell(2).SetCellValue(workGroup);
                                newRow.CreateCell(3).SetCellValue(productFullName);
                                //write PerLossMoney to cunrrent DepartmentGroup
                                if (departmentGroup != null)
                                {
                                    newRow.CreateCell(departmentGroup.CellNum).SetCellValue(vnaTotalResultDamageExcel.PerLossMoney.ToString());
                                }
                                //write general info, from cell （4-7）+ departmentGroupListCount is 总产量、总不合格量、合格率、单价
                                newRow.CreateCell(4 + departmentGroupListCount).SetCellValue(vnaTotalResultDamageExcel.TotalLength.ToString());
                                newRow.CreateCell(5 + departmentGroupListCount).SetCellValue(vnaTotalResultDamageExcel.TotalFailLength.ToString());
                                newRow.CreateCell(6 + departmentGroupListCount).SetCellValue(vnaTotalResultDamageExcel.PassPercent.ToString() + "%");
                                newRow.CreateCell(7 + departmentGroupListCount).SetCellValue(vnaTotalResultDamageExcel.Price.ToString());
                                //write PerLossMoney to cunrrent QualityLossGroup
                                newRow.CreateCell(qualityLossGroup.CellNum).SetCellValue(vnaTotalResultDamageExcel.PerLossMoney.ToString());
                                //add current group to tempGroupList
                                tempGroupList.Add(tempGroup);
                                startRow = startRow + 1;
                            }
                            else //if contains in tempGroupList， do not write general info
                            {
                                //get pre Row because Group is Ordered
                                IRow currentRow = worksheet.GetRow(startRow - 1);
                                //write PerLossMoney to cunrrent DepartmentGroup
                                if (departmentGroup != null)
                                {
                                    var oldDepartmentCell = currentRow.GetCell(departmentGroup.CellNum);
                                    if (oldDepartmentCell != null)
                                    {
                                        string oldValStr = oldDepartmentCell.StringCellValue;
                                        decimal newValDecimal = Convert.ToDecimal(oldValStr) + vnaTotalResultDamageExcel.PerLossMoney;
                                        oldDepartmentCell.SetCellValue(newValDecimal.ToString());
                                    }
                                    else
                                    {
                                        currentRow.CreateCell(departmentGroup.CellNum).SetCellValue(vnaTotalResultDamageExcel.PerLossMoney.ToString());
                                    }
                                }
                                //write PerLossMoney to cunrrent QualityLossGroup
                                var oldQualityLossCell = currentRow.GetCell(qualityLossGroup.CellNum);
                                if (oldQualityLossCell != null)
                                {
                                    string oldValStr = oldQualityLossCell.StringCellValue;
                                    decimal newValDecimal = Convert.ToDecimal(oldValStr) + vnaTotalResultDamageExcel.PerLossMoney;
                                    oldQualityLossCell.SetCellValue(newValDecimal.ToString());
                                }
                                else 
                                {
                                    currentRow.CreateCell(qualityLossGroup.CellNum).SetCellValue(vnaTotalResultDamageExcel.PerLossMoney.ToString());
                                }  
                            }
                        }
                        //startRow + 1 is to add a blank row
                        IRow totalRow = worksheet.CreateRow(startRow + 1);
                        IRow totalRowPercent = worksheet.CreateRow(startRow + 2);
                        totalRow.CreateCell(0).SetCellValue("合计");
                        //write DepartmentGroup TotalLossMoney
                        foreach(var departmentGroup in departmentGroupList)
                        {
                            int cellNum = departmentGroup.CellNum;
                            totalRow.CreateCell(cellNum).SetCellValue(departmentGroup.TotalLossMoney.ToString());
                        }
                        //write totalLength（总产量）
                        totalRow.CreateCell(4 + departmentGroupListCount).SetCellValue(totalLength.ToString());
                        //write totalFailLength（总不合格量）
                        totalRow.CreateCell(5 + departmentGroupListCount).SetCellValue(failTotalLength.ToString());
                        //write totalPercent（合格率）
                        totalRow.CreateCell(6 + departmentGroupListCount).SetCellValue(Math.Round(((totalLength-failTotalLength) / totalLength) * 100, 2, MidpointRounding.ToEven).ToString() + "%");
                        //write totalPerFailLength(in fact it is total loss money)
                        foreach (var qualityLossGroup in qualityLossGroupList)
                        {
                            int cellNum = qualityLossGroup.CellNum;
                            totalRow.CreateCell(cellNum).SetCellValue(qualityLossGroup.PerFailLengthTotal.ToString());
                            //write per qualityLoss group total loss money's percent in  totalLossMoney
                            totalRowPercent.CreateCell(cellNum).SetCellValue(Math.Round(qualityLossGroup.PerFailLengthTotal / totalLossMoney, 2, MidpointRounding.ToEven).ToString());
                        }
                        //add title to per qualityLoss group total loss money's percent in  totalLossMoney
                        totalRowPercent.CreateCell(7 + departmentGroupListCount).SetCellValue("损失占比");
                        //add department group list total loss money
                        totalRowPercent.CreateCell(4).SetCellValue("各部门损失合计");
                        totalRowPercent.CreateCell(5).SetCellValue(departmentGrouplistTotalLossMoney.ToString());
                        //add total Loss Money
                        totalRowPercent.CreateCell(0).SetCellValue("总损失");
                        totalRowPercent.CreateCell(1).SetCellValue(totalLossMoney.ToString());

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

            ViewBag.TotalDamage = totalDamage;
            chartList.Add(chart);
            return PartialView(chartList);
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
            
            //initial chart
            //DotNet.Highcharts.Highcharts chart = new DotNet.Highcharts.Highcharts("chart");
            List<Highcharts> chartList = new List<Highcharts> { };

            //call procedure
            string sql = "exec p_coaxialqualitydamage_departmentTab_Rp @testtimestart, @testtimestop, @drillingcrew, @workgroup, @producttypeId";
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
            //init vna total result damage department list, use VnaTotalResultDamageDepartment ViewModel
            List<VnaTotalResultDamageDepartment> vnaTotalResultDamageDptList = new List<VnaTotalResultDamageDepartment>();
            //departments total damage
            decimal? totalDamage = 0;
            if (dt.Rows.Count > 0)
            {
                //auto map dt to VnaTotalResultDamageDepartment
                DataTableReader dr = dt.CreateDataReader();
                vnaTotalResultDamageDptList = AutoMapper.Mapper.DynamicMap<IDataReader, List<VnaTotalResultDamageDepartment>>(dr);
                //get total departments
                var departmentsList = vnaTotalResultDamageDptList
                    .Select(p => new
                    {
                        p.DepartmentId,
                        p.DepartmentName,
                        p.DepartmentLossMoney
                    }).Distinct().ToList();
                //department count
                int dptCount = departmentsList.Count();
                //depatment categories,stroed department name
                string[] dptCategories = new string[dptCount];
                //Point array, stored per departemnt category
                DotNet.Highcharts.Options.Point[] dptPoints = new DotNet.Highcharts.Options.Point[dptCount];
                //dptData,stored departemnt data, include per producttype data of each department
                Data dptData = new Data(dptPoints);
                //foreach departmentsList, get department loss money, and get per producttype loss money in this.department
                int i = 0;
                foreach (var department in departmentsList)
                {
                    //totalDamage add
                    totalDamage = totalDamage + department.DepartmentLossMoney;
                    //assign department name to dptCategories
                    dptCategories[i] = department.DepartmentName;
                    //current department Point
                    DotNet.Highcharts.Options.Point currentDptPoint = new DotNet.Highcharts.Options.Point 
                                                                      {
                                                                          Y = (double)department.DepartmentLossMoney,
                                                                          Color = Color.FromName("colors[" + i + "]"),
                                                                      };
                    //get current department's producttype category
                    var productTypeList = vnaTotalResultDamageDptList
                        .Where(a => a.DepartmentId == department.DepartmentId && a.DepartmentName == department.DepartmentName)
                        .ToList();
                    //number of peoducttype in current department
                    int currentProductTypeCount = productTypeList.Count();
                    //current productTypes Drilldown
                    Drilldown currentProductTypesDrillDown = new Drilldown();
                    //currentProductTypeCategories, stored current producttyes name
                    string[] currentProductTypeCategories = new string[currentProductTypeCount];
                    //currentProductTypeData, stored percent of current producttypes loss money in current department loss money
                    object[] currentProductTypeData = new object[currentProductTypeCount];
                    int j = 0;
                    foreach (var productType in productTypeList)
                    {
                        currentProductTypeCategories[j] = productType.ProductFullName;
                        currentProductTypeData[j] = Math.Round((productType.ProductLossMoney / department.DepartmentLossMoney)*100,2,MidpointRounding.ToEven);
                        j = j + 1;//j is for loop productTypeList
                    }
                    //assign value to currentProductTypesDrillDown
                    currentProductTypesDrillDown.Name = "各型号产品";
                    currentProductTypesDrillDown.Categories = currentProductTypeCategories;
                    currentProductTypesDrillDown.Data = new Data(currentProductTypeData);
                    currentProductTypesDrillDown.Color = Color.FromName("colors[" + i + "]");
                    //assign currentProductTypesDrillDown to currentDptPoint
                    currentDptPoint.Drilldown = currentProductTypesDrillDown;
                    //assign currentDptPoint to dptPoints
                    dptPoints[i] = currentDptPoint;

                    i = i + 1;//i is for loop departmentList
                }

                const string NAME = "部门不合格统计";

                Highcharts chart = new Highcharts("chart")
                .InitChart(new Chart { DefaultSeriesType = ChartTypes.Column })
                .SetTitle(new Title { Text = "各部门同轴质量损失表（元）" })
                .SetSubtitle(new Subtitle { Text = "点击柱子查看详情或返回." })
                .SetXAxis(new XAxis { Categories = dptCategories })
                .SetYAxis(new YAxis { Title = new YAxisTitle { Text = "损失" } })
                .SetLegend(new Legend { Enabled = false })
                .SetTooltip(new Tooltip { Formatter = "TooltipFormatter" })
                .SetPlotOptions(new PlotOptions
                {
                    Column = new PlotOptionsColumn
                    {
                        Cursor = Cursors.Pointer,
                        Point = new PlotOptionsColumnPoint { Events = new PlotOptionsColumnPointEvents { Click = "ColumnPointClick" } }
                        //DataLabels = new PlotOptionsColumnDataLabels
                        //{
                        //    Enabled = true,
                        //    Color = Color.FromName("colors[0]"),
                        //    Formatter = "function() { return this.y +' ('+ Highcharts.numberFormat((this.y/" + totalDamage + ")*100) +'%)'; }",
                        //    Style = "fontWeight: 'bold'"
                        //}
                    }
                })
                .SetSeries(new Series
                {
                    Name = "Browser brands",
                    Data = dptData,
                    Color = Color.White
                })
                .SetExporting(new Exporting { Enabled = false })
                .AddJavascripFunction(
                    "TooltipFormatter",
                    @"var point = this.point, s = '';
                      if (point.drilldown) {
                        s += '损失量:<b>' + this.y +'</b><br/>点击查看'+ point.category +'详情';
                      } else {
                        s += '占比<b>' + this.y +'%</b><br/>点击返回';
                      }
                      return s;"
                )
                .AddJavascripFunction(
                    "ColumnPointClick",
                    @"var drilldown = this.drilldown;
                      if (drilldown) { // drill down
                        setChart(drilldown.name, drilldown.categories, drilldown.data.data, drilldown.color);
                      } else { // restore
                        setChart(name, categories, data.data);
                      }"
                )
                .AddJavascripFunction(
                    "setChart",
                    @"chart.xAxis[0].setCategories(categories);
                      chart.series[0].remove();
                      chart.addSeries({
                         name: name,
                         data: data,
                         color: color || 'white'
                      });",
                    "name", "categories", "data", "color"
                )
                .AddJavascripVariable("colors", "Highcharts.getOptions().colors")
                .AddJavascripVariable("name", "'{0}'".FormatWith(NAME))
                .AddJavascripVariable("categories", JsonSerializer.Serialize(dptCategories))
                .AddJavascripVariable("data", JsonSerializer.Serialize(dptData));
                chartList.Add(chart);
            }

            ViewBag.TotalDamage = totalDamage;
            return PartialView(chartList);
        }

        //质量损失-机台
        public ActionResult StatByDrillingCrew(string TestTimeStartDay = null, string TestTimeStartHour = null, string TestTimeStopDay = null, string TestTimeStopHour = null, string DrillingCrew = null, string WorkGroup = null, int ProductTypeId = 0)
        {
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

            //call procedure
            string sql = "exec p_coaxialqualitydamageTab_Rp @testtimestart, @testtimestop, @drillingcrew, @workgroup, @producttypeId";
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
            //init vna total result damage list, use VnaTotalResultDamage ViewModel
            List<VnaTotalResultDamage> vnaTotalResultDamageList = new List<VnaTotalResultDamage>();
            //vna total damage
            decimal? totalDamage = 0;
            if (dt.Rows.Count > 0)
            {
                //auto map dt to VnaTotalResultDamage
                DataTableReader dr = dt.CreateDataReader();
                vnaTotalResultDamageList = AutoMapper.Mapper.DynamicMap<IDataReader, List<VnaTotalResultDamage>>(dr);
                //fail（不合格） or IsGreenLight（放行） list,
                List<VnaTotalResultDamage> vnaFailResultDamageList = vnaTotalResultDamageList.Where(a => a.TestResult == 1 || (a.TestResult == 0 && a.IsGreenLight == 1)).ToList();
                // no defined qulity loss
                var noDefQualityLossList = vnaFailResultDamageList
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
                    //get totalDamage
                    totalDamage = vnaFailResultDamageList.Sum(a => a.LossMoney);
                    //get fail group list
                    var vnaFailGroupList = vnaFailResultDamageList
                        .GroupBy(a => new
                        {
                            a.DrillingCrew
                        })
                        .Select(ac => new
                        {
                            DrillingCrew = ac.Key.DrillingCrew,
                            Demage = ac.Sum(acs => acs.LossMoney)
                        }).ToList();
                    vnaFailGroupList = vnaFailGroupList.OrderBy(a => a.DrillingCrew).ToList();
                    //totalXObj for X(X list), totalYObj for Y(Y is fail lenght list)
                    var totalXObj = new string[vnaFailGroupList.Count()];
                    var totalYObj = new object[vnaFailGroupList.Count()];
                    for (int i = 0; i < vnaFailGroupList.Count(); i++)
                    {
                        var vnaFailGroup = vnaFailGroupList[i];
                        var drillingCrew = vnaFailGroup.DrillingCrew;
                        totalXObj[i] = drillingCrew;
                        //vnaFailGroup demage
                        totalYObj[i] = Convert.ToDecimal(vnaFailGroup.Demage);
                    }
                    chart
                    .SetTitle(new Title { Text = "各机台同轴质量损失表（元）" })
                    .SetXAxis(new XAxis
                    {
                        Categories = totalXObj
                    })
                    .SetYAxis(new YAxis
                    {
                        Title = new YAxisTitle { Text = "损失金额(元)" }
                    })
                    .SetSeries(new Series
                    {
                        Name = "机台",
                        Type = ChartTypes.Column,
                        Data = new Data(totalYObj)
                    })
                    .SetTooltip(new Tooltip
                    {
                        Formatter = @"function(){return '<b>损失金额</b>:' + this.y + '元<br/>' + '<b>损失比例</b>: ' + Highcharts.numberFormat((this.y/" + totalDamage + ")*100) + '%'}"
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
                                Formatter = "function() { return this.y + ' (' + Highcharts.numberFormat((this.y/" + totalDamage + ")*100) + '%)'; }",
                                Style = "fontWeight: 'bold'"
                            }
                        }
                    })
                    .AddJavascripVariable("colors", "Highcharts.getOptions().colors");
                }
            }

            ViewBag.TotalDamage = totalDamage;
            chartList.Add(chart);
            return PartialView(chartList);
        }

        //质量损失-班组
        public ActionResult StatByWorkGroup(string TestTimeStartDay = null, string TestTimeStartHour = null, string TestTimeStopDay = null, string TestTimeStopHour = null, string DrillingCrew = null, string WorkGroup = null, int ProductTypeId = 0)
        {
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

            //call procedure
            string sql = "exec p_coaxialqualitydamageTab_Rp @testtimestart, @testtimestop, @drillingcrew, @workgroup, @producttypeId";
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
            //init vna total result damage list, use VnaTotalResultDamage ViewModel
            List<VnaTotalResultDamage> vnaTotalResultDamageList = new List<VnaTotalResultDamage>();
            //vna total damage
            decimal? totalDamage = 0;
            if (dt.Rows.Count > 0)
            {
                //auto map dt to VnaTotalResultDamage
                DataTableReader dr = dt.CreateDataReader();
                vnaTotalResultDamageList = AutoMapper.Mapper.DynamicMap<IDataReader, List<VnaTotalResultDamage>>(dr);
                //fail（不合格） or IsGreenLight（放行） list,
                List<VnaTotalResultDamage> vnaFailResultDamageList = vnaTotalResultDamageList.Where(a => a.TestResult == 1 || (a.TestResult == 0 && a.IsGreenLight == 1)).ToList();
                // no defined qulity loss
                var noDefQualityLossList = vnaFailResultDamageList
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
                    
                }
                else
                {
                    //get totalDamage
                    totalDamage = vnaFailResultDamageList.Sum(a => a.LossMoney);
                    //get fail group list
                    var vnaFailGroupList = vnaFailResultDamageList
                        .GroupBy(a => new
                        {
                            a.WorkGroup
                        })
                        .Select(ac => new
                        {
                            WorkGroup = ac.Key.WorkGroup,
                            Demage = ac.Sum(acs => acs.LossMoney)
                        }).ToList();
                    vnaFailGroupList = vnaFailGroupList.OrderBy(a => a.WorkGroup).ToList();
                    //totalXObj for X(X list), totalYObj for Y(Y is fail lenght list)
                    var totalXObj = new string[vnaFailGroupList.Count()];
                    var totalYObj = new object[vnaFailGroupList.Count()];
                    for (int i = 0; i < vnaFailGroupList.Count(); i++)
                    {
                        var vnaFailGroup = vnaFailGroupList[i];
                        var workGroup = vnaFailGroup.WorkGroup;
                        totalXObj[i] = workGroup;
                        //vnaFailGroup demage
                        totalYObj[i] = Convert.ToDecimal(vnaFailGroup.Demage);
                    }
                    chart
                    .SetTitle(new Title { Text = "各班组同轴质量损失（元）" })
                    .SetXAxis(new XAxis
                    {
                        Categories = totalXObj
                    })
                    .SetYAxis(new YAxis
                    {
                        Title = new YAxisTitle { Text = "损失金额(元)" }
                    })
                    .SetSeries(new Series
                    {
                        Name = "班组",
                        Type = ChartTypes.Column,
                        Data = new Data(totalYObj)
                    })
                    .SetTooltip(new Tooltip
                    {
                        Formatter = @"function(){return '<b>损失金额</b>:' + this.y + '元<br/>' + '<b>损失比例</b>: ' + Highcharts.numberFormat((this.y/" + totalDamage + ")*100) + '%'}"
                    });
                }
            }

            ViewBag.ErrorMsg = errorMsg;
            ViewBag.TotalDamage = totalDamage;
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