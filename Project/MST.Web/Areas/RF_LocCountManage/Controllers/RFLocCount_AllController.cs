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

namespace MST.Web.Areas.RF_LocCountManage.Controllers
{
    public class RFLocCount_AllController : ControllerBase
    {
        private T_LocCountApp locCountApp = new T_LocCountApp();
        private T_LocCountRecordApp locCountRecordApp = new T_LocCountRecordApp();
        private ItemsDetailApp itemsDetailApp = new ItemsDetailApp();
        private T_StationApp stationApp = new T_StationApp();
        private T_ContainerApp containerApp = new T_ContainerApp();
        private T_ContainerDetailApp containerDetailApp = new T_ContainerDetailApp();
        private T_ItemApp itemApp = new T_ItemApp();
        private T_TaskApp taskApp = new T_TaskApp();
        T_LocationStateDetailApp locStateApp = new T_LocationStateDetailApp();

        #region 扫描容器条码，获取容器异常信息
        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetLocCountJson(string barCode)
        {
            LocCountRecordModel locCountRecModel = new LocCountRecordModel();
            T_StationEntity station = new T_StationEntity();
            T_LocCountEntity locCount = new T_LocCountEntity();
            T_LocCountRecordEntity locCountRec = new T_LocCountRecordEntity();

            if (string.IsNullOrEmpty(barCode)) return Error("容器条码不能为空", "");
            T_ContainerEntity container = containerApp.FindEntity(o => o.BarCode == barCode);
            if (container == null)
            {
                station = stationApp.FindEntity(o => o.BarCode == barCode);
                if (station == null) return Error("容器未到达站台", "");
                locCount = locCountApp.FindEntity(o => o.F_Id == station.CurOrderID);
                locCountRec = locCountRecordApp.FindEntity(o => o.LocCountID == station.CurOrderID && o.FactBarCode == barCode);

                locCountRecModel.StationID = station.F_Id;
                locCountRecModel.RefOrderCode = locCount.RefOrderCode;
                locCountRecModel.LocationCode = locCountRec.LocationCode;
                locCountRecModel.ContainerKindName = "-";
                locCountRecModel.FactBarCode = barCode;
                locCountRecModel.ErrorMsg = "新容器";
                locCountRecModel.ErrSolution = "需重新入库";

                return Content(locCountRecModel.ToJson());
            }
            else
            {
                if (container.ContainerKind == "Box" || container.ContainerKind == "Plastic") /// 纸箱或料箱
                {
                    station = stationApp.FindEntity(o => o.BarCode == barCode && o.StationCode == FixType.Station.StationOut_Normal.ToString());
                }
                else if (container.ContainerKind == "Rack") /// 料架
                {
                    station = stationApp.FindEntity(o => o.BarCode == barCode && o.StationCode == FixType.Station.StationOut_BigItem.ToString());
                }
                else return Error("未知的容器大类", "");

                if (station == null) return Error("容器未到达站台", "");
                if (string.IsNullOrEmpty(station.CurOrderID)) return Error("该站台没有作业单据", "");

                T_TaskEntity task = taskApp.FindEntity(o => o.OrderID == station.CurOrderID && o.TaskType == "TaskType_LocCountErrIn" && o.BarCode == barCode);
                if (task != null) return Error("容器已申请入库", "");

                locCount = locCountApp.FindEntity(o => o.F_Id == station.CurOrderID);
                List<T_LocCountRecordEntity> needLocCountList = locCountRecordApp.FindList(o => o.LocCountID == station.CurOrderID && o.CountResult != "Inner_Empty"
                                                                                                            && o.CountResult != "Inner_SameBoxCode").ToList();
                List<T_LocCountRecordEntity> locCountingList = needLocCountList.Where(o => o.CountState == "WaitConfirm").ToList();

                locCountRec = needLocCountList.FirstOrDefault(o => o.FactBarCode == barCode);
                if (locCountRec == null) return Error("未找到容器异常记录", "");
                if (locCountRec.CountState == "Over") return Error("容器异常已处理", "");

                locCountRecModel = locCountRec.ToObject<LocCountRecordModel>();
                locCountRecModel.F_Id = locCountRec.F_Id;
                locCountRecModel.StationID = station.F_Id;
                locCountRecModel.LocCountID = station.CurOrderID;
                locCountRecModel.RefOrderCode = locCount.RefOrderCode;
                locCountRecModel.ContainerKind = container.ContainerKind;
                locCountRecModel.ContainerKindName = itemsDetailApp.FindEnum<T_ContainerTypeEntity>(o => o.ContainerKind).FirstOrDefault(o => o.F_ItemCode == container.ContainerKind).F_ItemName;
                locCountRecModel.MustTimes = needLocCountList.Count;
                locCountRecModel.NoCountTimes = locCountingList.Count;
                locCountRecModel.ReadyConfirmAndOrderNeed = (needLocCountList.Count - locCountingList.Count).ToString("0.##") + "/" + needLocCountList.Count.ToString("0.##");

                /// 异常提示信息
                string countResult = locCountRec.CountResult;
                switch (countResult)
                {
                    case "Inner_SameBoxCode":
                    case "Inner_Empty":
                        {
                            locCountRecModel.ErrorMsg = "正常";
                            locCountRecModel.ErrSolution = "正常";
                        }
                        break;
                    case "Inner_DiffBoxCode":
                        {
                            locCountRecModel.ErrorMsg = "实际盘点容器条码和库存不一致";
                            locCountRecModel.ErrSolution = "扫码回库或扫码结束";
                        }
                        break;
                    case "Inner_MoreBoxCode":
                        {
                            locCountRecModel.ErrorMsg = "库存显示空货位，实际有容器存在";
                            locCountRecModel.ErrSolution = "扫码回库或扫码结束";
                        }
                        break;
                    case "Inner_NotFindBoxCode":
                        {
                            locCountRecModel.ErrorMsg = "未找到容器";
                            locCountRecModel.ErrSolution = "清空货位，清除库存";
                        }
                        break;
                    default:
                        break;
                }

                return Content(locCountRecModel.ToJson());
            }
        }
        #endregion

        #region 切换选项卡后，根据箱码获取单据，实现单据所有异常容器列表
        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetLocCountRecordList(string barCode)
        {
            List<T_LocCountRecordEntity> dataList = new List<T_LocCountRecordEntity>();

            IList<ItemsDetailEntity> enumCountResultList = itemsDetailApp.FindEnum<T_CountRecordEntity>(o => o.CountResult).ToList();

            if (string.IsNullOrEmpty(barCode)) return Error("容器不能为空", "");
            else
            {
                T_StationEntity station = stationApp.FindEntity(o => o.BarCode == barCode);


                T_LocCountRecordEntity locRec = locCountRecordApp.FindEntity(o => o.FactBarCode == barCode && o.CountState == "WaitConfirm");

                if (locRec == null) return Error("未找到容器货位盘点异常信息", "");
                if (station == null) return Error("容器未到站", "");

                if (locRec.LocCountID != station.CurOrderID) return Error("未找到容器货位盘点异常信息", "");

                dataList = locCountRecordApp.FindList(o => o.LocCountID == locRec.LocCountID && o.CountState != "Over").OrderBy(o => o.F_LastModifyTime).ToList();
                List<LocCountRecordModel> locCountRecModelList = new List<LocCountRecordModel>();
                foreach (var item in dataList)
                {
                    LocCountRecordModel model = item.ToObject<LocCountRecordModel>();
                    model.CountResultName = enumCountResultList.FirstOrDefault(o => o.F_ItemCode == model.CountResult).F_ItemName;
                    locCountRecModelList.Add(model);
                }

                return Content(locCountRecModelList.ToJson());
            }
        }
        #endregion

        #region RF提交异常处理信息
        [HttpPost]
        [HandlerAjaxOnly]
        [ValidateAntiForgeryToken]
        public ActionResult SubmitLocCountRecordForm(LocCountRecordModel postLocCountRecord)
        {
            using (var db = new RepositoryBase().BeginTrans())
            {
                LogObj logObj = new LogObj();
                logObj.Path = "RFLocCount_AllController.SubmitLocCountRecordForm";
                logObj.Parms = new { postLocCountRecord = postLocCountRecord };

                LogEntity logEntity = new LogEntity();
                logEntity.F_ModuleName = "货位盘点单";
                logEntity.F_Type = DbLogType.Submit.ToString();
                logEntity.F_Account = OperatorProvider.Provider.GetCurrent().UserCode;
                logEntity.F_NickName = OperatorProvider.Provider.GetCurrent().UserName;
                logEntity.F_Description = "RF货位盘点异常处理提交";

                logEntity.F_Path = logObj.Path;
                logEntity.F_Param = logObj.Parms.ToJson();

                try
                {
                    /*************************************************/
                    if (string.IsNullOrEmpty(postLocCountRecord.FactBarCode)) return Error("容器条码不能为空", "");

                    T_StationEntity station = new T_StationEntity();
                    if (string.IsNullOrEmpty(postLocCountRecord.StationID)) station = db.FindEntity<T_StationEntity>(o => o.BarCode == postLocCountRecord.FactBarCode);
                    else station = db.FindEntity<T_StationEntity>(o => o.F_Id == postLocCountRecord.StationID);

                    if (station == null) return Error("未找到容器操作站台", "");
                    string CurOrderID = station.CurOrderID;

                    T_LocCountRecordEntity locCountRec = db.FindEntity<T_LocCountRecordEntity>(o => o.LocCountID == station.CurOrderID && o.FactBarCode == postLocCountRecord.FactBarCode);
                    if (locCountRec == null) return Error("当前容器没有异常信息", "");

                    T_LocCountEntity locCount = db.FindEntity<T_LocCountEntity>(o => o.F_Id == station.CurOrderID);

                    if (locCountRec.CountState == "Over") return Error("当前容器已处理", "");
                    /// 分情况处理
                    bool isNeedBack = false;
                    string countResult = locCountRec.CountResult;
                    switch (countResult)
                    {
                        case "Inner_SameBoxCode":
                        case "Inner_Empty":
                            break;
                        case "Inner_DiffBoxCode":   /// 箱码不一致
                            {
                                /// 处理：1重新分配货位回库；2容器原货位（空，禁用）；3出库货位（空，正常）
                                T_ContainerEntity container = db.FindEntity<T_ContainerEntity>(o => o.BarCode == locCountRec.FactBarCode && o.F_DeleteMark == false);
                                if (container == null)
                                {
                                    /// 重新入库

                                    /// 当前出库货位：空，正常
                                    T_LocationEntity locCur = db.FindEntity<T_LocationEntity>(o => o.LocationCode == locCountRec.LocationCode);
                                    locCur.State = "Empty";
                                    locCur.ForbiddenState = "Normal";
                                    db.Update<T_LocationEntity>(locCur);
                                    db.SaveChanges();

                                    /// 记录货位状态变更
                                    locStateApp.SyncLocState(db, locCur, "OutType", "LocCount", "Out", "Empty", "");
                                }
                                else
                                {
                                    isNeedBack = true;
                                }
                            }
                            break;
                        case "Inner_MoreBoxCode":   /// 多余箱码
                            {
                                /// 处理：（情况Ⅰ）库存存在，重新分配货位回库（1重新分配货位回库；2容器原货位（空，禁用）；3出库货位（空，正常））
                                /// （情况Ⅱ）库存不存在，需重新入库
                                T_ContainerEntity container = db.FindEntity<T_ContainerEntity>(o => o.BarCode == locCountRec.FactBarCode && o.F_DeleteMark == false);
                                if (container == null)
                                {
                                    /// 重新入库

                                    /// 当前出库货位：空，正常
                                    T_LocationEntity locCur = db.FindEntity<T_LocationEntity>(o => o.LocationCode == locCountRec.LocationCode);
                                    locCur.State = "Empty";
                                    locCur.ForbiddenState = "Normal";
                                    db.Update<T_LocationEntity>(locCur);
                                    db.SaveChanges();

                                    /// 记录货位状态变更
                                    locStateApp.SyncLocState(db, locCur, "OutType", "LocCount", "Out", "Empty", "");
                                }
                                else
                                {
                                    isNeedBack = true;
                                }
                            }
                            break;
                        case "Inner_NotFindBoxCode":    /// 未找到箱码（清空货位，清除库存）
                            {
                                /// 清空货位
                                T_LocationEntity loc = db.FindEntity<T_LocationEntity>(o => o.F_Id == locCountRec.LocationID);
                                loc.State = "Empty";
                                loc.ForbiddenState = "Normal";
                                db.Update<T_LocationEntity>(loc);

                                /// 记录货位状态变更
                                locStateApp.SyncLocState(db, loc, "OutType", "LocCount", "Out", "Empty", "");

                                /// 查看是否在其它盘点记录中，未找到删除库存
                                var locRecOther = db.FindEntity<T_LocCountRecordEntity>(o => o.LocCountID == station.CurOrderID && o.BarCode == postLocCountRecord.FactBarCode && o.CountState == "Inner_NotFindBoxCode");
                                if (locRecOther == null)
                                {
                                    List<T_ContainerDetailEntity> cdList = db.FindList<T_ContainerDetailEntity>(o => o.LocationID == loc.F_Id);
                                    foreach (T_ContainerDetailEntity cd in cdList)
                                    {
                                        db.Delete<T_ContainerDetailEntity>(cd);
                                        db.SaveChanges();
                                    }
                                }
                            }
                            break;
                        default:
                            {
                                return Error($"未知的盘点结果{locCountRec.CountResult}", "");
                            }
                    }

                    locCountRec.CountState = "Over";
                    locCountRec.IsConfirm = "true";
                    db.Update<T_LocCountRecordEntity>(locCountRec);
                    db.SaveChanges();

                    /// 生成回库任务
                    /// 1重新分配货位回库；2容器原货位（空，禁用）；3出库货位（空，正常）
                    if (isNeedBack)
                    {
                        /// 根据容器判断回库区域
                        T_ContainerEntity containerEntity = db.FindEntity<T_ContainerEntity>(o => o.BarCode == locCountRec.FactBarCode && o.F_DeleteMark == false);
                        FixType.Area areaEnum;
                        if (containerEntity.ContainerKind == "Rack")
                        {
                            areaEnum = FixType.Area.BigItemArea;
                        }
                        else if (containerEntity.ContainerKind == "Box" || containerEntity.ContainerKind == "Plastic")
                        {
                            areaEnum = FixType.Area.NormalArea;
                        }
                        else return Error("未知的容器大类", "");

                        /// 根据站台单据类型，推断回库时的任务类型
                        string stationOrderType = station.OrderType;
                        T_AreaEntity area = db.FindEntity<T_AreaEntity>(o => o.AreaCode == areaEnum.ToString());

                        T_TaskEntity taskBack = new T_TaskEntity();
                        taskBack.F_Id = Guid.NewGuid().ToString();
                        taskBack.TaskNo = T_CodeGenApp.GenNum("TaskRule");
                        taskBack.TaskInOutType = "InType";
                        taskBack.ContainerID = containerEntity.F_Id;
                        taskBack.BarCode = containerEntity.BarCode;
                        taskBack.ContainerType = containerEntity.ContainerType;
                        taskBack.SrcLocationID = station.F_Id;
                        taskBack.SrcLocationCode = station.LeaveAddress;
                        taskBack.SrcWCSLocCode = station.LeaveAddress;
                        taskBack.TagAreaID = area.F_Id;
                        taskBack.TagLocationID = "";
                        taskBack.TagLocationCode = "";
                        taskBack.ApplyStationID = station.F_Id;
                        taskBack.WaveID = "";
                        taskBack.WaveCode = "";
                        taskBack.SEQ = 0;
                        taskBack.Level = 30; /// 盘点
                        taskBack.State = "New";
                        taskBack.IsWcsTask = "true";
                        taskBack.SendWCSTime = null;
                        taskBack.OrderType = "LocCount";
                        taskBack.OrderID = locCount.F_Id;
                        taskBack.OrderDetailID = locCountRec.F_Id;
                        taskBack.OrderCode = locCount.LocCountCode;
                        taskBack.OverTime = null;
                        taskBack.TaskType = "TaskType_LocCountErrIn";
                        taskBack.IsCanExec = "true";

                        db.Insert<T_TaskEntity>(taskBack);
                        db.SaveChanges();

                        /// FactBarCode对应的货位，容器原货位：空，禁用
                        T_LocationEntity locPre = db.FindEntity<T_LocationEntity>(o => o.LocationCode == containerEntity.LocationNo);
                        if (!(locPre.ForbiddenState == "Normal" && (locPre.State == "Empty" || locPre.State == "Stored")))
                        {
                            locPre.State = "Empty";
                            locPre.ForbiddenState = "Lock";
                            db.Update<T_LocationEntity>(locPre);


                            containerEntity.LocationID ="";
                            containerEntity.LocationNo = "";
                            containerEntity.AreaID = area.F_Id;
                            containerEntity.AreaCode = area.AreaCode;
                            containerEntity.AreaName = area.AreaName;
                            db.Update<T_ContainerEntity>(containerEntity);

                            /// 更新库存明细货位
                            IList<T_ContainerDetailEntity> containerDetailList = db.FindList<T_ContainerDetailEntity>(o => o.BarCode == containerEntity.BarCode);
                            foreach (T_ContainerDetailEntity detail in containerDetailList)
                            {
                                detail.LocationID = "";
                                detail.LocationNo = "";
                                detail.AreaID = area.F_Id;
                                detail.AreaCode = area.AreaCode;
                                detail.AreaName = area.AreaName;
                                db.Update<T_ContainerDetailEntity>(detail);
                            }
                        }

                        /// 记录货位状态变更
                        locStateApp.SyncLocState(db, locPre, "OutType", "LocCount", "Out", "Empty", "");

                        /// 实际出库货位：空，正常
                        T_LocationEntity locCur = db.FindEntity<T_LocationEntity>(o => o.LocationCode == locCountRec.LocationCode);
                        locCur.State = "Empty";
                        locCur.ForbiddenState = "Normal";
                        db.Update<T_LocationEntity>(locCur);
                        db.SaveChanges();

                        /// 记录货位状态变更
                        locStateApp.SyncLocState(db, locCur, "OutType", "LocCount", "Out", "Empty", "");

                        /// 大件站台直接申请货位
                        if (areaEnum == FixType.Area.BigItemArea)
                        {
                            string errMsg = "";
                            T_LocationApp locationApp = new T_LocationApp();
                            T_ContainerTypeEntity containerType = db.FindEntity<T_ContainerTypeEntity>(o => o.ContainerTypeCode == taskBack.ContainerType);
                            T_ContainerDetailEntity oneConDetail = db.FindEntity<T_ContainerDetailEntity>(o => o.BarCode == locCountRec.FactBarCode);
                            LogObj log = null;
                            T_ItemEntity item = db.FindEntity<T_ItemEntity>(o => o.F_Id == oneConDetail.ItemID);
                            T_LocationEntity loc = locationApp.GetLocIn(ref errMsg, ref log, db, containerType, taskBack.TagAreaID, false, oneConDetail.ERPWarehouseCode, oneConDetail.CheckState, true, null, item);
                            if (loc == null) return Error($"货位分配失败：{errMsg}", "");

                            taskBack.TagLocationCode = loc.LocationCode;
                            taskBack.TagLocationID = loc.F_Id;
                            taskBack.SendWCSTime = DateTime.Now;
                            db.SaveChanges();

                            string orderType = station.OrderType;
                            string taskType = "TaskType_LocCountErrOut";
                            List<T_TaskEntity> taskList = db.FindList<T_TaskEntity>(o => o.OrderID == station.CurOrderID && o.TagLocationID == station.F_Id && o.TaskType == taskType.ToString());
                            List<T_LocCountRecordEntity> list = db.FindList<T_LocCountRecordEntity>(o => o.LocCountID == station.CurOrderID && o.AreaCode == FixType.Area.BigItemArea.ToString()
                                                                && o.CountState != "Over" && o.CountResult != "Inner_NotFindBoxCode");

                            station.BarCode = "";
                            if (taskList.Count == 0 && list.Count == 0)
                            {
                                station.CurOrderID = "";
                                station.CurOrderDetailID = "";
                                station.WaveID = "";
                                station.OrderType = "";
                            }
                            db.Update<T_StationEntity>(station);

                            /// 推送WCS大件回库任务
                            IList<string> taskIDList = new List<string>();
                            taskIDList.Add(taskBack.TaskNo);
                            WCSResult wcsRes = new WCSPost().SendTask(db, taskIDList);
                            if (wcsRes.IsSuccess)
                            {
                                db.Update<T_TaskEntity>(taskBack);

                                logObj.Message = "大件准备回库";
                                LogFactory.GetLogger().Info(logObj);

                                logEntity.F_Result = true;
                                new LogApp().WriteDbLog(logEntity);
                            }
                            else
                            {
                                db.RollBack();
                                return Error("操作失败。", wcsRes.FailMsg);
                            }
                        }
                    }
                    else    /// 清空站台
                    {
                        FixType.Area areaEnum;
                        if (station.StationCode == FixType.Station.StationOut_BigItem.ToString()) areaEnum = FixType.Area.BigItemArea;
                        else if (station.StationCode == FixType.Station.StationOut_Normal.ToString()) areaEnum = FixType.Area.NormalArea;
                        else return Error("站台不是出库站台", "");

                        string taskType = "TaskType_LocCountErrOut";
                        List<T_TaskEntity> taskList = db.FindList<T_TaskEntity>(o => o.OrderID == station.CurOrderID && o.TagLocationID == station.F_Id && o.TaskType == taskType.ToString());
                        List<T_LocCountRecordEntity> list = db.FindList<T_LocCountRecordEntity>(o => o.LocCountID == station.CurOrderID && o.AreaCode == areaEnum.ToString()
                                                            && o.CountState != "Over" && o.CountResult != "Inner_NotFindBoxCode");

                        station.BarCode = "";
                        if (taskList.Count == 0 && list.Count == 0)
                        {
                            station.CurOrderID = "";
                            station.CurOrderDetailID = "";
                            station.WaveID = "";
                            station.OrderType = "";
                        }
                        db.Update<T_StationEntity>(station);
                    }

                    /// 判断单据是否完成
                    List<T_LocCountRecordEntity> noLocCountRecList = db.FindList<T_LocCountRecordEntity>(o => o.LocCountID == locCount.F_Id && o.CountState != "Over");
                    if (noLocCountRecList.Count == 0) /// 异常记录已处理完毕
                    {
                        locCount.State = "Over";
                        db.Update<T_LocCountEntity>(locCount);
                        db.SaveChanges();
                    }

                    db.SaveChanges();
                    db.CommitWithOutRollBack();

                    List<T_LocCountRecordEntity> needLocCountList = locCountRecordApp.FindList(o => o.LocCountID == locCount.F_Id && o.CountResult != "Inner_Empty"
                                                                                                                && o.CountResult != "Inner_SameBoxCode").ToList();
                    List<T_LocCountRecordEntity> locCountingList = needLocCountList.Where(o => o.CountState == "WaitConfirm").ToList();
                    string NoCountTimes = locCountingList.Count().ToString();    // 剩余次数
                    string ReadyConfirmAndOrderNeed = (needLocCountList.Count - locCountingList.Count).ToString("0.##") + "/" + needLocCountList.Count.ToString("0.##");  // 已盘 / 总盘

                    /**************************************************/

                    logObj.Message = "操作成功";
                    LogFactory.GetLogger().Info(logObj);

                    logEntity.F_Result = true;
                    new LogApp().WriteDbLog(logEntity);

                    return Success("操作成功。", new { NoCountTimes = NoCountTimes, ReadyConfirmAndOrderNeed = ReadyConfirmAndOrderNeed });
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
