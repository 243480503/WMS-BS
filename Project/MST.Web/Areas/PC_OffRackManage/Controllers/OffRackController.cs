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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace MST.Web.Areas.PC_OffRackManage.Controllers
{
    public class OffRackController : ControllerBase
    {
        private static object lockObj = new object();
        private T_OffRackApp offRackApp = new T_OffRackApp();
        private T_OffRackDetailApp offRackDetailApp = new T_OffRackDetailApp();
        private ItemsDetailApp itemsDetailApp = new ItemsDetailApp();
        //private T_LocationStateDetailApp locStateApp = new T_LocationStateDetailApp();

        #region 获取下架单列表
        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetGridJson(Pagination pagination, string queryJson)
        {
            List<T_OffRackEntity> data = offRackApp.GetList(pagination, queryJson);
            IList<ItemsDetailEntity> enumStateList = itemsDetailApp.FindEnum<T_OffRackEntity>(o => o.State).ToList();
            IList<ItemsDetailEntity> enumAreaTypeList = itemsDetailApp.FindEnum<T_OffRackEntity>(o => o.AreaType).ToList();
            IList<ItemsDetailEntity> enumGenTypeList = itemsDetailApp.FindEnum<T_OffRackEntity>(o => o.GenType).ToList();
            IList<ItemsDetailEntity> enumOffRackMethodList = itemsDetailApp.FindEnum<T_OffRackEntity>(o => o.OffRackMethod).ToList();

            List<OffRackModel> offRackModelList = new List<OffRackModel>();
            foreach (T_OffRackEntity item in data)
            {
                OffRackModel model = item.ToObject<OffRackModel>();
                model.AreaTypeName = enumAreaTypeList.FirstOrDefault(o => o.F_ItemCode == item.AreaType).F_ItemName;
                model.StateName = enumStateList.FirstOrDefault(o => o.F_ItemCode == item.State).F_ItemName;
                model.GenTypeName = enumGenTypeList.FirstOrDefault(o => o.F_ItemCode == item.GenType).F_ItemName;
                model.OffRackMethodName = enumOffRackMethodList.FirstOrDefault(o => o.F_ItemCode == item.OffRackMethod).F_ItemName;
                offRackModelList.Add(model);
            }

            var resultList = new
            {
                rows = offRackModelList,
                total = pagination.total,
                page = pagination.page,
                records = pagination.records
            };
            return Content(resultList.ToJson());
        }
        #endregion

        #region 查看下架单
        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetFormJson(string keyValue)
        {
            var data = offRackApp.GetForm(keyValue);

            OffRackModel model = data.ToObject<OffRackModel>();
            model.AreaTypeName = itemsDetailApp.FindEnum<T_OffRackEntity>(o => o.AreaType).FirstOrDefault(o => o.F_ItemCode == data.AreaType).F_ItemName;
            model.StateName = itemsDetailApp.FindEnum<T_OffRackEntity>(o => o.State).FirstOrDefault(o => o.F_ItemCode == data.State).F_ItemName;
            model.GenTypeName = itemsDetailApp.FindEnum<T_OffRackEntity>(o => o.GenType).FirstOrDefault(o => o.F_ItemCode == data.GenType).F_ItemName;
            model.OffRackMethodName = itemsDetailApp.FindEnum<T_OffRackEntity>(o => o.OffRackMethod).FirstOrDefault(o => o.F_ItemCode == data.OffRackMethod).F_ItemName;
            return Content(model.ToJson());
        }
        #endregion

        #region 新建/修改下架单
        [HttpPost]
        [HandlerAjaxOnly]
        [ValidateAntiForgeryToken]
        public ActionResult SubmitForm(T_OffRackEntity OffRackEntity, string keyValue)
        {
            using (var db = new RepositoryBase().BeginTrans())
            {
                LogObj logObj = new LogObj();
                logObj.Path = "OffRackController.SubmitForm";
                logObj.Parms = new { OffRackEntity = OffRackEntity, keyValue = keyValue };

                LogEntity logEntity = new LogEntity();
                logEntity.F_ModuleName = "下架单";
                logEntity.F_Type = DbLogType.Submit.ToString();
                logEntity.F_Account = OperatorProvider.Provider.GetCurrent().UserCode;
                logEntity.F_NickName = OperatorProvider.Provider.GetCurrent().UserName;
                logEntity.F_Description = "保存下架单";
                logEntity.F_Path = logObj.Path;
                logEntity.F_Param = logObj.Parms.ToJson();

                try
                {
                    /*****************************************/

                    if (OffRackEntity.State != "New") return Error("单据不是新建状态", "");

                    if (string.IsNullOrEmpty(OffRackEntity.RefOrderCode))
                    {
                        OffRackEntity.RefOrderCode = OffRackEntity.OffRackCode;
                    }
                    if (!string.IsNullOrEmpty(OffRackEntity.Remark))
                    {
                        OffRackEntity.Remark = OffRackEntity.Remark.Replace("\n", " ");
                    }

                    T_AreaEntity area = db.FindEntity<T_AreaEntity>(o=>o.F_Id == OffRackEntity.AreaID);
                    OffRackEntity.AreaType = area.AreaType;

                    if (string.IsNullOrEmpty(keyValue))
                    {
                        OffRackEntity.F_Id = Guid.NewGuid().ToString();
                        OffRackEntity.GenType = "MAN";
                        OffRackEntity.F_DeleteMark = false;
                        db.Insert<T_OffRackEntity>(OffRackEntity);
                    }
                    else
                    {
                        OffRackEntity.F_Id = keyValue;
                        db.Update<T_OffRackEntity>(OffRackEntity);
                    }
                    db.CommitWithOutRollBack();

                    /*****************************************/

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

        #region 删除下架单
        [HttpPost]
        [HandlerAjaxOnly]
        [HandlerAuthorize]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteForm(string keyValue)
        {
            using (var db = new RepositoryBase().BeginTrans())
            {
                LogObj logObj = new LogObj();
                logObj.Path = "OffRackController.DeleteForm";
                logObj.Parms = new { keyValue = keyValue };

                LogEntity logEntity = new LogEntity();
                logEntity.F_ModuleName = "下架单";
                logEntity.F_Type = DbLogType.Delete.ToString();
                logEntity.F_Account = OperatorProvider.Provider.GetCurrent().UserCode;
                logEntity.F_NickName = OperatorProvider.Provider.GetCurrent().UserName;
                logEntity.F_Description = "删除下架单";
                logEntity.F_Path = logObj.Path;
                logEntity.F_Param = logObj.Parms.ToJson();

                try
                {
                    /*************************************************/
                    T_OffRackEntity offRackEntity = db.FindEntity<T_OffRackEntity>(o => o.F_Id == keyValue);
                    if (offRackEntity.State != "New") return Error("非新建状态不可删除", "");

                    List<T_OffRackDetailEntity> detailList = db.FindList<T_OffRackDetailEntity>(o => o.OffRackID == offRackEntity.F_Id);
                    foreach (T_OffRackDetailEntity detail in detailList)
                    {
                        db.Delete<T_OffRackDetailEntity>(detail);
                    }

                    db.Delete<T_OffRackEntity>(offRackEntity);
                    db.SaveChanges();

                    db.CommitWithOutRollBack();

                    /**************************************************/

                    logObj.Message = "删除成功";
                    LogFactory.GetLogger().Info(logObj);

                    logEntity.F_Result = true;
                    new LogApp().WriteDbLog(logEntity);

                    return Success("删除成功。");
                }
                catch (Exception ex)
                {
                    db.RollBack();

                    logObj.Message = ex;
                    LogFactory.GetLogger().Error(logObj);

                    logEntity.F_Result = false;
                    logEntity.F_Msg = ex.ToJson();
                    new LogApp().WriteDbLog(logEntity);

                    return Error("删除失败。", ex.ToJson());
                }
            }
        }
        #endregion

        #region 设置下架单开始下架
        [HttpPost]
        [HandlerAjaxOnly]
        [ValidateAntiForgeryToken]
        public ActionResult OffRackOnOff(string offRackID)
        {
            using (var db = new RepositoryBase().BeginTrans())
            {
                LogObj logObj = new LogObj();
                logObj.Path = "OffRackController.OffRackOnOff";
                logObj.Parms = new { OffRackID = offRackID };

                LogEntity logEntity = new LogEntity();
                logEntity.F_ModuleName = "下架单";
                logEntity.F_Type = DbLogType.Submit.ToString();
                logEntity.F_Account = OperatorProvider.Provider.GetCurrent().UserCode;
                logEntity.F_NickName = OperatorProvider.Provider.GetCurrent().UserName;
                logEntity.F_Description = "开始下架";
                logEntity.F_Path = logObj.Path;
                logEntity.F_Param = logObj.Parms.ToJson();

                try
                {
                    lock (lockObj)
                    {
                        T_OffRackEntity offRack = db.FindEntity<T_OffRackEntity>(offRackID);
                        if (offRack == null) return Error("下架单据不存在", "");
                        if (offRack.State == "Over") return Error("单据已结束下架操作", "");
                        if (offRack.State == "OffRacking") return Error("单据已开始下架操作", "");

                        List<T_OffRackDetailEntity> detailList = db.FindList<T_OffRackDetailEntity>(o => o.OffRackID == offRackID);
                        if (detailList.Count == 0) return Error("下架单明细为空", "");

                        /// 货物下架明细的站台列表
                        List<T_OffRackDetailEntity> stationList = offRackDetailApp.FindList(o => o.OffRackID == offRackID).GroupBy(x => new { x.StationID }).Select(a => a.FirstOrDefault()).ToList();
                        foreach (var s in stationList)
                        {
                            if (string.IsNullOrEmpty(s.StationID)) continue;
                            T_StationEntity station = db.FindEntity<T_StationEntity>(o => o.F_Id == s.StationID);
                            if (station == null) return Error($"未找到下架到达站台 {station.StationName}", "");
                            if (!string.IsNullOrEmpty(station.CurOrderID)) return Error($"站台 {station.StationName} 已绑定单据", "");
                            if (station.CurOrderID == offRack.F_Id) return Error("站台已绑定该单据", "");

                            station.CurOrderID = offRack.F_Id;
                            station.OrderType = "OffRack";
                            db.Update<T_StationEntity>(station);
                        }

                        IList<string> TaskListNo = new List<string>();
                        foreach (var detail in detailList)
                        {
                            T_LocationEntity location = db.FindEntity<T_LocationEntity>(o => o.LocationCode == detail.LocationCode);
                            if (location.State != "Stored") return Error($"货位 { location.LocationCode } 不是存储状态", "");

                            List<T_ContainerDetailEntity> containerDetailList = db.FindList<T_ContainerDetailEntity>(o => o.LocationID == detail.LocationID).ToList();
                            /// 校验容器是否待出库待质检待盘点，是否未在库
                            foreach (var item in containerDetailList)
                            {
                                if (item.OutQty != 0 || item.CheckQty != 0 || item.LocationNo == FixType.Station.StationOut_BigItem.ToString() || item.LocationNo == FixType.Station.StationOut_Normal.ToString())
                                {
                                    return Error("下架货位物料有其它任务。", "");
                                }
                            }

                            string newTaskNo = T_CodeGenApp.GenNum("TaskRule");
                            T_StationEntity tagStation = db.FindEntity<T_StationEntity>(o => o.F_Id == detail.StationID);
                            string containerID = containerDetailList.FirstOrDefault().ContainerID;
                            T_ContainerEntity container = db.FindEntity<T_ContainerEntity>(o => o.F_Id == containerID);

                            foreach (T_ContainerDetailEntity cd in containerDetailList)
                            {
                                T_OffRackRecordEntity record = new T_OffRackRecordEntity();
                                record.F_Id = Guid.NewGuid().ToString();
                                record.OffRackID = detail.OffRackID;
                                record.OffRackDetailID = detail.F_Id;
                                record.ContainerID = container.F_Id;
                                record.BarCode = container.BarCode;
                                record.LocationID = location.F_Id;
                                record.LocationCode = location.LocationCode;
                                record.ContainerDetailID = cd.F_Id;
                                record.ItemID = cd.ItemID;
                                record.ItemCode = cd.ItemCode;
                                record.ItemName = cd.ItemName;
                                record.Lot = cd.Lot;
                                record.Qty = cd.Qty;
                                record.SupplierID = cd.SupplierID;
                                record.SupplierName = cd.SupplierName;
                                record.ItemBarCode = cd.ItemBarCode;
                                record.IsItemMark = cd.IsItemMark;
                                record.StationCode = detail.StationCode;
                                record.StationID = detail.StationID;
                                record.TaskNo = newTaskNo;
                                record.IsArrive = "false";
                                record.IsNeedBackWare = "false";
                                record.IsScanBack = "false";
                                record.F_DeleteMark = false;
                                record.CreatorUserName = OperatorProvider.Provider.GetCurrent().UserName;
                                record.F_CreatorUserId = OperatorProvider.Provider.GetCurrent().UserId;

                                record.State = "OffRacking";
                                db.Insert<T_OffRackRecordEntity>(record);
                            }

                            location.State = "Out";
                            db.Update<T_LocationEntity>(location);

                            detail.State = "OffRacking";
                            db.Update<T_OffRackDetailEntity>(detail);

                            /// 生成下架任务
                            T_TaskEntity task = new T_TaskEntity();
                            task.F_Id = Guid.NewGuid().ToString();
                            task.TaskNo = newTaskNo;
                            task.TaskInOutType = "OutType";
                            task.TaskType = "TaskType_OffRack";   /// 任务类型
                            task.ContainerID = container.F_Id;
                            task.BarCode = container.BarCode;
                            task.ContainerType = container.ContainerType;
                            task.SrcLocationID = location.F_Id;
                            task.SrcLocationCode = location.LocationCode;
                            task.TagLocationID = tagStation.F_Id;   /// 目标地址：站台
                            task.TagLocationCode = tagStation.LeaveAddress;
                            task.ApplyStationID = tagStation.F_Id;
                            task.Level = 20; /// 库存出库
                            task.State = "New";
                            task.SEQ = detail.SEQ;
                            task.IsWcsTask = "true";
                            task.IsCanExec = "true";
                            task.OrderType = "OffRack";
                            task.OrderID = offRack.F_Id;
                            task.OrderDetailID = detail.F_Id;
                            task.OrderCode = offRack.OffRackCode;
                            task.F_DeleteMark = false;
                            db.Insert<T_TaskEntity>(task);

                            db.SaveChanges();

                            TaskListNo.Add(task.TaskNo);                            
                        }

                        db.SaveChanges();

                        /// 修改单据状态
                        offRack.State = "OffRacking";
                        db.Update<T_OffRackEntity>(offRack);
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

        #region 设置强制完成下架
        [HttpPost]
        [HandlerAjaxOnly]
        [ValidateAntiForgeryToken]
        public ActionResult OffRackOver(string offRackID)
        {
            using (var db = new RepositoryBase().BeginTrans())
            {
                LogObj logObj = new LogObj();
                logObj.Path = "OffRackController.OffRackOver";
                logObj.Parms = new { OffRackID = offRackID };

                LogEntity logEntity = new LogEntity();
                logEntity.F_ModuleName = "下架单";
                logEntity.F_Type = DbLogType.Submit.ToString();
                logEntity.F_Account = OperatorProvider.Provider.GetCurrent().UserCode;
                logEntity.F_NickName = OperatorProvider.Provider.GetCurrent().UserName;
                logEntity.F_Description = "强制完成下架";
                logEntity.F_Path = logObj.Path;
                logEntity.F_Param = logObj.Parms.ToJson();

                try
                {
                    /*************************************************/
                    T_OffRackEntity entity = db.FindEntity<T_OffRackEntity>(o => o.F_Id == offRackID);
                    if (entity == null) return Error("下架单据不存在", "");
                    if (entity.State == "Over") return Error("下架单据已完成", "");
                    if (entity.State == "New") return Error("下架单据为新建状态", "");

                    entity.State = "Over";
                    db.Update<T_OffRackEntity>(entity);
                    db.SaveChanges();

                    /// 强制完成所有下架单明细
                    List<T_OffRackDetailEntity> offRackDetailList = db.FindList<T_OffRackDetailEntity>(o => o.OffRackID == entity.F_Id).ToList();
                    foreach (T_OffRackDetailEntity item in offRackDetailList)
                    {
                        item.State = "Over";
                        db.Update<T_OffRackDetailEntity>(item);
                    }

                    /// 强制完成所有下架单记录
                    List<T_OffRackRecordEntity> offRackRecordList = db.FindList<T_OffRackRecordEntity>(o => o.OffRackID == entity.F_Id).ToList();
                    foreach (T_OffRackRecordEntity item in offRackRecordList)
                    {
                        item.State = "Over";
                        db.Update<T_OffRackRecordEntity>(item);
                    }

                    /// 清空整个站台单据信息
                    List<T_StationEntity> stationList = db.FindList<T_StationEntity>(o => o.CurOrderID == entity.F_Id);
                    foreach (T_StationEntity station in stationList)
                    {
                        station.CurOrderID = "";
                        station.OrderType = "";
                        db.Update<T_StationEntity>(station);
                    }

                    db.CommitWithOutRollBack();

                    /*************************************************/

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
    }
}
