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

namespace MST.Web.Areas.RF_QAManage.Controllers
{
    public class RFQA_ReturnController : ControllerBase
    {
        private T_QARecordApp qaRecordApp = new T_QARecordApp();
        private T_StationApp stationApp = new T_StationApp();
        private T_ContainerApp containerApp = new T_ContainerApp();
        private T_ContainerTypeApp containerTypeApp = new T_ContainerTypeApp();
        private T_ContainerDetailApp containerDetailApp = new T_ContainerDetailApp();
        private T_QAApp qaApp = new T_QAApp();
        private T_QADetailApp qaDetailApp = new T_QADetailApp();
        private T_ItemApp itemApp = new T_ItemApp();
        private ItemsDetailApp itemsDetailApp = new ItemsDetailApp();
        private T_InOutDetailApp inOutDetailApp = new T_InOutDetailApp();
        private T_WaveApp waveApp = new T_WaveApp();

        #region 扫码 后获取容器的还样信息
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

            T_QAEntity qABack = qaApp.FindEntity(o => o.F_Id == station.CurOrderID);
            T_QAEntity qAGet = qaApp.FindEntity(o => o.QACode == qABack.RefOrderCode && o.QAOrderType == "GetSample");
            if (qAGet == null) return Error($"未找到对应取样单 { qABack.RefOrderCode }", "");

            IList<T_QARecordEntity> qaRecordList = qaRecordApp.FindList(o => o.BarCode == barCode && o.QAID == qAGet.F_Id && o.IsNeedBack == "true").ToList();
            IList<T_QARecordEntity> PickingList = qaRecordList.Where(o => o.State == "Returning").ToList();
            if (PickingList.Count() < 1)
            {
                IList<T_QARecordEntity> PickedList = PickingList.Where(o => o.State == "Over").ToList();
                if (PickedList.Count() < 1) return Error("容器无还样需求", "");
                else return Error("当前容器已还样", "");
            }

            T_QARecordEntity qaRecord = new T_QARecordEntity();
            if (string.IsNullOrEmpty(itemBarcode)) qaRecord = PickingList.FirstOrDefault();
            else
            {
                qaRecord = qaRecordList.FirstOrDefault(o => o.ItemBarCode == itemBarcode);
                if (qaRecord == null) return Error("子码无需还样", "");
                if (qaRecord.State == "Over") return Error("子码已还样", "");
            }

            T_ContainerDetailEntity containerDetailEntity = containerDetailApp.FindEntity(o => o.F_Id == qaRecord.ContainerDetailID);
            T_QAEntity qaEntity = qaApp.FindEntity(o => o.F_Id == qaRecord.QAID);
            T_QADetailEntity qaDetailEntity = qaDetailApp.FindEntity(o => o.QAID == qaEntity.F_Id);
            T_ItemEntity item = itemApp.FindEntity(o => o.F_Id == qaRecord.ItemID);

            QARecordModel qaRecordModel = qaRecord.ToObject<QARecordModel>();
            qaRecordModel.RefOrderCode = qaEntity.RefOrderCode;
            qaRecordModel.ItemCode = qaRecord.ItemCode;
            qaRecordModel.ItemName = qaRecord.ItemName;
            qaRecordModel.CreateCompany = item.Factory;
            qaRecordModel.ProductDate = containerDetailEntity.ProductDate;
            qaRecordModel.Lot = qaRecord.Lot;
            qaRecordModel.OrderQty = qaDetailEntity.SampleSumNum;
            qaRecordModel.PickedQty = qaRecord.PickedQty;
            qaRecordModel.ReadyQutAndOrderNeed = PickingList.Sum(o => o.ReturnQty ?? 0).ToString("0.##") + "/" + PickingList.Sum(o => o.PickedQty ?? 0).ToString("0.##");
            qaRecordModel.ContainerKind = container.ContainerKind;
            qaRecordModel.ContainerKindName = itemsDetailApp.FindEnum<T_ContainerTypeEntity>(o => o.ContainerKind).FirstOrDefault(o => o.F_ItemCode == container.ContainerKind).F_ItemName;
            qaRecordModel.QADetailID = qaRecord.QADetailID;
            qaRecordModel.QAID = station.CurOrderID;
            qaRecordModel.WaveID = station.WaveID;
            qaRecordModel.Spec = qaRecord.Spec;
            qaRecordModel.F_Id = qaRecord.F_Id;
            qaRecordModel.StationID = station.F_Id;
            qaRecordModel.ItemBarCode = qaRecord.ItemBarCode;
            qaRecordModel.IsItemMark = qaRecord.IsItemMark;
            qaRecordModel.MustTimes = qaRecordList.Count;
            qaRecordModel.NoPickTimes = PickingList.Count;
            return Content(qaRecordModel.ToJson());
        }
        #endregion

        #region 切换选项卡后，根据料箱编码获出库列表
        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetQARecordList(string qAID, string barCode)
        {
            IList<T_QARecordEntity> dataList = new List<T_QARecordEntity>();
            if (!string.IsNullOrEmpty(qAID))
            {
                /// 找到质检记录对应的QAID（取样单ID）
                T_QAEntity qABack = qaApp.FindEntity(o => o.F_Id == qAID);
                T_QAEntity qAGet = qaApp.FindEntity(o => o.QACode == qABack.RefOrderCode && o.QAOrderType == "GetSample");

                dataList = qaRecordApp.FindList(o => o.BarCode == barCode && o.QAID == qAGet.F_Id && o.State == "Over").OrderBy(o => o.PickDate).ToList();
            }
            else
            {
                T_QARecordEntity lastEntity = qaRecordApp.FindList(o => o.BarCode == barCode).OrderByDescending(o => o.PickDate).FirstOrDefault();
                if (lastEntity != null) dataList = qaRecordApp.FindList(o => o.BarCode == barCode && o.QAID == lastEntity.QAID && (o.State == "Picked" || o.State == "Over")).OrderBy(o => o.PickDate).ToList();
            }
            return Content(dataList.ToJson());
        }
        #endregion

        #region RF提交出库还样信息
        [HttpPost]
        [HandlerAjaxOnly]
        [ValidateAntiForgeryToken]
        public ActionResult SubmitBackRecordForm(QARecordModel postQARecord)
        {
            using (var db = new RepositoryBase().BeginTrans())
            {
                LogObj logObj = new LogObj();
                logObj.Path = "RFQA_ReturnController.SubmitBackRecordForm";
                logObj.Parms = new { postQARecord = postQARecord };

                LogEntity logEntity = new LogEntity();
                logEntity.F_ModuleName = "质检还样单";
                logEntity.F_Type = DbLogType.Submit.ToString();
                logEntity.F_Account = OperatorProvider.Provider.GetCurrent().UserCode;
                logEntity.F_NickName = OperatorProvider.Provider.GetCurrent().UserName;
                logEntity.F_Description = "RF质检还样";
                logEntity.F_Path = logObj.Path;
                logEntity.F_Param = logObj.Parms.ToJson();

                try
                {
                    /*************************************************/
                    if (string.IsNullOrEmpty(postQARecord.BarCode)) return Error("箱码不能为空", "");

                    T_StationEntity station = db.FindEntity<T_StationEntity>(o => o.F_Id == postQARecord.StationID);
                    T_QAEntity qABack = db.FindEntity<T_QAEntity>(o => o.F_Id == station.CurOrderID && o.QAOrderType == "BackSample");
                    T_QAEntity qAGet = db.FindEntity<T_QAEntity>(o => o.QACode == qABack.RefOrderCode && o.QAOrderType == "GetSample");
                    if (qAGet == null) return Error("未找到对应取样单", "");

                    T_QARecordEntity qARecord = db.FindEntity<T_QARecordEntity>(o => o.F_Id == postQARecord.F_Id);
                    T_QADetailEntity qADetailGet = db.FindEntity<T_QADetailEntity>(o => o.F_Id == qARecord.QADetailID); /// 质检记录对应的单据=取样单
                    T_QADetailEntity qADetailBack = db.FindEntity<T_QADetailEntity>(o => o.QAID == qABack.F_Id && o.ItemID == qADetailGet.ItemID && (o.Lot == qADetailGet.Lot || (string.IsNullOrEmpty(o.Lot) && string.IsNullOrEmpty(qADetailGet.Lot))));

                    /// 根据容器判断回库区域
                    FixType.Area areaEnum;
                    T_ContainerEntity containerEntity = db.FindEntity<T_ContainerEntity>(o => o.F_Id == qARecord.ContainerID);
                    if (containerEntity.ContainerKind == "Rack")
                    {
                        areaEnum = FixType.Area.BigItemArea;
                    }
                    else if (containerEntity.ContainerKind == "Box" || containerEntity.ContainerKind == "Plastic")
                    {
                        areaEnum = FixType.Area.NormalArea;
                    }
                    else return Error("未知的容器大类", "");

                    /// 非纸箱，是否扫码
                    if (containerEntity.ContainerKind != "Box")
                    {
                        List<T_QARecordEntity> ReturnList = db.FindList<T_QARecordEntity>(o => o.QAID == qAGet.F_Id && o.BarCode == postQARecord.BarCode && o.IsNeedBack == "true");

                        if (qARecord.IsItemMark == "true") /// 必须扫码
                        {
                            if (string.IsNullOrEmpty(postQARecord.ItemBarCode)) return Error("子码不能为空", "");
                            if (RuleConfig.OutConfig.RFScanCode.IsItemBarCodeSame) /// 扫码必须一致
                            {
                                /// 是否在质检记录内
                                T_QARecordEntity existsEntity = ReturnList.FirstOrDefault(o => o.ItemBarCode == postQARecord.ItemBarCode);
                                if (existsEntity == null) return Error("子码不正确", "");
                            }
                            else /// 允许不一致
                            {
                                if (qARecord.ItemBarCode != postQARecord.ItemBarCode) /// 扫码不一致
                                {
                                    /// 是否在质检记录内
                                    T_QARecordEntity existsEntity = ReturnList.FirstOrDefault(o => o.ItemBarCode == postQARecord.ItemBarCode);
                                    if (existsEntity != null) /// 存在拣选记录，用当前子码替换拣选记录
                                    {
                                        qARecord = existsEntity;
                                    }
                                    else
                                    {
                                        return Error("子码无需还样", "");
                                    }
                                }
                            }
                        }
                    }

                    if (qARecord.State == "Over")
                    {
                        return Error("子码已还样", "");
                    }

                    qARecord.State = "Over";
                    qARecord.IsReturnOver = "true";

                    if (postQARecord.PickedQty == null || postQARecord.PickedQty == 0) postQARecord.PickedQty = qARecord.PickedQty;    /// 未返回前端本次数量时

                    qARecord.ReturnQty = postQARecord.PickedQty;
                    //qARecord.AfterQty = (qARecord.AfterQty ?? 0) + (postQARecord.PickedQty ?? 0);
                    qARecord.PickDate = DateTime.Now;
                    qARecord.PickUserID = OperatorProvider.Provider.GetCurrent().UserId;
                    qARecord.PickUserName = OperatorProvider.Provider.GetCurrent().UserName;
                    db.Update<T_QARecordEntity>(qARecord);
                    db.SaveChanges();

                    T_ContainerDetailEntity containerDetail = db.FindEntity<T_ContainerDetailEntity>(o => o.F_Id == qARecord.ContainerDetailID);

                    if (qADetailGet.ResultState != "New") containerDetail.IsCheckFreeze = "false";    /// 取样库存解冻必须：1.还样 2.已录入结果
                    containerDetail.CheckQty = (containerDetail.CheckQty ?? 0) - (postQARecord.PickedQty ?? 0);
                    containerDetail.Qty = (containerDetail.Qty ?? 0) + (postQARecord.PickedQty ?? 0);
                    db.Update<T_ContainerDetailEntity>(containerDetail);
                    db.SaveChanges();

                    string orderType = "BackSample";
                    T_AreaEntity area = db.FindEntity<T_AreaEntity>(o => o.AreaCode == areaEnum.ToString());

                    /// 库存流水
                    inOutDetailApp.SyncInOutDetail(db, containerDetail, "InType", orderType, qARecord.AfterQty, postQARecord.PickedQty, qARecord.TaskNo);

                    IList<T_QARecordEntity> notOverList = db.FindList<T_QARecordEntity>(o => o.QAID == qAGet.F_Id && o.BarCode == postQARecord.BarCode && o.IsNeedBack == "true" && o.IsReturnOver != "true");
                    if (notOverList.Count < 1) //容器已还样完毕
                    {
                        T_TaskEntity taskBack = new T_TaskEntity();
                        taskBack.F_Id = Guid.NewGuid().ToString();
                        taskBack.TaskNo = T_CodeGenApp.GenNum("TaskRule");
                        taskBack.TaskInOutType = "InType";
                        taskBack.ContainerID = containerEntity.F_Id;
                        taskBack.BarCode = containerEntity.BarCode;
                        taskBack.ContainerType = containerEntity.ContainerType;
                        taskBack.SrcLocationID = station.StationCode;
                        taskBack.SrcLocationCode = station.LeaveAddress;
                        taskBack.SrcWCSLocCode = station.LeaveAddress;
                        taskBack.TagAreaID = area.F_Id;
                        taskBack.TagLocationID = "";
                        taskBack.TagLocationCode = "";
                        taskBack.ApplyStationID = station.F_Id;
                        taskBack.WaveID = qARecord.WaveID;
                        taskBack.WaveCode = qARecord.WaveCode;
                        taskBack.SEQ = qADetailBack.SEQ;
                        taskBack.Level = 40;
                        taskBack.State = "New";
                        taskBack.IsWcsTask = "true";
                        taskBack.SendWCSTime = null;
                        taskBack.OrderType = "BackSample";
                        taskBack.OrderID = qABack.F_Id;
                        taskBack.OrderDetailID = qADetailBack.F_Id;
                        taskBack.OrderCode = qABack.QACode;
                        taskBack.OverTime = null;
                        taskBack.TaskType = "TaskType_CheckReturnIn";
                        taskBack.IsCanExec = "true";
                        db.Insert<T_TaskEntity>(taskBack);

                        db.SaveChanges();
                    }

                    //判断单据是否完成
                    IList<T_QARecordEntity> noPickRecordList = db.FindList<T_QARecordEntity>(o => o.QADetailID == qARecord.QADetailID && o.IsNeedBack == "true" && o.IsReturnOver != "true").ToList();
                    if (noPickRecordList.Count < 1) //单据明细已拣选完毕
                    {
                        /// 还样单明细状态
                        if (qADetailBack.ResultState == "New") qADetailBack.State = "WaitResult";
                        else qADetailBack.State = "Over";
                        db.Update<T_QADetailEntity>(qADetailBack);
                        db.SaveChanges();

                        string[] qaDetailIDArray = db.FindList<T_WaveDetailEntity>(o => o.WaveID == qARecord.WaveID).Select(o => o.OutBoundDetailID).ToArray();
                        IList<T_QADetailEntity> noOverQADetailList = db.FindList<T_QADetailEntity>(o => qaDetailIDArray.Contains(o.F_Id) && o.State != "Over").ToList();
                        if (noOverQADetailList.Count < 1) //全部波次的单据明细已经完成
                        {
                            T_WaveEntity wave = db.FindEntity<T_WaveEntity>(o => o.F_Id == qARecord.WaveID);
                            wave.State = "Over";
                            db.Update<T_WaveEntity>(wave);
                            db.SaveChanges();
                        }

                        List<T_QADetailEntity> detailList = db.FindList<T_QADetailEntity>(o => o.QAID == qABack.F_Id);
                        if (detailList.All(o => o.State == "Over" || o.State == "WaitResult"))
                        {
                            if (detailList.Any(o => o.State == "WaitResult")) qABack.State = "WaitResult";
                            else qABack.State = "Over";
                            db.Update<T_QAEntity>(qABack);
                            db.SaveChanges();

                            /// 产生过账信息，并发送过账信息
                            if (RuleConfig.OrderTransRule.QATransRule.QABackTrans)
                            {
                                if (qAGet.GenType == "ERP")
                                {
                                    AjaxResult rst = new T_TransRecordApp().GenTransRecord(db, qABack.F_Id, "BackSample");
                                    if ((ResultType)rst.state == ResultType.success)
                                    {
                                        T_TransRecordEntity trans = (T_TransRecordEntity)rst.data;
                                        ERPPost post = new ERPPost();
                                        ERPResult erpRst = post.PostFactInOutQty(db, "BackSample", trans.F_Id);
                                    }
                                    else
                                    {
                                        return Error("过账信息产生失败", "");
                                    }
                                }
                            }
                        }
                    }

                    db.SaveChanges();
                    db.CommitWithOutRollBack();

                    IList<T_QARecordEntity> qaRecAllPickList = db.FindList<T_QARecordEntity>(o => o.BarCode == postQARecord.BarCode && o.WaveID == qARecord.WaveID && o.QAID == qARecord.QAID).ToList();
                    IList<T_QARecordEntity> qaRecNoPickList = qaRecAllPickList.Where(o => o.State == "Returning" || o.State == "WaitReturn").ToList();
                    string NoPickTimes = qaRecNoPickList.Count().ToString();    // 剩余次数
                    string ReadyQutAndOrderNeed = qaRecAllPickList.Sum(o => o.ReturnQty) + "/" + qaRecAllPickList.Sum(o => o.PickedQty);  // 已还 / 应还
                    /**************************************************/

                    logObj.Message = "操作成功";
                    LogFactory.GetLogger().Info(logObj);

                    logEntity.F_Result = true;
                    new LogApp().WriteDbLog(logEntity);

                    return Success("操作成功。", new { NoPickTimes = NoPickTimes, ReadyQutAndOrderNeed = ReadyQutAndOrderNeed });
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

        #region 料架-拣选回库，申请入库 （此处只针对料架，纸箱与料箱在拣选完毕时已产生任务）
        [HttpPost]
        [HandlerAjaxOnly]
        [ValidateAntiForgeryToken]
        public ActionResult RackBackIn(string barCode)
        {
            using (var db = new RepositoryBase().BeginTrans())
            {
                LogObj logObj = new LogObj();
                logObj.Path = "RFQA_ReturnController.RackBackIn";
                logObj.Parms = new { barCode = barCode };

                LogEntity logEntity = new LogEntity();
                logEntity.F_ModuleName = "还样单";
                logEntity.F_Type = DbLogType.Submit.ToString();
                logEntity.F_Account = OperatorProvider.Provider.GetCurrent().UserCode;
                logEntity.F_NickName = OperatorProvider.Provider.GetCurrent().UserName;
                logEntity.F_Description = "还样单申请回库";
                logEntity.F_Path = logObj.Path;
                logEntity.F_Param = logObj.Parms.ToJson();

                try
                {
                    /*************************************************/
                    /// 检测是否已申请入库
                    T_StationEntity station = db.FindEntity<T_StationEntity>(o => o.StationCode == FixType.Station.StationOut_BigItem.ToString());
                    T_TaskEntity taskEntity = db.FindEntity<T_TaskEntity>(o => o.BarCode == barCode && o.ApplyStationID == station.F_Id);
                    if (taskEntity == null) return Error("容器编码不正确。", "");
                    if (!string.IsNullOrEmpty(taskEntity.TagLocationID)) return Error("已申请过。", "");

                    /// 还样出库需要找到：质检取样单对应的质检记录
                    T_QAEntity qABack = db.FindEntity<T_QAEntity>(o => o.F_Id == station.CurOrderID);
                    if (qABack == null) return Error("单据已解绑站台", "");
                    T_QAEntity qAGet = db.FindEntity<T_QAEntity>(o => o.QACode == qABack.RefOrderCode && o.QAOrderType == "GetSample");
                    if (qAGet == null) return Error("未找到对应质检单", "");

                    string errMsg = "";
                    T_LocationApp locationApp = new T_LocationApp();
                    T_ContainerTypeEntity containerType = db.FindEntity<T_ContainerTypeEntity>(o => o.ContainerTypeCode == taskEntity.ContainerType);
                    //T_LocationEntity loc = locationApp.GetLocIn(ref errMsg, db, containerType, taskEntity.TagAreaID, true);


                    T_LocationEntity loc = null;
                    LogObj log = null;
                    if (taskEntity.TaskType == "TaskType_EmptyIn")
                    {
                        T_ItemEntity item = db.FindEntity<T_ItemEntity>(o => o.ItemCode == FixType.Item.EmptyRack.ToString());
                        loc = locationApp.GetLocIn(ref errMsg, ref log, db, containerType, taskEntity.TagAreaID, true, null, null, true, null, item);
                    }
                    else
                    {
                        T_ContainerDetailEntity oneConDetail = db.FindEntity<T_ContainerDetailEntity>(o => o.BarCode == barCode);
                        T_ItemEntity item = db.FindEntity<T_ItemEntity>(o => o.F_Id == oneConDetail.ItemID);
                        loc = locationApp.GetLocIn(ref errMsg, ref log, db, containerType, taskEntity.TagAreaID, false, oneConDetail.BarCode, oneConDetail.CheckState, true, null, item);
                    }

                    if (loc == null)
                    {
                        return Error("货位分配失败：" + errMsg, "");
                    }

                    taskEntity.TagLocationCode = loc.LocationCode;
                    taskEntity.TagLocationID = loc.F_Id;
                    taskEntity.TagWCSLocCode = loc.WCSLocCode;
                    db.SaveChanges();

                    taskEntity.SendWCSTime = DateTime.Now;

                    bool isOrderOver = false;
                    string orderType = station.OrderType;
                    IList<T_TaskEntity> taskList = db.FindList<T_TaskEntity>(o => o.OrderID == station.CurOrderID && o.TaskType == "TaskType_CheckReturnOut" && o.TagLocationID == station.F_Id); ;
                    IList<T_QARecordEntity> list = db.FindList<T_QARecordEntity>(o => o.QAID == qAGet.F_Id && o.State != "Over" && o.TagLocationID == station.F_Id && o.IsNeedBack == "true");

                    if (taskList.Count < 1 && list.Count < 1) isOrderOver = true;

                    station.BarCode = "";

                    if (isOrderOver) //单据已完成
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
