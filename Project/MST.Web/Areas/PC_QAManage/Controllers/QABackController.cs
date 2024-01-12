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
using MST.Application.WMSLogic;
using MST.Code;
using MST.Data;
using MST.Domain.Entity.SystemManage;
using MST.Domain.Entity.SystemSecurity;
using MST.Domain.Entity.WMSLogic;
using MST.Domain.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace MST.Web.Areas.PC_QAManage.Controllers
{
    public class QABackController : ControllerBase
    {
        private static object lockObj = new object();
        private T_QAApp qAApp = new T_QAApp();
        private T_QADetailApp qADetailApp = new T_QADetailApp();
        private T_QARecordApp qARecordApp = new T_QARecordApp();
        private T_QAResultApp qAResultApp = new T_QAResultApp();
        private ItemsDetailApp itemsDetailApp = new ItemsDetailApp();
        private T_ContainerDetailApp containerDetailApp = new T_ContainerDetailApp();
        private T_ItemApp itemApp = new T_ItemApp();
        private T_ContainerTypeApp containerTypeApp = new T_ContainerTypeApp();
        private T_StationApp stationApp = new T_StationApp();

        #region 获取质检还样单列表
        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetGridJson(Pagination pagination, string queryJson)
        {
            string qAOrderType = "BackSample";
            List<T_QAEntity> data = qAApp.GetList(pagination, qAOrderType, queryJson);

            List<QAModel> qaModelList = new List<QAModel>();
            IList<ItemsDetailEntity> enumStateList = itemsDetailApp.FindEnum<T_QAEntity>(o => o.State).ToList();
            IList<ItemsDetailEntity> enumOrderTypeList = itemsDetailApp.FindEnum<T_QAEntity>(o => o.QAOrderType).ToList();
            IList<ItemsDetailEntity> enumGenTypeList = itemsDetailApp.FindEnum<T_QAEntity>(o => o.GenType).ToList();

            foreach (T_QAEntity item in data)
            {
                QAModel model = item.ToObject<QAModel>();
                model.StateName = enumStateList.FirstOrDefault(o => o.F_ItemCode == item.State).F_ItemName;
                model.QAOrderTypeName = enumOrderTypeList.FirstOrDefault(o => o.F_ItemCode == item.QAOrderType).F_ItemName;
                model.GenTypeName = enumGenTypeList.FirstOrDefault(o => o.F_ItemCode == item.GenType).F_ItemName;
                qaModelList.Add(model);
            }

            var resultList = new
            {
                rows = qaModelList,
                total = pagination.total,
                page = pagination.page,
                records = pagination.records
            };
            return Content(resultList.ToJson());
        }
        #endregion

        #region 质检还样单详情
        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetFormJson(string keyValue)
        {
            var data = qAApp.GetForm(keyValue);
            QAModel model = data.ToObject<QAModel>();
            model.StateName = itemsDetailApp.FindEnum<T_QAEntity>(o => o.State).FirstOrDefault(o => o.F_ItemCode == model.State).F_ItemName;
            return Content(model.ToJson());
        }
        #endregion

        #region 开始质检还样
        [HttpPost]
        [HandlerAjaxOnly]
        [ValidateAntiForgeryToken]
        public ActionResult BeginReturn(string QAID)
        {
            using (var db = new RepositoryBase().BeginTrans())
            {
                LogObj logObj = new LogObj();
                logObj.Path = "QABackController.BeginReturn";
                logObj.Parms = new { QAID = QAID };

                LogEntity logEntity = new LogEntity();
                logEntity.F_ModuleName = "质检还样单";
                logEntity.F_Type = DbLogType.Submit.ToString();
                logEntity.F_Account = OperatorProvider.Provider.GetCurrent().UserCode;
                logEntity.F_NickName = OperatorProvider.Provider.GetCurrent().UserName;
                logEntity.F_Description = "开始质检还样";
                logEntity.F_Path = logObj.Path;
                logEntity.F_Param = logObj.Parms.ToJson();

                try
                {
                    lock (lockObj)
                    {
                        /*************************************************/
                        T_OffLineDownApp offLineDownApp = new T_OffLineDownApp();
                        bool IsHaveOffLine = offLineDownApp.IsHaveOff(db);
                        if (IsHaveOffLine)
                        {
                            return Error("存在未处理的离线数据","");
                        }

                        T_QAEntity qABack = db.FindEntity<T_QAEntity>(o => o.F_Id == QAID);
                        T_QAEntity qAGet = db.FindEntity<T_QAEntity>(o => o.QACode == qABack.RefOrderCode && o.QAOrderType == "GetSample");

                        if (qABack == null) return Error("还样单据不存在", "");
                        if (qABack.State == "Over") return Error("单据已结束还样", "");
                        if (qABack.State == "Returning") return Error("单据已是还样状态", "");
                        if (!(qAGet.State == "Over" || qAGet.State == "Picked" || qAGet.State == "WaitResult")) return Error("取样单未完成取样", "");

                        List<T_QARecordEntity> recordListAll = db.FindList<T_QARecordEntity>(o => o.QAID == qAGet.F_Id && o.IsNeedBack == "true" && o.IsReturnOver == "false").ToList();
                        if (recordListAll.Count == 0) return Error("没有需要还样的记录", "");

                        /// 获取还样站台列表
                        var stationList = qADetailApp.FindList(o => o.QAID == qABack.F_Id).GroupBy(x => new { x.StationID }).Select(a => a.FirstOrDefault()).ToList();
                        foreach (var s in stationList)
                        {
                            if (string.IsNullOrEmpty(s.StationID)) continue;
                            T_StationEntity station = db.FindEntity<T_StationEntity>(o => o.F_Id == s.StationID);
                            if (station == null) return Error($"未找到质检还样站台 {station.StationName}", "");
                            if (!string.IsNullOrEmpty(station.CurOrderID)) return Error($"站台 {station.StationName} 已绑定单据", "");
                            if (station.CurOrderID == qABack.F_Id) return Error("站台已绑定该单据", "");

                            station.CurOrderID = qABack.F_Id;
                            station.OrderType = "BackSample";
                            db.Update<T_StationEntity>(station);
                        }

                        List<T_QADetailEntity> qADetailBackList = qADetailApp.FindList(o => o.QAID == qABack.F_Id).ToList();

                        //List<string> taskNoList = new List<string>();
                        Dictionary<string, T_TaskEntity> dictTask = new Dictionary<string, T_TaskEntity>();
                        foreach (T_QADetailEntity detailBack in qADetailBackList)
                        {
                            T_StationEntity tagStation = db.FindEntity<T_StationEntity>(o => o.F_Id == detailBack.StationID);

                            var detailGet = db.FindEntity<T_QADetailEntity>(o => o.QAID == qAGet.F_Id && o.ItemID == detailBack.ItemID && o.Lot == detailBack.Lot);
                            var recordList = recordListAll.Where(o => o.QADetailID == detailGet.F_Id).ToList();
                            foreach (var record in recordList)
                            {
                                /// 更新质检记录
                                record.State = "WaitReturn";
                                db.Update<T_QARecordEntity>(record);

                                T_ContainerEntity container = db.FindEntity<T_ContainerEntity>(o => o.BarCode == record.BarCode && o.F_DeleteMark == false);
                                T_LocationEntity srcLocation = db.FindEntity<T_LocationEntity>(o => o.F_Id == container.LocationID);

                                if (srcLocation.LocationCode == FixType.Station.StationOut_BigItem.ToString() || srcLocation.LocationCode == FixType.Station.StationOut_Normal.ToString())
                                {
                                    return Error($"容器未入库 { container.BarCode }", "");
                                }

                                /// 生成还样出库任务
                                T_TaskEntity task = new T_TaskEntity();
                                task.F_Id = Guid.NewGuid().ToString();
                                task.TaskNo = T_CodeGenApp.GenNum("TaskRule");
                                task.TaskInOutType = "OutType";
                                task.TaskType = "TaskType_CheckReturnOut";
                                task.ContainerID = container.F_Id;
                                task.BarCode = container.BarCode;
                                task.ContainerType = container.ContainerType;
                                task.SrcLocationID = srcLocation.F_Id;  /// 起始地址ID
                                task.SrcLocationCode = srcLocation.LocationCode;    /// 起始地址编码
                                task.SrcWCSLocCode = srcLocation.WCSLocCode;
                                task.TagAreaID = "";
                                task.TagLocationID = tagStation.F_Id;    /// 目标地址ID
                                task.TagLocationCode = tagStation.TagAddress;    /// 目标地址编码
                                task.TagWCSLocCode = tagStation.TagAddress;
                                task.ApplyStationID = tagStation.F_Id;
                                task.WaveID = "";
                                task.WaveCode = "";
                                task.SEQ = detailBack.SEQ;
                                task.Level = 30; /// 质检
                                task.State = "New";
                                task.IsWcsTask = "true";
                                task.IsCanExec = "true";
                                task.SendWCSTime = null;
                                task.OverTime = null;
                                task.OrderType = "BackSample";
                                task.OrderID = qABack.F_Id;
                                task.OrderDetailID = detailBack.F_Id;
                                task.OrderCode = qABack.QACode;
                                task.F_DeleteMark = false;

                                if (!dictTask.ContainsKey(task.SrcLocationID + task.BarCode))
                                {
                                    dictTask.Add(task.SrcLocationID + task.BarCode, task);
                                    /// 锁定货位 不同批号不在同一货位
                                    srcLocation.State = "Out";
                                    db.Update<T_LocationEntity>(srcLocation);
                                }
                            }

                            /// 更新还样明细状态
                            detailBack.State = "WaitReturn";
                            db.Update<T_QADetailEntity>(detailBack);
                        }

                        IList<string> TaskListNo = new List<string>();
                        foreach (var item in dictTask)
                        {
                            db.Insert<T_TaskEntity>(item.Value);
                            TaskListNo.Add(item.Value.TaskNo);
                        }

                        /// 更新还样单状态
                        qABack.State = "Returning";
                        db.Update<T_QAEntity>(qABack);
                        db.SaveChanges();

                        /// 推送WCS任务
                        WCSResult wcsRes = new WCSPost().SendTask(db, TaskListNo);
                        if (wcsRes.IsSuccess)
                        {
                            db.CommitWithOutRollBack();
                            logObj.Message = "操作成功";
                            LogFactory.GetLogger().Info(logObj);

                            logEntity.F_Result = true;
                            new LogApp().WriteDbLog(logEntity);

                            return Success("操作成功。");
                        }
                        else
                        {
                            db.RollBack();
                            throw new Exception(wcsRes.FailMsg);
                        }
                    }
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
        }
        #endregion

        #region 删除质检还样单
        [HttpPost]
        [HandlerAjaxOnly]
        [HandlerAuthorize]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteForm(string keyValue)
        {
            LogObj logObj = new LogObj();
            logObj.Path = "QABackController.DeleteForm";
            logObj.Parms = new { keyValue = keyValue };

            LogEntity logEntity = new LogEntity();
            logEntity.F_ModuleName = "质检取样单";
            logEntity.F_Type = DbLogType.Delete.ToString();
            logEntity.F_Account = OperatorProvider.Provider.GetCurrent().UserCode;
            logEntity.F_NickName = OperatorProvider.Provider.GetCurrent().UserName;
            logEntity.F_Description = "删除质检取样单";
            logEntity.F_Path = logObj.Path;
            logEntity.F_Param = logObj.Parms.ToJson();

            try
            {
                qAApp.DeleteForm(keyValue);

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

        #region 手动过账
        [HttpPost]
        [HandlerAjaxOnly]
        [HandlerAuthorize]
        [ValidateAntiForgeryToken]
        public ActionResult TransByHand(string keyValue)
        {
            using (var db = new RepositoryBase().BeginTrans())
            {
                LogObj logObj = new LogObj();
                logObj.Path = "QABackController.TransByHand";
                logObj.Parms = new { keyValue = keyValue };

                LogEntity logEntity = new LogEntity();
                logEntity.F_ModuleName = "质检还样单";
                logEntity.F_Type = DbLogType.Delete.ToString();
                logEntity.F_Account = OperatorProvider.Provider.GetCurrent().UserCode;
                logEntity.F_NickName = OperatorProvider.Provider.GetCurrent().UserName;
                logEntity.F_Description = "手动过账";
                logEntity.F_Path = logObj.Path;
                logEntity.F_Param = logObj.Parms.ToJson();

                try
                {
                    T_QAEntity qAEntity = db.FindEntity<T_QAEntity>(o => o.F_Id == keyValue);
                    if (qAEntity.State != "Over" && qAEntity.State != "WaitResult") return Error("单据必须还样结束", "");
                    if (qAEntity.TransState == "OverTrans" || qAEntity.TransState == "UnNeedTrans") return Error("当前单据已过账或免过账", "");

                    string qaOrderType = qAEntity.QAOrderType;
                    string orderType;

                    switch (qaOrderType)
                    {
                        case "BackSample":
                            {
                                orderType = "BackSample";
                            }
                            break;
                        case "GetSample":
                            {
                                orderType = "GetSample";
                            }
                            break;
                        default:
                            {
                                return Error("单据类型未知。", "");
                            }
                    }

                    AjaxResult rst = new T_TransRecordApp().GenTransRecord(db, qAEntity.F_Id, orderType);
                    if ((ResultType)rst.state == ResultType.success)
                    {
                        qAEntity.TransState = "OverTrans";
                        db.Update<T_QAEntity>(qAEntity);

                        T_TransRecordEntity trans = (T_TransRecordEntity)rst.data;
                        ERPPost post = new ERPPost();
                        ERPResult erpRst = post.PostFactInOutQty(db, orderType, trans.F_Id);
                        db.CommitWithOutRollBack();
                        if (!erpRst.IsSuccess)
                        {
                            return Error(erpRst.FailMsg, "");
                        }
                    }
                    else return Error(rst.message, "");

                    logObj.Message = "操作成功";
                    LogFactory.GetLogger().Info(logObj);

                    logEntity.F_Result = true;
                    new LogApp().WriteDbLog(logEntity);

                    return Success("操作成功。");
                }
                catch (Exception ex)
                {
                    db.RollBack();
                    logObj.Message = ex;
                    LogFactory.GetLogger().Error(logObj);

                    logEntity.F_Result = false;
                    logEntity.F_Msg = ex.ToJson();
                    new LogApp().WriteDbLog(logEntity);

                    return Error("操作失败。", ex.ToJson());
                }
            }
        }
        #endregion

        #region 展开质检明细
        [HttpGet]
        [HandlerAuthorize]
        public virtual ActionResult QADetails()
        {
            return View();
        }
        #endregion

        #region 获取质检还样单明细
        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetGridJsonDetails(Pagination pagination, string QAID, string keyword)
        {
            T_QAEntity qABack = qAApp.FindEntity(o => o.F_Id == QAID);
            T_QAEntity qAGet = qAApp.FindEntity(o => o.QACode == qABack.RefOrderCode && o.QAOrderType == "GetSample");
            if (qAGet == null) return Error("未找到对应取样单", "");

            IList<T_QADetailEntity> detailList = qADetailApp.GetList(pagination, QAID, keyword);

            IList<QADetailModel> qaDetailModelList = detailList.ToObject<IList<QADetailModel>>();
            IList<ItemsDetailEntity> items = itemsDetailApp.FindEnum<T_QADetailEntity>(o => o.State).ToList();
            IList<ItemsDetailEntity> enumResultStateList = itemsDetailApp.FindEnum<T_QADetailEntity>(o => o.ResultState).ToList();
            IList<ItemsDetailEntity> enumQAResultList = itemsDetailApp.FindEnum<T_QADetailEntity>(o => o.QAResult).ToList();

            foreach (QADetailModel model in qaDetailModelList)
            {
                T_QADetailEntity entity = detailList.FirstOrDefault(o => o.F_Id == model.F_Id);
                model.SupplierCode = entity.SupplierCode;
                model.SupplierName = entity.SupplierUserName;
                model.SupplierID = entity.SupplierUserID;

                T_StationEntity t_stationEntity = stationApp.FindEntity(o => o.F_Id == model.StationID);
                model.StationName = t_stationEntity.StationName;
                model.StationCode = t_stationEntity.StationCode;

                model.StateName = items.FirstOrDefault(o => o.F_ItemCode == model.State).F_ItemName;
                if (!string.IsNullOrEmpty(model.ResultState)) model.ResultStateName = enumResultStateList.FirstOrDefault(o => o.F_ItemCode == model.ResultState).F_ItemName;
                else model.ResultState = "";
                if (!string.IsNullOrEmpty(model.QAResult)) model.QAResultName = enumQAResultList.FirstOrDefault(o => o.F_ItemCode == model.QAResult).F_ItemName;
                else model.QAResult = "";

                /// 还样数量 = 取样记录总数量
                model.QtySum = qARecordApp.FindList(o => o.QAID == qAGet.F_Id && o.ItemID == model.ItemID && o.Lot == model.Lot && o.State == "Picked").Sum(o => o.PickedQty);
            }

            var resultList = new
            {
                rows = qaDetailModelList,
                total = pagination.total,
                page = pagination.page,
                records = pagination.records
            };
            return Content(resultList.ToJson());
        }
        #endregion
    }
}
