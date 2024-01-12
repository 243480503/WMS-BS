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

namespace MST.Web.Areas.RF_OutBoundManage.Controllers
{
    public class RFOutBound_AllController : ControllerBase
    {
        private T_OutRecordApp outRecordApp = new T_OutRecordApp();
        private T_StationApp stationApp = new T_StationApp();
        private T_ContainerApp containerApp = new T_ContainerApp();
        private T_ContainerTypeApp containerTypeApp = new T_ContainerTypeApp();
        private T_ContainerDetailApp containerDetailApp = new T_ContainerDetailApp();
        private T_OutBoundApp outBoundApp = new T_OutBoundApp();
        private T_OutBoundDetailApp outBoundDetailApp = new T_OutBoundDetailApp();
        private T_ItemApp itemApp = new T_ItemApp();
        private ItemsDetailApp itemsDetailApp = new ItemsDetailApp();
        private T_InOutDetailApp inOutDetailApp = new T_InOutDetailApp();
        private T_WaveApp waveApp = new T_WaveApp();

        #region 扫码 后获取容器的出库信息
        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetDetailJson(string barCode, string itemBarCode)
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

            IList<T_OutRecordEntity> outRecordList = outRecordApp.FindList(o => o.BarCode == barCode && o.OutBoundID == station.CurOrderID).ToList();
            IList<T_OutRecordEntity> PickingList = outRecordList.Where(o => o.State == "Picking").ToList();
            if (PickingList.Count() < 1)
            {
                IList<T_OutRecordEntity> PickedList = outRecordList.Where(o => o.State == "OverPick").ToList();
                if (PickedList.Count() < 1) return Error("当前单据在容器中无出库需求", "");
                else return Error("当前容器已被拣选过", "");
            }

            T_OutRecordEntity outRecord = new T_OutRecordEntity();
            if (string.IsNullOrEmpty(itemBarCode)) outRecord = PickingList.FirstOrDefault();
            else
            {
                outRecord = outRecordList.FirstOrDefault(o => o.ItemBarCode == itemBarCode); /// 在所有拣选记录中找
                if (outRecord == null)
                {
                    T_ContainerDetailEntity cdNew = containerDetailApp.FindEntity(o => o.BarCode == barCode && o.ItemBarCode == itemBarCode);
                    if (cdNew == null) return Error("当前容器不存在该子码", "");
                    if (cdNew.IsCheckFreeze == "true" || cdNew.IsCountFreeze == "true" || cdNew.State == "Freeze") return Error("子码状态冻结", "");

                    T_OutRecordEntity canRec = new T_OutRecordEntity();
                    List<T_OutRecordEntity> canPickList = PickingList.Where(o => o.ItemID == cdNew.ItemID && (o.Lot == cdNew.Lot || (string.IsNullOrEmpty(o.Lot) && string.IsNullOrEmpty(cdNew.Lot))) && o.IsAuto == "true").ToList();
                    if (canPickList.Count == 0) return Error("子码不可替代拣选", "");
                    else
                    {
                        /// 判断数量，最优拣选
                        decimal? minQty = decimal.MaxValue;
                        decimal? diffQty = 0;
                        foreach (T_OutRecordEntity rec in canPickList)
                        {
                            if (cdNew.Qty < rec.NeedQty) continue;
                            diffQty = cdNew.Qty - rec.NeedQty;
                            if (minQty > diffQty)
                            {
                                minQty = diffQty;
                                canRec = rec;
                            }
                        }
                    }
                    if (canRec == null) return Error("子码标签数量不足", "");

                    outRecord = canRec;
                    outRecord.ItemBarCode = cdNew.ItemBarCode;
                    outRecord.OldQty = cdNew.Qty;
                }
            }

            if (outRecord.State == "OverPick") return Error("子码已拣选", "");

            T_ContainerDetailEntity containerDetailEntity = containerDetailApp.FindEntity(o => o.F_Id == outRecord.ContainerDetailID);
            T_OutBoundEntity outBoundEntity = outBoundApp.FindEntity(o => o.F_Id == outRecord.OutBoundID);
            T_OutBoundDetailEntity outDetailEntity = outBoundDetailApp.FindEntity(o => o.F_Id == outRecord.OutBoundDetailID);
            T_ItemEntity item = itemApp.FindEntity(o => o.F_Id == outRecord.ItemID);

            OutRecordModel outRecordModel = outRecord.ToObject<OutRecordModel>();
            outRecordModel.RefOrderCode = outBoundEntity.RefOrderCode;
            outRecordModel.ItemCode = outRecord.ItemCode;
            outRecordModel.ItemName = outRecord.ItemName;
            outRecordModel.CreateCompany = item.Factory;
            outRecordModel.ProductDate = containerDetailEntity.ProductDate;
            outRecordModel.Lot = outRecord.Lot;
            outRecordModel.OrderQty = outDetailEntity.Qty;
            outRecordModel.NeedQty = outRecord.NeedQty;
            outRecordModel.ReadyQutAndOrderNeed = (outRecordList.Sum(o => o.PickedQty ?? 0)).ToString("0.##") + "/" + (outRecordList.Sum(o => o.NeedQty ?? 0)).ToString("0.##");
            outRecordModel.ContainerKind = container.ContainerKind;
            outRecordModel.ContainerKindName = itemsDetailApp.FindEnum<T_ContainerTypeEntity>(o => o.ContainerKind).FirstOrDefault(o => o.F_ItemCode == container.ContainerKind).F_ItemName;
            outRecordModel.OutBoundDetailID = outRecord.OutBoundDetailID;
            outRecordModel.OutBoundID = outRecord.OutBoundID;
            outRecordModel.F_Id = outRecord.F_Id;
            outRecordModel.StationID = station.F_Id;
            outRecordModel.ItemBarCode = outRecord.ItemBarCode;
            outRecordModel.IsItemMark = outRecord.IsItemMark;
            outRecordModel.OverdueDate = outRecord.OverdueDate;
            outRecordModel.Spec = outRecord.Spec;
            outRecordModel.ItemUnitText = item.ItemUnitText;

            outRecordModel.MustTimes = outRecordList.Count;
            outRecordModel.NoPickTimes = PickingList.Count;
            return Content(outRecordModel.ToJson());

        }
        #endregion

        #region 切换选项卡后，根据料箱编码获出库列表
        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetOutRecordList(string OutBoundID, string barCode)
        {
            IList<T_OutRecordEntity> dataList = new List<T_OutRecordEntity>();
            if (!string.IsNullOrEmpty(OutBoundID))
            {
                dataList = outRecordApp.FindList(o => o.BarCode == barCode && o.OutBoundID == OutBoundID && o.State == "OverPick").OrderBy(o => o.PickDate).ToList();
            }
            else
            {
                T_OutRecordEntity lastEntity = outRecordApp.FindList(o => o.BarCode == barCode).OrderByDescending(o => o.PickDate).FirstOrDefault();
                if (lastEntity != null)
                {
                    dataList = outRecordApp.FindList(o => o.BarCode == barCode && o.OutBoundID == lastEntity.OutBoundID).OrderBy(o => o.PickDate).ToList();
                }
            }
            return Content(dataList.ToJson());
        }
        #endregion

        #region 切换选项卡后，根据料箱编码获出库列表
        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetContainerKind(string barCode)
        {
            T_ContainerEntity containerEntity = containerApp.FindEntity(o => o.BarCode == barCode);
            return Content(containerEntity.ToJson());
        }
        #endregion

        #region RF提交出库拣选信息，纸箱、料箱、料架的拣选 （纸箱关闭单据并清空站台，料箱、料架 关闭单据但不清空站台,纸箱的整箱拣选在出库任务完成时自动拣选,此处为非整箱拣选）
        [HttpPost]
        [HandlerAjaxOnly]
        [ValidateAntiForgeryToken]
        public ActionResult SubmitOutRecordForm(OutRecordModel postOutRecord)
        {
            using (var db = new RepositoryBase().BeginTrans())
            {
                LogObj logObj = new LogObj();
                logObj.Path = "RFInBound_PlasticController.SubmitOutRecordForm"; //按实际情况修改
                logObj.Parms = new { postOutRecord = postOutRecord }; //按实际情况修改

                LogEntity logEntity = new LogEntity();
                logEntity.F_ModuleName = "出库单"; //按实际情况修改
                logEntity.F_Type = DbLogType.Submit.ToString(); //按实际情况修改
                logEntity.F_Account = OperatorProvider.Provider.GetCurrent().UserCode;
                logEntity.F_NickName = OperatorProvider.Provider.GetCurrent().UserName;
                logEntity.F_Description = "RF出库拣选"; //按实际情况修改
                logEntity.F_Path = logObj.Path;
                logEntity.F_Param = logObj.Parms.ToJson();

                try
                {
                    /*************************************************/
                    int NoPickTimes = 0;    // 剩余次数
                    decimal? OverPickQty = 0;
                    decimal? AllNeedQty = 0;

                    AjaxResult tempRes = outRecordApp.PickRecord(db, postOutRecord.StationID, postOutRecord.BarCode, postOutRecord.F_Id, postOutRecord.ItemBarCode, postOutRecord.NeedQty, ref NoPickTimes, ref OverPickQty, ref AllNeedQty);
                    if (tempRes.state.ToString() != ResultType.success.ToString())
                    {
                        db.RollBack();
                        return Error("操作失败。", tempRes.message);
                    }
                    else
                    {
                        string ReadyQutAndOrderNeed = OverPickQty + "/" + AllNeedQty;  // 已出 / 总出
                        /**************************************************/
                        db.CommitWithOutRollBack();

                        logObj.Message = "操作成功";
                        LogFactory.GetLogger().Info(logObj);

                        logEntity.F_Result = true;
                        new LogApp().WriteDbLog(logEntity);

                        return Success("操作成功。", new { NoPickTimes = NoPickTimes, ReadyQutAndOrderNeed = ReadyQutAndOrderNeed });
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


        #region 料架-拣选回库，申请回库 （此处只针对将料架分配货位，并将任务推送到WCS，纸箱、料箱、料架在 拣选完毕时均已产生任务）
        [HttpPost]
        [HandlerAjaxOnly]
        [ValidateAntiForgeryToken]
        public ActionResult RackBackIn(string barCode)
        {
            using (var db = new RepositoryBase().BeginTrans())
            {
                LogObj logObj = new LogObj();
                logObj.Path = "RFOutBound_AllController.RackBackIn"; //按实际情况修改
                logObj.Parms = new { barCode = barCode }; //按实际情况修改

                LogEntity logEntity = new LogEntity();
                logEntity.F_ModuleName = "出库单"; //按实际情况修改
                logEntity.F_Type = DbLogType.Submit.ToString(); //按实际情况修改
                logEntity.F_Account = OperatorProvider.Provider.GetCurrent().UserCode;
                logEntity.F_NickName = OperatorProvider.Provider.GetCurrent().UserName;
                logEntity.F_Description = "RF料架申请回库"; //按实际情况修改
                logEntity.F_Path = logObj.Path;
                logEntity.F_Param = logObj.Parms.ToJson();

                try
                {
                    /*************************************************/
                    T_StationEntity station = db.FindEntity<T_StationEntity>(o => o.StationCode == FixType.Station.StationOut_BigItem.ToString());
                    T_TaskEntity taskEntity = db.FindEntity<T_TaskEntity>(o => o.BarCode == barCode && o.ApplyStationID == station.F_Id);
                    if (taskEntity == null)
                    {
                        return Error("容器编码不正确。", "");
                    }

                    if (!string.IsNullOrEmpty(taskEntity.TagLocationID))
                    {
                        return Error("已申请过。", "");
                    }
                    else
                    {
                        string errMsg = "";
                        T_LocationApp locationApp = new T_LocationApp();
                        T_ContainerTypeEntity containerType = db.FindEntity<T_ContainerTypeEntity>(o => o.ContainerTypeCode == taskEntity.ContainerType);
                        T_LocationEntity loc = null;
                        LogObj log = null;
                        if (taskEntity.TaskType == "TaskType_EmptyIn")
                        {
                            T_ItemEntity item = db.FindEntity<T_ItemEntity>(o => o.ItemCode == FixType.Item.EmptyRack.ToString());
                            loc = locationApp.GetLocIn(ref errMsg, ref log, db, containerType, taskEntity.TagAreaID, true, null,null, true,null, item);
                        }
                        else
                        {
                            T_ContainerDetailEntity oneConDetail = db.FindEntity<T_ContainerDetailEntity>(o => o.BarCode == barCode);
                            T_ItemEntity item = db.FindEntity<T_ItemEntity>(o => o.F_Id == oneConDetail.ItemID);
                            loc = locationApp.GetLocIn(ref errMsg, ref log, db, containerType, taskEntity.TagAreaID, false, oneConDetail.ERPWarehouseCode,oneConDetail.CheckState, true,null, item);
                        }

                        if (loc == null)
                        {
                            return Error("货位分配失败：" + errMsg, "");
                        }

                        taskEntity.TagLocationCode = loc.LocationCode;
                        taskEntity.TagLocationID = loc.F_Id;
                        taskEntity.TagWCSLocCode = loc.WCSLocCode;
                        taskEntity.SendWCSTime = DateTime.Now;
                        db.SaveChanges();

                        IList<string> taskIDList = new List<string>();
                        taskIDList.Add(taskEntity.TaskNo);
                        WCSResult wcsRes = new WCSPost().SendTask(db, taskIDList);
                        if (wcsRes.IsSuccess)
                        {
                            db.Update<T_TaskEntity>(taskEntity);

                            //清空站台
                            if (station.BarCode == barCode)
                            {
                                station.BarCode = "";
                            }

                            List<T_OutRecordEntity> list = db.FindList<T_OutRecordEntity>(o => o.OutBoundID == station.CurOrderID && o.BarCode == taskEntity.BarCode).ToList();
                            foreach (T_OutRecordEntity rec in list)
                            {
                                rec.IsScanBack = "true";
                                rec.IsNeedBackWare = "true";
                                db.Update<T_OutRecordEntity>(rec);
                            }

                            IList<T_OutRecordEntity> recList = db.FindList<T_OutRecordEntity>(o => o.TagLocationID == station.F_Id && o.OutBoundID == station.CurOrderID && o.State != "OverPick");//该站台该单据已出完
                            if (recList.Count < 1) //该站台已没有当前单据的拣选任务
                            {
                                station.CurOrderDetailID = "";
                                station.WaveID = "";
                                station.CurOrderID = "";
                                station.OrderType = "";
                            }
                            db.Update<T_StationEntity>(station);
                            db.SaveChanges();

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
                            return Error("操作失败。", wcsRes.FailMsg);
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
        #endregion

    }
}
