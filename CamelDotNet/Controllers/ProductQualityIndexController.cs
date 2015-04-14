using CamelDotNet.Models.DAL;
using CamelDotNet.Models.ViewModels;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace CamelDotNet.Controllers
{
    public class ProductQualityIndexController : Controller
    {
        List<string> path = new List<string>();
        public ProductQualityIndexController()
        {
            path.Add("报表管理");
            path.Add("产品指标统计");
            ViewBag.path = path;
            ViewBag.Name = "产品指标统计";
            ViewBag.Title = "产品指标统计";
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

        public ActionResult Get(string TestTimeStartDay = null, string TestTimeStartHour = null, string TestTimeStopDay = null, string TestTimeStopHour = null, string DrillingCrew = null, string WorkGroup = null, string OrderNo = null, int ProductTypeId = 0, int ClientId = 0, bool export = false)
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
            //initialize ProductQualityIndex List, use ProductQualityIndex ViewModel 
            List<ProductQualityIndex> productQualityIndexList = new List<ProductQualityIndex>() { };
            try 
            {
                //call procedure
                string sql = "exec p_productqualityindexTab_Rp @testtimestart, @testtimestop, @drillingcrew, @workgroup, @producttypeId, @clientId, @orderNo";
                SqlParameter[] param = new SqlParameter[7];
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
                param[5] = new SqlParameter("@clientId", SqlDbType.Int);
                param[5].Value = ClientId;
                param[6] = new SqlParameter("@orderNo", SqlDbType.NVarChar);
                param[6].Value = OrderNo;
                //get procedure result
                DataTable dt = CommonController.GetDateTable(sql, param);
                //if dt is not null, auto map dt to ProductQualityIndex
                if (dt.Rows.Count > 0)
                {
                    DataTableReader dr = dt.CreateDataReader();
                    productQualityIndexList = AutoMapper.Mapper.DynamicMap<IDataReader, List<ProductQualityIndex>>(dr);
                }
                else
                {
                    errorMsg = "记录为空";
                }
            }
            catch(Exception ex)
            {
                errorMsg = ex.Message;
            }

            //export excel
            if (export)
            {
                //initailize excel name
                string excelName = DateTime.Now.ToString("yyyyMMddHHmmss") + ".xls";

                if(errorMsg == null)
                {
                    MemoryStream stream = new MemoryStream();
                    HSSFWorkbook workbook = new HSSFWorkbook();
                    workbook.CreateSheet("sheet1");
                    ISheet worksheet = workbook.GetSheet("sheet1");
                    //title in row 1
                    IRow titleOne = worksheet.CreateRow(0);
                    //title in row 2
                    IRow titleTwo = worksheet.CreateRow(1);
                    //create SerialNumber(缆号) in title One, from cell 1
                    titleOne.CreateCell(0).SetCellValue("缆号");
                    //create ReelNumber(盘号) in title One, from cell 2
                    titleOne.CreateCell(1).SetCellValue("盘号");
                    //create SW1(驻波1) in titleOne, from cell 3
                    titleOne.CreateCell(2).SetCellValue("长度");
                    //create SW1(驻波1) in titleOne, from cell 4
                    titleOne.CreateCell(3).SetCellValue("驻波1");
                    //create SW2(驻波2) in titleOne, from cell 8
                    titleOne.CreateCell(7).SetCellValue("驻波2");
                    //create RL1(回波损耗1) in titleOne, from cell 12
                    titleOne.CreateCell(11).SetCellValue("回波损耗1");
                    //create RL2(回波损耗2) in titleOne, from cell 16
                    titleOne.CreateCell(15).SetCellValue("回波损耗2");
                    //create TDI(时域阻抗) in titleOne, from cell 20
                    titleOne.CreateCell(19).SetCellValue("时域阻抗");
                    //create Attenuation(衰减) in titleOne, from cell 21
                    titleOne.CreateCell(20).SetCellValue("衰减");
                    //create SW1(驻波1) channel 1-4 in titleTwo, from cell 4 to 7
                    titleTwo.CreateCell(3).SetCellValue("Channel1");
                    titleTwo.CreateCell(4).SetCellValue("Channel2");
                    titleTwo.CreateCell(5).SetCellValue("Channel3");
                    titleTwo.CreateCell(6).SetCellValue("Channel4");
                    //create SW1(驻波2) channel 1-4 in titleTwo, from cell 8 to 11
                    titleTwo.CreateCell(7).SetCellValue("Channel1");
                    titleTwo.CreateCell(8).SetCellValue("Channel2");
                    titleTwo.CreateCell(9).SetCellValue("Channel3");
                    titleTwo.CreateCell(10).SetCellValue("Channel4");
                    //create RL1(回波损耗1) channel 1-4 in titleTwo, from cell 12 to 15
                    titleTwo.CreateCell(11).SetCellValue("Channel1");
                    titleTwo.CreateCell(12).SetCellValue("Channel2");
                    titleTwo.CreateCell(13).SetCellValue("Channel3");
                    titleTwo.CreateCell(14).SetCellValue("Channel4");
                    //create RL2(回波损耗2) channel 1-4 in titleTwo, from cell 16 to 19
                    titleTwo.CreateCell(15).SetCellValue("Channel1");
                    titleTwo.CreateCell(16).SetCellValue("Channel2");
                    titleTwo.CreateCell(17).SetCellValue("Channel3");
                    titleTwo.CreateCell(18).SetCellValue("Channel4");
                    //create Attenuation(衰减) all freq in titleTwo, from cell 21 to 37
                    titleTwo.CreateCell(20).SetCellValue("100M");
                    titleTwo.CreateCell(21).SetCellValue("150M");
                    titleTwo.CreateCell(22).SetCellValue("200M");
                    titleTwo.CreateCell(23).SetCellValue("280M");
                    titleTwo.CreateCell(24).SetCellValue("450M");
                    titleTwo.CreateCell(25).SetCellValue("800M");
                    titleTwo.CreateCell(26).SetCellValue("900M");
                    titleTwo.CreateCell(27).SetCellValue("1000M");
                    titleTwo.CreateCell(28).SetCellValue("1500M");
                    titleTwo.CreateCell(29).SetCellValue("1800M");
                    titleTwo.CreateCell(30).SetCellValue("2000M");
                    titleTwo.CreateCell(31).SetCellValue("2200M");
                    titleTwo.CreateCell(32).SetCellValue("2400M");
                    titleTwo.CreateCell(33).SetCellValue("2500M");
                    titleTwo.CreateCell(34).SetCellValue("3000M");
                    titleTwo.CreateCell(35).SetCellValue("3400M");
                    titleTwo.CreateCell(36).SetCellValue("4000M");
                    //merge SerialNumber(缆号) in titleOne and titleTwo
                    worksheet.AddMergedRegion(new CellRangeAddress(0, 1, 0, 0));
                    //merge ReelNumber(盘号) in titleOne and titleTwo
                    worksheet.AddMergedRegion(new CellRangeAddress(0, 1, 1, 1));
                    //merge TotalLength(长度) in titleOne and titleTwo
                    worksheet.AddMergedRegion(new CellRangeAddress(0, 1, 2, 2));
                    //merge SW1(驻波1) in titleOne
                    worksheet.AddMergedRegion(new CellRangeAddress(0, 0, 3, 6));
                    //merge SW2(驻波2) in titleOne
                    worksheet.AddMergedRegion(new CellRangeAddress(0, 0, 7, 10));
                    //merge RL1(回波损耗1) in titleOne
                    worksheet.AddMergedRegion(new CellRangeAddress(0, 0, 11, 14));
                    //merge RL2(回波损耗2) in titleOne
                    worksheet.AddMergedRegion(new CellRangeAddress(0, 0, 15, 18));
                    //merge Attenuation(衰减) in titleOne
                    worksheet.AddMergedRegion(new CellRangeAddress(0, 0, 20, 36));
                    //write data to excel, from row 3
                    int startRowNum = 2;
                    foreach(var productQualityIndex in productQualityIndexList)
                    {
                        IRow currentRow = worksheet.CreateRow(startRowNum);
                        currentRow.CreateCell(0).SetCellValue(productQualityIndex.SerialNumber.ToString());
                        currentRow.CreateCell(1).SetCellValue(productQualityIndex.ReelNumber.ToString());
                        currentRow.CreateCell(2).SetCellValue(productQualityIndex.TotalLength.ToString());
                        currentRow.CreateCell(3).SetCellValue(String.Format("{0:0.##}", productQualityIndex.SWOneConeX / 1000000) + "/" + productQualityIndex.SWOneCone.ToString());
                        currentRow.CreateCell(4).SetCellValue(String.Format("{0:0.##}", productQualityIndex.SWOneCtwoX / 1000000) + "/" + productQualityIndex.SWOneCtwo.ToString());
                        currentRow.CreateCell(5).SetCellValue(String.Format("{0:0.##}", productQualityIndex.SWOneCthreeX / 1000000) + "/" + productQualityIndex.SWOneCthree.ToString());
                        currentRow.CreateCell(6).SetCellValue(String.Format("{0:0.##}", productQualityIndex.SWOneCfourX / 1000000) + "/" + productQualityIndex.SWOneCfour.ToString());
                        currentRow.CreateCell(7).SetCellValue(String.Format("{0:0.##}", productQualityIndex.SWTwoConeX / 1000000) + "/" + productQualityIndex.SWTwoCone.ToString());
                        currentRow.CreateCell(8).SetCellValue(String.Format("{0:0.##}", productQualityIndex.SWTwoCtwoX / 1000000) + "/" + productQualityIndex.SWTwoCtwo.ToString());
                        currentRow.CreateCell(9).SetCellValue(String.Format("{0:0.##}", productQualityIndex.SWTwoCthreeX / 1000000) + "/" + productQualityIndex.SWTwoCthree.ToString());
                        currentRow.CreateCell(10).SetCellValue(String.Format("{0:0.##}", productQualityIndex.SWTwoCfourX / 1000000) + "/" + productQualityIndex.SWTwoCfour.ToString());
                        currentRow.CreateCell(11).SetCellValue(productQualityIndex.RLOneCone.ToString());
                        currentRow.CreateCell(12).SetCellValue(productQualityIndex.RLOneCtwo.ToString());
                        currentRow.CreateCell(13).SetCellValue(productQualityIndex.RLOneCthree.ToString());
                        currentRow.CreateCell(14).SetCellValue(productQualityIndex.RLOneCfour.ToString());
                        currentRow.CreateCell(15).SetCellValue(productQualityIndex.RLTwoCone.ToString());
                        currentRow.CreateCell(16).SetCellValue(productQualityIndex.RLTwoCtwo.ToString());
                        currentRow.CreateCell(17).SetCellValue(productQualityIndex.RLTwoCthree.ToString());
                        currentRow.CreateCell(18).SetCellValue(productQualityIndex.RLTwoCfour.ToString());
                        //时域阻抗
                        currentRow.CreateCell(19).SetCellValue(productQualityIndex.TDI.ToString());
                        currentRow.CreateCell(20).SetCellValue(productQualityIndex.Attenuation100.ToString());
                        currentRow.CreateCell(21).SetCellValue(productQualityIndex.Attenuation150.ToString());
                        currentRow.CreateCell(22).SetCellValue(productQualityIndex.Attenuation200.ToString());
                        currentRow.CreateCell(23).SetCellValue(productQualityIndex.Attenuation280.ToString());
                        currentRow.CreateCell(24).SetCellValue(productQualityIndex.Attenuation450.ToString());
                        currentRow.CreateCell(25).SetCellValue(productQualityIndex.Attenuation800.ToString());
                        currentRow.CreateCell(26).SetCellValue(productQualityIndex.Attenuation900.ToString());
                        currentRow.CreateCell(27).SetCellValue(productQualityIndex.Attenuation1000.ToString());
                        currentRow.CreateCell(28).SetCellValue(productQualityIndex.Attenuation1500.ToString());
                        currentRow.CreateCell(29).SetCellValue(productQualityIndex.Attenuation1800.ToString());
                        currentRow.CreateCell(30).SetCellValue(productQualityIndex.Attenuation2000.ToString());
                        currentRow.CreateCell(31).SetCellValue(productQualityIndex.Attenuation2200.ToString());
                        currentRow.CreateCell(32).SetCellValue(productQualityIndex.Attenuation2400.ToString());
                        currentRow.CreateCell(33).SetCellValue(productQualityIndex.Attenuation2500.ToString());
                        currentRow.CreateCell(34).SetCellValue(productQualityIndex.Attenuation3000.ToString());
                        currentRow.CreateCell(35).SetCellValue(productQualityIndex.Attenuation3400.ToString());
                        currentRow.CreateCell(36).SetCellValue(productQualityIndex.Attenuation4000.ToString());

                        startRowNum = startRowNum + 1;
                    }
                    //get per MAX from productQualityIndexList
                    var maxGroup = productQualityIndexList.GroupBy(uselessConstant => 0)
                        .Select(a => new {
                            SWOneConeMax = a.Max(ac => ac.SWOneCone),
                            SWOneCtwoMax = a.Max(ac => ac.SWOneCtwo),
                            SWOneCthreeMax = a.Max(ac => ac.SWOneCthree),
                            SWOneCfourMax = a.Max(ac => ac.SWOneCfour),
                            SWTwoConeMax = a.Max(ac => ac.SWTwoCone),
                            SWTwoCtwoMax = a.Max(ac => ac.SWTwoCtwo),
                            SWTwoCthreeMax = a.Max(ac => ac.SWTwoCthree),
                            SWTwoCfourMax = a.Max(ac => ac.SWTwoCfour),
                            RLOneConeMax = a.Max(ac => ac.RLOneCone),
                            RLOneCtwoMax = a.Max(ac => ac.RLOneCtwo),
                            RLOneCthreeMax = a.Max(ac => ac.RLOneCthree),
                            RLOneCfourMax = a.Max(ac => ac.RLOneCfour),
                            RLTwoConeMax = a.Max(ac => ac.RLTwoCone),
                            RLTwoCtwoMax = a.Max(ac => ac.RLTwoCtwo),
                            RLTwoCthreeMax = a.Max(ac => ac.RLTwoCthree),
                            RLTwoCfourMax = a.Max(ac => ac.RLTwoCfour),
                            Attenuation100Max = a.Max(ac => ac.Attenuation100),
                            Attenuation150Max = a.Max(ac => ac.Attenuation150),
                            Attenuation200Max = a.Max(ac => ac.Attenuation200),
                            Attenuation280Max = a.Max(ac => ac.Attenuation280),
                            Attenuation450Max = a.Max(ac => ac.Attenuation450),
                            Attenuation800Max = a.Max(ac => ac.Attenuation800),
                            Attenuation900Max = a.Max(ac => ac.Attenuation900),
                            Attenuation1000Max = a.Max(ac => ac.Attenuation1000),
                            Attenuation1500Max = a.Max(ac => ac.Attenuation1500),
                            Attenuation1800Max = a.Max(ac => ac.Attenuation1800),
                            Attenuation2000Max = a.Max(ac => ac.Attenuation2000),
                            Attenuation2200Max = a.Max(ac => ac.Attenuation2200),
                            Attenuation2400Max = a.Max(ac => ac.Attenuation2400),
                            Attenuation2500Max = a.Max(ac => ac.Attenuation2500),
                            Attenuation3000Max = a.Max(ac => ac.Attenuation3000),
                            Attenuation3400Max = a.Max(ac => ac.Attenuation3400),
                            Attenuation4000Max = a.Max(ac => ac.Attenuation4000),
                            TDIMax = a.Max(ac => ac.TDI)
                        }).SingleOrDefault();
                    //write max Row to excel, in row startRowNum, because (startRowNum = startRowNum + 1) in foreach
                    IRow maxRow = worksheet.CreateRow(startRowNum);
                    //write tilte cell, in cell 3
                    maxRow.CreateCell(2).SetCellValue("最大值");
                    //write max data, from cell 4
                    maxRow.CreateCell(3).SetCellValue(maxGroup.SWOneConeMax.ToString());
                    maxRow.CreateCell(4).SetCellValue(maxGroup.SWOneCtwoMax.ToString());
                    maxRow.CreateCell(5).SetCellValue(maxGroup.SWOneCthreeMax.ToString());
                    maxRow.CreateCell(6).SetCellValue(maxGroup.SWOneCfourMax.ToString());
                    maxRow.CreateCell(7).SetCellValue(maxGroup.SWTwoConeMax.ToString());
                    maxRow.CreateCell(8).SetCellValue(maxGroup.SWTwoCtwoMax.ToString());
                    maxRow.CreateCell(9).SetCellValue(maxGroup.SWTwoCthreeMax.ToString());
                    maxRow.CreateCell(10).SetCellValue(maxGroup.SWTwoCfourMax.ToString());
                    maxRow.CreateCell(11).SetCellValue(maxGroup.RLOneConeMax.ToString());
                    maxRow.CreateCell(12).SetCellValue(maxGroup.RLOneCtwoMax.ToString());
                    maxRow.CreateCell(23).SetCellValue(maxGroup.RLOneCthreeMax.ToString());
                    maxRow.CreateCell(14).SetCellValue(maxGroup.RLOneCfourMax.ToString());
                    maxRow.CreateCell(15).SetCellValue(maxGroup.RLTwoConeMax.ToString());
                    maxRow.CreateCell(16).SetCellValue(maxGroup.RLTwoCtwoMax.ToString());
                    maxRow.CreateCell(17).SetCellValue(maxGroup.RLTwoCthreeMax.ToString());
                    maxRow.CreateCell(18).SetCellValue(maxGroup.RLTwoCfourMax.ToString());
                    maxRow.CreateCell(19).SetCellValue(maxGroup.TDIMax.ToString());
                    maxRow.CreateCell(20).SetCellValue(maxGroup.Attenuation100Max.ToString());
                    maxRow.CreateCell(21).SetCellValue(maxGroup.Attenuation150Max.ToString());
                    maxRow.CreateCell(22).SetCellValue(maxGroup.Attenuation200Max.ToString());
                    maxRow.CreateCell(23).SetCellValue(maxGroup.Attenuation280Max.ToString());
                    maxRow.CreateCell(24).SetCellValue(maxGroup.Attenuation450Max.ToString());
                    maxRow.CreateCell(25).SetCellValue(maxGroup.Attenuation800Max.ToString());
                    maxRow.CreateCell(26).SetCellValue(maxGroup.Attenuation900Max.ToString());
                    maxRow.CreateCell(27).SetCellValue(maxGroup.Attenuation1000Max.ToString());
                    maxRow.CreateCell(28).SetCellValue(maxGroup.Attenuation1500Max.ToString());
                    maxRow.CreateCell(29).SetCellValue(maxGroup.Attenuation1800Max.ToString());
                    maxRow.CreateCell(30).SetCellValue(maxGroup.Attenuation2000Max.ToString());
                    maxRow.CreateCell(31).SetCellValue(maxGroup.Attenuation2200Max.ToString());
                    maxRow.CreateCell(32).SetCellValue(maxGroup.Attenuation2400Max.ToString());
                    maxRow.CreateCell(33).SetCellValue(maxGroup.Attenuation2500Max.ToString());
                    maxRow.CreateCell(34).SetCellValue(maxGroup.Attenuation3000Max.ToString());
                    maxRow.CreateCell(35).SetCellValue(maxGroup.Attenuation3400Max.ToString());
                    maxRow.CreateCell(36).SetCellValue(maxGroup.Attenuation4000Max.ToString());
                    //get per MIN from productQualityIndexList
                    var minGroup = productQualityIndexList.GroupBy(uselessConstant => 0)
                        .Select(a => new
                        {
                            SWOneConeMin = a.Min(ac => ac.SWOneCone),
                            SWOneCtwoMin = a.Min(ac => ac.SWOneCtwo),
                            SWOneCthreeMin = a.Min(ac => ac.SWOneCthree),
                            SWOneCfourMin = a.Min(ac => ac.SWOneCfour),
                            SWTwoConeMin = a.Min(ac => ac.SWTwoCone),
                            SWTwoCtwoMin = a.Min(ac => ac.SWTwoCtwo),
                            SWTwoCthreeMin = a.Min(ac => ac.SWTwoCthree),
                            SWTwoCfourMin = a.Min(ac => ac.SWTwoCfour),
                            RLOneConeMin = a.Min(ac => ac.RLOneCone),
                            RLOneCtwoMin = a.Min(ac => ac.RLOneCtwo),
                            RLOneCthreeMin = a.Min(ac => ac.RLOneCthree),
                            RLOneCfourMin = a.Min(ac => ac.RLOneCfour),
                            RLTwoConeMin = a.Min(ac => ac.RLTwoCone),
                            RLTwoCtwoMin = a.Min(ac => ac.RLTwoCtwo),
                            RLTwoCthreeMin = a.Min(ac => ac.RLTwoCthree),
                            RLTwoCfourMin = a.Min(ac => ac.RLTwoCfour),
                            Attenuation100Min = a.Min(ac => ac.Attenuation100),
                            Attenuation150Min = a.Min(ac => ac.Attenuation150),
                            Attenuation200Min = a.Min(ac => ac.Attenuation200),
                            Attenuation280Min = a.Min(ac => ac.Attenuation280),
                            Attenuation450Min = a.Min(ac => ac.Attenuation450),
                            Attenuation800Min = a.Min(ac => ac.Attenuation800),
                            Attenuation900Min = a.Min(ac => ac.Attenuation900),
                            Attenuation1000Min = a.Min(ac => ac.Attenuation1000),
                            Attenuation1500Min = a.Min(ac => ac.Attenuation1500),
                            Attenuation1800Min = a.Min(ac => ac.Attenuation1800),
                            Attenuation2000Min = a.Min(ac => ac.Attenuation2000),
                            Attenuation2200Min = a.Min(ac => ac.Attenuation2200),
                            Attenuation2400Min = a.Min(ac => ac.Attenuation2400),
                            Attenuation2500Min = a.Min(ac => ac.Attenuation2500),
                            Attenuation3000Min = a.Min(ac => ac.Attenuation3000),
                            Attenuation3400Min = a.Min(ac => ac.Attenuation3400),
                            Attenuation4000Min = a.Min(ac => ac.Attenuation4000),
                            TDIMin = a.Min(ac => ac.TDI)
                        }).SingleOrDefault();
                    //write min Row to excel, in row startRowNum + 1, after max row
                    IRow minRow = worksheet.CreateRow(startRowNum + 1);
                    //write tilte cell, in cell 3
                    minRow.CreateCell(2).SetCellValue("最小值");
                    //write min data, from cell 4
                    minRow.CreateCell(3).SetCellValue(minGroup.SWOneConeMin.ToString());
                    minRow.CreateCell(4).SetCellValue(minGroup.SWOneCtwoMin.ToString());
                    minRow.CreateCell(5).SetCellValue(minGroup.SWOneCthreeMin.ToString());
                    minRow.CreateCell(6).SetCellValue(minGroup.SWOneCfourMin.ToString());
                    minRow.CreateCell(7).SetCellValue(minGroup.SWTwoConeMin.ToString());
                    minRow.CreateCell(8).SetCellValue(minGroup.SWTwoCtwoMin.ToString());
                    minRow.CreateCell(9).SetCellValue(minGroup.SWTwoCthreeMin.ToString());
                    minRow.CreateCell(10).SetCellValue(minGroup.SWTwoCfourMin.ToString());
                    minRow.CreateCell(11).SetCellValue(minGroup.RLOneConeMin.ToString());
                    minRow.CreateCell(12).SetCellValue(minGroup.RLOneCtwoMin.ToString());
                    minRow.CreateCell(13).SetCellValue(minGroup.RLOneCthreeMin.ToString());
                    minRow.CreateCell(14).SetCellValue(minGroup.RLOneCfourMin.ToString());
                    minRow.CreateCell(15).SetCellValue(minGroup.RLTwoConeMin.ToString());
                    minRow.CreateCell(16).SetCellValue(minGroup.RLTwoCtwoMin.ToString());
                    minRow.CreateCell(17).SetCellValue(minGroup.RLTwoCthreeMin.ToString());
                    minRow.CreateCell(18).SetCellValue(minGroup.RLTwoCfourMin.ToString());
                    minRow.CreateCell(19).SetCellValue(minGroup.TDIMin.ToString());
                    minRow.CreateCell(20).SetCellValue(minGroup.Attenuation100Min.ToString());
                    minRow.CreateCell(21).SetCellValue(minGroup.Attenuation150Min.ToString());
                    minRow.CreateCell(22).SetCellValue(minGroup.Attenuation200Min.ToString());
                    minRow.CreateCell(23).SetCellValue(minGroup.Attenuation280Min.ToString());
                    minRow.CreateCell(24).SetCellValue(minGroup.Attenuation450Min.ToString());
                    minRow.CreateCell(25).SetCellValue(minGroup.Attenuation800Min.ToString());
                    minRow.CreateCell(26).SetCellValue(minGroup.Attenuation900Min.ToString());
                    minRow.CreateCell(27).SetCellValue(minGroup.Attenuation1000Min.ToString());
                    minRow.CreateCell(28).SetCellValue(minGroup.Attenuation1500Min.ToString());
                    minRow.CreateCell(29).SetCellValue(minGroup.Attenuation1800Min.ToString());
                    minRow.CreateCell(30).SetCellValue(minGroup.Attenuation2000Min.ToString());
                    minRow.CreateCell(31).SetCellValue(minGroup.Attenuation2200Min.ToString());
                    minRow.CreateCell(32).SetCellValue(minGroup.Attenuation2400Min.ToString());
                    minRow.CreateCell(33).SetCellValue(minGroup.Attenuation2500Min.ToString());
                    minRow.CreateCell(34).SetCellValue(minGroup.Attenuation3000Min.ToString());
                    minRow.CreateCell(35).SetCellValue(minGroup.Attenuation3400Min.ToString());
                    minRow.CreateCell(36).SetCellValue(minGroup.Attenuation4000Min.ToString());
                    //get per Average from productQualityIndexList
                    var avgGroup = productQualityIndexList.GroupBy(uselessConstant => 0)
                        .Select(a => new
                        {
                            SWOneConeAverage = a.Average(ac => ac.SWOneCone),
                            SWOneCtwoAverage = a.Average(ac => ac.SWOneCtwo),
                            SWOneCthreeAverage = a.Average(ac => ac.SWOneCthree),
                            SWOneCfourAverage = a.Average(ac => ac.SWOneCfour),
                            SWTwoConeAverage = a.Average(ac => ac.SWTwoCone),
                            SWTwoCtwoAverage = a.Average(ac => ac.SWTwoCtwo),
                            SWTwoCthreeAverage = a.Average(ac => ac.SWTwoCthree),
                            SWTwoCfourAverage = a.Average(ac => ac.SWTwoCfour),
                            RLOneConeAverage = a.Average(ac => ac.RLOneCone),
                            RLOneCtwoAverage = a.Average(ac => ac.RLOneCtwo),
                            RLOneCthreeAverage = a.Average(ac => ac.RLOneCthree),
                            RLOneCfourAverage = a.Average(ac => ac.RLOneCfour),
                            RLTwoConeAverage = a.Average(ac => ac.RLTwoCone),
                            RLTwoCtwoAverage = a.Average(ac => ac.RLTwoCtwo),
                            RLTwoCthreeAverage = a.Average(ac => ac.RLTwoCthree),
                            RLTwoCfourAverage = a.Average(ac => ac.RLTwoCfour),
                            Attenuation100Average = a.Average(ac => ac.Attenuation100),
                            Attenuation150Average = a.Average(ac => ac.Attenuation150),
                            Attenuation200Average = a.Average(ac => ac.Attenuation200),
                            Attenuation280Average = a.Average(ac => ac.Attenuation280),
                            Attenuation450Average = a.Average(ac => ac.Attenuation450),
                            Attenuation800Average = a.Average(ac => ac.Attenuation800),
                            Attenuation900Average = a.Average(ac => ac.Attenuation900),
                            Attenuation1000Average = a.Average(ac => ac.Attenuation1000),
                            Attenuation1500Average = a.Average(ac => ac.Attenuation1500),
                            Attenuation1800Average = a.Average(ac => ac.Attenuation1800),
                            Attenuation2000Average = a.Average(ac => ac.Attenuation2000),
                            Attenuation2200Average = a.Average(ac => ac.Attenuation2200),
                            Attenuation2400Average = a.Average(ac => ac.Attenuation2400),
                            Attenuation2500Average = a.Average(ac => ac.Attenuation2500),
                            Attenuation3000Average = a.Average(ac => ac.Attenuation3000),
                            Attenuation3400Average = a.Average(ac => ac.Attenuation3400),
                            Attenuation4000Average = a.Average(ac => ac.Attenuation4000),
                            TDIAverage = a.Average(ac => ac.TDI)
                        }).SingleOrDefault();
                    //write avg Row to excel, in row startRowNum + 2, after MIN row
                    IRow avgRow = worksheet.CreateRow(startRowNum + 2);
                    //write tilte cell, in cell 3
                    avgRow.CreateCell(2).SetCellValue("平均值");
                    //write avg data, from cell 4
                    avgRow.CreateCell(3).SetCellValue(String.Format("{0:0.##}", avgGroup.SWOneConeAverage));
                    avgRow.CreateCell(4).SetCellValue(String.Format("{0:0.##}", avgGroup.SWOneCtwoAverage));
                    avgRow.CreateCell(5).SetCellValue(String.Format("{0:0.##}", avgGroup.SWOneCthreeAverage));
                    avgRow.CreateCell(6).SetCellValue(String.Format("{0:0.##}", avgGroup.SWOneCfourAverage));
                    avgRow.CreateCell(7).SetCellValue(String.Format("{0:0.##}", avgGroup.SWTwoConeAverage));
                    avgRow.CreateCell(8).SetCellValue(String.Format("{0:0.##}", avgGroup.SWTwoCtwoAverage));
                    avgRow.CreateCell(9).SetCellValue(String.Format("{0:0.##}", avgGroup.SWTwoCthreeAverage));
                    avgRow.CreateCell(10).SetCellValue(String.Format("{0:0.##}", avgGroup.SWTwoCfourAverage));
                    avgRow.CreateCell(11).SetCellValue(String.Format("{0:0.##}", avgGroup.RLOneConeAverage));
                    avgRow.CreateCell(12).SetCellValue(String.Format("{0:0.##}", avgGroup.RLOneCtwoAverage));
                    avgRow.CreateCell(13).SetCellValue(String.Format("{0:0.##}", avgGroup.RLOneCthreeAverage));
                    avgRow.CreateCell(14).SetCellValue(String.Format("{0:0.##}", avgGroup.RLOneCfourAverage));
                    avgRow.CreateCell(15).SetCellValue(String.Format("{0:0.##}", avgGroup.RLTwoConeAverage));
                    avgRow.CreateCell(16).SetCellValue(String.Format("{0:0.##}", avgGroup.RLTwoCtwoAverage));
                    avgRow.CreateCell(17).SetCellValue(String.Format("{0:0.##}", avgGroup.RLTwoCthreeAverage));
                    avgRow.CreateCell(18).SetCellValue(String.Format("{0:0.##}", avgGroup.RLTwoCfourAverage));
                    avgRow.CreateCell(19).SetCellValue(String.Format("{0:0.##}", avgGroup.TDIAverage));
                    avgRow.CreateCell(20).SetCellValue(String.Format("{0:0.##}", avgGroup.Attenuation100Average));
                    avgRow.CreateCell(21).SetCellValue(String.Format("{0:0.##}", avgGroup.Attenuation150Average));
                    avgRow.CreateCell(22).SetCellValue(String.Format("{0:0.##}", avgGroup.Attenuation200Average));
                    avgRow.CreateCell(23).SetCellValue(String.Format("{0:0.##}", avgGroup.Attenuation280Average));
                    avgRow.CreateCell(24).SetCellValue(String.Format("{0:0.##}", avgGroup.Attenuation450Average));
                    avgRow.CreateCell(25).SetCellValue(String.Format("{0:0.##}", avgGroup.Attenuation800Average));
                    avgRow.CreateCell(26).SetCellValue(String.Format("{0:0.##}", avgGroup.Attenuation900Average));
                    avgRow.CreateCell(27).SetCellValue(String.Format("{0:0.##}", avgGroup.Attenuation1000Average));
                    avgRow.CreateCell(28).SetCellValue(String.Format("{0:0.##}", avgGroup.Attenuation1500Average));
                    avgRow.CreateCell(29).SetCellValue(String.Format("{0:0.##}", avgGroup.Attenuation1800Average));
                    avgRow.CreateCell(30).SetCellValue(String.Format("{0:0.##}", avgGroup.Attenuation2000Average));
                    avgRow.CreateCell(31).SetCellValue(String.Format("{0:0.##}", avgGroup.Attenuation2200Average));
                    avgRow.CreateCell(32).SetCellValue(String.Format("{0:0.##}", avgGroup.Attenuation2400Average));
                    avgRow.CreateCell(33).SetCellValue(String.Format("{0:0.##}", avgGroup.Attenuation2500Average));
                    avgRow.CreateCell(34).SetCellValue(String.Format("{0:0.##}", avgGroup.Attenuation3000Average));
                    avgRow.CreateCell(35).SetCellValue(String.Format("{0:0.##}", avgGroup.Attenuation3400Average));
                    avgRow.CreateCell(36).SetCellValue(String.Format("{0:0.##}", avgGroup.Attenuation4000Average));

                    if (!workbook.IsWriteProtected)
                    {
                        workbook.Write(stream);
                    }
                    return File(stream.ToArray(), "application/vnd.ms-excel", excelName);
                }
                else
                {
                    MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(errorMsg));
                    return File(stream.ToArray(), "application/vnd.ms-excel", excelName);
                }
            }

            ViewBag.ErrorMsg = errorMsg;
            return PartialView(productQualityIndexList);
        }
	}
}