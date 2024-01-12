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
using MST.Web.Areas.PC_InventoryManage.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace MST.Web.Areas.RF_InBoundManage.Controllers
{
    public class RFInBound_PlasticController : ControllerBase
    {
        private T_InBoundApp inBoundApp = new T_InBoundApp();
        private T_InBoundDetailApp inBoundDetailApp = new T_InBoundDetailApp();
        private T_ReceiveRecordApp receiveRecordApp = new T_ReceiveRecordApp();
        private T_StationApp stationApp = new T_StationApp();
        private T_ContainerTypeApp containerTypeApp = new T_ContainerTypeApp();
        private T_ItemApp itemApp = new T_ItemApp();
        private T_MarkRecordApp markRecordApp = new T_MarkRecordApp();

        #region 获取选定的入库明细的详细信息
        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetDetailJson(string stationCode)
        {
            T_StationEntity station = stationApp.FindEntity(o => o.StationCode == stationCode);
            if (station == null)
            {
                return Error("站台未找到", "");
            }
            if (string.IsNullOrEmpty(station.CurOrderDetailID))
            {
                return Error("该站台没有作业单据", "");
            }
            ReceiveRecordModel receiveRecord = new ReceiveRecordModel();
            T_InBoundDetailEntity inBoundDetailEntity = inBoundDetailApp.FindEntity(o => o.F_Id == station.CurOrderDetailID);
            T_InBoundEntity inBoundEntity = inBoundApp.FindEntity(o => o.F_Id == inBoundDetailEntity.InBoundID);
            T_ItemEntity item = itemApp.FindEntity(o => o.F_Id == inBoundDetailEntity.ItemID);

            T_ContainerTypeEntity containerTypeEntity = containerTypeApp.FindEntity(o => o.ContainerTypeCode == item.ContainerType);
            string containerKindType = containerTypeEntity.ContainerKind;

            if (containerKindType == "Box")
            {
                return Error("该站台单据应为纸箱入库", "");
            }
            else if (containerKindType == "Rack")
            {
                return Error("该站台单据应为料架入库", "");
            }
            else if (containerKindType == "Plastic")
            {
                receiveRecord.OrderQty = inBoundDetailEntity.Qty;
                receiveRecord.ItemCode = inBoundDetailEntity.ItemCode;
                receiveRecord.ItemName = inBoundDetailEntity.ItemName;
                receiveRecord.Factory = inBoundDetailEntity.Factory;
                receiveRecord.InBoundDetailID = inBoundDetailEntity.F_Id;
                receiveRecord.InBoundID = inBoundDetailEntity.InBoundID;
                receiveRecord.RefOrderCode = string.IsNullOrEmpty(inBoundEntity.RefOrderCode) ? inBoundEntity.InBoundCode : inBoundEntity.RefOrderCode;
                receiveRecord.Lot = inBoundDetailEntity.Lot;
                receiveRecord.ProductDate = inBoundDetailEntity.ProductDate;
                receiveRecord.UnitQty = item.UnitQty;
                decimal noReceiveQty = (inBoundDetailEntity.Qty ?? 0) - (inBoundDetailEntity.CurQty ?? 0);
                receiveRecord.Qty = noReceiveQty < item.UnitQty ? noReceiveQty : item.UnitQty;
                receiveRecord.OverdueDate = inBoundDetailEntity.OverdueDate;
                receiveRecord.Spec = inBoundDetailEntity.Spec;
                receiveRecord.ItemUnitText = item.ItemUnitText;
                receiveRecord.CurQtyAndNeedQty = (inBoundDetailEntity.CurQty ?? 0).ToString("0.##") + "/" + (inBoundDetailEntity.Qty ?? 0).ToString("0.##");
                receiveRecord.IsItemMark = item.IsItemMark;
                return Content(receiveRecord.ToJson());
            }
            else
            {
                return Error("该站台单据容器类型未知", "");
            }
        }
        #endregion

        #region 根据料箱编码获组盘列表
        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetReceiveRecordList(string inBoundDetailID, string barCode)
        {
            IList<T_ReceiveRecordEntity> data = new List<T_ReceiveRecordEntity>();
            if (string.IsNullOrEmpty(inBoundDetailID) && string.IsNullOrEmpty(barCode))
            {
                return Content(data.ToJson());
            }

            IQueryable<T_ReceiveRecordEntity> query = receiveRecordApp.FindList(o => true);
            if (!string.IsNullOrEmpty(inBoundDetailID))
            {
                query = query.Where(o => o.InBoundDetailID == inBoundDetailID);
            }
            if (!string.IsNullOrEmpty(barCode))
            {
                query = query.Where(o => o.BarCode == barCode);
            }
            data = query.ToList();

            return Content(data.ToJson());
        }
        #endregion

        #region RF提交收货信息（料箱）
        [HttpPost]
        [HandlerAjaxOnly]
        [ValidateAntiForgeryToken]
        public ActionResult SubmitReceiveRecordForm(T_ReceiveRecordEntity postReceiveRecord)
        {
            using (var db = new RepositoryBase().BeginTrans())
            {
                LogObj logObj = new LogObj();
                logObj.Path = "RFInBound_PlasticController.SubmitReceiveRecordForm"; //按实际情况修改
                logObj.Parms = new { postReceiveRecord = postReceiveRecord }; //按实际情况修改

                LogEntity logEntity = new LogEntity();
                logEntity.F_ModuleName = "入库单"; //按实际情况修改
                logEntity.F_Type = DbLogType.Submit.ToString(); //按实际情况修改
                logEntity.F_Account = OperatorProvider.Provider.GetCurrent().UserCode;
                logEntity.F_NickName = OperatorProvider.Provider.GetCurrent().UserName;
                logEntity.F_Description = "RF组盘"; //按实际情况修改
                logEntity.F_Path = logObj.Path;
                logEntity.F_Param = logObj.Parms.ToJson();

                try
                {
                    /*************************************************/
                    if (postReceiveRecord.Qty < 1) return Error("数量至少1个", "");

                    T_InBoundDetailEntity t_InBoundDetailEntity = db.FindEntity<T_InBoundDetailEntity>(o => o.F_Id == postReceiveRecord.InBoundDetailID);
                    T_InBoundEntity inbound = db.FindEntity<T_InBoundEntity>(o => o.F_Id == t_InBoundDetailEntity.InBoundID);
                    postReceiveRecord.ItemID = t_InBoundDetailEntity.ItemID;

                    T_ItemEntity itemEntity = db.FindEntity<T_ItemEntity>(o => o.F_Id == t_InBoundDetailEntity.ItemID);
                    T_StationEntity t_StationEntity = db.FindEntity<T_StationEntity>(o => o.StationCode == FixType.Station.StationIn_Normal.ToString()); //正常入库站台
                    if (t_InBoundDetailEntity.StationID != t_StationEntity.F_Id)
                    {
                        return Error("操作失败:当前物料不能从该站台入库", "");
                    }

                    postReceiveRecord.CheckState = t_InBoundDetailEntity.CheckState;

                    IList<T_ReceiveRecordEntity> receiveRecordEntityList = db.FindList<T_ReceiveRecordEntity>(
                        o => o.BarCode == postReceiveRecord.BarCode && o.InBoundDetailID == postReceiveRecord.InBoundDetailID
                        && o.State == "NewGroup");



                    if (itemEntity.IsMixItem == "false")
                    {
                        var temp = receiveRecordEntityList.Where(o => o.ItemID != postReceiveRecord.ItemID).ToList();
                        if (temp.Count > 0)
                        {
                            return Error("操作失败:当前物料不允许混料", "");
                        }
                    }
                    else
                    {
                        var temp = receiveRecordEntityList.Where(o => o.ItemID != postReceiveRecord.ItemID).ToList();
                        foreach (T_ReceiveRecordEntity cell in temp)
                        {
                            T_ItemEntity item = db.FindEntity<T_ItemEntity>(o => o.F_Id == cell.ItemID);
                            if (item.IsMixItem == "false")
                            {
                                return Error("操作失败:当前料箱中的物料不允许混料", "");
                            }
                        }
                    }

                    if (itemEntity.IsMixLot == "false")
                    {
                        var temp = receiveRecordEntityList.Where(o => o.Lot != postReceiveRecord.Lot).ToList();
                        if (temp.Count > 0)
                        {
                            return Error("操作失败:当前物料不允许混批", "");
                        }
                    }
                    else
                    {
                        var temp = receiveRecordEntityList.Where(o => o.Lot != postReceiveRecord.Lot).ToList();
                        foreach (T_ReceiveRecordEntity cell in temp)
                        {
                            T_ItemEntity item = db.FindEntity<T_ItemEntity>(o => o.F_Id == cell.ItemID);
                            if (item.IsMixLot == "false")
                            {
                                return Error("操作失败:当前料箱中的物料不允许混批", "");
                            }
                        }
                    }

                    if (itemEntity.IsMixQA == "false")
                    {
                        var temp = receiveRecordEntityList.Where(o => o.CheckState != postReceiveRecord.CheckState).ToList();
                        if (temp.Count > 0)
                        {
                            return Error("操作失败:当前物料不同质检状态不允许混批", "");
                        }
                    }
                    else
                    {
                        var temp = receiveRecordEntityList.Where(o => o.CheckState != postReceiveRecord.CheckState).ToList();
                        foreach (T_ReceiveRecordEntity cell in temp)
                        {
                            T_ItemEntity item = db.FindEntity<T_ItemEntity>(o => o.F_Id == cell.ItemID);
                            if (item.IsMixQA == "false")
                            {
                                return Error("操作失败:当前料箱中的物料不允许不同质检状态混放", "");
                            }
                        }
                    }


                    IList<T_ReceiveRecordEntity> existsBarCodeList = db.FindList<T_ReceiveRecordEntity>(o => o.BarCode == postReceiveRecord.BarCode && o.InBoundDetailID != postReceiveRecord.InBoundDetailID && o.State != "PutawayOver").ToList();
                    if (existsBarCodeList.Count > 0)
                    {
                        return Error("操作失败:料箱已被其它单据占用：" + postReceiveRecord.BarCode, "");
                    }

                    IList<T_ContainerDetailEntity> existsBarCodeInContainerDetail = db.FindList<T_ContainerDetailEntity>(o => o.BarCode == postReceiveRecord.BarCode).ToList();
                    if (existsBarCodeInContainerDetail.Count > 0)
                    {
                        return Error("操作失败:库存中已存在相同料箱：" + postReceiveRecord.BarCode, "");
                    }

                    if (itemEntity.IsItemMark == "true")
                    {
                        T_MarkRuleEntity markRule = db.FindEntity<T_MarkRuleEntity>(o => o.InBoundDetailID == t_InBoundDetailEntity.F_Id);
                        IList<T_MarkRecordEntity> markRecordList = db.FindList<T_MarkRecordEntity>(o => o.MarkRuleID == markRule.F_Id && o.BarCode == postReceiveRecord.ItemBarCode);
                        if (markRecordList.Count < 1)
                        {
                            return Error("操作失败:标签条码不是该单据打印的标签：" + postReceiveRecord.ItemBarCode, "");
                        }

                        IList<T_ReceiveRecordEntity> existsItemBarCodeList = db.FindList<T_ReceiveRecordEntity>(o => o.ItemBarCode == postReceiveRecord.ItemBarCode).ToList();
                        if (existsItemBarCodeList.Count > 0)
                        {
                            return Error("操作失败:物料标签条码重复", "");
                        }

                        T_MarkRecordEntity markRecord = markRecordList.FirstOrDefault();
                        markRecord.Qty = postReceiveRecord.Qty;
                        markRecord.IsUsed = "true";
                        db.Update<T_MarkRecordEntity>(markRecord);
                    }
                    else
                    {
                        postReceiveRecord.ItemBarCode = postReceiveRecord.BarCode; //不管理标签，则标签为容器条码
                    }

                    T_ReceiveRecordEntity receiveRecordEntity = db.FindList<T_ReceiveRecordEntity>(o => o.InBoundDetailID == postReceiveRecord.InBoundDetailID && o.BarCode == postReceiveRecord.BarCode).OrderByDescending(o => o.F_CreatorTime).FirstOrDefault();
                    if (receiveRecordEntity != null && receiveRecordEntity.State == "LockOver")
                    {
                        return Error("操作失败:料箱已封箱", "");
                    }

                    T_InBoundDetailEntity inBoundDetailEntity = db.FindEntity<T_InBoundDetailEntity>(o => o.F_Id == postReceiveRecord.InBoundDetailID);
                    IList<T_ReceiveRecordEntity> receiveRecordAllList = db.FindList<T_ReceiveRecordEntity>(o => o.InBoundDetailID == postReceiveRecord.InBoundDetailID);

                    decimal? QtySum = receiveRecordAllList.Sum(o => o.Qty);
                    if ((QtySum + postReceiveRecord.Qty) > inBoundDetailEntity.Qty)
                    {
                        return Error("操作失败:数量超出应收数量", "");
                    }


                    T_InBoundEntity inBoundEntity = db.FindEntity<T_InBoundEntity>(o => o.F_Id == postReceiveRecord.InBoundID);
                    T_ItemEntity t_ItemEntity = itemApp.FindEntity(o => o.F_Id == inBoundDetailEntity.ItemID);

                    T_ContainerEntity containerEntity = db.FindEntity<T_ContainerEntity>(o => o.BarCode == postReceiveRecord.BarCode && o.F_DeleteMark == false);
                    if (containerEntity == null) //如果是新容器
                    {
                        containerEntity = new T_ContainerEntity();
                        containerEntity.F_Id = Guid.NewGuid().ToString();
                        containerEntity.BarCode = postReceiveRecord.BarCode;
                        T_ContainerTypeEntity containerTypeEntity = containerTypeApp.FindEntity(o => o.ContainerTypeCode == t_ItemEntity.ContainerType);
                        containerEntity.ContainerType = containerTypeEntity.ContainerTypeCode;
                        containerEntity.ContainerKind = containerTypeEntity.ContainerKind;
                        containerEntity.IsContainerVir = "0";
                        T_AreaEntity areaEntnty = db.FindEntity<T_AreaEntity>(o => o.F_Id == inBoundDetailEntity.StoreAreaID);
                        containerEntity.AreaID = areaEntnty.F_Id;
                        containerEntity.AreaCode = areaEntnty.AreaCode;
                        containerEntity.AreaName = areaEntnty.AreaName;
                        containerEntity.F_DeleteMark = false;
                        db.Insert<T_ContainerEntity>(containerEntity);
                    }
                    else
                    {
                        if (containerEntity.F_DeleteMark == true) //如果之前有记录，且已被假删除，则标记为未删除
                        {
                            containerEntity.BarCode = postReceiveRecord.BarCode;
                            T_ContainerTypeEntity containerTypeEntity = containerTypeApp.FindEntity(o => o.ContainerTypeCode == t_ItemEntity.ContainerType);
                            containerEntity.ContainerType = containerTypeEntity.ContainerTypeCode;
                            containerEntity.ContainerKind = containerTypeEntity.ContainerKind;
                            containerEntity.IsContainerVir = "0";
                            T_AreaEntity areaEntnty = db.FindEntity<T_AreaEntity>(o => o.F_Id == inBoundDetailEntity.StoreAreaID);
                            containerEntity.AreaID = areaEntnty.F_Id;
                            containerEntity.AreaCode = areaEntnty.AreaCode;
                            containerEntity.AreaName = areaEntnty.AreaName;
                            containerEntity.F_DeleteMark = false;
                            db.Update<T_ContainerEntity>(containerEntity);
                        }
                    }

                    /// 入库容器类型与单据物料容器类型不符
                    if (containerEntity.ContainerKind == "Rack")
                    {
                        return Error("容器类型错误：料架", ""); /// 料架
                    }
                    else if (containerEntity.ContainerKind == "Box")
                    {
                        return Error("容器类型错误：纸箱", ""); /// 纸箱
                    }
                    T_ReceiveRecordEntity prevRec = db.FindEntity<T_ReceiveRecordEntity>(o => o.InBoundDetailID == inBoundDetailEntity.F_Id
                                                                                                     && o.BarCode == postReceiveRecord.BarCode
                                                                                                     && o.ItemBarCode == postReceiveRecord.ItemBarCode
                                                                                                     && o.ItemID == itemEntity.F_Id
                                                                                                     && o.Lot == inBoundDetailEntity.Lot
                                                                                                     && o.State == "NewGroup");

                    if (itemEntity.IsItemMark == "true" || prevRec == null) //管理标签则插入，或者没有上个收货记录也需插入
                    {
                        postReceiveRecord.ContainerType = containerEntity.ContainerType;
                        postReceiveRecord.ContainerKind = containerEntity.ContainerKind;

                        postReceiveRecord.F_Id = Guid.NewGuid().ToString();
                        postReceiveRecord.ItemCode = inBoundDetailEntity.ItemCode;
                        postReceiveRecord.SupplierUserID = inBoundEntity.SupplierUserID;
                        postReceiveRecord.DoneUserID = OperatorProvider.Provider.GetCurrent().UserId;
                        postReceiveRecord.DoneUserName = OperatorProvider.Provider.GetCurrent().UserName;
                        postReceiveRecord.State = "NewGroup";
                        postReceiveRecord.TransState = "WaittingTrans";
                        postReceiveRecord.F_DeleteMark = false;
                        postReceiveRecord.ReceiveStaionID = t_StationEntity.F_Id; //当前入库站台
                        postReceiveRecord.AreaID = inBoundDetailEntity.StoreAreaID;
                        postReceiveRecord.ERPWarehouseCode = inBoundDetailEntity.ERPWarehouseCode;
                        postReceiveRecord.IsItemMark = itemEntity.IsItemMark;
                        postReceiveRecord.Factory = itemEntity.Factory;
                        postReceiveRecord.ValidityDayNum = inBoundDetailEntity.ValidityDayNum;
                        postReceiveRecord.OverdueDate = inBoundDetailEntity.OverdueDate;
                        postReceiveRecord.IsScanOver = "false";
                        postReceiveRecord.SEQ = inBoundDetailEntity.SEQ;
                        postReceiveRecord.Spec = itemEntity.Spec;
                        postReceiveRecord.ItemUnitText = itemEntity.ItemUnitText;
                        db.Insert<T_ReceiveRecordEntity>(postReceiveRecord);
                    }
                    else
                    {
                        prevRec.Qty = (prevRec.Qty ?? 0) + postReceiveRecord.Qty;
                        db.Update<T_ReceiveRecordEntity>(prevRec);
                    }

                    inBoundDetailEntity.CurQty = (inBoundDetailEntity.CurQty ?? 0) + (postReceiveRecord.Qty ?? 0);
                    db.Update<T_InBoundDetailEntity>(inBoundDetailEntity);
                    db.CommitWithOutRollBack();

                    bool isOver = false;
                    if (inBoundDetailEntity.CurQty >= inBoundDetailEntity.Qty)
                    {
                        isOver = true;
                    }

                    /**************************************************/

                    logObj.Message = "操作成功";
                    LogFactory.GetLogger().Info(logObj);

                    logEntity.F_Result = true;
                    new LogApp().WriteDbLog(logEntity);

                    return Content(new AjaxResult { state = ResultType.success.ToString(), message = "操作成功", data = new { isOver = isOver } }.ToJson());
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

        #region 删除组盘信息
        [HttpPost]
        [HandlerAjaxOnly]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteRecord(string recordID)
        {
            LogObj logObj = new LogObj();
            logObj.Path = "RFInBound_PlasticController.DeleteRecord"; //按实际情况修改
            logObj.Parms = new { recordID = recordID }; //按实际情况修改

            LogEntity logEntity = new LogEntity();
            logEntity.F_ModuleName = "入库单"; //按实际情况修改
            logEntity.F_Type = DbLogType.Submit.ToString(); //按实际情况修改
            logEntity.F_Account = OperatorProvider.Provider.GetCurrent().UserCode;
            logEntity.F_NickName = OperatorProvider.Provider.GetCurrent().UserName;
            logEntity.F_Description = "RF删除组盘信息"; //按实际情况修改
            logEntity.F_Path = logObj.Path;
            logEntity.F_Param = logObj.Parms.ToJson();

            try
            {
                /*************************************************/
                T_ReceiveRecordEntity recordEntity = receiveRecordApp.FindEntity(o => o.F_Id == recordID);
                if (recordEntity.State != "NewGroup")
                {
                    return Error("该记录不是新组货状态。", "");
                }
                T_InBoundDetailEntity inBoundDetail = inBoundDetailApp.FindEntity(o => o.F_Id == recordEntity.InBoundDetailID);
                inBoundDetail.CurQty = (inBoundDetail.CurQty ?? 0) - (recordEntity.Qty ?? 0);
                inBoundDetailApp.Update(inBoundDetail);
                receiveRecordApp.DeleteForm(recordID);

                if (recordEntity.IsItemMark == "true")
                {
                    T_MarkRecordEntity markRecord = markRecordApp.FindEntity(o => o.BarCode == recordEntity.ItemBarCode);
                    markRecord.IsUsed = "false";
                    markRecordApp.Update(markRecord);
                }

                /**************************************************/

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

        #region (采购入库单)RF提交收货数据，产生入库任务
        [HttpPost]
        [HandlerAjaxOnly]
        [ValidateAntiForgeryToken]
        public ActionResult GenPlasticInBoundTask(string barCode, string PointLoc)
        {
            using (var db = new RepositoryBase().BeginTrans())
            {
                LogObj logObj = new LogObj();
                logObj.Path = "RFInBound_PlasticController.GenBigItemInBoundTask"; //按实际情况修改
                logObj.Parms = new { barCode = barCode }; //按实际情况修改

                LogEntity logEntity = new LogEntity();
                logEntity.F_ModuleName = "入库单"; //按实际情况修改
                logEntity.F_Type = DbLogType.Submit.ToString(); //按实际情况修改
                logEntity.F_Account = OperatorProvider.Provider.GetCurrent().UserCode;
                logEntity.F_NickName = OperatorProvider.Provider.GetCurrent().UserName;
                logEntity.F_Description = "RF产生入库任务"; //按实际情况修改
                logEntity.F_Path = logObj.Path;
                logEntity.F_Param = logObj.Parms.ToJson();

                try
                {
                    /*************************************************/
                    T_TaskEntity tempTask = db.FindEntity<T_TaskEntity>(o => o.BarCode == barCode);
                    if (tempTask != null)
                    {
                        return Error("任务已存在。", "");
                    }

                    T_StationEntity t_StationEntity = db.FindEntity<T_StationEntity>(o => o.StationCode == FixType.Station.StationIn_Normal.ToString()); //正常情况应该前台下拉传入当前收货站台，此处站台只一个，因此省略
                    if (string.IsNullOrEmpty(t_StationEntity.CurOrderDetailID))
                    {
                        return Error("站台没有单据", "");
                    }
                    IList<T_ReceiveRecordEntity> receiveRecordList = db.FindList<T_ReceiveRecordEntity>(o => o.BarCode == barCode && o.InBoundDetailID == t_StationEntity.CurOrderDetailID && o.State == "NewGroup").ToList(); // 已收货，未入库

                    if (receiveRecordList.Count < 1)
                    {
                        T_TaskEntity existsTask = db.FindEntity<T_TaskEntity>(o => o.BarCode == barCode && t_StationEntity.CurOrderDetailID == o.OrderDetailID);

                        if (existsTask != null)
                        {
                            return Error("该料箱已提交过。", "");
                        }
                        return Error("该料箱没有任何物料。", "");
                    }

                    //t_StationEntity.BarCode = barCode;
                    //db.Update<T_StationEntity>(t_StationEntity);

                    string inBoundDetailID = receiveRecordList[0].InBoundDetailID;
                    string inBoundID = receiveRecordList[0].InBoundID;
                    string itemID = receiveRecordList[0].ItemID;
                    T_ItemEntity t_ItemEntity = db.FindEntity<T_ItemEntity>(o => o.F_Id == itemID);
                    T_ContainerEntity containerEntity = db.FindEntity<T_ContainerEntity>(o => o.BarCode == barCode && o.F_DeleteMark == false);
                    T_InBoundDetailEntity t_InBoundDetailEntity = db.FindEntity<T_InBoundDetailEntity>(o => o.F_Id == inBoundDetailID);
                    T_InBoundEntity t_InBoundEntity = db.FindEntity<T_InBoundEntity>(o => o.F_Id == inBoundID);


                    T_TaskEntity taskEntity = new T_TaskEntity();
                    taskEntity.F_Id = Guid.NewGuid().ToString();
                    taskEntity.TaskNo = T_CodeGenApp.GenNum("TaskRule");
                    if (!string.IsNullOrEmpty(PointLoc)) //手动指定货位
                    {
                        string errMsg = "";
                        T_LocationApp locationApp = new T_LocationApp();
                        T_ContainerTypeEntity containerType = db.FindEntity<T_ContainerTypeEntity>(o => o.ContainerTypeCode == containerEntity.ContainerType);
                        T_LocationEntity loc = null;

                        taskEntity.IsHandPointLoc = "true";
                        loc = locationApp.CheckLocIn(ref errMsg, db, containerType, t_InBoundDetailEntity.StoreAreaID, false, t_InBoundDetailEntity.ERPWarehouseCode, t_InBoundDetailEntity.ItemID, t_InBoundDetailEntity.CheckState, PointLoc, true);
                        if (loc == null)
                        {
                            throw new Exception(errMsg);
                        }

                        taskEntity.TagLocationID = loc.F_Id;
                        taskEntity.TagLocationCode = loc.LocationCode;
                        taskEntity.TagWCSLocCode = loc.WCSLocCode;
                    }
                    else  //自动指定货位
                    {
                        taskEntity.IsHandPointLoc = "false";
                    }
                    taskEntity.TaskInOutType = "InType";
                    taskEntity.TaskType = "TaskType_PurchaseIn";
                    taskEntity.ContainerType = containerEntity.ContainerType;
                    taskEntity.ContainerID = containerEntity.F_Id;
                    taskEntity.BarCode = containerEntity.BarCode;
                    taskEntity.SrcLocationID = t_StationEntity.F_Id;
                    taskEntity.SrcLocationCode = t_StationEntity.LeaveAddress;
                    taskEntity.SrcWCSLocCode = t_StationEntity.LeaveAddress;
                    taskEntity.Level = 10;
                    taskEntity.State = "New";
                    taskEntity.IsWcsTask = "true";
                    taskEntity.IsCanExec = "true";
                    taskEntity.OrderType = "PurchaseIn";
                    taskEntity.OrderID = t_InBoundDetailEntity.InBoundID;
                    taskEntity.OrderDetailID = t_InBoundDetailEntity.F_Id;
                    taskEntity.OrderCode = t_InBoundEntity.InBoundCode;
                    taskEntity.TagAreaID = t_InBoundDetailEntity.StoreAreaID;
                    taskEntity.F_DeleteMark = false;
                    taskEntity.ApplyStationID = t_InBoundDetailEntity.StationID;
                    taskEntity.SEQ = t_InBoundDetailEntity.SEQ;
                    taskEntity.ContainerType = containerEntity.ContainerType;
                    db.Insert<T_TaskEntity>(taskEntity);

                    foreach (T_ReceiveRecordEntity item in receiveRecordList)
                    {
                        item.State = "LockOver";
                        db.Update<T_ReceiveRecordEntity>(item);
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

                    return Error("操作失败:" + ex.Message, ex.ToJson());
                }
            }
        }
        #endregion

    }
}
