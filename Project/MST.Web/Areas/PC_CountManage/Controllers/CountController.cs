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

namespace MST.Web.Areas.PC_CountManage.Controllers
{
    public class CountController : ControllerBase
    {
        private static object lockObj = new object();
        private T_CountApp countApp = new T_CountApp();
        private T_CountDetailApp countDetailApp = new T_CountDetailApp();
        private ItemsDetailApp itemsDetailApp = new ItemsDetailApp();
        private T_ERPWarehouseApp erpWarehouseApp = new T_ERPWarehouseApp();
        private T_ContainerTypeApp containerTypeApp = new T_ContainerTypeApp();
        private T_ItemApp itemApp = new T_ItemApp();
        private T_LocationStateDetailApp locStateApp = new T_LocationStateDetailApp();

        /// <summary>
        /// 复盘界面
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [HandlerAuthorize]
        public virtual ActionResult ReCount()
        {
            return View();
        }

        #region 获取盘点单列表
        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetGridJson(Pagination pagination, string queryJson)
        {
            List<T_CountEntity> data = countApp.GetList(pagination, queryJson);
            IList<ItemsDetailEntity> enumAreaTypeList = itemsDetailApp.FindEnum<T_CountEntity>(o => o.AreaType).ToList();
            IList<ItemsDetailEntity> enumStateList = itemsDetailApp.FindEnum<T_CountEntity>(o => o.State).ToList();
            IList<ItemsDetailEntity> enumAuditStateList = itemsDetailApp.FindEnum<T_CountEntity>(o => o.AuditState).ToList();
            IList<ItemsDetailEntity> enumGenTypeList = itemsDetailApp.FindEnum<T_CountEntity>(o => o.GenType).ToList();

            List<CountModel> countModelList = new List<CountModel>();
            foreach (T_CountEntity item in data)
            {
                CountModel model = item.ToObject<CountModel>();
                model.AreaTypeName = enumAreaTypeList.FirstOrDefault(o => o.F_ItemCode == item.AreaType).F_ItemName;
                model.StateName = enumStateList.FirstOrDefault(o => o.F_ItemCode == item.State).F_ItemName;
                model.AuditStateName = enumAuditStateList.FirstOrDefault(o => o.F_ItemCode == item.AuditState).F_ItemName;
                model.GenTypeName = enumGenTypeList.FirstOrDefault(o => o.F_ItemCode == item.GenType).F_ItemName;

                if (!string.IsNullOrEmpty(item.ERPHouseCode))
                {
                    T_ERPWarehouseEntity t_ERPWarehouseEntity = erpWarehouseApp.FindEntity(o => o.ERPHouseCode == item.ERPHouseCode);
                    if (t_ERPWarehouseEntity != null)
                    {
                        model.ERPHouseName = t_ERPWarehouseEntity.ERPHouseName;
                    }
                }
                else
                {
                    model.ERPHouseName = "全部";
                }

                countModelList.Add(model);
            }

            var resultList = new
            {
                rows = countModelList,
                total = pagination.total,
                page = pagination.page,
                records = pagination.records
            };
            return Content(resultList.ToJson());
        }
        #endregion

        #region 查看盘点单
        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetFormJson(string keyValue)
        {
            var data = countApp.GetForm(keyValue);

            CountModel model = data.ToObject<CountModel>();
            model.AreaTypeName = itemsDetailApp.FindEnum<T_CountEntity>(o => o.AreaType).FirstOrDefault(o => o.F_ItemCode == data.AreaType).F_ItemName;
            model.StateName = itemsDetailApp.FindEnum<T_CountEntity>(o => o.State).FirstOrDefault(o => o.F_ItemCode == data.State).F_ItemName;
            model.AuditStateName = itemsDetailApp.FindEnum<T_CountEntity>(o => o.AuditState).FirstOrDefault(o => o.F_ItemCode == data.AuditState).F_ItemName;
            model.GenTypeName = itemsDetailApp.FindEnum<T_CountEntity>(o => o.GenType).FirstOrDefault(o => o.F_ItemCode == data.GenType).F_ItemName;

            if (!string.IsNullOrEmpty(data.ERPHouseCode))
            {
                T_ERPWarehouseEntity t_ERPWarehouseEntity = erpWarehouseApp.FindEntity(o => o.ERPHouseCode == data.ERPHouseCode);
                if (t_ERPWarehouseEntity != null)
                {
                    model.ERPHouseName = t_ERPWarehouseEntity.ERPHouseName;
                }
            }
            else
            {
                model.ERPHouseName = "全部";
            }

            return Content(model.ToJson());
        }
        #endregion

        #region 新建/修改盘点单
        [HttpPost]
        [HandlerAjaxOnly]
        [ValidateAntiForgeryToken]
        public ActionResult SubmitForm(T_CountEntity CountEntity, string keyValue)
        {
            using (var db = new RepositoryBase().BeginTrans())
            {
                LogObj logObj = new LogObj();
                logObj.Path = "CountController.SubmitForm";
                logObj.Parms = new { CountEntity = CountEntity, keyValue = keyValue };

                LogEntity logEntity = new LogEntity();
                logEntity.F_ModuleName = "盘点单";
                logEntity.F_Type = DbLogType.Submit.ToString();
                logEntity.F_Account = OperatorProvider.Provider.GetCurrent().UserCode;
                logEntity.F_NickName = OperatorProvider.Provider.GetCurrent().UserName;
                logEntity.F_Description = "保存盘点单";
                logEntity.F_Path = logObj.Path;
                logEntity.F_Param = logObj.Parms.ToJson();

                try
                {
                    /*************************************************/

                    if (CountEntity.State != "New") return Error("单据不是新建状态", "");
                    T_CountEntity countBefore = countApp.FindEntity(o => o.F_Id == keyValue);

                    if (string.IsNullOrEmpty(CountEntity.RefOrderCode)) CountEntity.RefOrderCode = CountEntity.CountCode;
                    CountEntity.GenType = "MAN";
                    CountEntity.CountMode = "GoodsToPeople";
                    CountEntity.CountMethod = "ByItem";
                    CountEntity.F_DeleteMark = false;
                    if (!string.IsNullOrEmpty(CountEntity.Remark)) CountEntity.Remark = CountEntity.Remark.Replace("\n", " ");

                    if (string.IsNullOrEmpty(keyValue))
                    {
                        CountEntity.F_Id = Guid.NewGuid().ToString();
                        CountEntity.AuditState = "WaitAudit";
                        CountEntity.AuditResult = "WaitApply";

                        db.Insert<T_CountEntity>(CountEntity);
                    }
                    else
                    {
                        CountEntity.F_Id = keyValue;
                        /// 已添加盘点明细，禁止修改盘点单ERP仓库和指定方式
                        var detail = db.FindList<T_CountDetailEntity>(o => o.CountID == CountEntity.F_Id);
                        if (detail.Count != 0)
                        {
                            if (CountEntity.ERPHouseCode != countBefore.ERPHouseCode)
                            {
                                db.RollBack();
                                return Error("已添加盘点明细，禁止修改盘点单ERP仓库", "操作失败");
                            }
                            else if (CountEntity.CountMethod != countBefore.CountMethod)
                            {
                                db.RollBack();
                                return Error("已添加盘点明细，禁止修改盘点单指定方式", "操作失败");
                            }
                        }

                        db.Update<T_CountEntity>(CountEntity);
                    }

                    db.CommitWithOutRollBack();

                    /**************************************************/

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

        #region 获取ERP仓库列表
        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetERPWarehouseList()
        {
            List<T_ERPWarehouseEntity> enumERPWarehouseList = new List<T_ERPWarehouseEntity>();
            var result = erpWarehouseApp.GetList();
            foreach (var item in result) enumERPWarehouseList.Add(item);
            return Content(enumERPWarehouseList.ToJson());
        }
        #endregion

        #region 删除盘点单
        [HttpPost]
        [HandlerAjaxOnly]
        [HandlerAuthorize]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteForm(string keyValue)
        {
            using (var db = new RepositoryBase().BeginTrans())
            {
                LogObj logObj = new LogObj();
                logObj.Path = "CountController.DeleteForm";
                logObj.Parms = new { keyValue = keyValue };

                LogEntity logEntity = new LogEntity();
                logEntity.F_ModuleName = "盘点单";
                logEntity.F_Type = DbLogType.Delete.ToString();
                logEntity.F_Account = OperatorProvider.Provider.GetCurrent().UserCode;
                logEntity.F_NickName = OperatorProvider.Provider.GetCurrent().UserName;
                logEntity.F_Description = "删除盘点单";
                logEntity.F_Path = logObj.Path;
                logEntity.F_Param = logObj.Parms.ToJson();

                try
                {
                    /*************************************************/
                    T_CountEntity count = db.FindEntity<T_CountEntity>(o => o.F_Id == keyValue);
                    if (count.State != "New") return Error("非新建状态不可删除", "");

                    List<T_CountDetailEntity> detailList = db.FindList<T_CountDetailEntity>(o => o.CountID == count.F_Id);
                    foreach (T_CountDetailEntity detail in detailList)
                    {
                        db.Delete<T_CountDetailEntity>(detail);
                    }
                    db.SaveChanges();

                    db.Delete<T_CountEntity>(count);
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

        #region 开始盘点
        [HttpPost]
        [HandlerAjaxOnly]
        [ValidateAntiForgeryToken]
        public ActionResult CountOnOff(string countID)
        {
            using (var db = new RepositoryBase().BeginTrans())
            {
                LogObj logObj = new LogObj();
                logObj.Path = "CountController.CountOnOff";
                logObj.Parms = new { countID = countID };

                LogEntity logEntity = new LogEntity();
                logEntity.F_ModuleName = "盘点单";
                logEntity.F_Type = DbLogType.Submit.ToString();
                logEntity.F_Account = OperatorProvider.Provider.GetCurrent().UserCode;
                logEntity.F_NickName = OperatorProvider.Provider.GetCurrent().UserName;
                logEntity.F_Description = "开始盘点";
                logEntity.F_Path = logObj.Path;
                logEntity.F_Param = logObj.Parms.ToJson();

                try
                {
                    lock (lockObj)
                    {
                        T_OffLineDownApp offLineDownApp = new T_OffLineDownApp();
                        bool IsHaveOffLine = offLineDownApp.IsHaveOff(db);
                        if (IsHaveOffLine)
                        {
                            return Error("存在未处理的离线数据", "");
                        }

                        IList<string> TaskListNo = new List<string>();
                        AjaxResult res = CountOnOff(db, countID, ref TaskListNo);
                        if ((ResultType)res.state != ResultType.success)
                        {
                            db.RollBack();
                            return Error(res.message, "");
                        }
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

                    /**************************************************/
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

        /// <summary>
        /// 开始物料盘点
        /// </summary>
        /// <param name="db"></param>
        /// <param name="countID"></param>
        /// <param name="TaskListNo">产生的任务编码</param>
        /// <returns></returns>
        public AjaxResult CountOnOff(IRepositoryBase db, string countID, ref IList<string> TaskListNo)
        {
            AjaxResult res = new AjaxResult();
            T_CountEntity count = db.FindEntity<T_CountEntity>(o => o.F_Id == countID);
            if (count == null)
            {
                res.state = ResultType.error;
                res.message = "盘点单据不存在。";
                return res;
            }
            if (count.State == "WaitAudit" || count.State == "WaitResult")
            {
                res.state = ResultType.error;
                res.message = "单据已结束盘点。";
                return res;
            }
            else if (count.State == "Over")
            {
                res.state = ResultType.error;
                res.message = "单据已结束盘点。";
                return res;
            }
            else if (count.State == "Outing")
            {
                res.state = ResultType.error;
                res.message = "单据已在出库中。";
                return res;
            }
            else if (count.State == "Counting")
            {
                res.state = ResultType.error;
                res.message = "单据正在盘点。";
                return res;
            }

            List<T_CountDetailEntity> countDetailList = db.FindList<T_CountDetailEntity>(o => o.CountID == countID).ToList();
            if (countDetailList.Count == 0)
            {
                res.state = ResultType.error;
                res.message = "盘点单明细为空。";
                return res;
            }


            /// 获取盘点明细的站台列表
            var stationList = countDetailApp.FindList(o => o.CountID == countID).GroupBy(x => new { x.StationID }).Select(a => a.FirstOrDefault()).ToList();
            foreach (var s in stationList)
            {
                if (string.IsNullOrEmpty(s.StationID)) continue;
                T_StationEntity station = db.FindEntity<T_StationEntity>(o => o.F_Id == s.StationID);
                if (station == null)
                {
                    res.state = ResultType.error;
                    res.message = $"未找到盘点站台 {station.StationName}";
                    return res;
                }
                if (!string.IsNullOrEmpty(station.CurOrderID))
                {
                    res.state = ResultType.error;
                    res.message = $"站台 {station.StationName} 已绑定单据";
                    return res;
                }
                if (station.CurOrderID == count.F_Id)
                {
                    res.state = ResultType.error;
                    res.message = "站台已绑定该单据。";
                    return res;
                }

                station.CurOrderID = count.F_Id;
                station.OrderType = "Count";
                db.Update<T_StationEntity>(station);
            }

            //过滤区域、巷道、行
            IQueryable<T_AreaEntity> areaQuery = db.IQueryable<T_AreaEntity>(o => o.IsEnable == "true"); //启用的区域
            IQueryable<T_DevRowEntity> wayQuery = db.IQueryable<T_DevRowEntity>(o => o.IsEnable == "true");//启用的巷道ID
            IQueryable<T_RowLineEntity> lineQuery = db.IQueryable<T_RowLineEntity>(o => o.IsEnable == "true"); //启用的行

            var wayAndLineQuery = areaQuery.Join(wayQuery, m => m.F_Id, n => n.AreaID, (m, n) => new { AreaID = n.AreaID, WayID = n.F_Id }).Join(lineQuery, m => m.WayID, n => n.DevRowID, (m, n) => new { m.AreaID, n.Line });

            //过滤货位,获取所有可用货位
            IList<string> locNoList = db.IQueryable<T_LocationEntity>(o =>
            o.State == "Stored"
            && (o.ForbiddenState == "Normal" || o.ForbiddenState == "OnlyOut")
            ).Join(wayAndLineQuery, m => new { m.AreaID, m.Line }, n => new { n.AreaID, n.Line }, (m, n) => m).Select(o => o.LocationCode).ToList();

            Dictionary<string, T_TaskEntity> dictTask = new Dictionary<string, T_TaskEntity>();
            IList<T_StationEntity> tagStationsList = db.FindList<T_StationEntity>(o=>true).ToList();
            List<T_ContainerDetailEntity> containerDetailAllList = db.FindList<T_ContainerDetailEntity>(o => true).ToList();
            IList<T_ContainerEntity> containerList = db.FindList<T_ContainerEntity>(o => true).ToList();
            IList<T_LocationEntity> locList = db.FindList<T_LocationEntity>(o => true).ToList();

            foreach (var countDetail in countDetailList)
            {
                T_StationEntity tagStation = tagStationsList.FirstOrDefault(o => o.F_Id == countDetail.StationID);

                List<T_ContainerDetailEntity> containerDetailList = new List<T_ContainerDetailEntity>();
                /// 判断无批号物料库存
                if (string.IsNullOrEmpty(countDetail.Lot))
                {
                    if (!string.IsNullOrEmpty(count.ERPHouseCode))
                    {
                        containerDetailList = containerDetailAllList.Where(o => o.ItemID == countDetail.ItemID && string.IsNullOrEmpty(o.Lot) && o.ERPWarehouseCode == count.ERPHouseCode).ToList();
                    }
                    else
                    {
                        containerDetailList = containerDetailAllList.Where(o => o.ItemID == countDetail.ItemID && string.IsNullOrEmpty(o.Lot)).ToList();
                    }

                }
                else
                {
                    if (!string.IsNullOrEmpty(count.ERPHouseCode))
                    {
                        containerDetailList = containerDetailAllList.Where(o => o.ItemID == countDetail.ItemID && o.Lot == countDetail.Lot && o.ERPWarehouseCode == count.ERPHouseCode).ToList();
                    }
                    else
                    {
                        containerDetailList = containerDetailAllList.Where(o => o.ItemID == countDetail.ItemID && o.Lot == countDetail.Lot).ToList();
                    }
                }

                /// 判断明细是否存在库存
                if (containerDetailList.Count == 0)
                {
                    res.state = ResultType.error;
                    res.message = $"物料[{countDetail.ItemCode}],批号[{countDetail.Lot}] 未找到库存";
                    return res;
                }

                /// 校验容器是否待出库待质检，是否未在库
                foreach (var item in containerDetailList)
                {
                    if (item.OutQty != 0 || item.CheckQty != 0 || item.LocationNo == FixType.Station.StationOut_BigItem.ToString() || item.LocationNo == FixType.Station.StationOut_Normal.ToString())
                    {
                        res.state = ResultType.error;
                        res.message = "盘点单物料有其它任务。";
                        return res;
                    }
                    //if (item.CheckState != "Qua" && item.CheckState != "UnNeed")
                    //{
                    //    res.state = ResultType.error;
                    //    res.message = "物料质检状态不可盘";
                    //    return res;
                    //}
                }

                //校验货位所处区域、巷道、行 是否被禁用
                foreach (var item in containerDetailList)
                {
                    if (!locNoList.Contains(item.LocationNo))
                    {
                        res.state = ResultType.error;
                        res.message = "货位：" + item.LocationNo + "区域、巷道或行 为禁用状态";
                        return res;
                    }
                }

                /// 检验储位是否被占用
                IList<string> notStoredLocations = locList.Where(o => o.State != "Stored").Select(o => o.LocationCode).Distinct().ToArray();
                IList<string> forbidLocations = locList.Where(o => o.ForbiddenState != "Normal").Select(o => o.LocationCode).Distinct().ToArray();

                IList<T_CountRecordEntity> insertRecordList = new List<T_CountRecordEntity>();
                IList<T_LocationEntity> updateLoc = new List<T_LocationEntity>();
                IList<T_ContainerDetailEntity> updateConDetailList = new List<T_ContainerDetailEntity>();
                
                foreach (var cd in containerDetailList)
                {
                    /// 冻结盘点库存
                    cd.IsCountFreeze = "true";
                    //db.Update<T_ContainerDetailEntity>(cd);
                    updateConDetailList.Add(cd);
                    T_ContainerEntity container = containerList.FirstOrDefault(o => o.F_Id == cd.ContainerID);
                    T_LocationEntity srcLocation = locList.FirstOrDefault(o => o.F_Id == cd.LocationID);

                    if (notStoredLocations.Contains(srcLocation.LocationCode))
                    {
                        res.state = ResultType.error;
                        res.message = $"储位不是已存储状态：{srcLocation.LocationCode}";
                        return res;
                    }
                    if (forbidLocations.Contains(srcLocation.LocationCode))
                    {
                        res.state = ResultType.error;
                        res.message = $"储位是锁定状态：{srcLocation.LocationCode}";
                        return res;
                    }

                    T_TaskEntity task = new T_TaskEntity();

                    T_CountRecordEntity countRecord = new T_CountRecordEntity();
                    countRecord.F_Id = Guid.NewGuid().ToString();
                    countRecord.SEQ = countDetail.SEQ;
                    countRecord.CountID = countDetail.CountID;
                    countRecord.CountDetailID = countDetail.F_Id;
                    countRecord.ContainerID = cd.ContainerID;
                    countRecord.ContainerDetailID = cd.F_Id; /// 库存明细ID
                    countRecord.StationID = countDetail.StationID;
                    countRecord.BarCode = container.BarCode;
                    countRecord.ItemBarCode = cd.ItemBarCode;
                    countRecord.ItemID = cd.ItemID;
                    countRecord.ItemCode = cd.ItemCode;
                    countRecord.ItemName = cd.ItemName;
                    countRecord.SupplierUserID = cd.SupplierID;
                    countRecord.SupplierUserName = cd.SupplierName;
                    countRecord.ERPCode = cd.ERPWarehouseCode;
                    countRecord.ProductDate = cd.ProductDate;
                    countRecord.Factory = cd.Factory;
                    countRecord.LocationID = cd.LocationID;
                    countRecord.LocationCode = cd.LocationNo;
                    countRecord.TagLocationID = countDetail.StationID;
                    countRecord.Lot = cd.Lot;
                    countRecord.Qty = cd.Qty;
                    countRecord.Spec = cd.Spec;
                    countRecord.OverdueDate = cd.OverdueDate;
                    countRecord.ItemUnitText = cd.ItemUnitText;
                    countRecord.CreatorUserName = OperatorProvider.Provider.GetCurrent().UserName;
                    countRecord.F_CreatorUserId = OperatorProvider.Provider.GetCurrent().UserId;
                    countRecord.IsItemMark = cd.IsItemMark; /// 是否物料贴标
                    countRecord.IsAdd = "false";
                    countRecord.IsArrive = "false";
                    countRecord.IsNeedBackWare = "false";
                    countRecord.IsScanBack = "false";

                    countDetail.Qty += cd.Qty;

                    /// 判断盘点明细物料, 手动/自动
                    if (cd.ItemID == countDetail.ItemID && (cd.Lot == countDetail.Lot || (string.IsNullOrEmpty(countDetail.Lot) && string.IsNullOrEmpty(cd.Lot))))
                    {
                        countRecord.AuditState = "WaitAudit";
                        countRecord.GenType = "MAN";
                        countRecord.TransState = "WaittingTrans";
                    }
                    else
                    {
                        countRecord.CountState = "NoNeed";
                        countRecord.AuditState = "Pass";
                        countRecord.GenType = "Auto";
                        countRecord.TransState = "UnNeedTrans";
                    }

                    /// 是否出库盘点 判断盘点任务
                    if (container.ContainerKind == "Box")
                    {
                        countRecord.IsOutCount = "false";
                        countRecord.CountState = "Counting";
                        task.TaskInOutType = "AGVCountType";
                        task.TaskType = "TaskType_CountAGV";  /// 纸箱，AGV在库盘点
                        task.TagLocationID = srcLocation.F_Id;    /// 目标地址ID
                        task.TagLocationCode = srcLocation.LocationCode;    /// 目标地址编码
                        task.TagWCSLocCode = srcLocation.WCSLocCode;
                        if (stationList != null && stationList.Count > 0)
                        {
                            task.ApplyStationID = stationList[0].F_Id;
                        }
                    }
                    else
                    {
                        countRecord.IsOutCount = "true";
                        countRecord.CountState = "Outing";
                        task.TaskInOutType = "OutType";
                        task.TaskType = "TaskType_CountOut"; /// 料箱 料架，出库盘点
                        task.TagLocationID = tagStation.F_Id;    /// 目标地址ID
                        task.TagLocationCode = tagStation.LeaveAddress;    /// 目标地址编码
                        task.TagWCSLocCode = tagStation.LeaveAddress;
                        task.ApplyStationID = tagStation.F_Id;
                    }
                    //db.Insert<T_CountRecordEntity>(countRecord);
                    insertRecordList.Add(countRecord);

                    /// 待出库数量、质检数量。不出库。终止生成盘点任务
                    //List<string> res = forbidContainerList.Where(o => o == item.ContainerID).ToList();
                    //if (res.Count != 0) return Error("盘点单有出库物料", "");

                    /// 生成盘点任务
                    task.F_Id = Guid.NewGuid().ToString();
                    task.TaskNo = T_CodeGenApp.GenNum("TaskRule");
                    task.ContainerID = container.F_Id;
                    task.BarCode = container.BarCode;
                    task.ContainerType = container.ContainerType;
                    task.SrcLocationID = srcLocation.F_Id;  /// 起始地址ID
                    task.SrcLocationCode = srcLocation.LocationCode;    /// 起始地址编码
                    task.SrcWCSLocCode = srcLocation.WCSLocCode;
                    task.Level = 30; /// 盘点
                    task.State = "New";
                    task.SEQ = countDetail.SEQ;
                    task.IsWcsTask = "true";
                    task.IsCanExec = "true";
                    task.OrderType = "Count";
                    task.OrderID = count.F_Id;
                    task.OrderDetailID = countDetail.F_Id;
                    task.OrderCode = count.CountCode;
                    task.F_DeleteMark = false;

                    if (!dictTask.ContainsKey(task.SrcLocationID + task.BarCode))
                    {
                        dictTask.Add(task.SrcLocationID + task.BarCode, task);
                        /// 锁定货位 不同批号不在同一货位
                        srcLocation.State = "Out";
                        //db.Update<T_LocationEntity>(srcLocation);
                        updateLoc.Add(srcLocation);
                        /// 货位状态变更记录
                        //locStateApp.SyncLocState(db, srcLocation, "OutType", "Count", "Stored", "Out", task.TaskNo);
                    }
                }
                db.BulkUpdate(updateConDetailList);
                db.BulkInsert(insertRecordList);
                db.BulkUpdate(updateLoc);
                db.BulkSaveChanges();
                db.SaveChanges();

                T_ItemEntity itemEntity = itemApp.FindEntity(o => o.F_Id == countDetail.ItemID);
                T_ContainerTypeEntity containerType = containerTypeApp.FindEntity(o => o.ContainerTypeCode == itemEntity.ContainerType);
                string containerKind = containerType.ContainerKind;
                if (containerKind == "Rack" || containerKind == "Plastic")
                {
                    countDetail.CountState = "Outing"; /// 盘点明细状态：出库中
                }
                else if (containerKind == "Box")
                {
                    countDetail.CountState = "Counting"; /// 盘点明细状态：盘点中，在库AGV盘
                }
                else
                {
                    res.state = ResultType.error;
                    res.message = "物料的容器类型未知:" + itemEntity.ItemCode;
                    return res;
                }
                db.Update<T_CountDetailEntity>(countDetail);
            }

            

            foreach (var item in dictTask)
            {
                //db.Insert<T_TaskEntity>(item.Value);
                TaskListNo.Add(item.Value.TaskNo);
            }
            IList<T_TaskEntity> insertTask = dictTask.Values.Select(o => o).ToList();
            db.BulkInsert(insertTask);
            db.BulkSaveChanges();
            db.SaveChanges();

            List<T_CountDetailEntity> detailList = db.FindList<T_CountDetailEntity>(o => o.CountID == countID && o.CountState == "Outing");
            if (detailList.Count == 0)
            {
                count.State = "Counting";    /// 全部是 在库盘点
            }
            else
            {
                count.State = "Outing"; /// 盘点单状态：出库中
            }

            db.Update<T_CountEntity>(count);
            db.SaveChanges();

            res.state = ResultType.success;
            return res;
        }
        #endregion

        #region 设置强制完成盘点
        [HttpPost]
        [HandlerAjaxOnly]
        [ValidateAntiForgeryToken]
        public ActionResult CountOver(string countID)
        {
            using (var db = new RepositoryBase().BeginTrans())
            {
                LogObj logObj = new LogObj();
                logObj.Path = "CountController.CountOver";
                logObj.Parms = new { CountID = countID };

                LogEntity logEntity = new LogEntity();
                logEntity.F_ModuleName = "盘点单";
                logEntity.F_Type = DbLogType.Submit.ToString();
                logEntity.F_Account = OperatorProvider.Provider.GetCurrent().UserCode;
                logEntity.F_NickName = OperatorProvider.Provider.GetCurrent().UserName;
                logEntity.F_Description = "强制完成盘点";
                logEntity.F_Path = logObj.Path;
                logEntity.F_Param = logObj.Parms.ToJson();

                try
                {
                    /*************************************************/
                    T_CountEntity entity = db.FindEntity<T_CountEntity>(o => o.F_Id == countID);
                    if (entity == null) return Error("盘点单据不存在", "");
                    if (entity.State == "Over") return Error("盘点单据已完成", "");
                    if (entity.State == "New") return Error("盘点单据为新建状态", "");

                    IList<T_TaskEntity> task = db.FindList<T_TaskEntity>(o => o.OrderCode == entity.CountCode);
                    if (task.Count > 0)
                    {
                        return Error("存在未结束的任务", "");
                    }

                    entity.State = "Over";
                    db.Update<T_CountEntity>(entity);
                    db.SaveChanges();

                    /// 强制完成所有盘点单明细
                    List<T_CountDetailEntity> countDetailList = db.FindList<T_CountDetailEntity>(o => o.CountID == entity.F_Id).ToList();
                    foreach (T_CountDetailEntity item in countDetailList)
                    {
                        item.CountState = "Over";
                        db.Update<T_CountDetailEntity>(item);
                    }

                    /// 强制完成所有盘点单记录
                    List<T_CountRecordEntity> countRecordList = db.FindList<T_CountRecordEntity>(o => o.CountID == entity.F_Id).ToList();
                    foreach (T_CountRecordEntity item in countRecordList)
                    {
                        item.CountState = "Over";
                        db.Update<T_CountRecordEntity>(item);

                        //还原库存盘点冻结状态为正常
                        T_ContainerDetailEntity conDetail = db.FindEntity<T_ContainerDetailEntity>(o => o.F_Id == item.ContainerDetailID);
                        conDetail.IsCountFreeze = "false";
                        db.Update<T_ContainerDetailEntity>(conDetail);

                        db.SaveChanges();
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

                    /**************************************************/

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

        #region 提交盘点结果审核
        [HttpPost]
        [HandlerAjaxOnly]
        [ValidateAntiForgeryToken]
        public ActionResult SubmitCountAudit(string countID)
        {
            using (var db = new RepositoryBase().BeginTrans())
            {
                LogObj logObj = new LogObj();
                logObj.Path = "CountController.SubmitCountAudit";
                logObj.Parms = new { CountID = countID };

                LogEntity logEntity = new LogEntity();
                logEntity.F_ModuleName = "盘点单";
                logEntity.F_Type = DbLogType.Submit.ToString();
                logEntity.F_Account = OperatorProvider.Provider.GetCurrent().UserCode;
                logEntity.F_NickName = OperatorProvider.Provider.GetCurrent().UserName;
                logEntity.F_Description = "提交盘点审核";
                logEntity.F_Path = logObj.Path;
                logEntity.F_Param = logObj.Parms.ToJson();

                try
                {
                    /*************************************************************/
                    T_CountEntity count = db.FindEntity<T_CountEntity>(countID);
                    if (count == null) return Error("盘点单据不存在", "");
                    if (count.State == "Over") return Error("盘点单据已结束", "");
                    if (count.State == "WaitResult" || count.AuditState != "WaitAudit") return Error("盘点单据已提交审核", "");
                    if (count.State != "WaitAudit") return Error("盘点单据未结束", "");

                    List<T_CountDetailEntity> detailList = db.FindList<T_CountDetailEntity>(o => o.CountID == count.F_Id);
                    if (detailList.Any(o => o.CountState != "WaitAudit")) return Error("盘点单据明细未全部结束", "");

                    List<T_CountRecordEntity> countRecList = db.FindList<T_CountRecordEntity>(o => o.CountID == countID && (o.CountResult == "Inner_DiffBoxCode"
                    || o.CountResult == "Inner_MoreBoxCode")).ToList();
                    if(countRecList.Count>0)
                    {
                        return Error("请先处理箱码不一致或多余箱码再提交审核", "");
                    }

                    ERPResult erpRes = new ERPPost().PostCountData(db, count.F_Id);
                    if (erpRes.IsSuccess)
                    {
                        db.CommitWithOutRollBack();
                    }
                    else
                    {
                        db.RollBack();
                        return Error(erpRes.FailMsg, "");
                    }

                    logObj.Message = "操作成功";
                    LogFactory.GetLogger().Info(logObj);

                    logEntity.F_Result = true;
                    new LogApp().WriteDbLog(logEntity);

                    return Success("操作成功");

                    /*************************************************************/
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

        #region 确认应用盘点审核结果
        [HttpPost]
        [HandlerAjaxOnly]
        [ValidateAntiForgeryToken]
        public ActionResult ConfirmCountAudit(string countID)
        {
            using (var db = new RepositoryBase().BeginTrans())
            {
                LogObj logObj = new LogObj();
                logObj.Path = "CountController.ConfirmCountAudit";
                logObj.Parms = new { CountID = countID };

                LogEntity logEntity = new LogEntity();
                logEntity.F_ModuleName = "盘点审核";
                logEntity.F_Type = DbLogType.Submit.ToString();
                logEntity.F_Account = OperatorProvider.Provider.GetCurrent().UserCode;
                logEntity.F_NickName = OperatorProvider.Provider.GetCurrent().UserName;
                logEntity.F_Description = "确认应用盘点审核结果";
                logEntity.F_Path = logObj.Path;
                logEntity.F_Param = logObj.Parms.ToJson();

                try
                {
                    /*************************************************************/
                    T_CountEntity count = db.FindEntity<T_CountEntity>(countID);
                    if (count == null) return Error("盘点单据不存在", "");
                    if (count.State == "Over") return Error("盘点单据已结束", "");
                    if (count.State != "WaitResult") return Error("盘点单据不是待审核反馈状态", "");

                    List<T_CountResultEntity> resultListAll = db.FindList<T_CountResultEntity>(o => o.RefOrderCode == count.RefOrderCode);
                    if (resultListAll.Count == 0) return Error("单据未收到审核结果", "");
                    if (resultListAll.All(o => o.IsUsed == "true")) return Error("单据已应用审核结果", "");

                    IList<T_CountDetailEntity> detailListAll = db.FindList<T_CountDetailEntity>(o => o.CountID == count.F_Id).ToList();
                    IList<T_CountRecordEntity> recordListAll = db.FindList<T_CountRecordEntity>(o => o.CountID == count.F_Id).ToList();
                    bool isChangeCD = false;

                    /// 应用审核结果
                    foreach (T_CountResultEntity result in resultListAll)
                    {
                        result.IsUsed = "true";

                        T_CountDetailEntity detail = detailListAll.FirstOrDefault(o => o.ItemID == result.ItemID && o.Lot == result.Lot && o.SEQ == result.SEQ);
                        if (detail == null) return Error($"不存在盘点明细：物料 {result.ItemName},批号 {result.Lot}", "");

                        result.CountID = detail.CountID;
                        result.CountDetailID = detail.F_Id;
                        db.Update<T_CountResultEntity>(result);
                        db.SaveChanges();

                        List<T_CountRecordEntity> recordList = recordListAll.Where(o => o.ItemID == result.ItemID && (o.Lot == result.Lot || (string.IsNullOrEmpty(o.Lot) && string.IsNullOrEmpty(result.Lot)))).ToList();
                        foreach (T_CountRecordEntity rec in recordList)
                        {
                            /// 更新盘点记录审核状态
                            rec.AuditState = result.CountResult;
                            db.Update<T_CountRecordEntity>(rec);

                            /// 盘点解冻
                            T_ContainerDetailEntity cd = db.FindEntity<T_ContainerDetailEntity>(o => o.F_Id == rec.ContainerDetailID);
                            if (cd != null)
                            {
                                cd.IsCountFreeze = "false";
                                db.Update<T_ContainerDetailEntity>(cd);
                            }

                            /// 根据结果更新库存
                            if (result.CountResult == "Pass")
                            {
                                string curCountResult = rec.CountResult;
                                switch (curCountResult)
                                {
                                    case "Inner_SameBoxCode":  /// 箱码一致（正常库存，不做任何修改）
                                        break;
                                    case "Inner_Empty":        /// 正常空货位（正常，不做任何修改）
                                        break;
                                    case "Inner_DiffBoxCode": /// 箱码不一致（修改库存箱码） /// 待完善 TODO
                                        {
                                            isChangeCD = true;
                                            cd.ItemBarCode = rec.FactBarCode;

                                            T_ContainerEntity containerCur = db.FindEntity<T_ContainerEntity>(o => o.BarCode == rec.FactBarCode && o.F_DeleteMark == false);
                                            T_ReceiveRecordEntity receiveRecord = db.FindEntity<T_ReceiveRecordEntity>(o => o.F_Id == cd.ReceiveRecordID);
                                            T_ItemEntity item = db.FindEntity<T_ItemEntity>(o => o.F_Id == rec.ItemID);
                                            T_ContainerTypeEntity containerTypeEntity = db.FindEntity<T_ContainerTypeEntity>(o => o.ContainerTypeCode == item.ContainerType);
                                            T_AreaEntity areaEntnty = db.FindEntity<T_AreaEntity>(o => o.F_Id == receiveRecord.AreaID);
                                            if (containerCur == null) /// 新容器
                                            {
                                                containerCur = new T_ContainerEntity();
                                                containerCur.F_Id = Guid.NewGuid().ToString();
                                                containerCur.BarCode = rec.FactBarCode;
                                                containerCur.ContainerType = containerTypeEntity.ContainerTypeCode;
                                                containerCur.ContainerKind = containerTypeEntity.ContainerKind;
                                                containerCur.IsContainerVir = "0";

                                                containerCur.AreaID = areaEntnty.F_Id;
                                                containerCur.AreaCode = areaEntnty.AreaCode;
                                                containerCur.AreaName = areaEntnty.AreaName;
                                                containerCur.F_DeleteMark = false;
                                                db.Insert<T_ContainerEntity>(containerCur);
                                            }
                                            else
                                            {
                                                if (containerCur.F_DeleteMark == true)
                                                {
                                                    containerCur.BarCode = rec.FactBarCode;
                                                    containerCur.ContainerType = containerTypeEntity.ContainerTypeCode;
                                                    containerCur.ContainerKind = containerTypeEntity.ContainerKind;
                                                    containerCur.IsContainerVir = "0";
                                                    containerCur.AreaID = areaEntnty.F_Id;
                                                    containerCur.AreaCode = areaEntnty.AreaCode;
                                                    containerCur.AreaName = areaEntnty.AreaName;
                                                    containerCur.F_DeleteMark = false;
                                                    db.Update<T_ContainerEntity>(containerCur);
                                                }
                                                else
                                                {
                                                    return Error("容器被占用", "");
                                                }
                                            }


                                            cd.ContainerID = containerCur.F_Id;
                                            cd.BarCode = containerCur.BarCode;
                                            db.Update<T_ContainerDetailEntity>(cd);
                                        }
                                        break;
                                    case "Inner_MoreBoxCode": /// 多余箱码（新增库存） /// 待完善 TODO
                                        {
                                            isChangeCD = true;
                                            /// TODO
                                        }
                                        break;
                                    case "Inner_NotFindBoxCode": /// 未找到箱码（删除库存）
                                        {
                                            /// 清空货位
                                            T_LocationEntity loc = db.FindEntity<T_LocationEntity>(o => o.F_Id == cd.LocationID);
                                            loc.State = "Empty";
                                            db.Update<T_LocationEntity>(loc);

                                            /// 删除容器
                                            T_ContainerEntity container = db.FindEntity<T_ContainerEntity>(o => o.F_Id == cd.ContainerID);
                                            container.F_DeleteMark = true;
                                            db.Update<T_ContainerEntity>(container);

                                            /// 库存流水
                                            T_InOutDetailApp inOutDetailApp = new T_InOutDetailApp();
                                            inOutDetailApp.SyncInOutDetail(db, cd, "OutType", "Count", cd.Qty, cd.Qty, "");

                                            /// 删除库存
                                            isChangeCD = true;
                                            db.Delete<T_ContainerDetailEntity>(cd);
                                            db.SaveChanges();
                                        }
                                        break;
                                    case "Outer_Normal": /// 正常（正常库存，不做任何修改）
                                        break;
                                    case "Outer_LessQty": /// 少数量（修改库存数量。当盘点数量为0时，删除库存，变更空容器）
                                        {
                                            T_InOutDetailApp inOutDetailApp = new T_InOutDetailApp();

                                            isChangeCD = true;
                                            if (rec.CountQty == 0) /// 删除库存，可能变更为空容器
                                            {
                                                List<T_ContainerDetailEntity> cdList = db.FindList<T_ContainerDetailEntity>(o => o.BarCode == rec.BarCode && o.F_Id != cd.F_Id); /// 容器其它库存
                                                List<T_CountRecordEntity> noRecList = recordList.Where(o => o.BarCode == rec.BarCode && o.F_Id != rec.F_Id).ToList();   /// 容器其它盘点记录
                                                List<T_CountRecordEntity> otherRecList = recordList.Where(o => o.ItemBarCode == rec.ItemBarCode && o.F_Id != rec.F_Id && o.CountResult == "Outer_MoreItemBarcode").ToList();    /// 标签其它盘点记录

                                                /// 变更空容器，删除库存                                                                                                                                                                                                                            
                                                if (cdList.Count == 0 && noRecList.Count == 0 && otherRecList.Count == 0)
                                                {
                                                    /// 删除库存
                                                    inOutDetailApp.SyncInOutDetail(db, cd, "OutType", "Count", cd.Qty, cd.Qty, "");

                                                    /// 变更空容器
                                                    T_ContainerDetailEntity cdOne = db.FindEntity<T_ContainerDetailEntity>(o => o.BarCode == rec.BarCode);
                                                    if (cdOne.ItemCode != FixType.Item.EmptyPlastic.ToString() && cdOne.ItemCode != FixType.Item.EmptyRack.ToString()) /// 不是空容器
                                                    {
                                                        T_ItemEntity item = null;
                                                        T_ContainerEntity container = db.FindEntity<T_ContainerEntity>(o => o.F_Id == cdOne.ContainerID);
                                                        if (container.ContainerKind == "Plastic")   /// 料箱
                                                        {
                                                            item = db.FindEntity<T_ItemEntity>(o => o.ItemCode == FixType.Item.EmptyPlastic.ToString());
                                                        }
                                                        else if (container.ContainerKind == "Rack")
                                                        {
                                                            item = db.FindEntity<T_ItemEntity>(o => o.ItemCode == FixType.Item.EmptyRack.ToString());
                                                        }
                                                        if (item == null) return Error("未定义空容器物料", "");

                                                        cdOne.KindCode = item.KindCode;
                                                        cdOne.KindName = item.KindName;
                                                        cdOne.ItemID = item.F_Id;
                                                        cdOne.ItemCode = item.ItemCode;
                                                        cdOne.ItemName = item.ItemName;
                                                        cdOne.ItemBarCode = "";
                                                        cdOne.Qty = 1;
                                                        cdOne.OutQty = 0;
                                                        cdOne.CheckQty = 0;
                                                        cdOne.ItemUnitText = item.ItemUnitText;
                                                        cdOne.CheckState = "UnNeed";
                                                        cdOne.CheckDetailID = "";
                                                        cdOne.CheckID = "";
                                                        cdOne.State = "Normal";
                                                        cdOne.IsCheckFreeze = "false";
                                                        cdOne.IsCountFreeze = "false";
                                                        cdOne.Lot = "";
                                                        cdOne.Spec = "";
                                                        cdOne.ERPWarehouseCode = "";
                                                        cdOne.Factory = "";
                                                        cdOne.ProductDate = null;
                                                        cdOne.OverdueDate = null;
                                                        cdOne.SupplierID = "";
                                                        cdOne.SupplierCode = "";
                                                        cdOne.SupplierName = "";
                                                        cdOne.ReceiveRecordID = "";
                                                        //cdOne.IsSpecial = "false";
                                                        cdOne.IsItemMark = "";
                                                        cdOne.F_DeleteMark = false;
                                                        db.Update<T_ContainerDetailEntity>(cdOne);
                                                        db.SaveChanges();

                                                        /// 库存流水
                                                        inOutDetailApp.SyncInOutDetail(db, cdOne, "InType", "EmptyIn", 0, cdOne.Qty, "");
                                                    }
                                                }
                                                /// 新增一个空容器，不删除库存
                                                else if (cdList.Count == 0 && noRecList.Count == 0 && otherRecList.Count != 0)
                                                {
                                                    T_ContainerDetailEntity cdEmpty = db.FindEntity<T_ContainerDetailEntity>(o => o.BarCode == cd.BarCode);
                                                    if (cdEmpty.ItemCode != FixType.Item.EmptyPlastic.ToString() && cdEmpty.ItemCode != FixType.Item.EmptyRack.ToString()) /// 不是空容器
                                                    {
                                                        T_ItemEntity item = null;
                                                        T_ContainerEntity container = db.FindEntity<T_ContainerEntity>(o => o.F_Id == cd.ContainerID);
                                                        if (container.ContainerKind == "Plastic")   /// 料箱
                                                        {
                                                            item = db.FindEntity<T_ItemEntity>(o => o.ItemCode == FixType.Item.EmptyPlastic.ToString());
                                                        }
                                                        else if (container.ContainerKind == "Rack")
                                                        {
                                                            item = db.FindEntity<T_ItemEntity>(o => o.ItemCode == FixType.Item.EmptyRack.ToString());
                                                        }
                                                        if (item == null) return Error("未定义空容器物料", "");

                                                        cdEmpty = new T_ContainerDetailEntity();
                                                        cdEmpty.F_Id = Guid.NewGuid().ToString();
                                                        cdEmpty.AreaCode = cd.AreaCode;
                                                        cdEmpty.AreaID = cd.AreaID;
                                                        cdEmpty.AreaName = cd.AreaName;
                                                        cdEmpty.ContainerID = cd.ContainerID;
                                                        cdEmpty.ContainerKind = cd.ContainerKind;
                                                        cdEmpty.ContainerType = cd.ContainerType;
                                                        cdEmpty.BarCode = cd.BarCode;
                                                        cdEmpty.CheckDetailID = "";
                                                        cdEmpty.CheckID = "";
                                                        cdEmpty.CheckQty = 0;
                                                        cdEmpty.CheckState = "UnNeed";
                                                        cdEmpty.LocationID = cd.LocationID;
                                                        cdEmpty.LocationNo = cd.LocationNo;
                                                        cdEmpty.KindCode = item.KindCode;
                                                        cdEmpty.KindName = item.KindName;
                                                        cdEmpty.ItemBarCode = "";
                                                        cdEmpty.ItemCode = item.ItemCode;
                                                        cdEmpty.ItemID = item.F_Id;
                                                        cdEmpty.ItemName = item.ItemName;
                                                        cdEmpty.Qty = 1;
                                                        cdEmpty.OutQty = 0;
                                                        cdEmpty.ItemUnitText = item.ItemUnitText;
                                                        cdEmpty.State = "Normal";
                                                        cdEmpty.IsCheckFreeze = "false";
                                                        cdEmpty.IsCountFreeze = "false";
                                                        cdEmpty.Lot = "";
                                                        cdEmpty.Spec = "";
                                                        cdEmpty.ERPWarehouseCode = "";
                                                        cdEmpty.Factory = "";
                                                        cdEmpty.OverdueDate = null;
                                                        cdEmpty.ProductDate = null;
                                                        cdEmpty.SupplierCode = "";
                                                        cdEmpty.SupplierID = "";
                                                        cdEmpty.SupplierName = "";
                                                        cdEmpty.ReceiveRecordID = "";
                                                        //cdEmpty.IsSpecial = "false";
                                                        cdEmpty.IsItemMark = "";
                                                        cdEmpty.IsVirItemBarCode = "";
                                                        cdEmpty.ValidityDayNum = 0;
                                                        cdEmpty.F_DeleteMark = false;
                                                        db.Insert<T_ContainerDetailEntity>(cdEmpty);
                                                        db.SaveChanges();

                                                        /// 库存流水
                                                        inOutDetailApp.SyncInOutDetail(db, cdEmpty, "InType", "EmptyIn", 0, cdEmpty.Qty, "");
                                                    }
                                                }
                                                /// 删除库存
                                                else if (otherRecList.Count == 0 && (cdList.Count != 0 || noRecList.Count != 0))
                                                {
                                                    inOutDetailApp.SyncInOutDetail(db, cd, "OutType", "Count", cd.Qty, cd.Qty, "");
                                                    db.Delete<T_ContainerDetailEntity>(cd);
                                                    db.SaveChanges();
                                                }
                                                /// 不空，也不删
                                                else if (otherRecList.Count != 0 && (cdList.Count != 0 || noRecList.Count != 0))
                                                {
                                                    /// 不做任何处理
                                                }
                                            }
                                            else /// 减少库存数量
                                            {
                                                inOutDetailApp.SyncInOutDetail(db, cd, "OutType", "Count", cd.Qty, cd.Qty - rec.CountQty, "");
                                                cd.Qty = rec.CountQty;
                                                db.Update<T_ContainerDetailEntity>(cd);
                                            }
                                        }
                                        break;
                                    case "Outer_MoreItemBarcode": /// 多标签（新增库存）
                                        {
                                            /// 容器内有相同物料，直接获取原物料信息，作为新标签的基本信息
                                            /// 空容器不会出库，空容器物料无法在此步骤进行新建库存

                                            T_InOutDetailApp inOutDetailApp = new T_InOutDetailApp();
                                            isChangeCD = true;

                                            T_ItemEntity item = db.FindEntity<T_ItemEntity>(o => o.F_Id == rec.ItemID);

                                            #region 标签条码，需要贴标
                                            if (item.IsItemMark == "true")
                                            {
                                                T_MarkRuleEntity rule = db.FindEntity<T_MarkRuleEntity>(o => o.InBoundDetailID == detail.F_Id);
                                                if (rule != null)
                                                {
                                                    rule.OverPicNum = rule.OverPicNum + 1;
                                                    rule.Qty = rule.Qty + rec.CountQty;
                                                    db.Update<T_MarkRuleEntity>(rule);

                                                    T_MarkRecordEntity record = db.FindEntity<T_MarkRecordEntity>(o => o.BarCode == rec.ItemBarCode);
                                                    if (record == null)
                                                    {
                                                        T_SupplierEntity supplier = db.FindEntity<T_SupplierEntity>(o => o.F_Id == rec.SupplierUserID && o.F_DeleteMark == false);
                                                        record = new T_MarkRecordEntity();
                                                        record.F_Id = Guid.NewGuid().ToString();
                                                        record.MarkRuleID = rule.F_Id;
                                                        record.BarCode = rec.ItemBarCode;
                                                        record.SupplierCode = supplier.SupplierCode;
                                                        record.SupplierName = supplier.SupplierName;
                                                        record.ItemCode = item.ItemCode;
                                                        record.ItemName = item.ItemName;
                                                        record.ItemID = item.F_Id;
                                                        record.IsUsed = "true"; /// 自动默认使用
                                                        record.Qty = rec.CountQty;
                                                        record.Lot = detail.Lot;
                                                        record.RepairPicNum = 0;
                                                        record.PicNum = 1;
                                                        record.IsHandPrint = "false";
                                                        record.F_DeleteMark = false;
                                                        record.F_CreatorTime = DateTime.Now;
                                                        record.F_CreatorUserId = OperatorProvider.Provider.GetCurrent().UserId;
                                                        record.CreatorUserName = OperatorProvider.Provider.GetCurrent().UserName;
                                                        db.Insert<T_MarkRecordEntity>(record);
                                                    }
                                                    else
                                                    {
                                                        record.IsUsed = "true";
                                                        record.Qty = rec.CountQty;
                                                        db.Update<T_MarkRecordEntity>(record);
                                                    }
                                                }
                                            }
                                            #endregion

                                            #region 收货记录表
                                            T_ReceiveRecordEntity receiveOther = db.FindEntity<T_ReceiveRecordEntity>(o => o.BarCode == rec.BarCode); /// 获取一条相同物料的收货记录
                                            T_ReceiveRecordEntity receive = db.FindEntity<T_ReceiveRecordEntity>(o => o.BarCode == rec.BarCode && o.ItemBarCode == rec.ItemBarCode);
                                            if (receiveOther != null && receive == null)
                                            {
                                                receive = new T_ReceiveRecordEntity();
                                                receive.Create();
                                                receive.InBoundID = receiveOther.InBoundID;
                                                receive.InBoundDetailID = receiveOther.InBoundDetailID;
                                                receive.ReceiveStaionID = receiveOther.ReceiveStaionID;
                                                receive.ContainerType = receiveOther.ContainerType;
                                                receive.BarCode = receiveOther.BarCode;
                                                receive.ItemBarCode = rec.ItemBarCode;
                                                receive.ItemID = receiveOther.ItemID;
                                                receive.ItemCode = receiveOther.ItemCode;
                                                receive.Qty = rec.CountQty;
                                                receive.ProductDate = receiveOther.ProductDate;
                                                receive.ERPWarehouseCode = receiveOther.ERPWarehouseCode;
                                                receive.AreaID = receiveOther.AreaID;
                                                receive.Lot = receiveOther.Lot;
                                                receive.Spec = receiveOther.Spec;
                                                receive.ItemUnitText = receiveOther.ItemUnitText;
                                                receive.CheckState = receiveOther.CheckState;
                                                receive.SupplierUserID = receiveOther.SupplierUserID;
                                                receive.DoneUserID = receiveOther.DoneUserID;
                                                receive.DoneUserName = receiveOther.DoneUserName;
                                                receive.LocationID = receiveOther.LocationID;
                                                receive.LocationCode = receiveOther.LocationCode;
                                                receive.State = receiveOther.State;
                                                receive.TransState = receiveOther.TransState;
                                                receive.ContainerKind = receiveOther.ContainerKind;
                                                receive.FailDesc = null;
                                                receive.F_DeleteMark = false;
                                                receive.IsItemMark = receiveOther.IsItemMark;
                                                receive.Factory = receiveOther.Factory;
                                                receive.ValidityDayNum = receiveOther.ValidityDayNum;
                                                receive.OverdueDate = receiveOther.OverdueDate;
                                                receive.SEQ = receiveOther.SEQ;
                                                db.Insert<T_ReceiveRecordEntity>(receive);
                                            }
                                            #endregion

                                            #region 新增库存
                                            T_ContainerDetailEntity cdOther = db.FindEntity<T_ContainerDetailEntity>(o => o.ItemID == rec.ItemID && (o.Lot == rec.Lot || (string.IsNullOrEmpty(o.Lot) && string.IsNullOrEmpty(rec.Lot))));    /// 获取一条相同库存
                                            T_ContainerEntity container = db.FindEntity<T_ContainerEntity>(o => o.BarCode == rec.BarCode && o.F_DeleteMark == false);
                                            if (cdOther == null) return Error("无其它参考库存，无法新建", "");

                                            /// 直接新增库存
                                            if (cd == null)
                                            {
                                                cd = new T_ContainerDetailEntity();
                                                cd.F_Id = Guid.NewGuid().ToString();
                                                cd.AreaCode = container.AreaCode;
                                                cd.AreaID = container.AreaID;
                                                cd.AreaName = container.AreaName;
                                                cd.ContainerID = container.F_Id;
                                                cd.ContainerKind = container.ContainerKind;
                                                cd.ContainerType = container.ContainerType;
                                                cd.LocationID = container.LocationID;
                                                cd.LocationNo = container.LocationNo;

                                                cd.CheckDetailID = cdOther.CheckDetailID;
                                                cd.CheckID = cdOther.CheckID;
                                                cd.CheckQty = 0;
                                                cd.CheckState = cdOther.CheckState;

                                                cd.KindCode = cdOther.KindCode;
                                                cd.KindName = cdOther.KindName;
                                                cd.ItemID = cdOther.ItemID;
                                                cd.ItemCode = cdOther.ItemCode;
                                                cd.ItemName = cdOther.ItemName;
                                                cd.ItemUnitText = cdOther.ItemUnitText;
                                                cd.State = cdOther.State;
                                                cd.IsCheckFreeze = cdOther.IsCheckFreeze;
                                                cd.Lot = cdOther.Lot;
                                                cd.Spec = cdOther.Spec;
                                                cd.ProductDate = cdOther.ProductDate;
                                                cd.OverdueDate = cdOther.OverdueDate;
                                                cd.ERPWarehouseCode = cdOther.ERPWarehouseCode;
                                                cd.IsItemMark = cdOther.IsItemMark;
                                                cd.Factory = cdOther.Factory;
                                                cd.ValidityDayNum = cdOther.ValidityDayNum;
                                                cd.SupplierID = cdOther.SupplierID;
                                                cd.SupplierCode = cdOther.SupplierCode;
                                                cd.SupplierName = cdOther.SupplierName;
                                                //cd.IsSpecial = cdOther.IsSpecial;
                                                cd.F_DeleteMark = false;
                                                cd.OutQty = 0;

                                                cd.BarCode = rec.BarCode;
                                                cd.ItemBarCode = rec.ItemBarCode;
                                                cd.Qty = rec.CountQty;
                                                cd.IsCountFreeze = "false";
                                                cd.ReceiveRecordID = receive.F_Id;
                                                cd.InBoundID = receive.InBoundID;
                                                cd.InBoundDetailID = receive.InBoundDetailID;
                                                db.Insert<T_ContainerDetailEntity>(cd);

                                                /// 库存流水
                                                inOutDetailApp.SyncInOutDetail(db, cd, "InType", "Count", 0, cd.Qty, "");
                                            }
                                            /// 从库存变更，可能产生空容器
                                            else
                                            {
                                                List<T_ContainerDetailEntity> cdList = db.FindList<T_ContainerDetailEntity>(o => o.BarCode == cd.BarCode && o.ItemBarCode != rec.ItemBarCode); /// 原容器其它库存
                                                List<T_CountRecordEntity> noRecList = recordList.Where(o => o.BarCode == cd.BarCode && o.F_Id != rec.F_Id && o.CountResult != "Outer_LessQty").ToList();   /// 原容器其它盘点记录

                                                /// 原容器 ---> 空容器，新增一个空容器
                                                if (cdList.Count == 0 && noRecList.Count == 0)
                                                {
                                                    T_ContainerDetailEntity cdEmpty = db.FindEntity<T_ContainerDetailEntity>(o => o.BarCode == cd.BarCode);
                                                    if (cdEmpty.ItemCode != FixType.Item.EmptyPlastic.ToString() && cdEmpty.ItemCode != FixType.Item.EmptyRack.ToString()) /// 不是空容器
                                                    {
                                                        T_ItemEntity itemEmpty = null;
                                                        T_ContainerEntity containerEmpty = db.FindEntity<T_ContainerEntity>(o => o.BarCode == cd.BarCode && o.F_DeleteMark == false);
                                                        if (containerEmpty.ContainerKind == "Plastic")   /// 料箱
                                                        {
                                                            itemEmpty = db.FindEntity<T_ItemEntity>(o => o.ItemCode == FixType.Item.EmptyPlastic.ToString());
                                                        }
                                                        else if (containerEmpty.ContainerKind == "Rack")
                                                        {
                                                            itemEmpty = db.FindEntity<T_ItemEntity>(o => o.ItemCode == FixType.Item.EmptyRack.ToString());
                                                        }
                                                        if (itemEmpty == null) return Error("未定义空容器物料", "");

                                                        cdEmpty = new T_ContainerDetailEntity();
                                                        cdEmpty.F_Id = Guid.NewGuid().ToString();
                                                        cdEmpty.AreaCode = cd.AreaCode;
                                                        cdEmpty.AreaID = cd.AreaID;
                                                        cdEmpty.AreaName = cd.AreaName;
                                                        cdEmpty.ContainerID = cd.ContainerID;
                                                        cdEmpty.ContainerKind = cd.ContainerKind;
                                                        cdEmpty.ContainerType = cd.ContainerType;
                                                        cdEmpty.BarCode = cd.BarCode;
                                                        cdEmpty.CheckDetailID = "";
                                                        cdEmpty.CheckID = "";
                                                        cdEmpty.CheckQty = 0;
                                                        cdEmpty.CheckState = "UnNeed";
                                                        cdEmpty.LocationID = cd.LocationID;
                                                        cdEmpty.LocationNo = cd.LocationNo;
                                                        cdEmpty.KindCode = itemEmpty.KindCode;
                                                        cdEmpty.KindName = itemEmpty.KindName;
                                                        cdEmpty.ItemBarCode = "";
                                                        cdEmpty.ItemCode = itemEmpty.ItemCode;
                                                        cdEmpty.ItemID = itemEmpty.F_Id;
                                                        cdEmpty.ItemName = itemEmpty.ItemName;
                                                        cdEmpty.Qty = 1;
                                                        cdEmpty.OutQty = 0;
                                                        cdEmpty.ItemUnitText = itemEmpty.ItemUnitText;
                                                        cdEmpty.State = "Normal";
                                                        cdEmpty.IsCheckFreeze = "false";
                                                        cdEmpty.IsCountFreeze = "false";
                                                        cdEmpty.Lot = "";
                                                        cdEmpty.Spec = "";
                                                        cdEmpty.ERPWarehouseCode = "";
                                                        cdEmpty.Factory = "";
                                                        cdEmpty.OverdueDate = null;
                                                        cdEmpty.ProductDate = null;
                                                        cdEmpty.SupplierCode = "";
                                                        cdEmpty.SupplierID = "";
                                                        cdEmpty.SupplierName = "";
                                                        cdEmpty.ReceiveRecordID = "";
                                                        //cdEmpty.IsSpecial = "false";
                                                        cdEmpty.IsItemMark = "";
                                                        cdEmpty.IsVirItemBarCode = "";
                                                        cdEmpty.ValidityDayNum = 0;
                                                        cdEmpty.F_DeleteMark = false;
                                                        db.Insert<T_ContainerDetailEntity>(cdEmpty);
                                                        db.SaveChanges();

                                                        /// 库存流水
                                                        inOutDetailApp.SyncInOutDetail(db, cdEmpty, "InType", "EmptyIn", 0, cdEmpty.Qty, "");
                                                    }
                                                }

                                                /// 变更库存容器信息
                                                cd.AreaID = container.AreaID;
                                                cd.AreaCode = container.AreaCode;
                                                cd.AreaName = container.AreaName;
                                                cd.ContainerID = container.F_Id;
                                                cd.ContainerKind = container.ContainerKind;
                                                cd.ContainerType = container.ContainerType;
                                                cd.LocationID = container.LocationID;
                                                cd.LocationNo = container.LocationNo;
                                                cd.State = cdOther.State;
                                                cd.ERPWarehouseCode = cdOther.ERPWarehouseCode;
                                                cd.BarCode = rec.BarCode;
                                                cd.ItemBarCode = rec.ItemBarCode;
                                                cd.Qty = rec.CountQty;
                                                cd.IsCountFreeze = "false";
                                                db.Update<T_ContainerDetailEntity>(cd);

                                                /// 产生流水
                                                if (cd.Qty != rec.CountQty)
                                                {
                                                    decimal? changeQty = rec.CountQty - cd.Qty;
                                                    if (changeQty > 0) inOutDetailApp.SyncInOutDetail(db, cd, "InType", "Count", cd.Qty, changeQty, "");
                                                    else inOutDetailApp.SyncInOutDetail(db, cd, "OutType", "Count", 0, -changeQty, "");
                                                }
                                            }
                                            #endregion
                                        }
                                        break;
                                    case "Outer_MoreQty": /// 多余数量（修改库存数量）
                                        {
                                            /// 库存流水
                                            T_InOutDetailApp inOutDetailApp = new T_InOutDetailApp();
                                            inOutDetailApp.SyncInOutDetail(db, cd, "InType", "Count", cd.Qty, rec.CountQty - cd.Qty, "");

                                            isChangeCD = true;
                                            cd.Qty = rec.CountQty;
                                            db.Update<T_ContainerDetailEntity>(cd);
                                        }
                                        break;
                                    default:
                                        {
                                            return Error($"未知的盘点结果{rec.CountResult}", "");
                                        }
                                }
                            }
                            else if (result.CountResult == "UnPass") { }    /// 不通过，什么都不做
                            else return Error($"未知的盘点结果参数{result.CountResult}", "");
                        }

                        /// 盘点明细状态
                        detail.AuditState = result.CountResult;
                        detail.CountResult = result.CountResult;
                        detail.CountState = "Over";
                        db.Update<T_CountDetailEntity>(detail);
                        db.SaveChanges();
                    }

                    /// 更新盘点单
                    count.State = "Over";
                    count.AuditResult = "Applied";
                    if (detailListAll.Any(o => o.AuditState == "UnPass"))
                    {
                        count.AuditState = "UnPass";
                    }
                    else count.AuditState = "Pass";
                    db.Update<T_CountEntity>(count);
                    db.SaveChanges();

                    if (isChangeCD && RuleConfig.OrderTransRule.CountTransRule.CountTrans)  /// 库存变动 && 允许盘点过账  
                    {
                        if (count.GenType == "ERP")
                        {
                            /// 产生过账信息，并发送过账信息
                            string transType = "Count";
                            AjaxResult rst = new T_TransRecordApp().GenTransRecord(db, count.F_Id, transType);
                            if ((ResultType)rst.state == ResultType.success)
                            {
                                T_TransRecordEntity trans = (T_TransRecordEntity)rst.data;
                                ERPPost post = new ERPPost();
                                ERPResult erpRst = post.PostFactInOutQty(db, transType, trans.F_Id);
                            }
                            else
                            {
                                return Error("过账信息产生失败", "");
                            }
                        }
                    }

                    db.SaveChanges();
                    db.CommitWithOutRollBack();
                    return Success("操作成功");
                    /*************************************************************/
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

        #region 复盘
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
        public ActionResult GenReCount(string countID, string countModel, string agvCodeStr, string remark)
        {
            using (var db = new RepositoryBase().BeginTrans())
            {
                LogObj logObj = new LogObj();
                logObj.Path = "PC_CountManage.GenReCount";
                logObj.Parms = new { countID = countID, countModel = countModel, remark = remark };

                LogEntity logEntity = new LogEntity();
                logEntity.F_ModuleName = "物料盘点";
                logEntity.F_Type = DbLogType.Submit.ToString();
                logEntity.F_Account = OperatorProvider.Provider.GetCurrent().UserCode;
                logEntity.F_NickName = OperatorProvider.Provider.GetCurrent().UserName;
                logEntity.F_Description = "复盘";
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

                        IList<string> TaskListNo = new List<string>();
                        AjaxResult res = ReCountOnOff(db, countID, agvCodeStr, remark, ref TaskListNo);
                        if ((ResultType)res.state != ResultType.success)
                        {
                            db.RollBack();
                            return Error(res.message, "");
                        }
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


        #region 开始复盘
        /// <summary>
        /// 复盘
        /// </summary>
        /// <param name="db"></param>
        /// <param name="countID"></param>
        /// <param name="TaskListNo">产生的任务编码</param>
        /// <returns></returns>
        public AjaxResult ReCountOnOff(IRepositoryBase db, string countID, string agvCodeStr, string remark, ref IList<string> TaskListNo)
        {
            AjaxResult res = new AjaxResult();
            T_CountEntity count = db.FindEntity<T_CountEntity>(o => o.F_Id == countID);
            if (count == null)
            {
                res.state = ResultType.error;
                res.message = "盘点单据不存在。";
                return res;
            }
            if (count.State != "WaitAudit")
            {
                res.state = ResultType.error;
                res.message = "复盘单据需为待提交审核。";
                return res;
            }

            List<T_CountRecordEntity> recordList = db.FindList<T_CountRecordEntity>(o => o.CountID == countID && o.CountResult != "Inner_Empty" && o.CountResult != "Inner_SameBoxCode" && o.CountResult != "Outer_Normal");
            if (recordList.Count == 0)
            {
                res.state = ResultType.error;
                res.message = "无异常盘点记录。";
                return res;
            }

            /// 获取盘点明细的站台列表
            var stationList = countDetailApp.FindList(o => o.CountID == countID).GroupBy(x => new { x.StationID }).Select(a => a.FirstOrDefault()).ToList();
            foreach (var s in stationList)
            {
                if (string.IsNullOrEmpty(s.StationID)) continue;
                T_StationEntity station = db.FindEntity<T_StationEntity>(o => o.F_Id == s.StationID);
                if (station == null)
                {
                    res.state = ResultType.error;
                    res.message = $"未找到盘点站台 {station.StationName}";
                    return res;
                }
                if (!string.IsNullOrEmpty(station.CurOrderID))
                {
                    res.state = ResultType.error;
                    res.message = $"站台 {station.StationName} 已绑定单据";
                    return res;
                }
                if (station.CurOrderID == count.F_Id)
                {
                    res.state = ResultType.error;
                    res.message = "站台已绑定该单据。";
                    return res;
                }

                station.CurOrderID = count.F_Id;
                station.OrderType = "Count";
                db.Update<T_StationEntity>(station);
            }

            //过滤区域、巷道、行
            IQueryable<T_AreaEntity> areaQuery = db.IQueryable<T_AreaEntity>(o => o.IsEnable == "true"); //启用的区域
            IQueryable<T_DevRowEntity> wayQuery = db.IQueryable<T_DevRowEntity>(o => o.IsEnable == "true");//启用的巷道ID
            IQueryable<T_RowLineEntity> lineQuery = db.IQueryable<T_RowLineEntity>(o => o.IsEnable == "true"); //启用的行

            var wayAndLineQuery = areaQuery.Join(wayQuery, m => m.F_Id, n => n.AreaID, (m, n) => new { AreaID = n.AreaID, WayID = n.F_Id }).Join(lineQuery, m => m.WayID, n => n.DevRowID, (m, n) => new { m.AreaID, n.Line });

            //过滤货位,获取所有可用货位
            IList<string> locNoList = db.IQueryable<T_LocationEntity>(o =>
            o.State == "Stored"
            && (o.ForbiddenState == "Normal" || o.ForbiddenState == "OnlyOut")
            ).Join(wayAndLineQuery, m => new { m.AreaID, m.Line }, n => new { n.AreaID, n.Line }, (m, n) => m).Select(o => o.LocationCode).ToList();


            string[] recordListID = recordList.Select(o => o.CountDetailID).ToArray();
            IList<T_CountDetailEntity> countDetailList = db.FindList<T_CountDetailEntity>(o => o.CountID == countID && recordListID.Contains(o.F_Id));
            Dictionary<string, T_TaskEntity> dictTask = new Dictionary<string, T_TaskEntity>();



            foreach (var countDetail in countDetailList)
            {
                string[] containerIDListID = recordList.Where(o => o.CountDetailID == countDetail.F_Id).Select(o => o.ContainerDetailID).ToArray();

                T_StationEntity tagStation = db.FindEntity<T_StationEntity>(o => o.F_Id == countDetail.StationID);

                List<T_ContainerDetailEntity> containerDetailList = new List<T_ContainerDetailEntity>();

                /// 判断无批号物料库存
                if (string.IsNullOrEmpty(countDetail.Lot))
                {
                    if (!string.IsNullOrEmpty(count.ERPHouseCode))
                    {
                        containerDetailList = db.FindList<T_ContainerDetailEntity>(o => o.ItemID == countDetail.ItemID && string.IsNullOrEmpty(o.Lot) && o.ERPWarehouseCode == count.ERPHouseCode).ToList();
                    }
                    else
                    {
                        containerDetailList = db.FindList<T_ContainerDetailEntity>(o => o.ItemID == countDetail.ItemID && string.IsNullOrEmpty(o.Lot)).ToList();
                    }

                }
                else
                {
                    if (!string.IsNullOrEmpty(count.ERPHouseCode))
                    {
                        containerDetailList = db.FindList<T_ContainerDetailEntity>(o => o.ItemID == countDetail.ItemID && o.Lot == countDetail.Lot && o.ERPWarehouseCode == count.ERPHouseCode).ToList();
                    }
                    else
                    {
                        containerDetailList = db.FindList<T_ContainerDetailEntity>(o => o.ItemID == countDetail.ItemID && o.Lot == countDetail.Lot).ToList();
                    }
                }

                containerDetailList = containerDetailList.Where(o => containerIDListID.Contains(o.F_Id)).ToList();

                /// 判断明细是否存在库存
                if (containerDetailList.Count != containerIDListID.Count())
                {
                    res.state = ResultType.error;
                    res.message = $"物料[{countDetail.ItemCode}],批号[{countDetail.Lot}] 库存已变动";
                    return res;
                }


                /// 校验容器是否待出库待质检，是否未在库
                foreach (var item in containerDetailList)
                {
                    if (item.OutQty != 0 || item.CheckQty != 0 || item.LocationNo == FixType.Station.StationOut_BigItem.ToString() || item.LocationNo == FixType.Station.StationOut_Normal.ToString())
                    {
                        res.state = ResultType.error;
                        res.message = "盘点单物料有其它任务。";
                        return res;
                    }
                    //if (item.CheckState != "Qua" && item.CheckState != "UnNeed")
                    //{
                    //    res.state = ResultType.error;
                    //    res.message = "物料质检状态不可盘";
                    //    return res;
                    //}
                }

                //校验货位所处区域、巷道、行 是否被禁用
                foreach (var item in containerDetailList)
                {
                    if (!locNoList.Contains(item.LocationNo))
                    {
                        res.state = ResultType.error;
                        res.message = "货位：" + item.LocationNo + "区域、巷道或行 为禁用状态";
                        return res;
                    }
                }

                /// 检验储位是否被占用
                IList<string> notStoredLocations = db.FindList<T_LocationEntity>(o => o.State != "Stored").Select(o => o.LocationCode).Distinct().ToArray();
                IList<string> forbidLocations = db.FindList<T_LocationEntity>(o => o.ForbiddenState != "Normal").Select(o => o.LocationCode).Distinct().ToArray();

                foreach (var cd in containerDetailList)
                {
                    /// 冻结盘点库存
                    cd.IsCountFreeze = "true";
                    db.Update<T_ContainerDetailEntity>(cd);

                    T_ContainerEntity container = db.FindEntity<T_ContainerEntity>(o => o.F_Id == cd.ContainerID);
                    T_LocationEntity srcLocation = db.FindEntity<T_LocationEntity>(o => o.F_Id == cd.LocationID);

                    if (notStoredLocations.Contains(srcLocation.LocationCode))
                    {
                        res.state = ResultType.error;
                        res.message = $"储位不是已存储状态：{srcLocation.LocationCode}";
                        return res;
                    }
                    if (forbidLocations.Contains(srcLocation.LocationCode))
                    {
                        res.state = ResultType.error;
                        res.message = $"储位是锁定状态：{srcLocation.LocationCode}";
                        return res;
                    }

                    T_TaskEntity task = new T_TaskEntity();

                    T_CountRecordEntity countRecord = db.FindEntity<T_CountRecordEntity>(o => o.CountID == countID && o.CountDetailID == countDetail.F_Id && o.ContainerDetailID == cd.F_Id);
                    countRecord.SEQ = countDetail.SEQ;
                    countRecord.CountID = countDetail.CountID;
                    countRecord.CountDetailID = countDetail.F_Id;
                    countRecord.ContainerID = cd.ContainerID;
                    countRecord.ContainerDetailID = cd.F_Id; /// 库存明细ID
                    countRecord.StationID = countDetail.StationID;
                    countRecord.BarCode = container.BarCode;
                    countRecord.ItemBarCode = cd.ItemBarCode;
                    countRecord.ItemID = cd.ItemID;
                    countRecord.ItemCode = cd.ItemCode;
                    countRecord.ItemName = cd.ItemName;
                    countRecord.SupplierUserID = cd.SupplierID;
                    countRecord.SupplierUserName = cd.SupplierName;
                    countRecord.ERPCode = cd.ERPWarehouseCode;
                    countRecord.ProductDate = cd.ProductDate;
                    countRecord.Factory = cd.Factory;
                    countRecord.LocationID = cd.LocationID;
                    countRecord.LocationCode = cd.LocationNo;
                    countRecord.TagLocationID = countDetail.StationID;
                    countRecord.Lot = cd.Lot;
                    countRecord.Qty = cd.Qty;
                    countRecord.Spec = cd.Spec;
                    countRecord.ItemUnitText = cd.ItemUnitText;
                    countRecord.OverdueDate = cd.OverdueDate;
                    countRecord.CreatorUserName = OperatorProvider.Provider.GetCurrent().UserName;
                    countRecord.F_CreatorUserId = OperatorProvider.Provider.GetCurrent().UserId;
                    countRecord.IsItemMark = cd.IsItemMark; /// 是否物料贴标
                    countRecord.IsAdd = "false";
                    countRecord.IsArrive = "false";
                    countRecord.IsNeedBackWare = "false";
                    countRecord.IsScanBack = "false";

                    countDetail.CountQty -= countRecord.CountQty;
                    
                    countRecord.CountQty = 0;
                    countRecord.CountResult = "";

                    /// 判断盘点明细物料, 手动/自动
                    if (cd.ItemID == countDetail.ItemID && (cd.Lot == countDetail.Lot || (string.IsNullOrEmpty(countDetail.Lot) && string.IsNullOrEmpty(cd.Lot))))
                    {
                        countRecord.AuditState = "WaitAudit";
                        countRecord.GenType = "MAN";
                        countRecord.TransState = "WaittingTrans";
                    }
                    else
                    {
                        countRecord.CountState = "NoNeed";
                        countRecord.AuditState = "Pass";
                        countRecord.GenType = "Auto";
                        countRecord.TransState = "UnNeedTrans";
                    }

                    /// 是否出库盘点 判断盘点任务
                    if (container.ContainerKind == "Box")
                    {
                        countRecord.IsOutCount = "false";
                        countRecord.CountState = "Counting";
                        task.TaskInOutType = "AGVCountType";
                        task.TaskType = "TaskType_CountAGV";  /// 纸箱，AGV在库盘点
                        task.TagLocationID = srcLocation.F_Id;    /// 目标地址ID
                        task.TagLocationCode = srcLocation.LocationCode;    /// 目标地址编码
                        task.TagWCSLocCode = srcLocation.WCSLocCode;
                        if (stationList != null && stationList.Count > 0)
                        {
                            task.ApplyStationID = stationList[0].F_Id;
                        }
                    }
                    else
                    {
                        countRecord.IsOutCount = "true";
                        countRecord.CountState = "Outing";
                        task.TaskInOutType = "OutType";
                        task.TaskType = "TaskType_CountOut"; /// 料箱 料架，出库盘点
                        task.TagLocationID = tagStation.F_Id;    /// 目标地址ID
                        task.TagLocationCode = tagStation.LeaveAddress;    /// 目标地址编码
                        task.TagWCSLocCode = tagStation.LeaveAddress;
                        task.ApplyStationID = tagStation.F_Id;
                    }
                    db.Update<T_CountRecordEntity>(countRecord);


                    /// 待出库数量、质检数量。不出库。终止生成盘点任务
                    //List<string> res = forbidContainerList.Where(o => o == item.ContainerID).ToList();
                    //if (res.Count != 0) return Error("盘点单有出库物料", "");

                    /// 生成盘点任务
                    task.F_Id = Guid.NewGuid().ToString();
                    task.TaskNo = T_CodeGenApp.GenNum("TaskRule");
                    task.ContainerID = container.F_Id;
                    task.BarCode = container.BarCode;
                    task.ContainerType = container.ContainerType;
                    task.SrcLocationID = srcLocation.F_Id;  /// 起始地址ID
                    task.SrcLocationCode = srcLocation.LocationCode;    /// 起始地址编码
                    task.SrcWCSLocCode = srcLocation.WCSLocCode;
                    task.Level = 30; /// 盘点
                    task.State = "New";
                    task.SEQ = countDetail.SEQ;
                    task.IsWcsTask = "true";
                    task.IsCanExec = "true";
                    task.OrderType = "Count";
                    task.OrderID = count.F_Id;
                    task.OrderDetailID = countDetail.F_Id;
                    task.OrderCode = count.CountCode;
                    task.F_DeleteMark = false;

                    if (!dictTask.ContainsKey(task.SrcLocationID + task.BarCode))
                    {
                        dictTask.Add(task.SrcLocationID + task.BarCode, task);
                        /// 锁定货位 不同批号不在同一货位
                        srcLocation.State = "Out";
                        db.Update<T_LocationEntity>(srcLocation);

                        /// 货位状态变更记录
                        locStateApp.SyncLocState(db, srcLocation, "OutType", "Count", "Stored", "Out", task.TaskNo);
                    }
                }
                db.SaveChanges();

                T_ItemEntity itemEntity = itemApp.FindEntity(o => o.F_Id == countDetail.ItemID);
                T_ContainerTypeEntity containerType = containerTypeApp.FindEntity(o => o.ContainerTypeCode == itemEntity.ContainerType);
                string containerKind = containerType.ContainerKind;
                if (containerKind == "Rack" || containerKind == "Plastic")
                {
                    countDetail.CountState = "Outing"; /// 盘点明细状态：出库中
                }
                else if (containerKind == "Box")
                {
                    countDetail.CountState = "Counting"; /// 盘点明细状态：盘点中，在库AGV盘
                }
                else
                {
                    res.state = ResultType.error;
                    res.message = "物料的容器类型未知:" + itemEntity.ItemCode;
                    return res;
                }
                db.Update<T_CountDetailEntity>(countDetail);
            }

            IList<string> agvCodeList = agvCodeStr.Split(',');
            IList<string> ctuList = agvCodeList.Where(o => o == "2001" || o == "2002" || o == "2003" || o == "2004").ToList();
            IList<string> agvList = agvCodeList.Where(o => o == "1001" || o == "1002").ToList();

            int countCTU = 0;
            int countAGV = 0;


            foreach (var item in dictTask)
            {
                T_ContainerTypeEntity containerType = containerTypeApp.FindEntity(o => o.ContainerTypeCode == item.Value.ContainerType);
                if (containerType.ContainerKind == "Rack")
                {
                    if (agvList.Count == 0)
                    {
                        res.state = ResultType.error;
                        res.message = "未选择执行的AGV";
                        return res;
                    }
                    item.Value.PointExecRobotCode = agvList[countCTU % (agvList.Count)];
                    countCTU = countCTU + 1;
                }
                else if (containerType.ContainerKind == "Box" || containerType.ContainerKind == "Plastic")
                {
                    if (ctuList.Count == 0)
                    {
                        res.state = ResultType.error;
                        res.message = "未选择执行的CTU";
                        return res;
                    }
                    item.Value.PointExecRobotCode = ctuList[countAGV % (ctuList.Count)];
                    countAGV = countAGV + 1;
                }
                db.Insert<T_TaskEntity>(item.Value);
                TaskListNo.Add(item.Value.TaskNo);
            }
            db.SaveChanges();

            //List<T_CountDetailEntity> detailList = db.FindList<T_CountDetailEntity>(o => o.CountID == countID && o.CountState == "Outing");
            List<string> itemCode = db.FindList<T_CountDetailEntity>(o => o.CountID == countID).Select(o=>o.ItemCode).ToList();
            List<string> containerTypeArray = db.FindList<T_ItemEntity>(o => itemCode.Contains(o.ItemCode)).Select(o => o.ContainerType).ToList();
            IList<T_ContainerTypeEntity> containerTypeList = db.FindList<T_ContainerTypeEntity>(o => containerTypeArray.Contains(o.ContainerTypeCode));
            bool isExistsBox = containerTypeList.Any(o => o.ContainerKind == "Box");
            bool isExistsRackOrPlastic = containerTypeList.Any(o => o.ContainerKind == "Rack" || o.ContainerKind == "Plastic");
            if (isExistsBox ==true && isExistsRackOrPlastic==false)/// 全部是 在库盘点
            {
                count.State = "Counting";
            }
            else
            {
                count.State = "Outing"; /// 盘点单状态：出库中
            }

            count.Remark = remark;
            db.Update<T_CountEntity>(count);
            db.SaveChanges();

            res.state = ResultType.success;
            return res;
        }
        #endregion
    }
}
