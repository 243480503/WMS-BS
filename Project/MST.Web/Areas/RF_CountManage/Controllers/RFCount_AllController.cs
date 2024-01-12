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
using MST.Domain.Entity.SystemSecurity;
using MST.Domain.Entity.WMSLogic;
using MST.Domain.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace MST.Web.Areas.RF_CountManage.Controllers
{
    public class RFCount_AllController : ControllerBase
    {
        private T_CountApp countApp = new T_CountApp();
        private T_CountDetailApp countDetailApp = new T_CountDetailApp();
        private T_CountRecordApp countRecordApp = new T_CountRecordApp();
        private ItemsDetailApp itemsDetailApp = new ItemsDetailApp();
        private T_StationApp stationApp = new T_StationApp();
        private T_ContainerApp containerApp = new T_ContainerApp();
        private T_ContainerDetailApp containerDetailApp = new T_ContainerDetailApp();
        private T_ItemApp itemApp = new T_ItemApp();
        private T_TaskApp taskApp = new T_TaskApp();

        #region 扫描后，获取容器的盘点信息
        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetDetailJson(string barCode, string itemBarcode)
        {
            if (string.IsNullOrEmpty(barCode)) return Error("箱码不能为空", "");
            T_ContainerEntity container = containerApp.FindEntity(o => o.BarCode == barCode);
            if (container == null) return Error("箱码不存在", "");

            T_StationEntity station = null;
            if (container.ContainerKind == "Box" || container.ContainerKind == "Plastic") //纸箱或料箱
            {
                station = stationApp.FindEntity(o => o.BarCode == barCode && o.StationCode == FixType.Station.StationOut_Normal.ToString());
            }
            else if (container.ContainerKind == "Rack") //料架
            {
                station = stationApp.FindEntity(o => o.BarCode == barCode && o.StationCode == FixType.Station.StationOut_BigItem.ToString());
            }
            else return Error("未知的容器大类", "");

            if (station == null) return Error("容器未到达站台", "");
            if (string.IsNullOrEmpty(station.CurOrderID)) return Error("该站台没有作业单据", "");

            T_TaskEntity task = taskApp.FindEntity(o => o.OrderID == station.CurOrderID && o.TaskType == "TaskType_CountIn" && o.BarCode == barCode);
            if (task != null) return Error("容器已申请入库", "");

            IList<T_CountRecordEntity> countRecAllList = countRecordApp.FindList(o => o.BarCode == barCode && o.CountID == station.CurOrderID).ToList();
            IList<T_CountRecordEntity> CountingList = countRecAllList.Where(o => o.CountState == "Counting").ToList();
            IList<T_CountRecordEntity> needCountList = countRecAllList.Where(o => o.CountState != "NoNeed" && o.IsAdd == "false").ToList();

            T_CountRecordEntity countRecord = new T_CountRecordEntity();
            if (string.IsNullOrEmpty(itemBarcode)) countRecord = CountingList.FirstOrDefault();
            else countRecord = countRecAllList.FirstOrDefault(o => o.ItemBarCode == itemBarcode);

            decimal ReadyOut = countRecAllList.Sum(o => o.CountQty ?? 0);
            decimal OrderNeed = needCountList.Sum(o => o.Qty ?? 0);

            if (countRecord == null)
            {
                /// 获取一条相同的记录显示到RF
                countRecord = countRecAllList.FirstOrDefault(o => true); 
                if(countRecord == null)
                {
                    return Error("没有盘点记录", "");
                }
                countRecord.ItemBarCode = itemBarcode;
                countRecord.CountState = "Counting";
                countRecord.Qty = 0;
                countRecord.CountQty = 0;
            }

            if (countRecord.CountState == "Over") return Error("子码已盘点", "");
            if (countRecord.CountState == "NoNeed") return Error("子码免盘", "");

            T_ContainerDetailEntity containerDetailEntity = containerDetailApp.FindEntity(o => o.F_Id == countRecord.ContainerDetailID);
            T_CountEntity countEntity = countApp.FindEntity(o => o.F_Id == countRecord.CountID);
            T_CountDetailEntity countDetailEntity = countDetailApp.FindEntity(o => o.CountID == station.CurOrderID);
            T_ItemEntity item = itemApp.FindEntity(o => o.F_Id == countRecord.ItemID);

            CountRecordModel countRecordModel = countRecord.ToObject<CountRecordModel>();
            countRecordModel.RefOrderCode = countEntity.RefOrderCode;
            countRecordModel.ItemCode = countRecord.ItemCode;
            countRecordModel.ItemName = countRecord.ItemName;
            countRecordModel.Factory = item.Factory;
            if (containerDetailEntity != null) countRecordModel.ProductDate = containerDetailEntity.ProductDate;
            countRecordModel.Lot = countRecord.Lot;
            countRecordModel.OrderQty = countDetailEntity.Qty;
            countRecordModel.Qty = countRecord.Qty;
            countRecordModel.ReadyQutAndOrderNeed = ReadyOut.ToString("0.##") + "/" + OrderNeed.ToString("0.##");
            countRecordModel.ContainerKind = container.ContainerKind;
            countRecordModel.ContainerKindName = itemsDetailApp.FindEnum<T_ContainerTypeEntity>(o => o.ContainerKind).FirstOrDefault(o => o.F_ItemCode == container.ContainerKind).F_ItemName;
            countRecordModel.CountDetailID = countRecord.CountDetailID;
            countRecordModel.CountID = station.CurOrderID;
            countRecordModel.F_Id = countRecord.F_Id;
            countRecordModel.StationID = station.F_Id;
            countRecordModel.ItemBarCode = countRecord.ItemBarCode;
            countRecordModel.IsItemMark = countRecord.IsItemMark;
            countRecordModel.MustTimes = needCountList.Count;
            countRecordModel.NoCountTimes = CountingList.Count;
            countRecordModel.IsOpen = countEntity.IsOpen;
            countRecordModel.Spec = countRecord.Spec;
            countRecordModel.OverdueDate = countRecord.OverdueDate;
            countRecordModel.ItemUnitText = countRecord.ItemUnitText;
            return Content(countRecordModel.ToJson());
        }
        #endregion

        #region 切换选项卡后，根据箱码获取盘点列表
        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetCountRecordList(string CountID, string barCode)
        {
            IList<T_CountRecordEntity> dataList = new List<T_CountRecordEntity>();
            if (!string.IsNullOrEmpty(CountID))
            {
                dataList = countRecordApp.FindList(o => o.BarCode == barCode && o.CountID == CountID 
                            && (o.CountState == "Counted" || o.CountState == "Over" || o.CountState == "NoNeed") /// 已盘点、结束、免盘
                            ).OrderBy(o => o.F_LastModifyTime).ToList();
            }
            else
            {
                T_CountRecordEntity lastEntity = countRecordApp.FindList(o => o.BarCode == barCode).OrderBy(o => o.F_LastModifyTime).FirstOrDefault();
                if (lastEntity != null)
                {
                    dataList = countRecordApp.FindList(o => o.BarCode == barCode && o.CountID == lastEntity.CountID).OrderBy(o => o.F_LastModifyTime).ToList();
                }
            }

            List<CountRecordModel> countRecordModelList = new List<CountRecordModel>();
            foreach (var item in dataList)
            {
                CountRecordModel model = item.ToObject<CountRecordModel>();
                model.StateName = itemsDetailApp.FindEnum<T_CountRecordEntity>(o => o.CountState).FirstOrDefault(o => o.F_ItemCode == item.CountState).F_ItemName;
                //if (model.IsItemMark == "false") model.ItemBarCode = ""; /// 未贴标条码，去掉容器默认条码
                countRecordModelList.Add(model);
            }

            return Content(countRecordModelList.ToJson());
        }
        #endregion

        #region 切换选项卡后，根据箱码获取容器大类
        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetContainerKind(string barCode)
        {
            T_ContainerEntity containerEntity = containerApp.FindEntity(o => o.BarCode == barCode);
            return Content(containerEntity.ToJson());
        }
        #endregion

        #region RF提交盘点信息
        [HttpPost]
        [HandlerAjaxOnly]
        [ValidateAntiForgeryToken]
        public ActionResult SubmitCountRecordForm(CountRecordModel postCountRecord)
        {
            using (var db = new RepositoryBase().BeginTrans())
            {
                LogObj logObj = new LogObj();
                logObj.Path = "RFCount_AllController.SubmitCountRecordForm";
                logObj.Parms = new { postCountRecord = postCountRecord };

                LogEntity logEntity = new LogEntity();
                logEntity.F_ModuleName = "盘点单";
                logEntity.F_Type = DbLogType.Submit.ToString();
                logEntity.F_Account = OperatorProvider.Provider.GetCurrent().UserCode;
                logEntity.F_NickName = OperatorProvider.Provider.GetCurrent().UserName;
                logEntity.F_Description = "RF盘点信息提交";

                logEntity.F_Path = logObj.Path;
                logEntity.F_Param = logObj.Parms.ToJson();

                try
                {
                    /*************************************************/
                    if (string.IsNullOrEmpty(postCountRecord.BarCode)) return Error("箱码不能为空", "");

                    T_StationEntity station = db.FindEntity<T_StationEntity>(o => o.F_Id == postCountRecord.StationID);
                    string CurOrderID = station.CurOrderID;

                    T_TaskEntity task = db.FindEntity<T_TaskEntity>(o => o.OrderID == CurOrderID && o.TaskType == "TaskType_CountIn" && o.BarCode == postCountRecord.BarCode);
                    if (task != null) return Error("容器已申请入库", "");

                    T_CountRecordEntity countRec = db.FindEntity<T_CountRecordEntity>(o => o.F_Id == postCountRecord.F_Id);
                    T_CountDetailEntity countDetailEntity = db.FindEntity<T_CountDetailEntity>(o => o.F_Id == countRec.CountDetailID);
                    T_CountEntity countEntity = db.FindEntity<T_CountEntity>(o => o.F_Id == station.CurOrderID);
                    T_ContainerEntity containerEntity = db.FindEntity<T_ContainerEntity>(o => o.F_Id == countRec.ContainerID);

                    bool isAddNewCd = false;
                    bool isChangeCd = false;
                    List<T_CountRecordEntity> CountList = db.FindList<T_CountRecordEntity>(o => o.CountID == countEntity.F_Id && o.BarCode == postCountRecord.BarCode);
                    T_ContainerDetailEntity cdNew = db.FindEntity<T_ContainerDetailEntity>(o => o.ItemBarCode == postCountRecord.ItemBarCode);

                    if (postCountRecord.CountQty == null) return Error("盘点数量错误", "");

                    /// 是否RF盘点扫码
                    if (containerEntity.ContainerKind != "Box" && countRec.IsItemMark == "true")    /// 非纸箱 && 必须扫码
                    {
                        if (string.IsNullOrEmpty(postCountRecord.ItemBarCode)) return Error("子码不能为空", "");
                        if (RuleConfig.OutConfig.RFScanCode.IsItemBarCodeSame) /// 扫码必须一致
                        {
                            T_CountRecordEntity existsEntity = CountList.FirstOrDefault(o => o.ItemBarCode == postCountRecord.ItemBarCode);
                            if (existsEntity == null)   /// 不存在盘点记录中
                            {
                                if (cdNew == null) isAddNewCd = true;  /// 不存在库存。新增记录---待审核通过新增库存
                                else  /// 存在库存。修改库存
                                {
                                    if (cdNew.ItemID == countDetailEntity.ItemID && (cdNew.Lot == countDetailEntity.Lot || (string.IsNullOrEmpty(cdNew.Lot) && string.IsNullOrEmpty(countDetailEntity.Lot))))
                                    {
                                        isChangeCd = true;
                                    }
                                    else return Error("子码不属于当前盘点物料批号", "");
                                }
                            }
                            else countRec = existsEntity;   /// 存在盘点记录-----用当前子码替换盘点记录
                        }
                        else /// 允许不一致
                        {
                            if (countRec.ItemBarCode != postCountRecord.ItemBarCode) /// 扫码不一致
                            {
                                T_CountRecordEntity existsEntity = CountList.FirstOrDefault(o => o.ItemBarCode == postCountRecord.ItemBarCode);
                                if (existsEntity == null)   /// 不存在盘点记录中
                                {
                                    if (cdNew == null) isAddNewCd = true;  /// 不存在库存。新增记录---待审核通过新增库存
                                    else  /// 存在库存。修改库存
                                    {
                                        if (cdNew.ItemID == countDetailEntity.ItemID && (cdNew.Lot == countDetailEntity.Lot || (string.IsNullOrEmpty(cdNew.Lot) && string.IsNullOrEmpty(countDetailEntity.Lot))))
                                        {
                                            isChangeCd = true;
                                        }
                                        else return Error("子码不属于当前盘点物料批号", "");
                                    }
                                }
                                else countRec = existsEntity;   /// 存在盘点记录-----用当前子码替换盘点记录
                            }
                        }
                    }

                    /// 新增盘点记录，待审核通过新增库存/修改库存
                    if (isAddNewCd || isChangeCd)
                    {
                        T_CountRecordEntity countRecOther = CountList.FirstOrDefault(o => true);

                        countRec = new T_CountRecordEntity();
                        countRec.F_Id = Guid.NewGuid().ToString();
                        countRec.SEQ = countRecOther.SEQ;
                        countRec.CountID = countRecOther.CountID;
                        countRec.CountDetailID = countRecOther.CountDetailID;
                        countRec.ContainerID = countRecOther.ContainerID;

                        if (isChangeCd)
                        {
                            countRec.ContainerDetailID = cdNew.F_Id;
                            cdNew.IsCountFreeze = "true";
                            db.Update<T_ContainerDetailEntity>(cdNew);

                        }
                        else if (isAddNewCd) countRec.ContainerDetailID = "";

                        countRec.StationID = countRecOther.StationID;
                        countRec.BarCode = postCountRecord.BarCode;
                        countRec.ItemBarCode = postCountRecord.ItemBarCode;
                        countRec.ItemID = countRecOther.ItemID;
                        countRec.ItemCode = countRecOther.ItemCode;
                        countRec.ItemName = countRecOther.ItemName;
                        countRec.SupplierUserID = countRecOther.SupplierUserID;
                        countRec.SupplierUserName = countRecOther.SupplierUserName;
                        countRec.ProductDate = countRecOther.ProductDate;
                        countRec.Factory = countRecOther.Factory;
                        countRec.LocationID = countRecOther.LocationID;
                        countRec.LocationCode = countRecOther.LocationCode;
                        countRec.TagLocationID = countRecOther.TagLocationID;
                        countRec.Lot = countRecOther.Lot;
                        countRec.Spec = countRecOther.Spec;
                        countRec.ItemUnitText = countRecOther.ItemUnitText;
                        countRec.OverdueDate = countRecOther.OverdueDate;
                        countRec.Qty = postCountRecord.CountQty;
                        countRec.CreatorUserName = OperatorProvider.Provider.GetCurrent().UserName;
                        countRec.F_CreatorUserId = OperatorProvider.Provider.GetCurrent().UserId;
                        countRec.IsItemMark = countRecOther.IsItemMark;
                        countRec.F_DeleteMark = false;
                        countRec.AuditState = countRecOther.AuditState;
                        countRec.GenType = countRecOther.GenType;
                        countRec.TransState = countRecOther.TransState;
                        countRec.IsOutCount = countRecOther.IsOutCount;
                        countRec.CountState = "Counting";
                        countRec.IsAdd = "true";
                        countRec.IsArrive = "true";
                        countRec.IsNeedBackWare = "true";
                        countRec.IsScanBack = "false";
                        countRec.F_DeleteMark = false;

                        db.Insert<T_CountRecordEntity>(countRec);
                        db.SaveChanges();
                    }

                    if (countRec.CountState == "Over") return Error("当前子码已盘点", "");
                    if (countRec.CountState == "Counting") countRec.CountState = "Over";

                    countRec.CountQty = postCountRecord.CountQty;
                    if (isAddNewCd || isChangeCd) countRec.CountResult = "Outer_MoreItemBarcode"; /// 新增记录，盘点结果：多余标签
                    else if (countRec.CountQty > countRec.Qty) countRec.CountResult = "Outer_MoreQty";  /// 数量多
                    else if (countRec.CountQty < countRec.Qty) countRec.CountResult = "Outer_LessQty";  /// 数量少，包括数量为0
                    else countRec.CountResult = "Outer_Normal";

                    countRec.IsNeedBackWare = "true";
                    db.Update<T_CountRecordEntity>(countRec);

                    countDetailEntity.CountQty = (countDetailEntity.CountQty ?? 0) + (postCountRecord.CountQty ?? 0);
                    db.Update<T_CountDetailEntity>(countDetailEntity);
                    db.SaveChanges();

                    db.CommitWithOutRollBack();

                    IList<T_CountRecordEntity> countRecAllList = db.FindList<T_CountRecordEntity>(o => o.BarCode == postCountRecord.BarCode && o.CountID == countRec.CountID).ToList();
                    IList<T_CountRecordEntity> CountingList = countRecAllList.Where(o => o.CountState == "Counting").ToList();
                    IList<T_CountRecordEntity> needCountList = countRecAllList.Where(o => o.CountState != "NoNeed" && o.IsAdd == "false").ToList();

                    string NoCountTimes = CountingList.Count().ToString();    // 剩余次数
                    string ReadyQutAndOrderNeed = (countRecAllList.Sum(o => o.CountQty ?? 0)).ToString("0.##") + "/" + (needCountList.Sum(o => o.Qty ?? 0)).ToString("0.##");  // 已盘 / 总盘

                    /**************************************************/

                    logObj.Message = "操作成功";
                    LogFactory.GetLogger().Info(logObj);

                    logEntity.F_Result = true;
                    new LogApp().WriteDbLog(logEntity);

                    return Success("操作成功。", new { NoCountTimes = NoCountTimes, ReadyQutAndOrderNeed = ReadyQutAndOrderNeed });
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

        #region 料架-盘点回库，申请入库 （此处直接申请货位只针对料架，纸箱与料箱通过入库站台自动申请）
        [HttpPost]
        [HandlerAjaxOnly]
        [ValidateAntiForgeryToken]
        public ActionResult RackBackIn(string barCode)
        {
            using (var db = new RepositoryBase().BeginTrans())
            {
                LogObj logObj = new LogObj();
                logObj.Path = "RFCount_AllController.RackBackIn";
                logObj.Parms = new { barCode = barCode };

                LogEntity logEntity = new LogEntity();
                logEntity.F_ModuleName = "盘点单";
                logEntity.F_Type = DbLogType.Submit.ToString();
                logEntity.F_Account = OperatorProvider.Provider.GetCurrent().UserCode;
                logEntity.F_NickName = OperatorProvider.Provider.GetCurrent().UserName;
                logEntity.F_Description = "RF盘点申请回库";
                logEntity.F_Path = logObj.Path;
                logEntity.F_Param = logObj.Parms.ToJson();

                try
                {
                    /*************************************************/
                    T_ContainerEntity containerEntity = db.FindEntity<T_ContainerEntity>(o => o.BarCode == barCode && o.F_DeleteMark == false);
                    if (containerEntity == null)
                    {
                        return Error("箱码不存在", "");
                    }
                    T_StationEntity station = db.FindEntity<T_StationEntity>(o => o.BarCode == containerEntity.BarCode);
                    if(station == null)
                    {
                        return Error("站台未绑定该容器", "");
                    }
                    T_TaskEntity task = db.FindEntity<T_TaskEntity>(o => o.OrderID == station.CurOrderID && o.TaskType == "TaskType_CountIn" && o.BarCode == barCode);
                    if (task != null)
                    {
                        return Error("容器已申请入库", "");
                    }

                    /// 标记未完成盘点的记录，CountQty = 0, CountResult : Outer_LessQty
                    List<T_CountRecordEntity> recordList = db.FindList<T_CountRecordEntity>(o => o.CountID == station.CurOrderID && o.BarCode == station.BarCode && o.CountState != "Over" && o.CountState != "NoNeed");
                    foreach (T_CountRecordEntity record in recordList)
                    {
                        record.CountState = "Over";
                        record.CountQty = 0;
                        record.CountResult = "Outer_LessQty";
                        record.IsNeedBackWare = "true";
                        db.Update<T_CountRecordEntity>(record);
                        db.SaveChanges();

                        T_CountDetailEntity countDetail = db.FindEntity<T_CountDetailEntity>(o => o.F_Id == record.CountDetailID);
                        countDetail.CountQty = (countDetail.CountQty ?? 0) + (record.CountQty ?? 0);
                        db.Update<T_CountDetailEntity>(countDetail);
                    }

                    T_CountRecordEntity recordTemp = db.FindEntity<T_CountRecordEntity>(o => o.CountID == station.CurOrderID && o.BarCode == station.BarCode);

                    T_CountEntity countEntity = db.FindEntity<T_CountEntity>(o => o.F_Id == station.CurOrderID);
                    T_CountDetailEntity detailEntity = db.FindEntity<T_CountDetailEntity>(o =>o.F_Id == recordTemp.CountDetailID);

                    /// 判断单据是否完成
                    IList<T_CountRecordEntity> noCountRecordList = db.FindList<T_CountRecordEntity>(o => o.CountDetailID == detailEntity.F_Id && o.CountState != "Over" && o.CountState != "NoNeed").ToList();
                    if (noCountRecordList.Count < 1) //单据明细已盘点完毕
                    {
                        detailEntity.CountState = "WaitAudit";
                        db.Update<T_CountDetailEntity>(detailEntity);
                        db.SaveChanges();

                        IList<T_CountDetailEntity> detailList = db.FindList<T_CountDetailEntity>(o => o.CountID == station.CurOrderID);
                        if (detailList.All(o => o.CountState == "WaitAudit"))
                        {
                            countEntity.State = "WaitAudit";
                            db.Update<T_CountEntity>(countEntity);
                            db.SaveChanges();
                        }
                    }

                    /// 根据容器判断回库区域
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

                    /// 生成回库任务
                    T_TaskEntity taskEntity = new T_TaskEntity();
                    taskEntity.F_Id = Guid.NewGuid().ToString();
                    taskEntity.TaskNo = T_CodeGenApp.GenNum("TaskRule");
                    taskEntity.TaskInOutType = "InType";
                    taskEntity.ContainerID = containerEntity.F_Id;
                    taskEntity.BarCode = containerEntity.BarCode;
                    taskEntity.ContainerType = containerEntity.ContainerType;
                    taskEntity.SrcLocationID = station.F_Id;
                    taskEntity.SrcLocationCode = station.LeaveAddress;
                    taskEntity.SrcWCSLocCode = station.LeaveAddress;
                    taskEntity.TagAreaID = area.F_Id;
                    taskEntity.TagLocationID = "";
                    taskEntity.TagLocationCode = "";
                    taskEntity.ApplyStationID = station.F_Id;
                    taskEntity.WaveID = "";
                    taskEntity.WaveCode = "";
                    taskEntity.SEQ = detailEntity.SEQ;
                    taskEntity.Level = 40;
                    taskEntity.State = "New";
                    taskEntity.IsWcsTask = "true";
                    taskEntity.SendWCSTime = null;
                    taskEntity.OrderType = "Count";
                    taskEntity.OrderID = countEntity.F_Id;
                    taskEntity.OrderDetailID = detailEntity.F_Id;
                    taskEntity.OrderCode = countEntity.CountCode;
                    taskEntity.OverTime = null;
                    taskEntity.TaskType = "TaskType_CountIn";
                    taskEntity.IsCanExec = "true";
                    db.Insert<T_TaskEntity>(taskEntity);
                    db.SaveChanges();

                    if (containerEntity.ContainerKind == "Rack")
                    {
                        if (!string.IsNullOrEmpty(taskEntity.TagLocationID)) return Error("已申请过。", "");
                        else
                        {
                            List<T_CountRecordEntity> recList = db.FindList<T_CountRecordEntity>(o => o.CountID == station.CurOrderID && o.BarCode == containerEntity.BarCode);
                            foreach (T_CountRecordEntity rec in recList)
                            {
                                rec.IsScanBack = "true";
                                db.Update<T_CountRecordEntity>(rec);
                            }

                            string errMsg = "";
                            T_LocationApp locationApp = new T_LocationApp();
                            T_ContainerTypeEntity containerType = db.FindEntity<T_ContainerTypeEntity>(o => o.ContainerTypeCode == taskEntity.ContainerType);
                            T_ContainerDetailEntity oneConDetail = db.FindEntity<T_ContainerDetailEntity>(o=>o.BarCode == barCode);
                            LogObj log = null;
                            T_ItemEntity item = db.FindEntity<T_ItemEntity>(o => o.F_Id == oneConDetail.ItemID);
                            T_LocationEntity loc = locationApp.GetLocIn(ref errMsg,ref log, db, containerType, taskEntity.TagAreaID,false, oneConDetail.ERPWarehouseCode,oneConDetail.CheckState, true,null, item);
                            if (loc == null) return Error("货位分配失败：" + errMsg, "");

                            taskEntity.TagLocationCode = loc.LocationCode;
                            taskEntity.TagLocationID = loc.F_Id;
                            taskEntity.TagWCSLocCode = loc.WCSLocCode;
                            db.SaveChanges();

                            taskEntity.SendWCSTime = DateTime.Now;

                            bool isOrderOver = false;
                            string orderType = station.OrderType;
                            string taskType = "TaskType_CountOut";

                            IList<T_TaskEntity> taskList = db.FindList<T_TaskEntity>(o => o.OrderID == station.CurOrderID && o.TagLocationID == station.F_Id && o.TaskType == taskType.ToString());
                            IList<string> stationList = db.FindList<T_CountDetailEntity>(o => o.CountID == station.CurOrderID && o.StationID == station.F_Id).GroupBy(o => o.StationID).Select(o => o.FirstOrDefault().F_Id).Distinct().ToList();
                            IList<T_CountRecordEntity> list = db.FindList<T_CountRecordEntity>(o => o.CountID == station.CurOrderID && stationList.Contains(o.CountDetailID) 
                                                            && o.CountState != "Over" && o.CountState != "NoNeed");

                            if (taskList.Count < 1 && list.Count < 1) isOrderOver = true;

                            station.BarCode = "";
                            if (isOrderOver) /// 单据已完成
                            {
                                station.CurOrderID = "";
                                station.WaveID = "";
                                station.OrderType = "";
                                station.CurOrderDetailID = "";
                            }
                            db.Update<T_StationEntity>(station);

                            IList<string> taskIDList = new List<string>();
                            taskIDList.Add(taskEntity.TaskNo);
                            WCSResult wcsRes = new WCSPost().SendTask(db, taskIDList);
                            if (wcsRes.IsSuccess)
                            {
                                db.Update<T_TaskEntity>(taskEntity);
                            } 
                            else return Error("操作失败。", wcsRes.FailMsg);
                        }
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
    }
}
