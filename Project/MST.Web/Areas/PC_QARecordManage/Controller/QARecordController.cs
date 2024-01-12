/*******************************************************************************
 * 软件名称: 崇令智能仓储管理系统
 * 技术讨论: 18718690940 (微信同号)
 * QQ交流群:  795702839
 * 最新源码：https://github.com/243480503/WMS-BS
*********************************************************************************/
using MST.Application;
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
/**********精确到主容器中子容器的波次运算***************/
namespace MST.Web.Areas.PC_QARecordManage.Controllers
{
    public class QARecordController : ControllerBase
    {
        private T_QARecordApp qaRecordApp = new T_QARecordApp();
        private ItemsDetailApp itemsDetailApp = new ItemsDetailApp();
        private T_SupplierApp supplierApp = new T_SupplierApp();
        private T_ContainerTypeApp containerTypeApp = new T_ContainerTypeApp();
        private T_ContainerDetailApp containerDetailApp = new T_ContainerDetailApp();
        private T_ContainerApp containerApp = new T_ContainerApp();
        private T_QADetailApp qaDetailApp = new T_QADetailApp();
        private T_InBoundApp inboundApp = new T_InBoundApp();
        private T_InBoundDetailApp inboundDetailApp = new T_InBoundDetailApp();
        private T_QAApp qaApp = new T_QAApp();
        private T_WaveDetailApp waveDetailApp = new T_WaveDetailApp();
        private T_WaveApp waveApp = new T_WaveApp();
        private T_StationApp stationApp = new T_StationApp();
        private static object lockObj = new object();

        #region 打开质检记录窗口
        [HttpGet]
        [HandlerAuthorize]
        public virtual ActionResult QADetail()
        {
            return View();
        }
        #endregion

        #region 打开指定库存窗口（外观质检，标签个数）
        [HttpGet]
        [HandlerAuthorize]
        public virtual ActionResult IndexByCnt()
        {
            return View();
        }
        #endregion

        #region 打开质检记录窗口（外观质检）
        [HttpGet]
        [HandlerAuthorize]
        public virtual ActionResult QADetailByCnt()
        {
            return View();
        }
        #endregion

        #region 质检指定库存窗口头部信息
        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetQADetail(string qaDetailID)
        {
            T_QADetailEntity data = qaDetailApp.FindEntity(o => o.F_Id == qaDetailID);
            QARecordModel recordModel = data.ToObject<QARecordModel>();
            recordModel.Qty = qaRecordApp.FindList(o => o.QADetailID == qaDetailID).ToList().Sum(o => o.Qty ?? 0);
            recordModel.SampleSumCnt = data.SampleSumCnt;
            recordModel.ChooseSumCnt = qaRecordApp.FindList(o => o.QADetailID == qaDetailID).Count();
            return Content(recordModel.ToJson());
        }
        #endregion

        #region 获取选取列表
        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetGridJson(Pagination pagination, string qaDetailID, string keyword)
        {
            T_QADetailEntity detail = qaDetailApp.FindEntity(o => o.F_Id == qaDetailID);
            T_QAEntity qa = qaApp.FindEntity(o => o.F_Id == detail.QAID);
            T_InBoundEntity inbound = inboundApp.FindEntity(o=>o.RefOrderCode == qa.RefInBoundCode);
            if(inbound == null) return Error("质检单的来源入库单不存在", "");

            T_InBoundDetailEntity inboundDetail = new T_InBoundDetailEntity();
            if (string.IsNullOrEmpty(detail.Lot)) inboundDetail = inboundDetailApp.FindEntity(o => o.ItemID == detail.ItemID && string.IsNullOrEmpty(o.Lot) && o.InBoundID == inbound.F_Id);
            else inboundDetail = inboundDetailApp.FindEntity(o => o.ItemID == detail.ItemID && o.Lot == detail.Lot && o.InBoundID == inbound.F_Id);
            if (inboundDetail == null) return Error("质检单的来源入库单明细不存在", "");

            //所有库存信息
            IList<T_ContainerDetailEntity> containerDetailEntityList = new List<T_ContainerDetailEntity>();
            containerDetailEntityList = containerDetailApp.GetQAItemList(pagination, detail.ItemID, detail.Lot, inboundDetail.F_Id, keyword);

            IList<ContainerDetailModel> ContainerDetailModelList = containerDetailEntityList.Where(o => o.Qty > 0).Select(o => new ContainerDetailModel
            {
                F_Id = o.F_Id,
                ItemID = o.ItemID,
                ItemName = o.ItemName,
                ItemCode = o.ItemCode,
                ItemBarCode = o.ItemBarCode,
                ItemUnitText = o.ItemUnitText,
                Lot = o.Lot,
                Spec = o.Spec,
                OverdueDate = o.OverdueDate,
                SupplierCode = o.SupplierCode,
                SupplierID = o.SupplierID,
                SupplierName = o.SupplierName,
                Qty = o.Qty,
                OutQty = o.OutQty,
                ReceiveRecordID = o.ReceiveRecordID,
                //IsSpecial = o.IsSpecial,
                ContainerID = o.ContainerID,
                LocationNo = o.LocationNo,
                BarCode = o.BarCode,
                ContainerKind = o.ContainerKind
            }).ToList();

            /// 已添加的出库记录信息
            IList<T_QARecordEntity> qaRecordEntityList = qaRecordApp.FindList(o => o.QADetailID == qaDetailID).ToList();
            IList<ItemsDetailEntity> itemDetailList = itemsDetailApp.FindEnum<T_ContainerTypeEntity>(o => o.ContainerKind).ToList();
            foreach (ContainerDetailModel entity in ContainerDetailModelList)
            {
                T_QARecordEntity qaRecordEntity = qaRecordEntityList.FirstOrDefault(o => o.QADetailID == qaDetailID && o.ContainerDetailID == entity.F_Id);
                if (qaRecordEntity != null)
                {
                    entity.CanUseQty = entity.Qty - entity.OutQty + qaRecordEntity.Qty;
                    entity.OutQty = qaRecordEntity.Qty ?? 0;
                    entity.SampleSumNum = qaRecordEntity.Qty ?? 0;
                }
                else
                {
                    entity.CanUseQty = entity.Qty - entity.OutQty;
                    entity.OutQty = 0;
                    entity.SampleSumNum = 0;
                }
                entity.ContainerKindName = itemDetailList.FirstOrDefault(o => o.F_ItemCode == entity.ContainerKind).F_ItemName;
            }

            ContainerDetailModelList = ContainerDetailModelList.Where(o => o.CanUseQty > 0 && o.LocationNo != FixType.Station.StationOut_BigItem.ToString() && o.LocationNo != FixType.Station.StationOut_Normal.ToString()
                                                                                       && o.LocationNo != FixType.Station.StationIn_BigItem.ToString() && o.LocationNo != FixType.Station.StationIn_Normal.ToString()
                                                                                       && o.LocationNo != FixType.Station.StationEmpty.ToString()
                                                                      ).ToList();
            var resultList = new
            {
                rows = ContainerDetailModelList,
                total = pagination.total,
                page = pagination.page,
                records = pagination.records
            };
            return Content(resultList.ToJson());
        }
        #endregion

        #region 自动选择出库货位（取样质检）
        [HttpPost]
        [HandlerAjaxOnly]
        [ValidateAntiForgeryToken]
        public ActionResult AutoSelect_QA(string qaDetailID, string handChooseListStr)
        {
            using (var db = new RepositoryBase().BeginTrans())
            {
                LogObj logObj = new LogObj();
                logObj.Path = "QARecordController.AutoSelect_QA";
                logObj.Parms = new { qaDetailID = qaDetailID };
                LogEntity logEntity = new LogEntity();
                logEntity.F_ModuleName = "质检指定货位分配";
                logEntity.F_Type = DbLogType.Submit.ToString();
                logEntity.F_Account = OperatorProvider.Provider.GetCurrent().UserCode;
                logEntity.F_NickName = OperatorProvider.Provider.GetCurrent().UserName;
                logEntity.F_Description = "自动出库货位分配（取样质检)";
                logEntity.F_Path = logObj.Path;
                logEntity.F_Param = logObj.Parms.ToJson();

                try
                {
                    /*************************************************/
                    lock (lockObj)
                    {
                        string[] qaDetailIDList = new string[] { qaDetailID };

                        AjaxResult ajaxResult = new AjaxResult();
                        /// 存在波次则删除
                        IList<string> waveIDList = waveDetailApp.FindList(o => o.OutBoundDetailID == qaDetailID).Select(o => o.WaveID).Distinct().ToList();
                        T_QARecordApp qaRecApp = new T_QARecordApp();

                        T_WaveEntity wave = db.FindList<T_WaveEntity>(o => waveIDList.Contains(o.F_Id) && (o.State == "New")).FirstOrDefault();
                        if (wave == null)
                        {
                            wave = db.FindList<T_WaveEntity>(o => waveIDList.Contains(o.F_Id) && (o.State == "Execing")).FirstOrDefault();
                            if (wave != null) return Error("波次已执行", "");
                        }
                        else
                        {
                            ajaxResult = qaRecApp.WaveDel_QA(db, wave.F_Id);
                            if ((ResultType)ajaxResult.state != ResultType.success)
                            {
                                throw new Exception(ajaxResult.message);
                            }
                        }

                        T_QADetailEntity detail = db.FindEntity<T_QADetailEntity>(o => o.F_Id == qaDetailID);
                        if (detail.State == "Outing") return Error("单据已处于出库中", "");
                        if (detail.State == "Over") return Error("单据明细已完成", "");

                        detail.State = "Waved";
                        detail.ActionType = "Equ";
                        db.Update(detail);
                        db.SaveChanges();

                        T_QAEntity qaHead = db.FindEntity<T_QAEntity>(o => o.F_Id == detail.QAID);
                        IList<T_QADetailEntity> notWaveList = db.FindList<T_QADetailEntity>(o => o.QAID == qaHead.F_Id && o.State == "New").ToList();
                        if (notWaveList.Count < 1) //不存在新建状态的  
                        {
                            qaHead.State = "New";
                        }
                        else
                        {
                            qaHead.State = "WavedPart";
                        }
                        db.Update<T_QAEntity>(qaHead);
                        db.SaveChanges();

                        string waveType;
                        IList<ContainerDetailModel> handChooseList = handChooseListStr.ToObject<IList<ContainerDetailModel>>();

                        /// 判断自动还是手动
                        if (handChooseList == null || handChooseList.Count < 1) waveType = "Auto";
                        else
                        {
                            if (handChooseList.Sum(o => o.HandQty) == detail.SampleSumNum) waveType = "Hand";
                            else if (handChooseList.Sum(o => o.HandQty) > detail.SampleSumNum) return Error("数量超过需求数量", "");
                            else waveType = "Mix";
                        }

                        ajaxResult = qaRecApp.WaveGen_QA(db, waveType, handChooseList, qaDetailIDList, true); /// 取样质检，按抽样数量

                        if ((ResultType)ajaxResult.state == ResultType.success)
                        {
                            db.CommitWithOutRollBack();
                        }
                        else
                        {
                            db.RollBack();

                            logObj.Message = ajaxResult.message;
                            LogFactory.GetLogger().Error(logObj);

                            logEntity.F_Result = false;
                            logEntity.F_Msg = ajaxResult.ToJson();
                            new LogApp().WriteDbLog(logEntity);

                            return Error("操作失败:" + ajaxResult.message, ajaxResult.ToJson());
                        }
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

        #region 自动选择出库货位（外观质检）
        [HttpPost]
        [HandlerAjaxOnly]
        [ValidateAntiForgeryToken]
        public ActionResult AutoSelect_QA_CntBar(string qaDetailID, string handChooseListStr)
        {
            using (var db = new RepositoryBase().BeginTrans())
            {
                LogObj logObj = new LogObj();
                logObj.Path = "QARecordController.AutoSelect_QA_CntBar";
                logObj.Parms = new { qaDetailID = qaDetailID };
                LogEntity logEntity = new LogEntity();
                logEntity.F_ModuleName = "质检指定货位分配";
                logEntity.F_Type = DbLogType.Submit.ToString();
                logEntity.F_Account = OperatorProvider.Provider.GetCurrent().UserCode;
                logEntity.F_NickName = OperatorProvider.Provider.GetCurrent().UserName;
                logEntity.F_Description = "自动出库货位分配(外观质检)";
                logEntity.F_Path = logObj.Path;
                logEntity.F_Param = logObj.Parms.ToJson();

                try
                {
                    /*************************************************/
                    lock (lockObj)
                    {
                        string[] qaDetailIDList = new string[] { qaDetailID };

                        AjaxResult ajaxResult = new AjaxResult();
                        /// 存在波次则删除
                        IList<string> waveIDList = waveDetailApp.FindList(o => o.OutBoundDetailID == qaDetailID).Select(o => o.WaveID).Distinct().ToList();
                        T_QARecordApp qaRecApp = new T_QARecordApp();

                        T_WaveEntity wave = db.FindList<T_WaveEntity>(o => waveIDList.Contains(o.F_Id) && (o.State == "New")).FirstOrDefault();
                        if (wave == null)
                        {
                            wave = db.FindList<T_WaveEntity>(o => waveIDList.Contains(o.F_Id) && (o.State == "Execing")).FirstOrDefault();
                            if (wave != null)
                            {
                                return Error("波次已执行", "");
                            }
                        }
                        else
                        {
                            ajaxResult = qaRecApp.WaveDel_QA(db, wave.F_Id);
                            if ((ResultType)ajaxResult.state != ResultType.success)
                            {
                                throw new Exception(ajaxResult.message);
                            }
                        }

                        T_QADetailEntity detail = db.FindEntity<T_QADetailEntity>(o => o.F_Id == qaDetailID);
                        if (detail.State == "Outing")
                        {
                            return Error("单据已处于出库中", "");
                        }
                        if (detail.State == "Over")
                        {
                            return Error("单据明细已完成", "");
                        }
                        detail.State = "Waved";
                        detail.ActionType = "Equ";
                        db.Update(detail);
                        db.SaveChanges();

                        T_QAEntity qaHead = db.FindEntity<T_QAEntity>(o => o.F_Id == detail.QAID);
                        IList<T_QADetailEntity> notWaveList = db.FindList<T_QADetailEntity>(o => o.QAID == qaHead.F_Id && o.State == "New").ToList();
                        if (notWaveList.Count < 1) //不存在新建状态的  
                        {
                            qaHead.State = "New";
                        }
                        else
                        {
                            qaHead.State = "WavedPart";
                        }
                        db.Update<T_QAEntity>(qaHead);
                        db.SaveChanges();

                        string waveType;
                        IList<ContainerDetailModel> handChooseList = handChooseListStr.ToObject<IList<ContainerDetailModel>>();
                        if (handChooseList == null || handChooseList.Count < 1)
                        {
                            waveType = "Auto";
                        }
                        else
                        {
                            if (handChooseList.Count == detail.SampleSumCnt) waveType = "Hand";  /// 统计所选库存明细的个数
                            else if (handChooseList.Count > detail.SampleSumCnt)
                            {
                                return Error("标签个数超过需求数量", "");
                            }
                            else
                            {
                                waveType = "Mix";
                            }
                        }

                        ajaxResult = qaRecApp.WaveGen_QA_CntBar(db, waveType, handChooseList, qaDetailIDList, true);    /// 外观质检，按标签个数

                        if ((ResultType)ajaxResult.state == ResultType.success)
                        {
                            db.CommitWithOutRollBack();
                        }
                        else
                        {
                            db.RollBack();

                            logObj.Message = ajaxResult.message;
                            LogFactory.GetLogger().Error(logObj);

                            logEntity.F_Result = false;
                            logEntity.F_Msg = ajaxResult.ToJson();
                            new LogApp().WriteDbLog(logEntity);

                            return Error("操作失败:" + ajaxResult.message, ajaxResult.ToJson());
                        }
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

        #region 波次删除（界面）
        [HttpPost]
        [HandlerAjaxOnly]
        [ValidateAntiForgeryToken]
        public ActionResult ClearWave_QA(string qaDetailID)
        {
            using (var db = new RepositoryBase().BeginTrans())
            {
                LogObj logObj = new LogObj();
                logObj.Path = "QARecordController.ClearWave_QA";
                logObj.Parms = new { qaDetailID = qaDetailID };
                LogEntity logEntity = new LogEntity();
                logEntity.F_ModuleName = "波次";
                logEntity.F_Type = DbLogType.Submit.ToString();
                logEntity.F_Account = OperatorProvider.Provider.GetCurrent().UserCode;
                logEntity.F_NickName = OperatorProvider.Provider.GetCurrent().UserName;
                logEntity.F_Description = "清除波次";
                logEntity.F_Path = logObj.Path;
                logEntity.F_Param = logObj.Parms.ToJson();
                try
                {
                    /*************************************************/
                    lock (lockObj)
                    {
                        AjaxResult ajaxResult = new AjaxResult();
                        //存在波次则删除
                        IList<string> waveIDList = db.FindList<T_WaveDetailEntity>(o => o.OutBoundDetailID == qaDetailID).Select(o => o.WaveID).Distinct().ToList();
                        T_WaveEntity waveDetail = db.FindList<T_WaveEntity>(o => waveIDList.Contains(o.F_Id) && (o.State == "New")).FirstOrDefault();
                        if (waveDetail == null)
                        {
                            waveDetail = db.FindList<T_WaveEntity>(o => waveIDList.Contains(o.F_Id) && (o.State == "Execing")).FirstOrDefault();
                            if (waveDetail == null) return Error("没有新建的波次", "");
                            else return Error("波次已执行", "");
                        }
                        T_QARecordApp qaRecApp = new T_QARecordApp();
                        ajaxResult = qaRecApp.WaveDel_QA(db, waveDetail.F_Id);
                        if ((ResultType)ajaxResult.state != ResultType.success)
                        {
                            return Error(ajaxResult.message, "");
                        }


                        T_QADetailEntity detail = db.FindEntity<T_QADetailEntity>(o => o.F_Id == qaDetailID);
                        if (detail.State == "Outing" || detail.State == "Over") return Error("单据已处于出库中", "");
                        if (detail.State == "Over") return Error("单据明细已完成", "");

                        detail.State = "New";
                        db.Update(detail);
                        db.SaveChanges();

                        T_QAEntity qaHead = db.FindEntity<T_QAEntity>(o => o.F_Id == detail.QAID);
                        IList<T_QADetailEntity> notWaveList = db.FindList<T_QADetailEntity>(o => o.QAID == qaHead.F_Id && o.State != "New").ToList();
                        if (notWaveList.Count < 1) //全部为新建
                        {
                            qaHead.State = "New";
                        }
                        else
                        {
                            qaHead.State = "WavedPart";
                        }
                        db.Update<T_QAEntity>(qaHead);

                        db.CommitWithOutRollBack();
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

        #region 获取质检记录列表
        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetQARecordGridJson(Pagination pagination, string qAID, string qaDetailID, string keyword)
        {
            IList<T_QARecordEntity> qaRecordEntityList = qaRecordApp.GetList(pagination, qaDetailID, keyword);
            if (qaRecordEntityList.Count == 0)
            {
                T_QADetailEntity qADetailBack = qaDetailApp.FindEntity(o => o.F_Id == qaDetailID);
                T_QAEntity qAEntityBack = qaApp.FindEntity(o => o.F_Id == qAID);

                T_QAEntity qAEntityGet = qaApp.FindEntity(o => o.QACode == qAEntityBack.RefOrderCode && o.QAOrderType == "GetSample");
                if (qAEntityGet == null) return Error($"未找到对应取样单{qAEntityBack.RefOrderCode}", "");

                T_QADetailEntity qADetailGet = qaDetailApp.FindEntity(o => o.QAID == qAEntityGet.F_Id && o.ItemID == qADetailBack.ItemID && o.Lot == qADetailBack.Lot);
                qaRecordEntityList = qaRecordApp.GetList(pagination, qADetailGet.F_Id, keyword);
            }

            IList<T_StationEntity> outStationList = stationApp.FindList(o => true).ToList();
            IList<ItemsDetailEntity> sysitemList = itemsDetailApp.FindEnum<T_QARecordEntity>(o => o.State).ToList();
            IList<QARecordModel> qaRecordModelList = qaRecordEntityList.ToObject<IList<QARecordModel>>();
            foreach (QARecordModel entity in qaRecordModelList)
            {
                if (entity.IsItemMark == "false" && entity.ItemBarCode == entity.BarCode) entity.ItemBarCode = ""; /// 未贴标物料不显示标签条码 

                if (entity.IsAppearQA == "true") /// 外观质检
                {
                    if (entity.State == "WaitPick") entity.StateName = "待外观质检";
                    else if (entity.State == "Picking") entity.StateName = "外观质检中";
                    else if (entity.State == "Picked") entity.StateName = "已外观质检";
                    else entity.StateName = sysitemList.FirstOrDefault(o => o.F_ItemCode == entity.State).F_ItemName;
                }
                else entity.StateName = sysitemList.FirstOrDefault(o => o.F_ItemCode == entity.State).F_ItemName;
            }
            var resultList = new
            {
                rows = qaRecordModelList,
                total = pagination.total,
                page = pagination.page,
                records = pagination.records
            };

            return Content(resultList.ToJson());
        }
        #endregion
    }
}
