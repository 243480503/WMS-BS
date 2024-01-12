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
    public class RFQA_PickController : ControllerBase
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

        #region 扫码 后获取容器的出库信息
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

            IList<T_QARecordEntity> qaRecordList = qaRecordApp.FindList(o => o.BarCode == barCode && o.QAID == station.CurOrderID).ToList();
            IList<T_QARecordEntity> PickingList = qaRecordList.Where(o => o.State == "Picking").ToList();
            if (PickingList.Count() < 1)
            {
                IList<T_QARecordEntity> PickedList = qaRecordList.Where(o => o.State == "Picked").ToList();
                if (PickedList.Count() < 1) return Error("该容器无需抽样", "");
                else return Error("该容器已抽样", "");
            }

            T_QARecordEntity qaRecord = new T_QARecordEntity();
            if (string.IsNullOrEmpty(itemBarcode)) qaRecord = PickingList.FirstOrDefault();
            else
            {
                qaRecord = qaRecordList.FirstOrDefault(o => o.ItemBarCode == itemBarcode); /// 在所有拣选记录中找
                if (qaRecord == null)
                {
                    T_ContainerDetailEntity cdNew = containerDetailApp.FindEntity(o => o.BarCode == barCode && o.ItemBarCode == itemBarcode);
                    if (cdNew == null) return Error("当前容器不存在该子码", "");
                    if (cdNew.IsCountFreeze == "true" || cdNew.State == "Freeze") return Error("子码状态冻结", "");

                    T_QARecordEntity canRec = new T_QARecordEntity();
                    List<T_QARecordEntity> canPickList = PickingList.Where(o => o.ItemID == cdNew.ItemID && (o.Lot == cdNew.Lot || (string.IsNullOrEmpty(o.Lot) && string.IsNullOrEmpty(cdNew.Lot))) && o.IsAuto == "true").ToList();
                    if (canPickList.Count == 0) return Error("子码不可替代拣选", "");
                    else
                    {
                        canRec = canPickList.FirstOrDefault();  /// 只要在 canPickList 中找到一个就可以替换
                        if (canRec.IsAppearQA == "true")
                        {
                            canRec.Qty = cdNew.OutQty;   /// 外观质检，数量全取
                        }
                        else
                        {
                            /// 判断数量，最优拣选
                            decimal? minQty = decimal.MaxValue;
                            decimal? diffQty = 0;
                            foreach (T_QARecordEntity rec in canPickList)
                            {
                                if (cdNew.Qty < rec.Qty) continue;
                                diffQty = cdNew.Qty - rec.Qty;
                                if (minQty > diffQty)
                                {
                                    minQty = diffQty;
                                    canRec = rec;
                                }
                            }
                        }
                    }
                    if (canRec == null) return Error("子码标签数量不足", "");

                    qaRecord = canRec;
                    qaRecord.ItemBarCode = cdNew.ItemBarCode;
                    qaRecord.OldQty = cdNew.Qty;
                }
            }

            if (qaRecord.State == "Picked") return Error("子码已取样", "");

            T_ContainerDetailEntity containerDetailEntity = containerDetailApp.FindEntity(o => o.F_Id == qaRecord.ContainerDetailID);
            T_QAEntity qaEntity = qaApp.FindEntity(o => o.F_Id == qaRecord.QAID);
            T_QADetailEntity outDetailEntity = qaDetailApp.FindEntity(o => o.QAID == station.CurOrderID);
            T_ItemEntity item = itemApp.FindEntity(o => o.F_Id == qaRecord.ItemID);

            QARecordModel qaRecordModel = qaRecord.ToObject<QARecordModel>();
            qaRecordModel.RefOrderCode = qaEntity.RefOrderCode;
            qaRecordModel.ItemCode = qaRecord.ItemCode;
            qaRecordModel.ItemName = qaRecord.ItemName;
            qaRecordModel.CreateCompany = item.Factory;
            qaRecordModel.ProductDate = containerDetailEntity.ProductDate;
            qaRecordModel.Lot = qaRecord.Lot;
            qaRecordModel.OrderQty = outDetailEntity.SampleSumNum;
            qaRecordModel.Qty = qaRecord.Qty;
            qaRecordModel.ReadyQutAndOrderNeed = (qaRecordList.Sum(o => o.PickedQty ?? 0)).ToString("0.##") + "/" + (qaRecordList.Sum(o => o.Qty ?? 0)).ToString("0.##");
            qaRecordModel.ContainerKind = container.ContainerKind;
            qaRecordModel.ContainerKindName = itemsDetailApp.FindEnum<T_ContainerTypeEntity>(o => o.ContainerKind).FirstOrDefault(o => o.F_ItemCode == container.ContainerKind).F_ItemName;
            qaRecordModel.QADetailID = qaRecord.QADetailID;
            qaRecordModel.QAID = station.CurOrderID;
            qaRecordModel.WaveID = station.WaveID;
            qaRecordModel.F_Id = qaRecord.F_Id;
            qaRecordModel.Spec = qaRecord.Spec;
            qaRecordModel.StationID = station.F_Id;
            qaRecordModel.ItemBarCode = qaRecord.ItemBarCode;
            qaRecordModel.IsItemMark = qaRecord.IsItemMark;
            qaRecordModel.MustTimes = qaRecordList.Count;
            qaRecordModel.NoPickTimes = PickingList.Count;
            if (qaRecord.IsAppearQA == "true") qaRecordModel.IsAppearQAStr = "外观质检";
            else qaRecordModel.IsAppearQAStr = "取样质检";
            return Content(qaRecordModel.ToJson());
        }
        #endregion

        #region 切换选项卡后，根据料箱编码获取质检列表
        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetQARecordList(string QAID, string barCode)
        {
            IList<T_QARecordEntity> dataList = new List<T_QARecordEntity>();
            if (!string.IsNullOrEmpty(QAID))
            {
                dataList = qaRecordApp.FindList(o => o.BarCode == barCode && o.QAID == QAID && (o.State == "Picked" || o.State == "Over")).OrderBy(o => o.PickDate).ToList();
            }
            else
            {
                T_QARecordEntity lastEntity = qaRecordApp.FindList(o => o.BarCode == barCode).OrderByDescending(o => o.PickDate).FirstOrDefault();
                if (lastEntity != null)
                {
                    dataList = qaRecordApp.FindList(o => o.BarCode == barCode && o.QAID == lastEntity.QAID).OrderBy(o => o.PickDate).ToList();
                }
            }
            return Content(dataList.ToJson());
        }
        #endregion

        #region 切换选项卡后，根据料箱编码获取容器大类
        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetContainerKind(string barCode)
        {
            T_ContainerEntity containerEntity = containerApp.FindEntity(o => o.BarCode == barCode);
            return Content(containerEntity.ToJson());
        }
        #endregion

        #region RF提交质检取样拣选信息
        [HttpPost]
        [HandlerAjaxOnly]
        [ValidateAntiForgeryToken]
        public ActionResult SubmitQARecordForm(QARecordModel postQARecord)
        {
            using (var db = new RepositoryBase().BeginTrans())
            {
                LogObj logObj = new LogObj();
                logObj.Path = "RFQA_PickController.SubmitQARecordForm";
                logObj.Parms = new { postQARecord = postQARecord };

                LogEntity logEntity = new LogEntity();
                logEntity.F_ModuleName = "质检取样单";
                logEntity.F_Type = DbLogType.Submit.ToString();
                logEntity.F_Account = OperatorProvider.Provider.GetCurrent().UserCode;
                logEntity.F_NickName = OperatorProvider.Provider.GetCurrent().UserName;
                logEntity.F_Description = "RF质检取样拣选";

                logEntity.F_Path = logObj.Path;
                logEntity.F_Param = logObj.Parms.ToJson();

                try
                {
                    /*************************************************/
                    if (string.IsNullOrEmpty(postQARecord.BarCode)) return Error("箱码不能为空", "");

                    T_StationEntity station = db.FindEntity<T_StationEntity>(o => o.F_Id == postQARecord.StationID);
                    //string waveID = station.WaveID;
                    string CurOrderID = station.CurOrderID;

                    T_QARecordEntity qaRec = db.FindEntity<T_QARecordEntity>(o => o.F_Id == postQARecord.F_Id);
                    T_QADetailEntity qaDetailEntity = db.FindEntity<T_QADetailEntity>(o => o.F_Id == qaRec.QADetailID);
                    T_QAEntity qaEntity = db.FindEntity<T_QAEntity>(o => o.F_Id == station.CurOrderID);

                    /// 根据容器判断回库区域
                    FixType.Area areaEnum;
                    T_ContainerEntity containerEntity = db.FindEntity<T_ContainerEntity>(o => o.F_Id == qaRec.ContainerID);
                    if (containerEntity.ContainerKind == "Rack")
                    {
                        areaEnum = FixType.Area.BigItemArea;
                    }
                    else if (containerEntity.ContainerKind == "Box" || containerEntity.ContainerKind == "Plastic")
                    {
                        areaEnum = FixType.Area.NormalArea;
                    }
                    else return Error("未知的容器大类", "");

                    /// 非纸箱，更换标签条码
                    if (containerEntity.ContainerKind != "Box")
                    {
                        List<T_QARecordEntity> qaRecordList = db.FindList<T_QARecordEntity>(o => o.BarCode == postQARecord.BarCode && o.QAID == CurOrderID);
                        List<T_QARecordEntity> PickingList = qaRecordList.Where(o => o.State == "Picking").ToList();

                        if (qaRec.IsItemMark == "true") /// 必须扫码
                        {
                            if (string.IsNullOrEmpty(postQARecord.ItemBarCode)) return Error("子码不能为空", "");
                            if (RuleConfig.OutConfig.RFScanCode.IsItemBarCodeSame) /// 扫码必须一致
                            {
                                /// 是否在质检记录内
                                qaRec = qaRecordList.FirstOrDefault(o => o.ItemBarCode == postQARecord.ItemBarCode); /// 存在拣选记录-----用当前子码替换拣选记录
                                if (qaRec == null) return Error("子码不正确", "");
                            }
                            else /// 允许不一致
                            {
                                T_ContainerDetailEntity cdNew = db.FindEntity<T_ContainerDetailEntity>(o => o.BarCode == postQARecord.BarCode && o.ItemBarCode == postQARecord.ItemBarCode);
                                if (cdNew == null) return Error("当前容器不存在该子码", "");

                                T_ContainerDetailEntity cdOld = db.FindEntity<T_ContainerDetailEntity>(o => o.F_Id == qaRec.ContainerDetailID);
                                /// 库存明细不一致，更换标签条码
                                if (cdOld.F_Id != cdNew.F_Id)
                                {
                                    /// 是否在质检记录内
                                    qaRec = qaRecordList.FirstOrDefault(o => o.ItemBarCode == cdNew.ItemBarCode); /// 存在拣选记录-----用当前子码替换拣选记录
                                    if (qaRec == null)  /// 子码不在Picking记录中，用库存替代
                                    {
                                        /// 物料+批号 不一致
                                        if (!(cdNew.ItemID == cdOld.ItemID && (cdNew.Lot == cdOld.Lot || (string.IsNullOrEmpty(cdNew.Lot) && string.IsNullOrEmpty(cdOld.Lot)))))
                                        {
                                            return Error("子码物料不是拣选物料", "");
                                        }
                                        if (cdNew.IsCountFreeze == "true" || cdNew.State == "Freeze") return Error("子码状态冻结", "");

                                        T_QARecordEntity canRec = new T_QARecordEntity();
                                        List<T_QARecordEntity> canPickList = PickingList.Where(o => o.ItemID == cdNew.ItemID && (o.Lot == cdNew.Lot || (string.IsNullOrEmpty(o.Lot) && string.IsNullOrEmpty(cdNew.Lot))) && o.IsAuto == "true").ToList();
                                        if (canPickList.Count == 0) return Error("子码不可替代拣选", "");
                                        else
                                        {
                                            canRec = canPickList.FirstOrDefault();  /// 只要在 canPickList 中找到一个就可以替换
                                            if (canRec.IsAppearQA == "true")
                                            {
                                                canRec.Qty = cdNew.OutQty;  /// 外观质检，数量全取
                                            }
                                            else
                                            {
                                                /// 判断数量，最优拣选
                                                decimal? minQty = decimal.MaxValue;
                                                decimal? diffQty = 0;
                                                foreach (T_QARecordEntity rec in canPickList)
                                                {
                                                    if (cdNew.Qty < rec.Qty) continue;
                                                    diffQty = cdNew.Qty - rec.Qty;
                                                    if (minQty > diffQty)
                                                    {
                                                        minQty = diffQty;
                                                        canRec = rec;
                                                    }
                                                }
                                            }
                                        }

                                        if (canRec == null) return Error("子码标签数量不足", "");
                                        qaRec = canRec;

                                        /// 用新库存替换原拣选信息
                                        cdNew.OutQty = (cdNew.OutQty ?? 0) + (qaRec.Qty ?? 0);
                                        cdOld.OutQty = (cdOld.OutQty ?? 0) - (qaRec.Qty ?? 0);

                                        qaRec.ItemBarCode = cdNew.ItemBarCode;
                                        qaRec.ReceiveRecordID = cdNew.ReceiveRecordID;
                                        qaRec.ContainerDetailID = cdNew.F_Id;
                                        qaRec.OldQty = cdNew.Qty;

                                        db.Update<T_ContainerDetailEntity>(cdNew);
                                        db.Update<T_ContainerDetailEntity>(cdOld);
                                        db.Update<T_QARecordEntity>(qaRec);
                                        db.SaveChanges();
                                    }
                                }
                            }
                        }
                        else /// 不扫码，不处理
                        {

                        }
                    }

                    if (qaRec.State == "Picked") return Error("当前子码已取样", "");

                    if (postQARecord.Qty == null || postQARecord.Qty == 0) postQARecord.Qty = qaRec.Qty;    /// 未返回前端本次数量时
                    qaRec.State = "Picked";
                    qaRec.PickedQty = (qaRec.PickedQty ?? 0) + postQARecord.Qty;
                    qaRec.AfterQty = qaRec.OldQty - qaRec.PickedQty;
                    qaRec.PickDate = DateTime.Now;
                    qaRec.PickUserID = OperatorProvider.Provider.GetCurrent().UserId;
                    qaRec.PickUserName = OperatorProvider.Provider.GetCurrent().UserName;

                    /// 外观质检直接归还回库
                    if (qaRec.IsAppearQA == "true")
                    {
                        qaRec.ReturnQty = qaRec.PickedQty;
                        qaRec.AfterQty = (qaRec.AfterQty ?? 0) + (qaRec.PickedQty ?? 0);
                    }
                    db.Update<T_QARecordEntity>(qaRec);

                    if (qaRec.IsAppearQA == "false") /// 只统计取样质检的出库数量
                    {
                        qaDetailEntity.OutQty = (qaDetailEntity.OutQty ?? 0) + (postQARecord.Qty ?? 0);
                        db.Update<T_QADetailEntity>(qaDetailEntity);
                    }

                    T_ContainerDetailEntity containerDetail = db.FindEntity<T_ContainerDetailEntity>(o => o.F_Id == qaRec.ContainerDetailID);
                    if (qaRec.IsAppearQA == "true") /// 外观质检
                    {
                        containerDetail.OutQty = 0;
                    }
                    else /// 取样质检
                    {
                        containerDetail.Qty = (containerDetail.Qty ?? 0) - (postQARecord.Qty ?? 0);
                        containerDetail.OutQty = (containerDetail.OutQty ?? 0) - (postQARecord.Qty ?? 0);

                        T_ItemEntity item = db.FindEntity<T_ItemEntity>(o => o.F_Id == containerDetail.ItemID);
                        if (qaDetailEntity.IsBroken == "false")
                        {
                            containerDetail.CheckQty = (containerDetail.CheckQty ?? 0) + postQARecord.Qty;
                        }

                        /// 产生库存流水
                        string orderType = "GetSample";
                        inOutDetailApp.SyncInOutDetail(db, containerDetail, "OutType", orderType, qaRec.OldQty, postQARecord.Qty, qaRec.TaskNo);
                    }

                    if (containerDetail.Qty == 0 && containerDetail.CheckQty == 0) db.Delete<T_ContainerDetailEntity>(containerDetail); /// 库存数量清空，删除库存
                    else db.Update<T_ContainerDetailEntity>(containerDetail);
                    db.SaveChanges();


                    /// 根据站台单据类型，推断回库时的任务类型
                    string stationOrderType = station.OrderType;
                    T_AreaEntity area = db.FindEntity<T_AreaEntity>(o => o.AreaCode == areaEnum.ToString());

                    IList<T_ContainerDetailEntity> containerDetailList = db.FindList<T_ContainerDetailEntity>(o => o.BarCode == containerDetail.BarCode).ToList();
                    if (containerDetailList.Count() < 1) /// 已没有库存，空容器回库
                    {
                        qaDetailEntity.State = "WaitResult";
                        db.Update<T_QADetailEntity>(qaDetailEntity);

                        if (containerEntity.ContainerKind == "Box") /// 纸箱不用回库，但需关闭单据和清空站台
                        {
                            List<T_QARecordEntity> noNeedBcakRecList = db.FindList<T_QARecordEntity>(o => o.QAID == qaRec.QAID && o.BarCode == postQARecord.BarCode);
                            foreach (T_QARecordEntity rec in noNeedBcakRecList)
                            {
                                rec.IsNeedBackWare_Get = "false";
                                db.Update<T_QARecordEntity>(rec);
                            }

                            if (station.BarCode == postQARecord.BarCode) station.BarCode = "";

                            IList<T_QARecordEntity> list = db.FindList<T_QARecordEntity>(o => o.QADetailID == qaRec.QADetailID && o.QAID == qaRec.QAID && o.State != "Picked");
                            if (list.Count < 1) /// 单据明细已拣完
                            {
                                IList<T_QARecordEntity> recList = db.FindList<T_QARecordEntity>(o => o.TagLocationID == station.F_Id && o.QAID == station.CurOrderID && o.State != "Picked");/// 该站台该单据已出完

                                if (recList.Count < 1) /// 该站台已没有当前单据任务和拣选任务
                                {
                                    /// 清空站台
                                    station.CurOrderDetailID = "";
                                    station.CurOrderID = "";
                                    station.WaveID = "";
                                    station.OrderType = "";
                                }
                            }
                            db.Update<T_StationEntity>(station);
                            db.SaveChanges();
                        }
                        else if (containerEntity.ContainerKind == "Plastic"
                                 || containerEntity.ContainerKind == "Rack") /// 空容器入库（料箱 或 料架），产生任务，关闭单据，但不清空站台
                        {

                            List<T_QARecordEntity> noNeedBcakRecList = db.FindList<T_QARecordEntity>(o => o.QAID == qaRec.QAID && o.BarCode == postQARecord.BarCode);
                            foreach (T_QARecordEntity rec in noNeedBcakRecList)
                            {
                                rec.IsNeedBackWare_Get = "false";
                                db.Update<T_QARecordEntity>(rec);
                            }

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
                            taskBack.WaveID = qaRec.WaveID;
                            taskBack.WaveCode = qaRec.WaveCode;
                            taskBack.SEQ = qaDetailEntity.SEQ;
                            taskBack.Level = 40;
                            taskBack.State = "New";
                            taskBack.IsWcsTask = "true";
                            taskBack.SendWCSTime = null;
                            taskBack.OrderType = "EmptyIn";
                            taskBack.OrderID = qaDetailEntity.QAID;
                            taskBack.OrderDetailID = qaDetailEntity.F_Id;
                            taskBack.OrderCode = qaEntity.QACode;
                            taskBack.OverTime = null;
                            taskBack.TaskType = "TaskType_EmptyIn";
                            taskBack.IsCanExec = "true";

                            db.Insert<T_TaskEntity>(taskBack);
                        }
                        else return Error("容器类型未知", "");
                    }
                    else
                    {
                        IList<T_QARecordEntity> list = db.FindList<T_QARecordEntity>(o => o.BarCode == postQARecord.BarCode && o.QADetailID == qaRec.QADetailID && o.QAID == qaRec.QAID && o.State != "Picked");
                        if (list.Count > 0) /// 当前容器还有抽样需求
                        {
                            /// 更新站台当前单据明细信息
                            station.CurOrderDetailID = list.FirstOrDefault().QADetailID;
                            db.Update<T_StationEntity>(station);
                        }
                        else
                        {
                            List<T_QARecordEntity> noNeedBcakRecList = db.FindList<T_QARecordEntity>(o => o.QAID == qaRec.QAID && o.BarCode == postQARecord.BarCode);
                            foreach (T_QARecordEntity rec in noNeedBcakRecList)
                            {
                                rec.IsNeedBackWare_Get = "false";
                                db.Update<T_QARecordEntity>(rec);
                            }

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
                            taskBack.WaveID = qaRec.WaveID;
                            taskBack.WaveCode = qaRec.WaveCode;
                            taskBack.SEQ = qaDetailEntity.SEQ;
                            taskBack.Level = 40;
                            taskBack.State = "New";
                            taskBack.IsWcsTask = "true";
                            taskBack.SendWCSTime = null;
                            taskBack.OrderType = "GetSample";
                            taskBack.OrderID = qaDetailEntity.QAID;
                            taskBack.OrderDetailID = qaDetailEntity.F_Id;
                            taskBack.OrderCode = qaEntity.QACode;
                            taskBack.OverTime = null;
                            taskBack.TaskType = "TaskType_CheckPickIn";
                            taskBack.IsCanExec = "true";
                            db.Insert<T_TaskEntity>(taskBack);
                        }
                    }
                    db.SaveChanges();

                    /// 判断单据是否完成
                    IList<T_QARecordEntity> noPickQARecordList = db.FindList<T_QARecordEntity>(o => o.QADetailID == qaRec.QADetailID && o.State != "Picked").ToList();
                    if (noPickQARecordList.Count < 1) /// 单据明细已拣选完毕
                    {
                        if (qaDetailEntity.IsBroken == "true" || qaDetailEntity.IsAppearQA == "true") qaDetailEntity.State = "WaitResult";
                        if (qaDetailEntity.State != "WaitResult") qaDetailEntity.State = "Picked";
                        db.Update<T_QADetailEntity>(qaDetailEntity);
                        db.SaveChanges();

                        /// 关闭波次
                        T_WaveEntity wave = db.IQueryable<T_WaveEntity>().Join(db.IQueryable<T_WaveDetailEntity>(), m => m.F_Id, n => n.WaveID, (m, n) => new { wave = m, waveDetail = n }).Where(o => o.wave.State == "Execing" && o.waveDetail.OutBoundDetailID == qaRec.QADetailID).Select(o => o.wave).FirstOrDefault();
                        if (wave == null) return Error("波次未找到", "");

                        IList<T_WaveDetailEntity> waveDetail = db.FindList<T_WaveDetailEntity>(o => o.WaveID == wave.F_Id);
                        if (waveDetail.Count < 1) return Error("波次明细未找到", "");

                        string[] allOrderDetailID = waveDetail.Select(o => o.OutBoundDetailID).ToArray();
                        IList<T_QADetailEntity> noOverQADetailList = db.FindList<T_QADetailEntity>(o => allOrderDetailID.Contains(o.F_Id) && o.State != "Picked").ToList();
                        if (noOverQADetailList.Count < 1) /// 当前波次所有单据明细已经完成
                        {
                            T_WaveEntity wavedb = db.FindEntity<T_WaveEntity>(o => o.F_Id == wave.F_Id);
                            wavedb.State = "Over";
                            db.Update<T_WaveEntity>(wavedb);
                            db.SaveChanges();
                        }

                        IList<T_QADetailEntity> detailList = db.FindList<T_QADetailEntity>(o => o.QAID == CurOrderID);
                        if (detailList.All(o => o.State == "Picked" || o.State == "WaitResult"))
                        {
                            if (detailList.All(o => o.State == "WaitResult")) qaEntity.State = "WaitResult";
                            else qaEntity.State = "Picked";
                            db.Update<T_QAEntity>(qaEntity);
                            db.SaveChanges();

                            /// 产生过账信息，并发送过账信息
                            if (RuleConfig.OrderTransRule.QATransRule.QAGetTrans)
                            {
                                if (qaEntity.GenType == "ERP")
                                {
                                    AjaxResult rst = new T_TransRecordApp().GenTransRecord(db, qaEntity.F_Id, "GetSample");
                                    if ((ResultType)rst.state == ResultType.success)
                                    {
                                        T_TransRecordEntity trans = (T_TransRecordEntity)rst.data;
                                        ERPPost post = new ERPPost();
                                        ERPResult erpRst = post.PostFactInOutQty(db, "GetSample", trans.F_Id);
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

                    IList<T_QARecordEntity> outRecAllPickList = db.FindList<T_QARecordEntity>(o => o.BarCode == postQARecord.BarCode && o.WaveID == qaRec.WaveID && o.QAID == qaRec.QAID).ToList();
                    IList<T_QARecordEntity> outRecNoPickList = outRecAllPickList.Where(o => o.State == "Picking" || o.State == "WaitPick").ToList();
                    string NoPickTimes = outRecNoPickList.Count().ToString();    // 剩余次数
                    string ReadyQutAndOrderNeed = outRecAllPickList.Sum(o => o.PickedQty) + "/" + outRecAllPickList.Sum(o => o.Qty);  // 已出 / 总出
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

        #region 料架-抽样回库，申请入库 （此处只针对料架，纸箱与料箱在抽样完毕时已产生任务）
        [HttpPost]
        [HandlerAjaxOnly]
        [ValidateAntiForgeryToken]
        public ActionResult RackBackIn(string barCode)
        {
            using (var db = new RepositoryBase().BeginTrans())
            {
                LogObj logObj = new LogObj();
                logObj.Path = "RFQA_PickController.RackBackIn";
                logObj.Parms = new { barCode = barCode };

                LogEntity logEntity = new LogEntity();
                logEntity.F_ModuleName = "质检单";
                logEntity.F_Type = DbLogType.Submit.ToString();
                logEntity.F_Account = OperatorProvider.Provider.GetCurrent().UserCode;
                logEntity.F_NickName = OperatorProvider.Provider.GetCurrent().UserName;
                logEntity.F_Description = "RF取样申请回库";
                logEntity.F_Path = logObj.Path;
                logEntity.F_Param = logObj.Parms.ToJson();

                try
                {
                    /*************************************************/
                    T_StationEntity station = db.FindEntity<T_StationEntity>(o => o.StationCode == FixType.Station.StationOut_BigItem.ToString());
                    T_TaskEntity taskEntity = db.FindEntity<T_TaskEntity>(o => o.BarCode == barCode && o.ApplyStationID == station.F_Id);
                    if (taskEntity == null) return Error("容器编码不正确。", "");

                    if (!string.IsNullOrEmpty(taskEntity.TagLocationID)) return Error("已申请过。", "");
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
                            loc = locationApp.GetLocIn(ref errMsg, ref log, db, containerType, taskEntity.TagAreaID, true, null, null, true, null, item);
                        }
                        else
                        {
                            T_ContainerDetailEntity oneConDetail = db.FindEntity<T_ContainerDetailEntity>(o => o.BarCode == barCode);
                            T_ItemEntity item = db.FindEntity<T_ItemEntity>(o => o.F_Id == oneConDetail.ItemID);
                            loc = locationApp.GetLocIn(ref errMsg, ref log, db, containerType, taskEntity.TagAreaID, false, oneConDetail.ERPWarehouseCode, oneConDetail.CheckState, true, null, item);
                        }

                        if (loc == null) return Error("货位分配失败：" + errMsg, "");

                        taskEntity.TagLocationCode = loc.LocationCode;
                        taskEntity.TagLocationID = loc.F_Id;
                        taskEntity.TagWCSLocCode = loc.WCSLocCode;
                        db.SaveChanges();

                        taskEntity.SendWCSTime = DateTime.Now;

                        bool isOrderOver = false;
                        string orderType = station.OrderType;
                        string taskType = "TaskType_CheckPickOut";
                        IList<T_TaskEntity> taskList = db.FindList<T_TaskEntity>(o => o.OrderID == station.CurOrderID && o.TagLocationID == station.F_Id && o.TaskType == taskType.ToString());
                        IList<T_QARecordEntity> list = db.FindList<T_QARecordEntity>(o => o.QAID == station.CurOrderID && o.TagLocationID == station.F_Id && o.State != "Picked");

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
                            List<T_QARecordEntity> noNeedBcakRecList = db.FindList<T_QARecordEntity>(o => o.QAID == station.CurOrderID && o.BarCode == taskEntity.BarCode);
                            foreach (T_QARecordEntity rec in noNeedBcakRecList)
                            {
                                rec.IsNeedBackWare_Get = "false";
                                db.Update<T_QARecordEntity>(rec);
                            }

                            db.Update<T_TaskEntity>(taskEntity);

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
