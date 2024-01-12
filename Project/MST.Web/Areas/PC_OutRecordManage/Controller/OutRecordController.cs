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
namespace MST.Web.Areas.PC_OutRecordManage.Controllers
{
    public class OutRecordController : ControllerBase
    {
        private T_OutRecordApp outRecordApp = new T_OutRecordApp();
        private ItemsDetailApp itemsDetailApp = new ItemsDetailApp();
        private T_AreaApp areaApp = new T_AreaApp();
        private T_SupplierApp supplierApp = new T_SupplierApp();
        private T_ContainerTypeApp containerTypeApp = new T_ContainerTypeApp();
        private T_ContainerDetailApp containerDetailApp = new T_ContainerDetailApp();
        private T_ContainerApp containerApp = new T_ContainerApp();
        private T_OutBoundDetailApp outBoundDetailApp = new T_OutBoundDetailApp();
        private T_OutBoundApp outBoundApp = new T_OutBoundApp();
        private T_WaveDetailApp waveDetailApp = new T_WaveDetailApp();
        private T_WaveApp waveApp = new T_WaveApp();
        private T_StationApp stationApp = new T_StationApp();
        private static object lockObj = new object();


        [HttpGet]
        [HandlerAuthorize]
        public virtual ActionResult OutDetail()
        {
            return View();
        }

        #region 获取自动获取时选取列表
        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetGridJson(Pagination pagination, string outBoundDetailID, string keyword)
        {
            T_OutBoundDetailEntity detail = outBoundDetailApp.FindEntity(o => o.F_Id == outBoundDetailID);
            T_OutBoundEntity outbound = outBoundApp.FindEntity(o => o.F_Id == detail.OutBoundID);

            string[] areaIDArray = areaApp.FindList(o => o.IsEnable == "true").Select(o=>o.F_Id).ToArray();

            //所有库存信息
            string outBoundType = outbound.OutBoundType;
            IList<T_ContainerDetailEntity> containerDetailEntityList = containerDetailApp.GetOutItemList(pagination, outBoundType, outbound.F_Id, detail.ItemID, detail.Lot, areaIDArray,detail.SourceInOrderCode, keyword);
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
            IList<T_OutRecordEntity> outRecordEntityList = outRecordApp.FindList(o => o.OutBoundDetailID == outBoundDetailID).ToList();
            IList<ItemsDetailEntity> itemDetailList = itemsDetailApp.FindEnum<T_ContainerTypeEntity>(o => o.ContainerKind).ToList();
            foreach (ContainerDetailModel entity in ContainerDetailModelList)
            {
                T_OutRecordEntity outRecordEntity = outRecordEntityList.FirstOrDefault(o => o.OutBoundDetailID == outBoundDetailID && o.ContainerDetailID == entity.F_Id);
                if (outRecordEntity != null)
                {
                    entity.CanUseQty = entity.Qty - entity.OutQty + outRecordEntity.NeedQty;
                    entity.OutQty = outRecordEntity.NeedQty ?? 0;
                }
                else
                {
                    entity.CanUseQty = entity.Qty - entity.OutQty;
                    entity.OutQty = 0;
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


        #region 获取自动获取时选取列表
        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetOutRecordGridJson(Pagination pagination, string outBoundDetailID, string keyword)
        {
            //出库记录信息
            IList<T_OutRecordEntity> outRecordEntityList = outRecordApp.GetList(pagination, outBoundDetailID, keyword).ToList();
            IList<T_StationEntity> outStationList = stationApp.FindList(o => true).ToList();
            IList<ItemsDetailEntity> sysitemList = itemsDetailApp.FindEnum<T_OutRecordEntity>(o => o.State).ToList();
            IList<OutRecordModel> outRecordModelList = outRecordEntityList.ToObject<IList<OutRecordModel>>();
            foreach (OutRecordModel entity in outRecordModelList)
            {
                entity.StateName = sysitemList.FirstOrDefault(o => o.F_ItemCode == entity.State).F_ItemName;
            }

            var resultList = new
            {
                rows = outRecordModelList,
                total = pagination.total,
                page = pagination.page,
                records = pagination.records
            };
            return Content(resultList.ToJson());
        }
        #endregion


        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetFormJson(string keyValue)
        {
            var data = outRecordApp.GetForm(keyValue);
            return Content(data.ToJson());
        }

        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetOutBoundDetail(string outBoundDetailID)
        {
            var data = outBoundDetailApp.FindEntity(o => o.F_Id == outBoundDetailID);
            return Content(data.ToJson());
        }


        #region 自动选择出库货位
        [HttpPost]
        [HandlerAjaxOnly]
        [ValidateAntiForgeryToken]
        public ActionResult AutoSelect(string outBoundDetailID,string handChooseListStr)
        {
            using (var db = new RepositoryBase().BeginTrans())
            {
                LogObj logObj = new LogObj();
                logObj.Path = "OutRecordController.AutoSelect"; //按实际情况修改
                logObj.Parms = new { outBoundDetailID = outBoundDetailID }; //按实际情况修改
                LogEntity logEntity = new LogEntity();
                logEntity.F_ModuleName = "货位分配"; //按实际情况修改
                logEntity.F_Type = DbLogType.Submit.ToString(); //按实际情况修改
                logEntity.F_Account = OperatorProvider.Provider.GetCurrent().UserCode;
                logEntity.F_NickName = OperatorProvider.Provider.GetCurrent().UserName;
                logEntity.F_Description = "自动出库货位分配"; //按实际情况修改
                logEntity.F_Path = logObj.Path;
                logEntity.F_Param = logObj.Parms.ToJson();
                try
                {
                    /*************************************************/
                    lock (lockObj)
                    {
                        string[] outBoundDetailIDList = new string[] { outBoundDetailID };
                       

                        AjaxResult ajaxResult = new AjaxResult();
                        //存在波次则删除
                        IList<string> waveIDList = waveDetailApp.FindList(o => o.OutBoundDetailID == outBoundDetailID).Select(o => o.WaveID).Distinct().ToList();
                        T_OutRecordApp outRecApp = new T_OutRecordApp();

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
                            ajaxResult = outRecApp.WaveDel(db, wave.F_Id);
                            if ((ResultType)ajaxResult.state != ResultType.success)
                            {
                                throw new Exception(ajaxResult.message);
                            }
                        }

                        T_OutBoundDetailEntity detail = db.FindEntity<T_OutBoundDetailEntity>(o => o.F_Id == outBoundDetailID);
                        if(detail.State == "Outing" || detail.State == "Over")
                        {
                            return Error("单据已处于出库中", "");
                        }

                        if (detail.State == "Over")
                        {
                            return Error("单据已完成", "");
                        }

                        if (detail.ActionType == "Hand")
                        {
                            return Error("单据应手动出库", "");
                        }

                        detail.State = "Waved";
                        db.Update(detail);
                        db.SaveChanges();

                        T_OutBoundEntity outBound = db.FindEntity<T_OutBoundEntity>(o => o.F_Id == detail.OutBoundID);
                        IList<T_OutBoundDetailEntity> notWaveList = db.FindList<T_OutBoundDetailEntity>(o =>o.F_Id == outBound.F_Id && o.State == "New").ToList();
                        if (notWaveList.Count < 1) //不存在新建状态的
                        {
                            outBound.State = "Waved";
                        }
                        else
                        {
                            outBound.State = "WavedPart";
                        }
                        db.Update<T_OutBoundEntity>(outBound);
                        db.SaveChanges();

                        string waveType;
                        IList<ContainerDetailModel> handChooseList = handChooseListStr.ToObject<IList<ContainerDetailModel>>();
                        if (handChooseList == null || handChooseList.Count < 1)
                        {
                            waveType = "Auto";
                        }
                        else
                        {
                            if (handChooseList.Sum(o => o.HandQty) == detail.Qty)
                            {
                                waveType = "Hand";
                            }
                            else if (handChooseList.Sum(o => o.HandQty) > detail.Qty)
                            {
                                return Error("数量超过需求数量", "");
                            }
                            else
                            {
                                waveType = "Mix";
                            }
                        }

                        ajaxResult = outRecApp.WaveGen(db, waveType, handChooseList, outBoundDetailIDList, true);

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
        public ActionResult ClearWave(string outBoundDetailID)
        {
            using (var db = new RepositoryBase().BeginTrans())
            {
                LogObj logObj = new LogObj();
                logObj.Path = "OutRecordController.ClearWave"; //按实际情况修改
                logObj.Parms = new { outBoundDetailID = outBoundDetailID }; //按实际情况修改
                LogEntity logEntity = new LogEntity();
                logEntity.F_ModuleName = "波次"; //按实际情况修改
                logEntity.F_Type = DbLogType.Submit.ToString(); //按实际情况修改
                logEntity.F_Account = OperatorProvider.Provider.GetCurrent().UserCode;
                logEntity.F_NickName = OperatorProvider.Provider.GetCurrent().UserName;
                logEntity.F_Description = "清除波次"; //按实际情况修改
                logEntity.F_Path = logObj.Path;
                logEntity.F_Param = logObj.Parms.ToJson();
                try
                {
                    /*************************************************/
                    lock (lockObj)
                    {
                        AjaxResult ajaxResult = new AjaxResult();
                        //存在波次则删除
                        IList<string> waveIDList =db.FindList<T_WaveDetailEntity>(o => o.OutBoundDetailID == outBoundDetailID).Select(o => o.WaveID).Distinct().ToList();
                        T_WaveEntity waveDetail = db.FindList<T_WaveEntity>(o=> waveIDList.Contains(o.F_Id) && (o.State == "New")).FirstOrDefault();
                        if (waveDetail == null)
                        {
                            waveDetail = db.FindList<T_WaveEntity>(o => waveIDList.Contains(o.F_Id) && (o.State == "Execing")).FirstOrDefault();
                            if (waveDetail == null)
                            {
                                return Error("没有新建的波次", "");
                            }
                            else
                            {
                                return Error("波次已执行", "");
                            }
                        }
                        T_OutRecordApp outRecApp = new T_OutRecordApp();
                        ajaxResult = outRecApp.WaveDel(db, waveDetail.F_Id);
                        if ((ResultType)ajaxResult.state != ResultType.success)
                        {
                            return Error(ajaxResult.message, "");
                        }


                        T_OutBoundDetailEntity detail = db.FindEntity<T_OutBoundDetailEntity>(o=>o.F_Id == outBoundDetailID);
                        if (detail.State == "Outing" || detail.State == "Over")
                        {
                            return Error("单据已处于出库中", "");
                        }

                        if (detail.State == "Over")
                        {
                            return Error("单据已完成", "");
                        }
                        detail.State = "New";
                        db.Update(detail);
                        db.SaveChanges();

                        T_OutBoundEntity outBound = db.FindEntity<T_OutBoundEntity>(o=>o.F_Id == detail.OutBoundID);
                        IList<T_OutBoundDetailEntity> notWaveList = db.FindList<T_OutBoundDetailEntity>(o => o.OutBoundID == outBound.F_Id && o.State != "New").ToList();
                        if(notWaveList.Count<1) //全部为新建
                        {
                            outBound.State = "New";
                        }
                        else
                        {
                            outBound.State = "WavedPart";
                        }
                        db.Update<T_OutBoundEntity>(outBound);

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


        #region 菜单中的出货明细
        [HttpGet]
        [HandlerAuthorize]
        public virtual ActionResult MenuRecord()
        {
            return View();
        }

        [HttpGet]
        [HandlerAuthorize]
        public virtual ActionResult MenuDetails()
        {
            return View();
        }

        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetMenuGridJson(Pagination pagination, string outBoundDetailID, string keyword)
        {
            var data = outRecordApp.GetList(pagination, outBoundDetailID, keyword);
            IList<OutRecordModel> outRecordModel = new List<OutRecordModel>();

            IList<ItemsDetailEntity> enumStateList = itemsDetailApp.FindEnum<T_OutRecordEntity>(o => o.State).ToList();
            IList<ItemsDetailEntity> enumTransStateList = itemsDetailApp.FindEnum<T_OutRecordEntity>(o => o.TransState).ToList();
            IList<ItemsDetailEntity> enumOutBoundTypeList = itemsDetailApp.FindEnum<T_OutRecordEntity>(o => o.OutBoundType).ToList();

            foreach (T_OutRecordEntity entity in data)
            {
                OutRecordModel model = entity.ToObject<OutRecordModel>();

                T_OutBoundEntity outbound = outBoundApp.FindEntity(o => o.F_Id == model.OutBoundID);
                model.RefOrderCode = outbound.RefOrderCode;
                model.OutBoundTypeName = enumOutBoundTypeList.FirstOrDefault(o => o.F_ItemCode == entity.OutBoundType).F_ItemName;
                model.StateName = enumStateList.FirstOrDefault(o => o.F_ItemCode == entity.State).F_ItemName;
                model.TransStateName = enumTransStateList.FirstOrDefault(o => o.F_ItemCode == entity.TransState).F_ItemName;
                outRecordModel.Add(model);
            }

            var resultList = new
            {
                rows = outRecordModel,
                total = pagination.total,
                page = pagination.page,
                records = pagination.records
            };

            return Content(resultList.ToJson());
        }

        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetMenuFormJson(string keyValue)
        {
            T_OutRecordEntity data = outRecordApp.GetForm(keyValue);
            OutRecordModel model = data.ToObject<OutRecordModel>();
            IList<SysItemsModel> detailList = itemsDetailApp.FindEnum<T_OutRecordEntity>().ToList();
            T_OutBoundEntity outbound = outBoundApp.FindEntity(o => o.F_Id == model.OutBoundID);
            model.RefOrderCode = outbound.RefOrderCode;
            model.OutBoundTypeName = detailList.FirstOrDefault(o => o.F_EnCode == "OutBoundType").DetailList.FirstOrDefault(o => o.F_ItemCode == model.OutBoundType).F_ItemName;
            model.StateName = detailList.FirstOrDefault(o => o.F_EnCode == "State").DetailList.FirstOrDefault(o => o.F_ItemCode == model.State).F_ItemName;
            model.TransStateName = detailList.FirstOrDefault(o => o.F_EnCode == "TransState").DetailList.FirstOrDefault(o => o.F_ItemCode == model.TransState).F_ItemName;
            return Content(model.ToJson());
        }
        #endregion

    }
}
