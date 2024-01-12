/*******************************************************************************
 * 软件名称: 崇令智能仓储管理系统
 * 技术讨论: 18718690940 (微信同号)
 * QQ交流群:  795702839
 * 最新源码：https://github.com/243480503/WMS-BS
*********************************************************************************/
using MST.Application;
using MST.Application.APIPost;
using MST.Application.SystemManage;
using MST.Application.SystemSecurity;
using MST.Application.WebMsg;
using MST.Application.WMSLogic;
using MST.Code;
using MST.Data;
using MST.Domain.Entity.SystemManage;
using MST.Domain.Entity.SystemSecurity;
using MST.Domain.Entity.WMSLogic;
using MST.Domain.ViewModel;
using MST.Web.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using System.Web.Mvc;

namespace MST.Web.Areas.PC_BaseDataManage.Controllers
{
    public class TimerTaskController : ControllerBase
    {
        private T_TimerApp timerApp = new T_TimerApp();


        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetGridJson(Pagination pagination, string keyword)
        {
            var resultList = new
            {
                rows = timerApp.GetList(pagination, keyword),
                total = pagination.total,
                page = pagination.page,
                records = pagination.records
            };

            return Content(resultList.ToJson());
        }


        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetFormJson(string keyValue)
        {
            var data = timerApp.GetForm(keyValue);
            return Content(data.ToJson());
        }

        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetTaskInfo()
        {
            IList<CustTimer> timerList = new List<CustTimer>();
            timerList.Add(FinanceTimer);
            timerList.Add(ItemLowerTimer);
            timerList.Add(OverdueTimer);
            timerList.Add(ContainerDetailPhoTimer);
            timerList.Add(DBBakTimer);
            return Content(timerList.ToJson());
        }


        #region 新增/修改任务
        [HttpPost]
        [HandlerAjaxOnly]
        [ValidateAntiForgeryToken]
        public ActionResult SubmitForm(T_TimerEntity timerEntity, string keyValue)
        {
            LogObj logObj = new LogObj();
            logObj.Path = "TimerTaskController.SubmitForm";
            logObj.Parms = new { timerEntity = timerEntity, keyValue = keyValue };

            LogEntity logEntity = new LogEntity();
            logEntity.F_ModuleName = "任务管理";
            logEntity.F_Type = DbLogType.Submit.ToString();
            logEntity.F_Account = OperatorProvider.Provider.GetCurrent().UserCode;
            logEntity.F_NickName = OperatorProvider.Provider.GetCurrent().UserName;
            logEntity.F_Description = "提交新增/修改任务";
            logEntity.F_Path = logObj.Path;
            logEntity.F_Param = logObj.Parms.ToJson();

            try
            {
                OperatorModel user = OperatorProvider.Provider.GetCurrent();
                if (!string.IsNullOrEmpty(keyValue))
                {
                    if (!user.IsSystem)
                    {
                        T_TimerEntity entity = timerApp.FindEntityAsNoTracking(o => o.F_Id == keyValue);
                        if (entity.IsBase == "true")
                        {
                            return Error("系统数据不允许修改。", "");
                        }
                    }
                }

                int isExistsCode = timerApp.FindList(o => o.TimerCode == timerEntity.TimerCode && o.F_Id != keyValue).Count();
                if (isExistsCode > 0)
                {
                    return Error("编码已存在", "");
                }

                timerEntity.IsChange = "true";
                timerApp.SubmitForm(timerEntity, keyValue);

                InitTimer(timerEntity);

                logObj.Message = "操作成功";
                LogFactory.GetLogger().Info(logObj);

                logEntity.F_Result = true;
                new LogApp().WriteDbLog(logEntity);

                return Success("操作成功。");
            }
            catch (Exception ex)
            {
                logObj.Message = ex;
                LogFactory.GetLogger().Error(logObj);

                logEntity.F_Result = false;
                logEntity.F_Msg = ex.ToJson();
                new LogApp().WriteDbLog(logEntity);

                return Error("操作失败。", ex.ToJson());
            }
        }
        #endregion

        #region 删除任务
        [HttpPost]
        [HandlerAjaxOnly]
        [HandlerAuthorize]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteForm(string keyValue)
        {
            LogObj logObj = new LogObj();
            logObj.Path = "TimerTaskController.DeleteForm";
            logObj.Parms = new { keyValue = keyValue };

            LogEntity logEntity = new LogEntity();
            logEntity.F_ModuleName = "任务管理";
            logEntity.F_Type = DbLogType.Delete.ToString();
            logEntity.F_Account = OperatorProvider.Provider.GetCurrent().UserCode;
            logEntity.F_NickName = OperatorProvider.Provider.GetCurrent().UserName;
            logEntity.F_Description = "删除任务";
            logEntity.F_Path = logObj.Path;
            logEntity.F_Param = logObj.Parms.ToJson();

            try
            {
                OperatorModel user = OperatorProvider.Provider.GetCurrent();
                if (!user.IsSystem)
                {
                    T_TimerEntity entity = timerApp.FindEntity(o => o.F_Id == keyValue);
                    if (entity.IsBase == "true")
                    {
                        return Error("系统数据不允许删除。", "");
                    }
                }

                timerApp.DeleteForm(keyValue);

                logObj.Message = "删除成功";
                LogFactory.GetLogger().Info(logObj);

                logEntity.F_Result = true;
                new LogApp().WriteDbLog(logEntity);

                return Success("删除成功。");
            }
            catch (Exception ex)
            {
                logObj.Message = ex;
                LogFactory.GetLogger().Error(logObj);

                logEntity.F_Result = false;
                logEntity.F_Msg = ex.ToJson();
                new LogApp().WriteDbLog(logEntity);

                return Error("删除失败。", ex.ToJson());
            }
        }
        #endregion

        #region 定时器逻辑
        private static CustTimer FinanceTimer = new CustTimer() { IsEventInit = false, MaxCount = Int32.MaxValue, OverCount = 0, LockObj = new object(), BeginTime = DateTime.MinValue, EndTime = DateTime.MaxValue, IsEnable = true };
        private static CustTimer ItemLowerTimer = new CustTimer() { IsEventInit = false, MaxCount = Int32.MaxValue, OverCount = 0, LockObj = new object(), BeginTime = DateTime.MinValue, EndTime = DateTime.MaxValue, IsEnable = true };
        private static CustTimer OverdueTimer = new CustTimer() { IsEventInit = false, MaxCount = Int32.MaxValue, OverCount = 0, LockObj = new object(), BeginTime = DateTime.MinValue, EndTime = DateTime.MaxValue, IsEnable = true };
        private static CustTimer ContainerDetailPhoTimer = new CustTimer() { IsEventInit = false, MaxCount = Int32.MaxValue, OverCount = 0, LockObj = new object(), BeginTime = DateTime.MinValue, EndTime = DateTime.MaxValue, IsEnable = true };
        private static CustTimer DBBakTimer = new CustTimer() { IsEventInit = false, MaxCount = Int32.MaxValue, OverCount = 0, LockObj = new object(), BeginTime = DateTime.MinValue, EndTime = DateTime.MaxValue, IsEnable = true };

        public static void Timer_Init()
        {
            using (var db = new RepositoryBase().BeginTrans())
            {
                IList<T_TimerEntity> timerList = db.FindList<T_TimerEntity>(o => o.F_DeleteMark == false);

                foreach (T_TimerEntity cell in timerList)
                {
                    InitTimer(cell);
                }
            }
        }

        private static void InitTimer(T_TimerEntity cell)
        {
            int timeGapSecond = (cell.Rate ?? 0) * 1000;
            FixType.TimerCode timerCode = (FixType.TimerCode)Enum.Parse(typeof(FixType.TimerCode), cell.TimerCode);
            switch (timerCode)
            {
                case FixType.TimerCode.Finance: //过账
                    {
                        FinanceTimer.TimerName = cell.TimerName;
                        FinanceTimer.TimerCode = cell.TimerCode;
                        FinanceTimer.Interval = timeGapSecond;
                        FinanceTimer.OverCount = cell.OverCount ?? 0;
                        FinanceTimer.MaxCount = (cell.MaxCount ?? 0) == 0 ? Int32.MaxValue : (cell.MaxCount ?? 0); //等于0，相当于不受限制
                        FinanceTimer.BeginTime = cell.BeginTime == null ? DateTime.MinValue : (DateTime)cell.BeginTime;
                        FinanceTimer.EndTime = cell.EndTime == null ? DateTime.MaxValue : (DateTime)cell.EndTime;
                        FinanceTimer.IsEnable = cell.IsEnable == "true" ? true : false;

                        if (!FinanceTimer.IsEventInit)
                        {
                            FinanceTimer.Elapsed += new ElapsedEventHandler((s, e) => Finance_Elapsed(s, e, FinanceTimer));
                            FinanceTimer.IsEventInit = true;
                        }
                        FinanceTimer.Start();
                    }
                    break;
                case FixType.TimerCode.ItemLower: //库存预警
                    {
                        ItemLowerTimer.TimerName = cell.TimerName;
                        ItemLowerTimer.TimerCode = cell.TimerCode;
                        ItemLowerTimer.Interval = timeGapSecond;
                        ItemLowerTimer.OverCount = cell.OverCount ?? 0;
                        ItemLowerTimer.MaxCount = (cell.MaxCount ?? 0) == 0 ? Int32.MaxValue : (cell.MaxCount ?? 0); //等于0，相当于不受限制
                        ItemLowerTimer.BeginTime = cell.BeginTime == null ? DateTime.MinValue : (DateTime)cell.BeginTime;
                        ItemLowerTimer.EndTime = cell.EndTime == null ? DateTime.MaxValue : (DateTime)cell.EndTime;
                        ItemLowerTimer.IsEnable = cell.IsEnable == "true" ? true : false;

                        if (!ItemLowerTimer.IsEventInit)
                        {
                            ItemLowerTimer.Elapsed += new ElapsedEventHandler((s, e) => ItemLower_Elapsed(s, e, ItemLowerTimer));
                            ItemLowerTimer.IsEventInit = true;
                        }
                        ItemLowerTimer.Start();
                    }
                    break;
                case FixType.TimerCode.Overdue://过期预警
                    {
                        OverdueTimer.TimerName = cell.TimerName;
                        OverdueTimer.TimerCode = cell.TimerCode;
                        OverdueTimer.Interval = timeGapSecond;
                        OverdueTimer.OverCount = cell.OverCount ?? 0;
                        OverdueTimer.MaxCount = (cell.MaxCount ?? 0) == 0 ? Int32.MaxValue : (cell.MaxCount ?? 0); //等于0，相当于不受限制
                        OverdueTimer.BeginTime = cell.BeginTime == null ? DateTime.MinValue : (DateTime)cell.BeginTime;
                        OverdueTimer.EndTime = cell.EndTime == null ? DateTime.MaxValue : (DateTime)cell.EndTime;
                        OverdueTimer.IsEnable = cell.IsEnable == "true" ? true : false;

                        if (!OverdueTimer.IsEventInit)
                        {
                            OverdueTimer.Elapsed += new ElapsedEventHandler((s, e) => Overdue_Elapsed(s, e, OverdueTimer));
                            OverdueTimer.IsEventInit = true;
                        }
                        OverdueTimer.Start();
                    }
                    break;
                case FixType.TimerCode.ContainerDetailPho://库存快照
                    {
                        ContainerDetailPhoTimer.TimerName = cell.TimerName;
                        ContainerDetailPhoTimer.TimerCode = cell.TimerCode;
                        ContainerDetailPhoTimer.Interval = timeGapSecond;
                        ContainerDetailPhoTimer.OverCount = cell.OverCount ?? 0;
                        ContainerDetailPhoTimer.MaxCount = (cell.MaxCount ?? 0) == 0 ? Int32.MaxValue : (cell.MaxCount ?? 0); //等于0，相当于不受限制
                        ContainerDetailPhoTimer.BeginTime = cell.BeginTime == null ? DateTime.MinValue : (DateTime)cell.BeginTime;
                        ContainerDetailPhoTimer.EndTime = cell.EndTime == null ? DateTime.MaxValue : (DateTime)cell.EndTime;
                        ContainerDetailPhoTimer.IsEnable = cell.IsEnable == "true" ? true : false;

                        if (!ContainerDetailPhoTimer.IsEventInit)
                        {
                            ContainerDetailPhoTimer.Elapsed += new ElapsedEventHandler((s, e) => ContainerDetailPho_Elapsed(s, e, ContainerDetailPhoTimer));
                            ContainerDetailPhoTimer.IsEventInit = true;
                        }
                        ContainerDetailPhoTimer.Start();
                    }
                    break;
                case FixType.TimerCode.DBBak://数据库备份
                    {
                        DBBakTimer.TimerName = cell.TimerName;
                        DBBakTimer.TimerCode = cell.TimerCode;
                        DBBakTimer.Interval = timeGapSecond;
                        DBBakTimer.OverCount = cell.OverCount ?? 0;
                        DBBakTimer.MaxCount = (cell.MaxCount ?? 0) == 0 ? Int32.MaxValue : (cell.MaxCount ?? 0); //等于0，相当于不受限制
                        DBBakTimer.BeginTime = cell.BeginTime == null ? DateTime.MinValue : (DateTime)cell.BeginTime;
                        DBBakTimer.EndTime = cell.EndTime == null ? DateTime.MaxValue : (DateTime)cell.EndTime;
                        DBBakTimer.IsEnable = cell.IsEnable == "true" ? true : false;

                        if (!DBBakTimer.IsEventInit)
                        {
                            DBBakTimer.Elapsed += new ElapsedEventHandler((s, e) => DBBak_Elapsed(s, e, DBBakTimer));
                            DBBakTimer.IsEventInit = true;
                        }
                        DBBakTimer.Start();
                    }
                    break;
                default:
                    {

                    }
                    break;
            }
        }

        #region 过账
        /// <summary>
        /// 过账
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void Finance_Elapsed(object sender, ElapsedEventArgs e, CustTimer curTimer)
        {
            lock (curTimer.LockObj)
            {
                bool isOverCount = curTimer.OverCount >= curTimer.MaxCount; //是否次数已超
                bool isNotBeginTime = curTimer.BeginTime > DateTime.Now; //是否未到达开始时间
                bool isNotEndTime = curTimer.EndTime < DateTime.Now; //是否已超结束时间

                //任意一个true，则返回
                if (isOverCount || isNotBeginTime || isNotEndTime || (!curTimer.IsEnable))
                {
                    return;
                }

                using (var db = new RepositoryBase().BeginTrans())
                {
                    UserEntity user = new UserApp().GetEntity("Sys");
                    OperatorModel operatorModel = new OperatorModel();
                    operatorModel.UserCode = user.F_Account;
                    operatorModel.UserId = user.F_Id;
                    operatorModel.UserName = user.F_RealName;

                    LogObj logObj = new LogObj();
                    logObj.Path = "MST.Web.Global";
                    LogEntity logEntity = new LogEntity();
                    logEntity.F_ModuleName = "过账";
                    logEntity.F_Type = DbLogType.Other.ToString();
                    logEntity.F_Account = operatorModel.UserCode;
                    logEntity.F_NickName = operatorModel.UserName;
                    logEntity.F_Description = "定时器执行";
                    logEntity.F_Path = logObj.Path;
                    logEntity.F_Param = logObj.Parms.ToJson();

                    try
                    {
                        IList<T_TransRecordEntity> transList = db.FindList<T_TransRecordEntity>(o => o.IsIgnore == "false" && o.MaxTransCount > o.ErrCount && (o.State == "Err" || o.State == "New"));
                        if (transList.Count > 0)
                        {
                            foreach (T_TransRecordEntity cell in transList)
                            {
                                string orderType = cell.OrderType;
                                ERPPost post = new ERPPost();
                                ERPResult erpRst = post.PostFactInOutQty(db, orderType, cell.F_Id, operatorModel);
                            }
                        }
                        T_TimerEntity timer = db.FindEntity<T_TimerEntity>(o => o.TimerCode == FixType.TimerCode.Finance.ToString());
                        timer.OverCount = (timer.OverCount ?? 0) + 1;
                        db.Update<T_TimerEntity>(timer, operatorModel);

                        db.CommitWithOutRollBack();
                        curTimer.OverCount = curTimer.OverCount + 1;
                        logObj.Message = "操作成功";
                        LogFactory.GetLogger().Info(logObj);

                        logEntity.F_Result = true;
                        new LogApp().WriteDbLogThread(logEntity, operatorModel);
                    }
                    catch (Exception ex)
                    {
                        db.RollBack();
                        logObj.Message = ex;
                        LogFactory.GetLogger().Error(logObj);

                        logEntity.F_Result = false;
                        logEntity.F_Msg = ex.ToJson();
                        new LogApp().WriteDbLogThread(logEntity, operatorModel);
                    }
                }
            }
        }
        #endregion

        #region 库存预警
        private static void ItemLower_Elapsed(object sender, ElapsedEventArgs e, CustTimer curTimer)
        {
            lock (curTimer.LockObj)
            {
                bool isOverCount = curTimer.OverCount >= curTimer.MaxCount; //是否次数已超
                bool isNotBeginTime = curTimer.BeginTime > DateTime.Now; //是否未到达开始时间
                bool isNotEndTime = curTimer.EndTime < DateTime.Now; //是否已超结束时间

                //任意一个true，则返回
                if (isOverCount || isNotBeginTime || isNotEndTime || (!curTimer.IsEnable))
                {
                    return;
                }

                using (var db = new RepositoryBase().BeginTrans())
                {
                    UserEntity user = new UserApp().GetEntity("Sys");
                    OperatorModel operatorModel = new OperatorModel();
                    operatorModel.UserCode = user.F_Account;
                    operatorModel.UserId = user.F_Id;
                    operatorModel.UserName = user.F_RealName;

                    LogObj logObj = new LogObj();
                    logObj.Path = "MST.Web.Global";
                    LogEntity logEntity = new LogEntity();
                    logEntity.F_ModuleName = "库存预警";
                    logEntity.F_Type = DbLogType.Other.ToString();
                    logEntity.F_Account = operatorModel.UserCode;
                    logEntity.F_NickName = operatorModel.UserName;
                    logEntity.F_Description = "定时器执行";
                    logEntity.F_Path = logObj.Path;
                    logEntity.F_Param = logObj.Parms.ToJson();

                    try
                    {
                        var allItemList = db.IQueryable<T_ContainerDetailEntity>(o => true)
                            .Join(db.IQueryable<T_ItemEntity>(o => o.IsBase != "true" && o.F_DeleteMark == false), m => m.ItemID, n => n.F_Id, (m, n) => new { ItemID = n.F_Id, n.ItemName, ItemCode = n.ItemCode, Lot = m.Lot, n.WarningQty, Qty = m.Qty })
                            .GroupBy(o => new { ItemID = o.ItemID, o.ItemCode, o.Lot, o.ItemName, WarningQty = o.WarningQty })
                            .Select(o => new { Item = o.Key.ItemID, ItemName = o.Key.ItemName, Lot = o.Key.Lot, ItemCode = o.Key.ItemCode, WarningQty = o.Key.WarningQty, SumQty = o.Sum(i => i.Qty) }).ToList();

                        var lowerItemList = allItemList.Where(o => o.WarningQty > o.SumQty).ToList();
                        if (lowerItemList.Count > 0)
                        {
                            ModuleButtonEntity but = db.FindEntity<ModuleButtonEntity>(o => o.F_EnCode == FixType.WarringInfo.NumWarringInfo.ToString());
                            IList<string> authIDList = db.FindList<RoleAuthorizeEntity>(o => o.F_ItemType == 2 && o.F_ItemId == but.F_Id).Select(o => o.F_ObjectId).ToArray();
                            IList<string> roleIDList = db.FindList<RoleEntity>(o => authIDList.Contains(o.F_Id)).Select(o => o.F_Id).ToArray();
                            IList<UserEntity> userList = db.FindList<UserEntity>(o => (roleIDList.Contains(o.F_RoleId) || o.F_Account == "admin") && o.F_Account != "Sys" && o.F_Account != "WCS" && o.F_Account != "ERP").Distinct().ToList();
                            foreach (UserEntity u in userList)
                            {
                                StringBuilder itemBuStr = new StringBuilder();
                                foreach (var cell in lowerItemList)
                                {
                                    itemBuStr.Append("【 物料:" + cell.ItemName + ",批号:" + cell.Lot + ",库存量/预警量:" + (int)cell.SumQty + "/" + (int)cell.WarningQty + " 】");
                                }
                                string itemStr = itemBuStr.ToString();


                                T_SendMsgEntity msgEntity = new T_SendMsgEntity();
                                msgEntity.F_Id = Guid.NewGuid().ToString();
                                msgEntity.ReceiveID = u.F_Id;
                                msgEntity.MsgType = "ItemLower";
                                msgEntity.Msg = "库存预警:" + itemStr;
                                msgEntity.SendTime = DateTime.Now;
                                msgEntity.IsReadOver = "false";
                                msgEntity.F_DeleteMark = false;
                                db.Insert<T_SendMsgEntity>(msgEntity, operatorModel);
                                db.SaveChanges();

                                int notRead = db.FindList<T_SendMsgEntity>(o => o.ReceiveID == u.F_Id && o.IsReadOver == "false").Count();
                                if (notRead > 0)
                                {
                                    new MsgHub().SendSingle(u.F_Id, MsgType.NoReadNum, new WebSocketResult() { IsSuccess = true, Data = notRead });
                                }
                            }
                        }
                        T_TimerEntity timer = db.FindEntity<T_TimerEntity>(o => o.TimerCode == FixType.TimerCode.ItemLower.ToString());
                        timer.OverCount = (timer.OverCount ?? 0) + 1;
                        db.Update<T_TimerEntity>(timer, operatorModel);
                        db.CommitWithOutRollBack();
                        curTimer.OverCount = curTimer.OverCount + 1;
                        logObj.Message = "操作成功";
                        LogFactory.GetLogger().Info(logObj);

                        logEntity.F_Result = true;
                        new LogApp().WriteDbLogThread(logEntity, operatorModel);
                    }
                    catch (Exception ex)
                    {
                        db.RollBack();
                        logObj.Message = ex;
                        LogFactory.GetLogger().Error(logObj);

                        logEntity.F_Result = false;
                        logEntity.F_Msg = ex.ToJson();
                        new LogApp().WriteDbLogThread(logEntity, operatorModel);
                    }
                }
            }
        }
        #endregion

        #region 过期
        private static void Overdue_Elapsed(object sender, ElapsedEventArgs e, CustTimer curTimer)
        {
            lock (curTimer.LockObj)
            {
                bool isOverCount = curTimer.OverCount >= curTimer.MaxCount; //是否次数已超
                bool isNotBeginTime = curTimer.BeginTime > DateTime.Now; //是否未到达开始时间
                bool isNotEndTime = curTimer.EndTime < DateTime.Now; //是否已超结束时间

                //任意一个true，则返回
                if (isOverCount || isNotBeginTime || isNotEndTime || (!curTimer.IsEnable))
                {
                    return;
                }

                using (var db = new RepositoryBase().BeginTrans())
                {
                    UserEntity user = new UserApp().GetEntity("Sys");
                    OperatorModel operatorModel = new OperatorModel();
                    operatorModel.UserCode = user.F_Account;
                    operatorModel.UserId = user.F_Id;
                    operatorModel.UserName = user.F_RealName;

                    LogObj logObj = new LogObj();
                    logObj.Path = "MST.Web.Global";
                    LogEntity logEntity = new LogEntity();
                    logEntity.F_ModuleName = "过期预警";
                    logEntity.F_Type = DbLogType.Other.ToString();
                    logEntity.F_Account = operatorModel.UserCode;
                    logEntity.F_NickName = operatorModel.UserName;
                    logEntity.F_Description = "定时器执行";
                    logEntity.F_Path = logObj.Path;
                    logEntity.F_Param = logObj.Parms.ToJson();

                    try
                    {
                        var allItemList = db.IQueryable<T_ContainerDetailEntity>(o => true).Join(db.IQueryable<T_ItemEntity>(o => o.IsBase != "true" && o.F_DeleteMark == false), m => m.ItemID, n => n.F_Id, (m, n) => new { ItemID = n.F_Id, n.ItemName, ItemCode = n.ItemCode, Lot = m.Lot, m.OverdueDate, m.ProductDate, n.ValidityWarning, n.ValidityDayNum, Qty = m.Qty })
                            .GroupBy(o => new { ItemID = o.ItemID, o.ItemCode, o.Lot, o.ItemName, OverdueDate = o.OverdueDate, ProductDate = o.ProductDate, ValidityWarning = o.ValidityWarning, ValidityDayNum = o.ValidityDayNum })
                            .Select(o => new { Item = o.Key.ItemID, ItemName = o.Key.ItemName, Lot = o.Key.Lot, ItemCode = o.Key.ItemCode, o.Key.ProductDate, ValidityWarning = o.Key.ValidityWarning, o.Key.ValidityDayNum, OverdueDate = o.Key.OverdueDate, SumQty = o.Sum(i => i.Qty) });

                        //ValidityDayNum=0 有效期天数，永不过期。  ValidityWarning=0 预警天数，0不预警
                        var overdueList = allItemList.Where(o => o.ValidityDayNum != 0 && o.ValidityWarning != 0 && o.OverdueDate != null).ToList().Where(o => o.OverdueDate <= DateTime.Now.AddDays(o.ValidityWarning ?? 0)).ToList();
                        if (overdueList.Count > 0)
                        {
                            ModuleButtonEntity but = db.FindEntity<ModuleButtonEntity>(o => o.F_EnCode == FixType.WarringInfo.OutExpWarringInfo.ToString());
                            IList<string> authIDList = db.FindList<RoleAuthorizeEntity>(o => o.F_ItemType == 2 && o.F_ItemId == but.F_Id).Select(o => o.F_ObjectId).ToArray();
                            IList<string> roleIDList = db.FindList<RoleEntity>(o => authIDList.Contains(o.F_Id)).Select(o => o.F_Id).ToArray();
                            IList<UserEntity> userList = db.FindList<UserEntity>(o => (roleIDList.Contains(o.F_RoleId) || o.F_Account == "admin") && o.F_Account != "Sys" && o.F_Account != "WCS" && o.F_Account != "ERP").Distinct().ToList();
                            foreach (UserEntity u in userList)
                            {
                                StringBuilder itemBuStr = new StringBuilder();
                                foreach (var cell in overdueList)
                                {
                                    itemBuStr.Append("【 物料:" + cell.ItemName + ",批号:" + cell.Lot + ",失效日期:" + cell.OverdueDate.Value.ToString("yyyy-MM-dd") + " 】");
                                }
                                string itemStr = itemBuStr.ToString();


                                T_SendMsgEntity msgEntity = new T_SendMsgEntity();
                                msgEntity.F_Id = Guid.NewGuid().ToString();
                                msgEntity.ReceiveID = u.F_Id;
                                msgEntity.MsgType = "Overdue";
                                msgEntity.Msg = "库存即将失效:" + itemStr;
                                msgEntity.SendTime = DateTime.Now;
                                msgEntity.IsReadOver = "false";
                                msgEntity.F_DeleteMark = false;
                                db.Insert<T_SendMsgEntity>(msgEntity, operatorModel);
                                db.SaveChanges();

                                int notRead = db.FindList<T_SendMsgEntity>(o => o.ReceiveID == u.F_Id && o.IsReadOver == "false").Count();
                                if (notRead > 0)
                                {
                                    new MsgHub().SendSingle(u.F_Id, MsgType.NoReadNum, new WebSocketResult() { IsSuccess = true, Data = notRead });
                                }
                            }
                        }

                        T_TimerEntity timer = db.FindEntity<T_TimerEntity>(o => o.TimerCode == FixType.TimerCode.Overdue.ToString());
                        timer.OverCount = (timer.OverCount ?? 0) + 1;
                        db.Update<T_TimerEntity>(timer, operatorModel);
                        db.CommitWithOutRollBack();
                        curTimer.OverCount = curTimer.OverCount + 1;
                        logObj.Message = "操作成功";
                        LogFactory.GetLogger().Info(logObj);

                        logEntity.F_Result = true;
                        new LogApp().WriteDbLogThread(logEntity, operatorModel);
                    }
                    catch (Exception ex)
                    {
                        db.RollBack();
                        logObj.Message = ex;
                        LogFactory.GetLogger().Error(logObj);

                        logEntity.F_Result = false;
                        logEntity.F_Msg = ex.ToJson();
                        new LogApp().WriteDbLogThread(logEntity, operatorModel);
                    }
                }
            }
        }
        #endregion

        #region 数据备份
        private static void DBBak_Elapsed(object sender, ElapsedEventArgs e, CustTimer curTimer)
        {
            lock (curTimer.LockObj)
            {
                bool isOverCount = curTimer.OverCount >= curTimer.MaxCount; //是否次数已超
                bool isNotBeginTime = curTimer.BeginTime > DateTime.Now; //是否未到达开始时间
                bool isNotEndTime = curTimer.EndTime < DateTime.Now; //是否已超结束时间

                //任意一个true，则返回
                if (isOverCount || isNotBeginTime || isNotEndTime || (!curTimer.IsEnable))
                {
                    return;
                }

                using (var db = new RepositoryBase().BeginTrans())
                {
                    UserEntity user = new UserApp().GetEntity("Sys");
                    OperatorModel operatorModel = new OperatorModel();
                    operatorModel.UserCode = user.F_Account;
                    operatorModel.UserId = user.F_Id;
                    operatorModel.UserName = user.F_RealName;

                    LogObj logObj = new LogObj();
                    logObj.Path = "MST.Web.Global";
                    LogEntity logEntity = new LogEntity();
                    logEntity.F_ModuleName = "数据备份";
                    logEntity.F_Type = DbLogType.Other.ToString();
                    logEntity.F_Account = operatorModel.UserCode;
                    logEntity.F_NickName = operatorModel.UserName;
                    logEntity.F_Description = "定时器执行";
                    logEntity.F_Path = logObj.Path;
                    logEntity.F_Param = logObj.Parms.ToJson();

                    try
                    {
                        DbBackupEntity dbBackupEntity = new DbBackupEntity();
                        dbBackupEntity.F_Id = Common.GuId();
                        dbBackupEntity.F_FileName = Common.CreateNo();
                        dbBackupEntity.F_FilePath = AppDomain.CurrentDomain.BaseDirectory + "Resource\\DbBackup\\" + dbBackupEntity.F_FileName + ".bak";
                        dbBackupEntity.F_FileName = dbBackupEntity.F_FileName + ".bak";
                        dbBackupEntity.F_Description = "定时器备份";
                        dbBackupEntity.F_BackupType = "1";
                        dbBackupEntity.F_EnabledMark = true;
                        dbBackupEntity.F_BackupTime = DateTime.Now;
                        dbBackupEntity.F_DbName = "WMSDB";
                        dbBackupEntity.F_CreatorUserId = operatorModel.UserId;
                        new DbBackupApp().SubmitForm(dbBackupEntity);

                        T_TimerEntity timer = db.FindEntity<T_TimerEntity>(o => o.TimerCode == FixType.TimerCode.DBBak.ToString());
                        timer.OverCount = (timer.OverCount ?? 0) + 1;
                        db.Update<T_TimerEntity>(timer, operatorModel);
                        db.CommitWithOutRollBack();
                        curTimer.OverCount = curTimer.OverCount + 1;
                        logObj.Message = "操作成功";
                        LogFactory.GetLogger().Info(logObj);

                        logEntity.F_Result = true;
                        new LogApp().WriteDbLogThread(logEntity, operatorModel);
                    }
                    catch (Exception ex)
                    {
                        db.RollBack();
                        logObj.Message = ex;
                        LogFactory.GetLogger().Error(logObj);

                        logEntity.F_Result = false;
                        logEntity.F_Msg = ex.ToJson();
                        new LogApp().WriteDbLogThread(logEntity, operatorModel);
                    }
                }
            }

        }
        #endregion

        #region 库存快照
        private static void ContainerDetailPho_Elapsed(object sender, ElapsedEventArgs e, CustTimer curTimer)
        {
            lock (curTimer.LockObj)
            {
                bool isOverCount = curTimer.OverCount >= curTimer.MaxCount; //是否次数已超
                bool isNotBeginTime = curTimer.BeginTime > DateTime.Now; //是否未到达开始时间
                bool isNotEndTime = curTimer.EndTime < DateTime.Now; //是否已超结束时间

                //任意一个true，则返回
                if (isOverCount || isNotBeginTime || isNotEndTime || (!curTimer.IsEnable))
                {
                    return;
                }
                using (var db = new RepositoryBase().BeginTrans())
                {
                    UserEntity user = new UserApp().GetEntity("Sys");
                    OperatorModel operatorModel = new OperatorModel();
                    operatorModel.UserCode = user.F_Account;
                    operatorModel.UserId = user.F_Id;
                    operatorModel.UserName = user.F_RealName;

                    LogObj logObj = new LogObj();
                    logObj.Path = "MST.Web.Global";
                    LogEntity logEntity = new LogEntity();
                    logEntity.F_ModuleName = "库存快照";
                    logEntity.F_Type = DbLogType.Other.ToString();
                    logEntity.F_Account = operatorModel.UserCode;
                    logEntity.F_NickName = operatorModel.UserName;
                    logEntity.F_Description = "定时器执行";
                    logEntity.F_Path = logObj.Path;
                    logEntity.F_Param = logObj.Parms.ToJson();

                    try
                    {
                        T_PhoApp phoHeadApp = new T_PhoApp();
                        T_PhoEntity pho = new T_PhoEntity();
                        pho.F_Id = Guid.NewGuid().ToString();
                        pho.PhoCode = T_CodeGenApp.GenNum("PhoRule", operatorModel);
                        pho.PhoTime = DateTime.Now;
                        pho.F_DeleteMark = false;
                        pho.PhoName = pho.PhoCode;
                        pho.F_CreatorTime = DateTime.Now;
                        pho.F_CreatorUserId = operatorModel.UserId;
                        pho.CreatorUserName = operatorModel.UserName;
                        new T_PhoApp().TimerCreatePhoe(db, pho, operatorModel);

                        T_TimerEntity timer = db.FindEntity<T_TimerEntity>(o => o.TimerCode == FixType.TimerCode.ContainerDetailPho.ToString());
                        timer.OverCount = (timer.OverCount ?? 0) + 1;
                        db.Update<T_TimerEntity>(timer, operatorModel);
                        db.CommitWithOutRollBack();
                        curTimer.OverCount = curTimer.OverCount + 1;
                        logObj.Message = "操作成功";
                        LogFactory.GetLogger().Info(logObj);

                        logEntity.F_Result = true;
                        new LogApp().WriteDbLogThread(logEntity, operatorModel);
                    }
                    catch (Exception ex)
                    {
                        db.RollBack();
                        logObj.Message = ex;
                        LogFactory.GetLogger().Error(logObj);

                        logEntity.F_Result = false;
                        logEntity.F_Msg = ex.ToJson();
                        new LogApp().WriteDbLogThread(logEntity, operatorModel);
                    }
                }
            }
        }
        #endregion
        #endregion
    }
}
