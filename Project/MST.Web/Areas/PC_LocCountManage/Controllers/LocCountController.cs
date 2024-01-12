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

namespace MST.Web.Areas.PC_LocCountManage.Controllers
{
    public class LocCountController : ControllerBase
    {
        private static object lockObj = new object();
        private T_LocCountApp locCountApp = new T_LocCountApp();
        private ItemsDetailApp itemsDetailApp = new ItemsDetailApp();
        private T_SupplierApp supplierApp = new T_SupplierApp();
        private T_TaskApp taskApp = new T_TaskApp();
        private T_AreaApp areaApp = new T_AreaApp();
        private T_ItemApp itemApp = new T_ItemApp();
        private T_LocationStateDetailApp locStateApp = new T_LocationStateDetailApp();


        [HttpGet]
        [HandlerAuthorize]
        public virtual ActionResult ReCount()
        {
            return View();
        }

        #region 货位校对单列表
        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetGridJson(Pagination pagination, string queryJson)
        {
            var data = locCountApp.GetList(pagination, queryJson);
            IList<ItemsDetailEntity> enumStateList = itemsDetailApp.FindEnum<T_LocCountEntity>(o => o.State).ToList();
            IList<ItemsDetailEntity> enumGenTypeList = itemsDetailApp.FindEnum<T_LocCountEntity>(o => o.GenType).ToList();

            IList<LocCountModel> locCountList = new List<LocCountModel>();
            foreach (T_LocCountEntity item in data)
            {
                LocCountModel model = item.ToObject<LocCountModel>();
                model.StateName = enumStateList.FirstOrDefault(o => o.F_ItemCode == item.State).F_ItemName;
                model.GenTypeName = enumGenTypeList.FirstOrDefault(o => o.F_ItemCode == item.GenType).F_ItemName;
                locCountList.Add(model);
            }

            var resultList = new
            {
                rows = locCountList,
                total = pagination.total,
                page = pagination.page,
                records = pagination.records
            };
            return Content(resultList.ToJson());
        }
        #endregion

        #region 货位校对单详情
        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetFormJson(string keyValue)
        {
            var data = locCountApp.GetForm(keyValue);
            LocCountModel model = data.ToObject<LocCountModel>();
            model.StateName = itemsDetailApp.FindEnum<T_LocCountEntity>(o => o.State).FirstOrDefault(o => o.F_ItemCode == data.State).F_ItemName;
            model.GenTypeName = itemsDetailApp.FindEnum<T_LocCountEntity>(o => o.GenType).FirstOrDefault(o => o.F_ItemCode == data.GenType).F_ItemName;
            return Content(model.ToJson());
        }
        #endregion

        #region 区域下拉框列表
        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetAreaDicList()
        {
            List<T_AreaEntity> areaList = new List<T_AreaEntity>();
            areaList = areaApp.FindList(o => (o.AreaType == "Tunnel" && o.AreaCode != FixType.Area.BigItemArea.ToString() && o.AreaCode != FixType.Area.EmptyArea.ToString()) || o.AreaType == "Concentrate" || o.AreaType == "AGV").ToList();
            return Content(areaList.ToJson());
        }
        #endregion

        #region 新建/编辑货位校对单
        [HttpPost]
        [HandlerAjaxOnly]
        [ValidateAntiForgeryToken]
        public ActionResult SubmitForm(T_LocCountEntity LocCountEntity, string keyValue)
        {
            using (var db = new RepositoryBase().BeginTrans())
            {
                LogObj logObj = new LogObj();
                logObj.Path = "LocCountController.SubmitForm";
                logObj.Parms = new { LocCountEntity = LocCountEntity, keyValue = keyValue };

                LogEntity logEntity = new LogEntity();
                logEntity.F_ModuleName = "货位校对单";
                logEntity.F_Type = DbLogType.Submit.ToString();
                logEntity.F_Account = OperatorProvider.Provider.GetCurrent().UserCode;
                logEntity.F_NickName = OperatorProvider.Provider.GetCurrent().UserName;
                logEntity.F_Description = "保存货位校对单";
                logEntity.F_Path = logObj.Path;
                logEntity.F_Param = logObj.Parms.ToJson();

                try
                {
                    /*************************************************/
                    if (LocCountEntity.State != "New") return Error("单据不是新建状态", "");
                    T_LocCountEntity locCountBefore = locCountApp.FindEntity(o => o.F_Id == keyValue);

                    if (string.IsNullOrEmpty(LocCountEntity.RefOrderCode)) LocCountEntity.RefOrderCode = LocCountEntity.LocCountCode;
                    LocCountEntity.GenType = "MAN";
                    LocCountEntity.TaskCellNum = 0;
                    LocCountEntity.F_DeleteMark = false;
                    if (!string.IsNullOrEmpty(LocCountEntity.Remark)) LocCountEntity.Remark = LocCountEntity.Remark.Replace("\n", " ");

                    if (string.IsNullOrEmpty(keyValue))
                    {
                        LocCountEntity.F_Id = Guid.NewGuid().ToString();
                        T_AreaEntity area = areaApp.FindEntity(o => o.AreaCode == LocCountEntity.AreaCode);
                        LocCountEntity.AreaID = area.F_Id;
                        LocCountEntity.AreaName = area.AreaName;
                        db.Insert<T_LocCountEntity>(LocCountEntity);
                    }
                    else
                    {
                        LocCountEntity.F_Id = keyValue;
                        /// 已添加校对明细，禁止修改校对单校对区域
                        var recordList = db.FindList<T_LocCountRecordEntity>(o => o.LocCountID == LocCountEntity.F_Id);
                        if (recordList.Count != 0)
                        {
                            if (LocCountEntity.AreaCode != locCountBefore.AreaCode)
                            {
                                db.RollBack();
                                return Error("已添加货位校对明细，禁止修改校对单校对区域", "操作失败");
                            }
                        }

                        T_AreaEntity area = areaApp.FindEntity(o => o.AreaCode == LocCountEntity.AreaCode);
                        LocCountEntity.AreaID = area.F_Id;
                        LocCountEntity.AreaName = area.AreaName;

                        db.Update<T_LocCountEntity>(LocCountEntity);
                    }
                    db.SaveChanges();

                    /// 生成全盘记录
                    List<T_LocationEntity> locationList = db.FindList<T_LocationEntity>(o => o.AreaCode == LocCountEntity.AreaCode);   /// 货位列表
                    if (locationList.Count < 1)
                    {
                        db.RollBack();
                        return Error("区域无货位", "操作失败");
                    }

                    if (LocCountEntity.IsAllCount == "true")
                    {
                        IList<T_LocCountRecordEntity> locRecList = new List<T_LocCountRecordEntity>();
                        foreach (T_LocationEntity loc in locationList)
                        {
                            T_LocCountRecordEntity record = new T_LocCountRecordEntity();
                            record.F_Id = Guid.NewGuid().ToString();
                            record.LocCountID = LocCountEntity.F_Id;
                            record.AreaID = loc.AreaID;
                            record.AreaCode = loc.AreaCode;
                            record.AreaName = loc.AreaName;
                            record.LocationID = loc.F_Id;
                            record.LocationCode = loc.LocationCode;
                            record.LocState = loc.State;
                            record.ForbiddenState = loc.ForbiddenState;
                            record.FactBarCode = "";
                            record.CountState = "New";
                            record.CountResult = "";
                            record.GenType = "MAN";
                            record.IsConfirm = "false";
                            record.IsArrive = "false";
                            record.IsNeedBackWare = "true";
                            record.IsScanBack = "false";
                            record.F_DeleteMark = false;

                            locRecList.Add(record);
                            //db.Insert<T_LocCountRecordEntity>(record);
                        }

                        db.BulkInsert(locRecList);
                        db.BulkSaveChanges();
                    }
                    db.SaveChanges();
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

        #region 删除货位校对单
        [HttpPost]
        [HandlerAjaxOnly]
        [HandlerAuthorize]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteForm(string keyValue)
        {
            using (var db = new RepositoryBase().BeginTrans())
            {
                LogObj logObj = new LogObj();
                logObj.Path = "LocCountController.DeleteForm";
                logObj.Parms = new { keyValue = keyValue };

                LogEntity logEntity = new LogEntity();
                logEntity.F_ModuleName = "货位校对单";
                logEntity.F_Type = DbLogType.Delete.ToString();
                logEntity.F_Account = OperatorProvider.Provider.GetCurrent().UserCode;
                logEntity.F_NickName = OperatorProvider.Provider.GetCurrent().UserName;
                logEntity.F_Description = "删除货位校对单";
                logEntity.F_Path = logObj.Path;
                logEntity.F_Param = logObj.Parms.ToJson();

                try
                {
                    /*************************************************/
                    T_LocCountEntity locCountEntity = db.FindEntity<T_LocCountEntity>(o => o.F_Id == keyValue);
                    if (locCountEntity.State != "New")
                    {
                        return Error("非新建状态不可删除", "");
                    }

                    IList<T_LocCountRecordEntity> delList = db.FindList<T_LocCountRecordEntity>(o => o.LocCountID == locCountEntity.F_Id);
                    db.BulkDelete(delList);
                    db.BulkSaveChanges();

                    db.Delete<T_LocCountEntity>(locCountEntity);
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

        #region 开始货位校对
        /// <summary>
        /// 开始货位校对
        /// </summary>
        /// <param name="locCountID"></param>
        /// <returns></returns>
        [HttpPost]
        [HandlerAjaxOnly]
        [ValidateAntiForgeryToken]
        public ActionResult BeginLocCount(string locCountID)
        {
            using (var db = new RepositoryBase().BeginTrans())
            {
                LogObj logObj = new LogObj();
                logObj.Path = "LocCountController.BeginLocCount";
                logObj.Parms = new { LocCountID = locCountID };

                LogEntity logEntity = new LogEntity();
                logEntity.F_ModuleName = "货位校对单";
                logEntity.F_Type = DbLogType.Submit.ToString();
                logEntity.F_Account = OperatorProvider.Provider.GetCurrent().UserCode;
                logEntity.F_NickName = OperatorProvider.Provider.GetCurrent().UserName;
                logEntity.F_Description = "开始校对";
                logEntity.F_Path = logObj.Path;
                logEntity.F_Param = logObj.Parms.ToJson();

                try
                {
                    /*************************************************/
                    lock (lockObj)
                    {
                        T_OffLineDownApp offLineDownApp = new T_OffLineDownApp();
                        bool IsHaveOffLine = offLineDownApp.IsHaveOff(db);
                        if (IsHaveOffLine)
                        {
                            return Error("存在未处理的离线数据", "");
                        }

                        T_LocCountEntity locCount = db.FindEntity<T_LocCountEntity>(o => o.F_Id == locCountID);
                        if (locCount == null) return Error("货位校对单据不存在。", "");
                        if (locCount.State == "Over" || locCount.State == "WaitConfirm")
                        {
                            return Error("单据已执行", "");
                        }
                        else if (locCount.State == "Overing") return Error("单据已在结束中", "");
                        else if (locCount.State == "Counting") return Error("单据正在校对", "");

                        /// 判断站台物料
                        List<T_ContainerDetailEntity> cdInStationList = new List<T_ContainerDetailEntity>();
                        List<T_StationEntity> inStationList = new List<T_StationEntity>();
                        //获取指定执行设备
                        IList<string> equList = string.IsNullOrEmpty(locCount.PointRobotCode) ? new List<string>() : locCount.PointRobotCode.Split(',').ToList<string>();
                        if (locCount.AreaCode == FixType.Area.BigItemArea.ToString())
                        {
                            equList.Remove("2001");
                            equList.Remove("2002");
                            equList.Remove("2003");
                            equList.Remove("2004");
                            cdInStationList = db.FindList<T_ContainerDetailEntity>(o => o.LocationNo == FixType.Station.StationOut_BigItem.ToString());
                            inStationList = db.FindList<T_StationEntity>(o => o.StationCode == FixType.Station.StationIn_BigItem.ToString());
                        }
                        else if (locCount.AreaCode == FixType.Area.NormalArea.ToString())
                        {
                            equList.Remove("1001");
                            equList.Remove("1002");

                            cdInStationList = db.FindList<T_ContainerDetailEntity>(o => o.LocationNo == FixType.Station.StationOut_Normal.ToString());
                            inStationList = db.FindList<T_StationEntity>(o => o.StationCode == FixType.Station.StationIn_Normal.ToString());
                        }
                        else
                        {
                            return Error("该区域不可货位校对", "");
                        }

                        /// 判断站台是否存在物料
                        if (cdInStationList.Count != 0) return Error("出库站台有物料待操作", "");

                        /// 判断站台是否有正在执行入库单据
                        if (inStationList.Any(o => !string.IsNullOrEmpty(o.CurOrderID))) return Error("入库站台有正在入库单据", "");

                        IList<T_LocCountRecordEntity> recordList = db.FindList<T_LocCountRecordEntity>(o => o.LocCountID == locCountID);    /// 已生成的校对记录
                        List<T_LocationEntity> locationList = db.FindList<T_LocationEntity>(o => o.AreaCode == locCount.AreaCode);   /// 货位列表

                        /// 没有校对数据
                        if (recordList.Count == 0) return Error("需要先添加校对货位列表", "");

                        /// 开始校对
                        IList<T_TaskEntity> taskList = new List<T_TaskEntity>();
                        IList<T_LocCountRecordEntity> locCountRecList = new List<T_LocCountRecordEntity>();
                        IList<T_LocationEntity> locList = new List<T_LocationEntity>();

                        IList<string> taskNoList = new List<string>();
                        List<T_ContainerEntity> containerList = db.FindList<T_ContainerEntity>(o => o.F_DeleteMark == false);
                        IList<T_ContainerDetailEntity> containerDetailList = db.FindListAsNoTracking<T_ContainerDetailEntity>(o => o.F_DeleteMark == false);

                        IList<string> genNumList = T_CodeGenApp.GenNum("TaskRule", recordList.Count);
                        int icount = 0;
                        foreach (T_LocCountRecordEntity record in recordList)
                        {
                            string genNum = genNumList[icount];
                            icount = icount + 1;

                            T_LocationEntity loc = locationList.FirstOrDefault(o => o.LocationCode == record.LocationCode);
                            /// 判断所有校对货位状态。全盘条件：除待入库/待出库外，均可以
                            //if (loc.ForbiddenState != "Normal") return Error($"货位[{ loc.LocationCode }]锁定状态不可校对", "");
                            if (loc.State != "Empty" && loc.State != "Stored")
                            {
                                return Error($"货位[{ loc.LocationCode }]状态不可校对", "");
                            }

                            if (record.LocState != loc.State)
                            {
                                return Error($"货位[{ loc.LocationCode }]状态已变更，请删除后重新添加", "");
                            }
                            //if(loc.State == "Stored")
                            //{
                            //    T_ContainerDetailEntity conDetail = containerDetailList.FirstOrDefault(o => o.LocationNo == loc.LocationCode);
                            //    if(conDetail.BarCode != record.BarCode)
                            //    {
                            //        return Error($"货位[{ loc.LocationCode }]容器已变更，请删除后重新添加", "");
                            //    }
                            //}


                            /// 更新校对记录中的货位状态
                            record.LocState = loc.State;
                            record.ForbiddenState = loc.ForbiddenState;
                            record.CountState = "Counting";

                            /// 更新校对记录中的容器，物料状态
                            /// 已存储，获取容器信息
                            T_ContainerEntity container = containerList.FirstOrDefault(o => o.LocationNo == loc.LocationCode);
                            if (loc.State == "Stored")
                            {
                                if (container == null)
                                {
                                    record.ContainerID = "";
                                    record.BarCode = "";
                                    record.ContainerKind = "";
                                }
                                else
                                {
                                    record.ContainerID = container.F_Id;
                                    record.BarCode = container.BarCode;
                                    record.ContainerKind = container.ContainerKind;
                                }
                            }
                            else if (loc.State == "Empty")
                            {
                                record.ContainerID = "";
                                record.BarCode = "";
                                record.ContainerKind = "";
                            }

                            //db.Update<T_LocCountRecordEntity>(record);
                            locCountRecList.Add(record);
                            /// 更新货位状态
                            loc.State = "Out";

                            //db.Update<T_LocationEntity>(loc);
                            //db.SaveChanges();
                            locList.Add(loc);

                            /// 产生在库校对任务
                            T_TaskEntity task = new T_TaskEntity();
                            task.F_Id = Guid.NewGuid().ToString();
                            task.TaskNo = genNum;
                            task.ContainerID = record.ContainerID;  /// 货位为空时，容器为空货位
                            task.BarCode = record.BarCode;
                            if (!string.IsNullOrEmpty(record.ContainerID))
                            {
                                if (container != null) task.ContainerType = container.ContainerType;
                                else task.ContainerType = "";
                            }
                            else task.ContainerType = "";

                            task.TaskInOutType = "AGVCountType";
                            task.TaskType = "TaskType_LocCount";  /// 货位校对
                            task.TagLocationID = record.LocationID;    /// 目标地址ID
                            task.TagLocationCode = record.LocationCode;    /// 目标地址编码
                            task.TagWCSLocCode = loc.WCSLocCode;
                            task.SrcLocationID = record.LocationID;  /// 起始地址ID
                            task.SrcLocationCode = record.LocationCode;    /// 起始地址编码
                            task.SrcWCSLocCode = loc.WCSLocCode;
                            task.Level = 30; /// 校对
                            task.State = "New";
                            task.SEQ = 0;
                            task.IsWcsTask = "true";
                            task.IsCanExec = "true";
                            task.OrderType = "LocCount"; /// 货位校对单
                            task.OrderID = locCount.F_Id;
                            task.OrderDetailID = record.F_Id;
                            task.OrderCode = locCount.LocCountCode;
                            task.F_DeleteMark = false;
                            task.PointExecRobotCode = equList.Count == 0 ? null : equList[(icount - 1) % equList.Count];
                            //db.Insert<T_TaskEntity>(task);
                            taskList.Add(task);
                            taskNoList.Add(task.TaskNo);

                            /// 货位状态变更记录
                            //locStateApp.SyncLocState(db, loc, "OutType", "LocCount", "Stored", "Out", task.TaskNo);
                        }
                        db.BulkUpdate(locCountRecList);
                        db.BulkUpdate(locList);
                        db.BulkInsert(taskList);
                        db.BulkSaveChanges();

                        /// 修改货位校对单状态
                        locCount.State = "Counting";
                        db.Update<T_LocCountEntity>(locCount);
                        db.SaveChanges();


                        /// 推送WCS任务
                        WCSResult wcsRes = new WCSPost().SendTaskBulk(db, taskNoList);
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

                    /*************************************************/
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


        #region 重新生成校对单
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sourceID"></param>
        /// <param name="countModel">1全部，2仅异常</param>
        /// <param name="remark"></param>
        /// <returns></returns>
        [HttpPost]
        [HandlerAjaxOnly]
        [ValidateAntiForgeryToken]
        public ActionResult GenReCount(string sourceID, string countModel, string remark)
        {
            using (var db = new RepositoryBase().BeginTrans())
            {
                LogObj logObj = new LogObj();
                logObj.Path = "LocCountController.GenReCount";
                logObj.Parms = new { sourceID = sourceID, countModel = countModel, remark = remark };

                LogEntity logEntity = new LogEntity();
                logEntity.F_ModuleName = "货位校对单";
                logEntity.F_Type = DbLogType.Submit.ToString();
                logEntity.F_Account = OperatorProvider.Provider.GetCurrent().UserCode;
                logEntity.F_NickName = OperatorProvider.Provider.GetCurrent().UserName;
                logEntity.F_Description = "重新校对";
                logEntity.F_Path = logObj.Path;
                logEntity.F_Param = logObj.Parms.ToJson();

                try
                {
                    /*************************************************/
                    WCSResult rst = new WCSResult();
                    T_LocCountEntity locCountEntity = db.FindEntity<T_LocCountEntity>(o => o.F_Id == sourceID);


                    IList<T_LocCountRecordEntity> locRecList = db.FindList<T_LocCountRecordEntity>(o => o.LocCountID == locCountEntity.F_Id);
                    if (locRecList.Any(o => o.CountState == "New" || o.CountState == "Counting"))
                    {
                        return Error("单据为新建或正在校对中。", "");
                    }


                    IList<T_LocCountRecordEntity> needReCountList = new List<T_LocCountRecordEntity>();
                    IList<T_LocCountRecordEntity> exceptionList = locRecList.Where(o => o.CountResult != "Inner_Empty"
                                                                                  && o.CountResult != "Inner_SameBoxCode").ToList();
                     if (countModel == "1") //全部
                    {
                        needReCountList = locRecList;
                    }
                    else if (countModel == "2") //仅异常
                    {

                        if (exceptionList.Count < 1)
                        {
                            return Error("没有异常记录。", "");
                        }

                        needReCountList = exceptionList;
                    }
                     
                    //还原异常数据
                    LocCountRecordController recController = new LocCountRecordController();
                    foreach (T_LocCountRecordEntity cell in exceptionList)
                    {
                        string msg = "";
                        bool isSuccess = recController.CancleRecord(db, cell.F_Id, "已新建记录", ref msg);
                        if (!isSuccess)
                        {
                            return Error(msg, msg);
                        }
                    }

                   

                    T_LocCountEntity newLocCountEntity = new T_LocCountEntity();
                    newLocCountEntity.Create();
                    newLocCountEntity.LocCountCode = T_CodeGenApp.GenNum("LocCountRule");
                    newLocCountEntity.AreaCode = locCountEntity.AreaCode;
                    newLocCountEntity.AreaID = locCountEntity.AreaID;
                    newLocCountEntity.AreaName = locCountEntity.AreaName;
                    newLocCountEntity.GenType = locCountEntity.GenType;
                    if (countModel == "2")
                    {
                        newLocCountEntity.IsAllCount = "false";
                    }
                    else
                    {
                        newLocCountEntity.IsAllCount = locCountEntity.IsAllCount;
                    }

                    newLocCountEntity.RefOrderCode = locCountEntity.RefOrderCode;
                    newLocCountEntity.Remark = remark;
                    newLocCountEntity.State = "New";
                    newLocCountEntity.TaskCellNum = locCountEntity.TaskCellNum;
                    db.Insert(newLocCountEntity);
                    db.SaveChanges();

                    IList<T_LocCountRecordEntity> ReCountRecNeedInsertList = new List<T_LocCountRecordEntity>();
                    foreach (T_LocCountRecordEntity cell in needReCountList)
                    {
                        T_LocCountRecordEntity newRec = new T_LocCountRecordEntity();
                        newRec.Create();
                        newRec.LocCountID = newLocCountEntity.F_Id;
                        newRec.AreaCode = cell.AreaCode;
                        newRec.AreaID = cell.AreaID;
                        newRec.AreaName = cell.AreaName;
                        newRec.AreaType = cell.AreaType;
                        newRec.BarCode = cell.BarCode;
                        newRec.ContainerID = cell.ContainerID;
                        newRec.ContainerKind = cell.ContainerKind;
                        newRec.CountState = "New";
                        newRec.ForbiddenState = cell.ForbiddenState;
                        newRec.GenType = cell.GenType;
                        newRec.IsArrive = "false";
                        newRec.IsConfirm = "false";
                        newRec.IsNeedBackWare = "true";
                        newRec.IsScanBack = "false";
                        newRec.LocationCode = cell.LocationCode;
                        newRec.LocationID = cell.LocationID;
                        newRec.LocState = cell.LocState;
                        newRec.Remark = cell.Remark;

                        ReCountRecNeedInsertList.Add(newRec);
                    }
                    db.BulkInsert<T_LocCountRecordEntity>(ReCountRecNeedInsertList);
                    db.BulkSaveChanges();
                    db.SaveChanges();
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


        #region 结束货位校对
        [HttpPost]
        [HandlerAjaxOnly]
        [ValidateAntiForgeryToken]
        public ActionResult StopLocCount(string locCountID)
        {
            using (var db = new RepositoryBase().BeginTrans())
            {
                LogObj logObj = new LogObj();
                logObj.Path = "LocCountController.StopLocCount";
                logObj.Parms = new { LocCountID = locCountID };

                LogEntity logEntity = new LogEntity();
                logEntity.F_ModuleName = "货位校对单";
                logEntity.F_Type = DbLogType.Submit.ToString();
                logEntity.F_Account = OperatorProvider.Provider.GetCurrent().UserCode;
                logEntity.F_NickName = OperatorProvider.Provider.GetCurrent().UserName;
                logEntity.F_Description = "结束校对";
                logEntity.F_Path = logObj.Path;
                logEntity.F_Param = logObj.Parms.ToJson();

                try
                {
                    /*************************************************/
                    WCSResult rst = new WCSResult();
                    T_LocCountEntity locCountEntity = db.FindEntity<T_LocCountEntity>(o => o.F_Id == locCountID);

                    if (locCountEntity.State == "Overing")
                    {
                        return Error("单据已处于结束中。", "");
                    }
                    if (locCountEntity.State == "Over")
                    {
                        return Error("单据已结束。", "");
                    }

                    IList<T_TaskEntity> taskList = new T_TaskApp().FindList(o => o.OrderCode == locCountEntity.LocCountCode).ToList();
                    if (taskList.Count < 1) //任务表中不存在该货位校对单的任务
                    {
                        locCountEntity.State = "Over";
                    }
                    else
                    {
                        locCountEntity.State = "Overing"; //此处标记为结束中，真正结束在任务完成后
                    }
                    db.Update<T_LocCountEntity>(locCountEntity);
                    db.SaveChanges();
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
