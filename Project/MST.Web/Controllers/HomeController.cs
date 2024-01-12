/*******************************************************************************
 * 软件名称: 崇令智能仓储管理系统
 * 技术讨论: 18718690940 (微信同号)
 * QQ交流群:  795702839
 * 最新源码：https://github.com/243480503/WMS-BS
*********************************************************************************/
using MST.Application.SystemManage;
using MST.Application.WMSLogic;
using MST.Code;
using MST.Domain.Entity.SystemManage;
using MST.Domain.Entity.WMSLogic;
using System.Collections.Generic;
using System.Text;
using System.Web.Mvc;
using System.Linq;
using MST.Application;
using MST.Application.APIPost;
using MST.Application.WebMsg;
using MST.Data;
using System;
using MST.Domain.ViewModel;
using System.Web;

namespace MST.Web.Controllers
{
    [HandlerLogin]
    public class HomeController : Controller
    {
        [HttpGet]
        public ActionResult Index()
        {
            ViewBag.SoftName = Configs.GetValue("SoftName");
            ViewBag.SoftPubName = Configs.GetValue("SoftPubName");
            ViewBag.WebSite = Configs.GetValue("WebSite");
            ViewBag.Version = Configs.GetValue("Version");
            ViewBag.CompanyName = Configs.GetValue("CompanyName");
            ViewBag.LogoPath = Configs.GetValue("LogoPath");
            ViewBag.LogoScale = Configs.GetValue("LogoScale").Split(' ')[1];
            ViewBag.WelcomePage = Configs.GetValue("WelcomePage");
            return View();
        }
        [HttpGet]
        public ActionResult Default()
        {
            return View();
        }
        [HttpGet]
        public ActionResult About()
        {
            return View();
        }

        #region 获取主页统计数据
        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetWallData()
        {
            T_InBoundApp inboundApp = new T_InBoundApp();
            T_OutBoundApp outboundAPP = new T_OutBoundApp();
            T_CountApp countApp = new T_CountApp();
            T_QAApp qaApp = new T_QAApp();
            T_InOutDetailApp inoutApp = new T_InOutDetailApp();
            T_TaskHisApp taskHisApp = new T_TaskHisApp();
            T_LocationApp locationApp = new T_LocationApp();
            T_ContainerDetailApp conDetailApp = new T_ContainerDetailApp();

            int inboundCount = inboundApp.FindList(o => true).Count();
            int outboundCount = outboundAPP.FindList(o => true).Count();
            int countCount = countApp.FindList(o => true).Count();
            int qaCount = qaApp.FindList(o => true).Count();
            int taskCount = taskHisApp.FindList(o => true).Count();


            //月累计出入库数量
            DateTime now = DateTime.Now.Date;
            DateTime OneYear = now.AddYears(-1).AddMonths(1);
            var monthList_temp = inoutApp.FindList(o => o.F_CreatorTime >= OneYear).GroupBy(o => new { Year = o.F_CreatorTime.Value.Year, Month = o.F_CreatorTime.Value.Month }).Select(o => new { Year = o.Key.Year, Month = o.Key.Month, Value = (o.Key.Month) }).OrderBy(o => o.Year).ThenBy(o => o.Month).ToList();

            var inBound_Temp = inoutApp.FindList(o => o.OrderType == "PurchaseIn" && o.F_CreatorTime >= OneYear
                                        ).GroupBy(o => new { Year = o.F_CreatorTime.Value.Year, Month = o.F_CreatorTime.Value.Month }).Select(o => new { Year = o.Key.Year, Month = o.Key.Month, SumQty = (o.Sum(i => i.ChangeQty)) }).OrderBy(o => o.Year).ThenBy(o => o.Month).ToList();

            var outBound_Temp = inoutApp.FindList(o => (o.OrderType == "GetItemOut"
                                                        || o.OrderType == "VerBackOut"
                                                        || o.OrderType == "WarehouseBackOut"
                                                        || o.OrderType == "OtherOut"
                                                        ) && o.F_CreatorTime >= OneYear
                                        ).GroupBy(o => new { Year = o.F_CreatorTime.Value.Year, Month = o.F_CreatorTime.Value.Month }).Select(o => new { Year = o.Key.Year, Month = o.Key.Month, SumQty = (o.Sum(i => i.ChangeQty)) }).OrderBy(o => o.Year).ThenBy(o => o.Month).ToList();

            var getSample_Temp = inoutApp.FindList(o => o.OrderType == "GetSample" && o.F_CreatorTime >= OneYear
                                        ).GroupBy(o => new { Year = o.F_CreatorTime.Value.Year, Month = o.F_CreatorTime.Value.Month }).Select(o => new { Year = o.Key.Year, Month = o.Key.Month, SumQty = (o.Sum(i => i.ChangeQty)) }).OrderBy(o => o.Year).ThenBy(o => o.Month).ToList();

            var backSample_Temp = inoutApp.FindList(o => o.OrderType == "BackSample" && o.F_CreatorTime >= OneYear
                                        ).GroupBy(o => new { Year = o.F_CreatorTime.Value.Year, Month = o.F_CreatorTime.Value.Month }).Select(o => new { Year = o.Key.Year, Month = o.Key.Month, SumQty = (o.Sum(i => i.ChangeQty)) }).OrderBy(o => o.Year).ThenBy(o => o.Month).ToList();

            var countIn_Temp = inoutApp.FindList(o => o.OrderType == "Count" && o.InOutType == "InType" && o.F_CreatorTime >= OneYear
                                        ).GroupBy(o => new { Year = o.F_CreatorTime.Value.Year, Month = o.F_CreatorTime.Value.Month }).Select(o => new { Year = o.Key.Year, Month = o.Key.Month, SumQty = (o.Sum(i => i.ChangeQty)) }).OrderBy(o => o.Year).ThenBy(o => o.Month).ToList();

            var countOut_Temp = inoutApp.FindList(o => o.OrderType == "Count" && o.InOutType == "OutType" && o.F_CreatorTime >= OneYear
                                        ).GroupBy(o => new { Year = o.F_CreatorTime.Value.Year, Month = o.F_CreatorTime.Value.Month }).Select(o => new { Year = o.Key.Year, Month = o.Key.Month, SumQty = (o.Sum(i => i.ChangeQty)) }).OrderBy(o => o.Year).ThenBy(o => o.Month).ToList();

            for (DateTime tempT = OneYear; tempT <= now; tempT = tempT.AddMonths(1))
            {
                if (!monthList_temp.Where(o => o.Month == tempT.Month && o.Year == tempT.Year).Any())
                {
                    monthList_temp.Add(new { Year = tempT.Year, Month = tempT.Month, Value = 0 });
                }
                if (!inBound_Temp.Where(o => o.Month == tempT.Month && o.Year == tempT.Year).Any())
                {
                    inBound_Temp.Add(new { Year = tempT.Year, Month = tempT.Month, SumQty = (decimal?)0 });
                }
                if (!outBound_Temp.Where(o => o.Month == tempT.Month && o.Year == tempT.Year).Any())
                {
                    outBound_Temp.Add(new { Year = tempT.Year, Month = tempT.Month, SumQty = (decimal?)0 });
                }
                if (!getSample_Temp.Where(o => o.Month == tempT.Month && o.Year == tempT.Year).Any())
                {
                    getSample_Temp.Add(new { Year = tempT.Year, Month = tempT.Month, SumQty = (decimal?)0 });
                }
                if (!backSample_Temp.Where(o => o.Month == tempT.Month && o.Year == tempT.Year).Any())
                {
                    backSample_Temp.Add(new { Year = tempT.Year, Month = tempT.Month, SumQty = (decimal?)0 });
                }
                if (!countIn_Temp.Where(o => o.Month == tempT.Month && o.Year == tempT.Year).Any())
                {
                    countIn_Temp.Add(new { Year = tempT.Year, Month = tempT.Month, SumQty = (decimal?)0 });
                }
                if (!countOut_Temp.Where(o => o.Month == tempT.Month && o.Year == tempT.Year).Any())
                {
                    countOut_Temp.Add(new { Year = tempT.Year, Month = tempT.Month, SumQty = (decimal?)0 });
                }
            }

            var monthList = monthList_temp.OrderBy(o => o.Year).ThenBy(o => o.Month).ToList();
            var inBound = inBound_Temp.OrderBy(o => o.Year).ThenBy(o => o.Month).ToList();
            var outBound = outBound_Temp.OrderBy(o => o.Year).ThenBy(o => o.Month).ToList();
            var getSample = getSample_Temp.OrderBy(o => o.Year).ThenBy(o => o.Month).ToList();
            var backSample = backSample_Temp.OrderBy(o => o.Year).ThenBy(o => o.Month).ToList();
            var countIn = countIn_Temp.OrderBy(o => o.Year).ThenBy(o => o.Month).ToList();
            var countOut = countOut_Temp.OrderBy(o => o.Year).ThenBy(o => o.Month).ToList();


            //历史任务统计
            int taskHis_in = taskHisApp.FindList(o => o.OrderType == "PurchaseIn").Count();
            int taskHis_out = taskHisApp.FindList(o => o.OrderType == "GetItemOut"
                                                                                 || o.OrderType == "VerBackOut"
                                                                                 || o.OrderType == "WarehouseBackOut"
                                                                                 || o.OrderType == "OtherOut").Count();

            int taskHis_qa = taskHisApp.FindList(o => o.OrderType == "GetSample" || o.OrderType == "BackSample").Count();
            int taskHis_count = taskHisApp.FindList(o => o.OrderType == "Count").Count();

            //货位数量统计

            //int emptyBigCount = locationApp.FindList(o => o.AreaCode == FixType.Area.BigItemArea.ToString() && o.State == "Empty").Count();
            //var locQuery = locationApp.FindList(o => o.AreaCode == FixType.Area.BigItemArea.ToString() && o);
            T_ItemKindEntity sysItem = new T_ItemKindApp().FindEntity(o => o.IsBase == "true");
            var emptyBigCount = conDetailApp.FindList(o => o.AreaCode == FixType.Area.BigItemArea.ToString() && o.KindCode == sysItem.KindCode).GroupBy(o => o.BarCode).Count(); //大货架，系统物料(指空料架)

            //int notEmptyBigCount = locationApp.FindList(o => o.AreaCode == FixType.Area.BigItemArea.ToString() && o.State != "Empty").Count();
            int notEmptyBigCount = conDetailApp.FindList(o => o.AreaCode == FixType.Area.BigItemArea.ToString() && o.KindCode != sysItem.KindCode).GroupBy(o => o.BarCode).Count(); //大货架，非系统物料(指非空料架)

            int emptyCount = locationApp.FindList(o => o.AreaCode == FixType.Area.NormalArea.ToString() && o.High == 400 && o.State == "Empty").Count();
            int notEmptyCount = locationApp.FindList(o => o.AreaCode == FixType.Area.NormalArea.ToString() && o.High == 400 && o.State != "Empty").Count();
            int emptySmallCount = locationApp.FindList(o => o.AreaCode == FixType.Area.NormalArea.ToString() && o.High == 350 && o.State == "Empty").Count();
            int notEmptySmallCount = locationApp.FindList(o => o.AreaCode == FixType.Area.NormalArea.ToString() && o.High == 350 && o.State != "Empty").Count();

            var data = new
            {
                InBoundCount = inboundCount,
                OutBoundCount = outboundCount,
                CountCount = countCount,
                QACount = qaCount,
                TaskCount = taskCount,
                MonthInOut = new { InBound = inBound, OutBound = outBound, GetSample = getSample, BackSample = backSample, CountIn = countIn, CountOut = countOut, Month = monthList },
                TaskHis = new { TaskHisIn = taskHis_in, TaskHisOut = taskHis_out, TaskHisQA = taskHis_qa, TaskHisCount = taskHis_count },
                LocInfo = new { EmptyBigCount = emptyBigCount, NotEmptyBigCount = notEmptyBigCount, EmptyCount = emptyCount, NotEmptyCount = notEmptyCount, EmptySmallCount = emptySmallCount, NotEmptySmallCount = notEmptySmallCount }
            };
            return Content(data.ToJson());
        }
        #endregion

        #region 获取主页列表数据

        private class OrderListData
        {
            /// <summary>
            /// 单据类型
            /// </summary>
            public string OrderTypeName { get; set; }

            /// <summary>
            /// 来源单据
            /// </summary>
            public string RefOrderCode { get; set; }

            /// <summary>
            /// 单据编码
            /// </summary>
            public string OrderCode { get; set; }

            /// <summary>
            /// 单据状态名称
            /// </summary>
            public string StateName { get; set; }

            /// <summary>
            /// 单据状态
            /// </summary>
            public string State { get; set; }

            /// <summary>
            /// 单据创建时间
            /// </summary>
            public DateTime? CreateDate { get; set; }
        }
        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetListData()
        {
            T_InBoundApp inboundApp = new T_InBoundApp();
            T_OutBoundApp outboundAPP = new T_OutBoundApp();
            T_CountApp countApp = new T_CountApp();
            T_QAApp qaApp = new T_QAApp();
            T_TaskApp taskApp = new T_TaskApp();
            T_SendMsgApp sendMsgApp = new T_SendMsgApp();
            T_InOutDetailApp inoutApp = new T_InOutDetailApp();
            T_TaskHisApp taskHisApp = new T_TaskHisApp();
            ItemsDetailApp itemsApp = new ItemsDetailApp();

            //入库单
            IList<OrderListData> inBoundList = new T_InBoundApp().FindList(o => o.F_DeleteMark == false).Select(o => new OrderListData { OrderCode = o.InBoundCode, RefOrderCode = o.RefOrderCode, State = o.State, CreateDate = o.F_CreatorTime, OrderTypeName = "入库单" }).ToList();
            IList<ItemsDetailEntity> inBoundStateList = itemsApp.FindEnum<T_InBoundEntity>(o => o.State);
            foreach (OrderListData cell in inBoundList)
            {
                cell.StateName = inBoundStateList.FirstOrDefault(o => o.F_ItemCode == cell.State).F_ItemName;
            }

            //出库单
            IList<OrderListData> outBoundList = new T_OutBoundApp().FindList(o => o.F_DeleteMark == false).Select(o => new OrderListData { OrderCode = o.OutBoundCode, RefOrderCode = o.RefOrderCode, State = o.State, CreateDate = o.F_CreatorTime, OrderTypeName = "出库单" }).ToList();
            IList<ItemsDetailEntity> outBoundStateList = itemsApp.FindEnum<T_OutBoundEntity>(o => o.State);
            foreach (OrderListData cell in outBoundList)
            {
                cell.StateName = outBoundStateList.FirstOrDefault(o => o.F_ItemCode == cell.State).F_ItemName;
            }

            //质检取样单
            IList<OrderListData> qaList = new T_QAApp().FindList(o => o.F_DeleteMark == false && o.QAOrderType == "GetSample").Select(o => new OrderListData { OrderCode = o.QACode, RefOrderCode = o.RefOrderCode, State = o.State, CreateDate = o.F_CreatorTime, OrderTypeName = "质检取样单" }).ToList();
            IList<ItemsDetailEntity> qaStateList = itemsApp.FindEnum<T_QAEntity>(o => o.State);
            foreach (OrderListData cell in qaList)
            {
                cell.StateName = qaStateList.FirstOrDefault(o => o.F_ItemCode == cell.State).F_ItemName;
            }

            //质检还样单
            IList<OrderListData> qaReturnList = new T_QAApp().FindList(o => o.F_DeleteMark == false && o.QAOrderType == "BackSample").Select(o => new OrderListData { OrderCode = o.QACode, RefOrderCode = o.RefOrderCode, State = o.State, CreateDate = o.F_CreatorTime, OrderTypeName = "质检还样单" }).ToList();
            IList<ItemsDetailEntity> qaReturnStateList = itemsApp.FindEnum<T_QAEntity>(o => o.State);
            foreach (OrderListData cell in qaReturnList)
            {
                cell.StateName = qaStateList.FirstOrDefault(o => o.F_ItemCode == cell.State).F_ItemName;
            }

            //盘点单
            IList<OrderListData> countList = new T_CountApp().FindList(o => o.F_DeleteMark == false).Select(o => new OrderListData { OrderCode = o.CountCode, RefOrderCode = o.RefOrderCode, State = o.State, CreateDate = o.F_CreatorTime, OrderTypeName = "盘点单" }).ToList();
            IList<ItemsDetailEntity> countStateList = itemsApp.FindEnum<T_CountEntity>(o => o.State);
            foreach (OrderListData cell in countList)
            {
                cell.StateName = countStateList.FirstOrDefault(o => o.F_ItemCode == cell.State).F_ItemName;
            }

            //移库单
            IList<OrderListData> moveList = new T_MoveApp().FindList(o => o.F_DeleteMark == false).Select(o => new OrderListData { OrderCode = o.MoveCode, RefOrderCode = o.MoveCode, State = o.State, CreateDate = o.F_CreatorTime, OrderTypeName = "移库单" }).ToList();
            IList<ItemsDetailEntity> moveStateList = itemsApp.FindEnum<T_MoveEntity>(o => o.State);
            foreach (OrderListData cell in moveList)
            {
                cell.StateName = moveStateList.FirstOrDefault(o => o.F_ItemCode == cell.State).F_ItemName;
            }


            //合并单
            List<OrderListData> orderAllList = new List<OrderListData>();
            orderAllList.AddRange(inBoundList);
            orderAllList.AddRange(outBoundList);
            orderAllList.AddRange(qaList);
            orderAllList.AddRange(qaReturnList);
            orderAllList.AddRange(countList);
            orderAllList.AddRange(moveList);

            orderAllList = orderAllList.OrderByDescending(o => o.CreateDate).Take(8).ToList();

            //任务
            IList<TaskModel> taskList = new List<TaskModel>();
            IList<T_TaskEntity> taskEntityList = taskApp.FindList(o => true).OrderByDescending(o => o.F_CreatorTime).Take(8).ToList();
            IList<ItemsDetailEntity> taskStateList = itemsApp.FindEnum<T_TaskEntity>(o => o.State);
            foreach (T_TaskEntity cell in taskEntityList)
            {
                TaskModel taskModel = cell.ToObject<TaskModel>();
                taskModel.StateName = taskStateList.FirstOrDefault(o => o.F_ItemCode == cell.State).F_ItemName;
                taskList.Add(taskModel);
            }

            //推送的消息
            OperatorModel user = OperatorProvider.Provider.GetCurrent();
            List<T_SendMsgEntity> msgList = sendMsgApp.FindList(o => o.ReceiveID == user.UserId).OrderByDescending(o => o.F_CreatorTime).Take(8).ToList();


            var data = new
            {
                orderAllList = orderAllList,
                taskList = taskList,
                msgList = msgList
            };
            return Content(data.ToJson());
        }
        #endregion


        #region 获取消息个数
        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetMsgCount()
        {
            OperatorModel user = OperatorProvider.Provider.GetCurrent();
            IList<T_SendMsgEntity> msgList = new T_SendMsgApp().FindList(o => o.ReceiveID == user.UserId && o.IsReadOver == "false").ToList();
            int msgCount = msgList.Count();
            var data = new { MsgCount = msgCount };
            return Content(data.ToJson());
        }
        #endregion

        #region 入库货位分配测试
        /// <summary>
        /// 入库货位分配，只用于前端测试
        /// </summary>
        /// <param name="num">需要分配货位的数量</param>
        /// <param name="areaPrefix">区域前缀(N,B,Z)</param>
        /// <param name="isEmpty">是否空料箱(空料箱分配货位需结婚是否ERP物理区域，若非ERP物理区域未启用，则优先分配到右侧货位)</param>
        /// <param name="erpCode">ERP物理区域启用的情况下必填，否则可为空</param>
        /// <param name="containertype">容器类型编码(非大类编码)</param>
        /// <returns></returns>
        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult LocInPar(string num, string areaPrefix, bool isEmpty, string erpCode, string containertype, string checkPyType, string itemCode)
        {
            int count = 0;
            try
            {
                count = Convert.ToInt32(num);
            }
            catch
            {
                var data = new { success = false, msg = "数字转换失败" };
                return Content(data.ToJson());
            }

            if (string.IsNullOrEmpty(areaPrefix) || (areaPrefix != "N" && areaPrefix != "B" && areaPrefix != "Z"))
            {
                var data = new { success = false, msg = "LocIn(num,areaCode)区域参数不正确（N：一般存储区，B：大件存储区，Z：空料箱暂存区）" };
                return Content(data.ToJson());
            }

            T_AreaApp areaApp = new T_AreaApp();
            T_LocationApp locApp = new T_LocationApp();
            T_DevRowApp devRowApp = new T_DevRowApp();
            T_RowLineApp rowLineApp = new T_RowLineApp();

            T_AreaEntity areaEntity = new T_AreaEntity();

            switch (areaPrefix)
            {
                case "N":
                    {
                        areaEntity = areaApp.FindEntity(o => o.AreaCode == FixType.Area.NormalArea.ToString());
                    }
                    break;
                case "B":
                    {
                        areaEntity = areaApp.FindEntity(o => o.AreaCode == FixType.Area.BigItemArea.ToString());
                    }
                    break;
                case "Z":
                    {
                        areaEntity = areaApp.FindEntity(o => o.AreaCode == FixType.Area.EmptyArea.ToString());
                    }
                    break;
                default:
                    {
                        var data = new { success = false, msg = "未知区域编码" };
                        return Content(data.ToJson());
                    }
            }

            List<object> dic = new List<object>();
            for (int i = 0; i < count; i++)
            {
                using (var db = new RepositoryBase().BeginTrans())
                {
                    string msg = "";
                    T_ContainerTypeEntity containerType = db.FindEntity<T_ContainerTypeEntity>(o => o.ContainerTypeCode == containertype);

                    LogObj log = null;
                    T_ItemEntity item = db.FindEntity<T_ItemEntity>(o => o.ItemCode == itemCode);
                    T_LocationEntity loc = locApp.GetLocIn(ref msg, ref log, db, containerType, areaEntity.F_Id, isEmpty, erpCode, checkPyType, false, null, item);
                    LocationModel locModel = loc.ToObject<LocationModel>();
                    if (loc == null)
                    {
                        var data = new { success = false, msg = "获取失败：" + msg };
                        return Content(data.ToJson());
                    }
                    else
                    {
                        T_RowLineEntity lineEntity = rowLineApp.FindEntity(o => o.Line == loc.Line);
                        T_DevRowEntity devRow = devRowApp.FindEntity(o => o.F_Id == lineEntity.DevRowID);
                        locModel.WayCode = devRow.WayCode;

                        object obj = new { Loc = locModel, conType = containerType, log = log.Message };
                        dic.Add(obj);
                    }
                    db.Commit();
                }
            }
            var successdata = new { success = true, msg = "", List = dic };
            return Content(successdata.ToJson());
        }


        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult LocInTask(string num, string taskInNo, string ERPCode, string checkPyType, string itemCode)
        {
            int count = 0;
            try
            {
                count = Convert.ToInt32(num);
            }
            catch
            {
                var data = new { success = false, msg = "数字转换失败" };
                return Content(data.ToJson());
            }

            T_LocationApp locApp = new T_LocationApp();
            T_DevRowApp devRowApp = new T_DevRowApp();
            T_RowLineApp rowLineApp = new T_RowLineApp();


            List<object> dic = new List<object>();
            for (int i = 0; i < count; i++)
            {
                using (var db = new RepositoryBase().BeginTrans())
                {
                    string msg = "";
                    T_TaskEntity task = db.FindEntity<T_TaskEntity>(o => o.TaskNo == taskInNo);
                    if (task == null)
                    {
                        var data = new { success = false, msg = "任务不存在" };
                        return Content(data.ToJson());
                    }
                    if (task.TaskInOutType != "InType")
                    {
                        var data = new { success = false, msg = "任务非入库类型" };
                        return Content(data.ToJson());
                    }

                    T_ContainerTypeEntity containerType = db.FindEntity<T_ContainerTypeEntity>(o => o.ContainerTypeCode == task.ContainerType);
                    bool isEmptyIn = task.TaskType == "TaskType_EmptyIn";
                    LogObj log = null;
                    T_ItemEntity item = db.FindEntity<T_ItemEntity>(o => o.ItemCode == itemCode);
                    T_LocationEntity loc = locApp.GetLocIn(ref msg, ref log, db, containerType, task.TagAreaID, isEmptyIn, ERPCode, checkPyType, false, null, item);
                    LocationModel locModel = loc.ToObject<LocationModel>();
                    if (loc == null)
                    {
                        var data = new { success = false, msg = "获取失败：" + msg };
                        return Content(data.ToJson());
                    }
                    else
                    {
                        T_RowLineEntity lineEntity = rowLineApp.FindEntity(o => o.Line == loc.Line);
                        T_DevRowEntity devRow = devRowApp.FindEntity(o => o.F_Id == lineEntity.DevRowID);
                        locModel.WayCode = devRow.WayCode;

                        object obj = new { Loc = locModel, conType = containerType, log = log.Message };
                        dic.Add(obj);
                    }
                    db.Commit();
                }
            }
            var successdata = new { success = true, msg = "", List = dic };
            return Content(successdata.ToJson());
        }
        #endregion
    }
}
