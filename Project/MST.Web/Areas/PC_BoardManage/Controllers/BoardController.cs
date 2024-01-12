/*******************************************************************************
 * 软件名称: 崇令智能仓储管理系统
 * 技术讨论: 18718690940 (微信同号)
 * QQ交流群:  795702839
 * 最新源码：https://github.com/243480503/WMS-BS
*********************************************************************************/
using MST.Code;
using MST.Data;
using MST.Domain.Entity.WMSLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace MST.Web.Areas.PC_BoardManage.Controllers
{
    [HandlerLogin(false)]
    public class BoardController : ControllerBase
    {
        [HttpGet]
        public virtual ActionResult BoardView()
        {
            return View();
        }

        /// <summary>
        /// 当日作业
        /// </summary>
        private class CurDayWork
        {
            /// <summary>
            /// 入库作业任务数(采购入库单)
            /// </summary>
            public decimal? InTaskCount { get; set; }
            /// <summary>
            /// 出库作业任务数(领料单、仓退单、验退单、其它单)
            /// </summary>
            public decimal? OutTaskCount { get; set; }

            /// <summary>
            /// 盘点作业任务数（盘点单）
            /// </summary>
            public decimal? QATaskCount { get; set; }

            /// <summary>
            /// 质检作业任务数(质检取样单)
            /// </summary>
            public decimal? CountTaskCount { get; set; }
        }

        #region 物料库存变化
        /// <summary>
        /// 物料库存变化
        /// </summary>
        private class ItemChange
        {
            public int? YearNum { get; set; }

            public IList<ItemChangeCell> ItemList { get; set; }
        }

        private class ItemChangeCell
        {
            public string ItemCode { get; set; }
            public string ItemName { get; set; }

            public IList<ItemChangeCellMonth> MonthData { get; set; }

        }

        private class ItemChangeCellMonth
        {
            public int? Month { get; set; }
            public decimal? SumQty { get; set; }
        }

        #endregion

        /// <summary>
        /// 货位占用率
        /// </summary>
        private class LocPer
        {
            public int? LocStored { get; set; }

            public int? LocEmpty { get; set; }
        }

        /// <summary>
        /// 出入库计数
        /// </summary>
        private class InOutCount
        {
            public decimal? InNum { get; set; }

            public decimal? OutNum { get; set; }
        }

        /// <summary>
        /// 单据数量
        /// </summary>
        private class OrderPer
        {
            /// <summary>
            /// 入库作业任务数
            /// </summary>
            public decimal? InOrderCount { get; set; }

            public decimal? InOrderCount_Sum { get; set; }
            /// <summary>
            /// 出库作业任务数
            /// </summary>
            public decimal? OutOrderCount { get; set; }
            public decimal? OutOrderCount_Sum { get; set; }
            /// <summary>
            /// 盘点作业任务数
            /// </summary>
            public decimal? QAOrderCount { get; set; }
            public decimal? QAOrderCount_Sum { get; set; }

            /// <summary>
            /// 质检作业任务数
            /// </summary>
            public decimal? CountOrderCount { get; set; }
            public decimal? CountOrderCount_Sum { get; set; }
        }

        /// <summary>
        /// 每日出入库量
        /// </summary>
        private class DayInOut
        {
            /// <summary>
            /// 出入库类型（In，Out）
            /// </summary>
            public string InOutType { get; set; }

            public IList<DayInOutCell> DataList { get; set; }
        }

        private class DayInOutCell
        {

            /// <summary>
            /// 日期
            /// </summary>
            public int? Day { get; set; }

            /// <summary>
            /// 物料总数
            /// </summary>
            public decimal? DayQty { get; set; }
        }

        /// <summary>
        /// 当前库存统计
        /// </summary>
        private class ContainerQty
        {
            public decimal? Qty { get; set; }
            public string ItemCode { get; set; }
            public string ItemName { get; set; }
        }

        [HttpGet]
        public ActionResult GetGridJson()
        {
            try
            {
                using (var db = new RepositoryBase().BeginTrans())
                {

                    //累计出入库数量
                    InOutCount curInOutCountData =(InOutCount)GetStatisticsInfo("CurInOutCountData").Data;
                    //当日作业汇总
                    CurDayWork curDayWorkData = (CurDayWork)GetStatisticsInfo("CurDayWorkData").Data;
                    //单据进度
                    OrderPer curOrderPerData = (OrderPer)GetStatisticsInfo("CurOrderPerData").Data;
                    //物料库存变化(按年月)
                    IList<ItemChange> curItemChangeData = (IList<ItemChange>)GetStatisticsInfo("CurItemChangeData").Data;
                    //货位占用率
                    LocPer curLocPerData = (LocPer)GetStatisticsInfo("CurLocPerData").Data;
                    //每日出入库数量(30天)
                    IList<DayInOut> curDayInOut = (IList<DayInOut>)GetStatisticsInfo("CurDayInOut").Data;
                    //库存统计
                    IList<ContainerQty> curContainerQtyData = (IList<ContainerQty>)GetStatisticsInfo("CurContainerQtyData").Data;


                    var data = new
                    {
                        curDayWorkData = curDayWorkData,
                        curItemChangeData = curItemChangeData,
                        curLocPerData = curLocPerData,
                        curInOutCountData = curInOutCountData,
                        curOrderPerData = curOrderPerData,
                        curDayInOut = curDayInOut,
                        curContainerQtyData = curContainerQtyData
                    };
                    return Success("获取成功", data);
                }

            }
            catch (Exception ex)
            {
                return Error("获取数据失败", ex.Message);
            }

        }

        public EBoardResult GetStatisticsInfo(string type)
        {
            using (IRepositoryBase db = new RepositoryBase().BeginTrans())
            {
                EBoardResult result = new EBoardResult();
                switch (type)
                {
                    case "CurContainerQtyData": //当前库存统计
                        {
                            //库存统计
                            IList<ContainerQty> curContainerQtyData = db.IQueryable<T_ContainerDetailEntity>().GroupBy(o => new { ItemName = o.ItemName, ItemCode = o.ItemCode }).Select(o => new ContainerQty() { ItemCode = o.Key.ItemCode, ItemName = o.Key.ItemName, Qty = o.Sum(i => i.Qty) }).Take(8).ToList();
                            result.Data = curContainerQtyData;
                        }
                        break;
                    case "CurDayInOut": //每日出入库数量
                        {
                            //每日出入库数量(30天)
                            IList<DayInOut> curDayInOut = new List<DayInOut>();
                            int CurDayCount = DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month);
                            DateTime MonthFisrtDay = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                            DateTime nextMonthFisrtDay = MonthFisrtDay.AddMonths(1);

                            curDayInOut.Add(new DayInOut() { InOutType = "In", DataList = new List<DayInOutCell>() });
                            curDayInOut.Add(new DayInOut() { InOutType = "Out", DataList = new List<DayInOutCell>() });

                            IList<T_InOutDetailEntity> inOutBoundList = db.FindList<T_InOutDetailEntity>(o => o.F_CreatorTime >= MonthFisrtDay && o.F_CreatorTime < nextMonthFisrtDay);
                            foreach (DayInOut cell in curDayInOut)
                            {
                                IList<T_InBoundDetailEntity> InAndOutList = new List<T_InBoundDetailEntity>();
                                if (cell.InOutType == "In")
                                {
                                    inOutBoundList.Where(o => o.InOutType == "InType");
                                }
                                else
                                {
                                    inOutBoundList.Where(o => o.InOutType == "OutType");
                                }
                                for (int i = 0; i < CurDayCount; i++)
                                {
                                    cell.DataList.Add(new DayInOutCell() { Day = i, DayQty = inOutBoundList.Where(o => o.F_CreatorTime.Value.Day == (i + 1)).Sum(o => o.ChangeQty) });
                                }
                            }

                            result.Data = curDayInOut;
                        }
                        break;
                    case "CurDayWorkData": //当日作业汇总
                        {
                            //当日作业汇总
                            CurDayWork curDayWorkData = new CurDayWork();
                            DateTime curTempData = DateTime.Now.Date;
                            curDayWorkData.InTaskCount = db.IQueryable<T_TaskHisEntity>(o => curTempData < o.OverTime && o.OrderType == "PurchaseIn").Count();
                            curDayWorkData.OutTaskCount = db.IQueryable<T_TaskHisEntity>(o => curTempData < o.OverTime &&
                                                                                                 (o.OrderType == "GetItemOut"
                                                                                                 || o.OrderType == "VerBackOut"
                                                                                                 || o.OrderType == "WarehouseBackOut"
                                                                                                 || o.OrderType == "OtherOut")
                                                                                                ).Count();
                            curDayWorkData.QATaskCount = db.IQueryable<T_TaskHisEntity>(o => curTempData < o.OverTime && o.OrderType == "GetSample").Count();
                            curDayWorkData.CountTaskCount = db.IQueryable<T_TaskHisEntity>(o => curTempData < o.OverTime && o.OrderType == "Count").Count();

                            result.Data = curDayWorkData;
                        }
                        break;
                    case "CurInOutCountData": //累计出入库数量
                        {
                            //累计出入库数量
                            InOutCount curInOutCountData = new InOutCount();
                            curInOutCountData.InNum = db.IQueryable<T_InOutDetailEntity>(o => o.OrderType == "PurchaseIn").Sum(o => o.ChangeQty);
                            curInOutCountData.OutNum = db.IQueryable<T_InOutDetailEntity>(o => o.OrderType == "GetItemOut"
                                                                                                 || o.OrderType == "VerBackOut"
                                                                                                 || o.OrderType == "WarehouseBackOut"
                                                                                                 || o.OrderType == "OtherOut").Sum(o => o.ChangeQty);
                            result.Data = curInOutCountData;
                        }
                        break;
                    case "CurItemChangeData": //物料库存变化
                        {
                            //物料库存变化(按年月)
                            IList<ItemChange> curItemChangeData = new List<ItemChange>();

                            int Year1 = DateTime.Now.Year;
                            int Year2 = Year1 - 1;


                            IList<V_Board_YearMonthDayItemChangeEntity> yearMonthDayItemChangeList = db.IQueryable<V_Board_YearMonthDayItemChangeEntity>(o => o.YearNum == Year1 || o.YearNum == Year2).ToList();
                            //选取合理的2物料
                            var itemSelect = yearMonthDayItemChangeList.GroupBy(o => new { o.ItemCode, o.ItemName }).Select(o => new { o.Key.ItemCode, o.Key.ItemName, Qty = o.Sum(i => i.SumQty ?? 0) }).OrderByDescending(o => o.Qty).Take(2).Select(o => new { ItemCode = o.ItemCode, ItemName = o.ItemName }).ToList();
                            string[] itemCodeArray = itemSelect.Select(o => o.ItemCode).ToArray();
                            yearMonthDayItemChangeList = yearMonthDayItemChangeList.Where(o => itemCodeArray.Contains(o.ItemCode)).ToList();

                            curItemChangeData.Add(new ItemChange() { YearNum = Year1 });
                            curItemChangeData.Add(new ItemChange() { YearNum = Year2 });

                            foreach (ItemChange cell in curItemChangeData)
                            {
                                cell.ItemList = new List<ItemChangeCell>();
                                foreach (var item in itemSelect)
                                {
                                    ItemChangeCell itemchangecell = new ItemChangeCell() { ItemCode = item.ItemCode, ItemName = item.ItemName };
                                    itemchangecell.MonthData = new List<ItemChangeCellMonth>();
                                    for (int i = 0; i < 12; i++)
                                    {
                                        ItemChangeCellMonth cellMonth = new ItemChangeCellMonth() { Month = (i + 1) };
                                        cellMonth.SumQty = yearMonthDayItemChangeList.Where(o => o.ItemCode == item.ItemCode && o.YearNum == cell.YearNum && o.MonthNum == cellMonth.Month).Sum(o => o.SumQty);
                                        itemchangecell.MonthData.Add(cellMonth);
                                    }
                                    cell.ItemList.Add(itemchangecell);
                                }
                            }
                            curItemChangeData = curItemChangeData.OrderBy(o => o.YearNum).ToList();

                            result.Data = curItemChangeData;
                        }
                        break;
                    case "CurLocPerData": //货位占用率
                        {
                            //货位占用率
                            LocPer curLocPerData = new LocPer();
                            curLocPerData.LocStored = db.IQueryable<T_LocationEntity>(o => o.LocationType == "Cube" && (o.State == "Stored" || o.State == "In")).Count();
                            curLocPerData.LocEmpty = db.IQueryable<T_LocationEntity>(o => o.LocationType == "Cube" && (o.State == "Empty" || o.State == "Out")).Count();

                            result.Data = curLocPerData;

                        }
                        break;
                    case "CurOrderPerData": //单据进度
                        {
                            //单据进度
                            OrderPer curOrderPerData = new OrderPer();
                            string[] inboundID = db.IQueryable<T_InBoundEntity>(o => o.State != "Over").Select(o => o.F_Id).ToArray();
                            IList<T_InBoundDetailEntity> inboundDetail = db.IQueryable<T_InBoundDetailEntity>(o => inboundID.Contains(o.InBoundID)).ToList();
                            curOrderPerData.InOrderCount = inboundDetail.Sum(o => o.OverInQty);
                            curOrderPerData.InOrderCount_Sum = inboundDetail.Sum(o => o.Qty);

                            string[] outboundID = db.IQueryable<T_OutBoundEntity>(o => o.State != "Over").Select(o => o.F_Id).ToArray();
                            IList<T_OutBoundDetailEntity> outboundDetail = db.IQueryable<T_OutBoundDetailEntity>(o => outboundID.Contains(o.OutBoundID)).ToList();
                            curOrderPerData.OutOrderCount = outboundDetail.Sum(o => o.OutQty);
                            curOrderPerData.OutOrderCount_Sum = outboundDetail.Sum(o => o.Qty);

                            string[] qaboundID_Get = db.IQueryable<T_QAEntity>(o => o.QAOrderType == "GetSample" && o.State != "Over").Select(o => o.F_Id).ToArray();
                            IList<T_QARecordEntity> qaDetail_Get = db.IQueryable<T_QARecordEntity>(o => qaboundID_Get.Contains(o.QAID)).ToList();

                            string[] qaboundID_Put = db.IQueryable<T_QAEntity>(o => o.QAOrderType == "BackSample" && o.State != "Over").Select(o => o.F_Id).ToArray();
                            IList<T_QARecordEntity> qaDetail_Put = db.IQueryable<T_QARecordEntity>(o => qaboundID_Put.Contains(o.QAID)).ToList();

                            curOrderPerData.QAOrderCount = qaDetail_Get.Sum(o => o.PickedQty) + qaDetail_Put.Where(o => o.IsNeedBack == "true" && o.IsReturnOver == "true").Sum(o => o.ReturnQty); //质检单明细已出数量+还样单明细已还样数量
                            curOrderPerData.QAOrderCount_Sum = qaDetail_Get.Sum(o => o.Qty) + qaDetail_Put.Where(o => o.IsNeedBack == "true").Sum(o => o.PickedQty); //质检单明细波次数量+还样单明细需还样数量

                            string[] countID = db.IQueryable<T_CountEntity>(o => o.State != "Over").Select(o => o.F_Id).ToArray();
                            IList<T_CountDetailEntity> countDetail = db.IQueryable<T_CountDetailEntity>(o => countID.Contains(o.CountID)).ToList();
                            curOrderPerData.CountOrderCount = countDetail.Sum(o => o.CountQty);
                            curOrderPerData.CountOrderCount_Sum = countDetail.Sum(o => o.Qty);

                            result.Data = curOrderPerData;
                        }
                        break;
                    default:
                        {
                            throw new Exception("未知的方法类型");
                        };
                }

                result.IsSuccess = true;
                result.FailCode = "0000";
                return result;
            }
        }
    }
}
