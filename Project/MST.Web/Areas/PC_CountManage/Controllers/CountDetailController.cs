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
using System.Web;
using System.Web.Mvc;

namespace MST.Web.Areas.PC_CountManage.Controllers
{
    public class CountDetailController : ControllerBase
    {
        private T_CountApp countApp = new T_CountApp();
        private T_CountDetailApp countDetailApp = new T_CountDetailApp();
        private ItemsDetailApp itemsDetailApp = new ItemsDetailApp();
        private T_ItemApp itemApp = new T_ItemApp();
        private T_ContainerDetailApp containerDetailApp = new T_ContainerDetailApp();
        private T_SupplierApp supplierApp = new T_SupplierApp();
        private T_StationApp stationApp = new T_StationApp();
        private T_ContainerTypeApp containerTypeApp = new T_ContainerTypeApp();

        #region 获取盘点单明细, 维护明细右侧盘点明细列表
        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetGridJson(Pagination pagination, string CountID, string keyword)
        {
            List<T_CountDetailEntity> data = countDetailApp.GetList(pagination, CountID, keyword);
            IList<ItemsDetailEntity> enumCountStateList = itemsDetailApp.FindEnum<T_CountDetailEntity>(o => o.CountState).ToList();
            IList<ItemsDetailEntity> enumAuditStateList = itemsDetailApp.FindEnum<T_CountDetailEntity>(o => o.AuditState).ToList();
            IList<ItemsDetailEntity> enumContainerKindList = itemsDetailApp.FindEnum<T_ContainerTypeEntity>(o => o.ContainerKind).ToList();

            IList<T_StationEntity> stationList = stationApp.GetList();
            IList<T_ItemEntity> itemList = itemApp.GetList();
            IList<T_ContainerTypeEntity> containerTypeList = containerTypeApp.GetList();

            IList<CountDetailModel> detailModelList = data.ToObject<IList<CountDetailModel>>();
            foreach (CountDetailModel model in detailModelList)
            {
                model.StateName = enumCountStateList.FirstOrDefault(o => o.F_ItemCode == model.CountState).F_ItemName;
                model.AuditStateName = enumAuditStateList.FirstOrDefault(o => o.F_ItemCode == model.AuditState).F_ItemName;

                if (!string.IsNullOrEmpty(model.StationID))
                {
                    T_StationEntity station = stationList.FirstOrDefault(o => o.F_Id == model.StationID);
                    model.StationCode = station.StationCode;
                    model.StationName = station.StationName;
                }
                else
                {
                    model.StationCode = "";
                    model.StationName = "";
                }

                T_ItemEntity item = itemList.FirstOrDefault(o => o.F_Id == model.ItemID);
                T_ContainerTypeEntity containerTypeEntity = containerTypeList.FirstOrDefault(o => o.ContainerTypeCode == item.ContainerType);
                model.ContainerKind = containerTypeEntity.ContainerKind;
                model.ContainerKindName = enumContainerKindList.FirstOrDefault(o => o.F_ItemCode == model.ContainerKind).F_ItemName;
                model.IsItemMark = item.IsItemMark;
            }

            var resultList = new
            {
                rows = detailModelList,
                total = pagination.total,
                page = pagination.page,
                records = pagination.records
            };
            return Content(resultList.ToJson());
        }
        #endregion

        #region 查看盘点单明细
        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetFormJson(string keyValue)
        {
            var data = countDetailApp.GetForm(keyValue);
            return Content(data.ToJson());
        }
        #endregion

        #region 获取左侧列表物料数据
        private class CountLeftModel
        {
            public string F_Id { get; set; }
            public string ItemID { get; set; }
            public string ItemCode { get; set; }
            public string ItemName { get; set; }
            public string Factory { get; set; } /// 生产厂家
            public string Lot { get; set; }
            public string Spec {  get; set; }
            public string ItemUnitText { get; set; }
            public DateTime? OverdueDate { get; set; }
            public string SupplierName { get; set; }
            public string SupplierID { get; set; }
            public string SupplierCode { get; set; }
            public decimal? Qty { get; set; }   /// 库存数量
            public int? ContainerCount { get; set; } /// 容器数量
        }
        [HttpGet]
        [HandlerAjaxOnly]
        public ActionResult GetItemList(Pagination pagination,string kindID, string keyValue, string keyword)
        {
            T_CountEntity erpHouse = countApp.FindEntity(o => o.F_Id == keyValue);
            List<T_ContainerDetailEntity> data = containerDetailApp.GetCountItemList(pagination, erpHouse.ERPHouseCode, kindID, keyword);

            /// 过滤系统物料
            IList<string> itemSys = new T_ItemApp().FindList(o => o.IsBase == "true").Select(o => o.F_Id).Distinct().ToArray();
            /// 过滤非存储货位
            IList<string> outLoc = new T_LocationApp().FindList(o => o.State != "Stored").Select(o => o.LocationCode).Distinct().ToArray();
          
            List<CountLeftModel> modelList = data.Where(o => !itemSys.Contains(o.ItemID) && !outLoc.Contains(o.LocationNo)  /// 过滤系统数据 && 过滤货位【非存储】库存
                  && o.State != "Freeze" /// 过滤库存冻结物料 
                  //&& (o.CheckState == "UnNeed" || o.CheckState == "Qua")  /// 合格 && 免检
                  && o.LocationNo != FixType.Station.StationOut_BigItem.ToString() && o.LocationNo != FixType.Station.StationOut_Normal.ToString()    /// 过滤站台物料
                  && o.IsCheckFreeze == "false" && o.IsCountFreeze == "false")   /// 过滤质检冻结、盘点冻结物料
                .GroupBy(o => new { o.ItemID, o.ItemCode, o.ItemName, o.Lot, o.Spec, o.ItemUnitText, o.OverdueDate })
                .Select(o => new CountLeftModel
                {
                    F_Id = o.Key.ItemID + o.Key.Lot,
                    ItemID = o.Key.ItemID,
                    ItemCode = o.Key.ItemCode,
                    ItemName = o.Key.ItemName,
                    Factory = o.FirstOrDefault().Factory,
                    Lot = o.Key.Lot,
                    Spec = o.Key.Spec,
                    ItemUnitText = o.Key.ItemUnitText,
                    OverdueDate = o.Key.OverdueDate,
                    SupplierID = o.FirstOrDefault().SupplierID,
                    SupplierCode = o.FirstOrDefault().SupplierCode,
                    SupplierName = o.FirstOrDefault().SupplierName,
                    Qty = o.Sum(k => k.Qty),
                    ContainerCount = o.Count()
                }).ToList();

            return Content(modelList.ToJson());
        }
        #endregion

        #region 穿梭窗口
        [HttpGet]
        [HandlerAuthorize]
        public virtual ActionResult TransferForm()
        {
            return View();
        }
        #endregion

        #region PC保存右侧选中列表
        [HttpPost]
        [HandlerAjaxOnly]
        [ValidateAntiForgeryToken]
        public ActionResult SubmitFormList(string CountDetailEntityListStr, string CountID)
        {
            using (var db = new RepositoryBase().BeginTrans())
            {
                LogObj logObj = new LogObj();
                logObj.Path = "CountDetailController.SubmitFormList";
                logObj.Parms = new { CountDetailEntity = CountDetailEntityListStr, keyValue = CountID };

                LogEntity logEntity = new LogEntity();
                logEntity.F_ModuleName = "盘点单明细";
                logEntity.F_Type = DbLogType.Submit.ToString();
                logEntity.F_Account = OperatorProvider.Provider.GetCurrent().UserCode;
                logEntity.F_NickName = OperatorProvider.Provider.GetCurrent().UserName;
                logEntity.F_Description = "保存盘点单明细";
                logEntity.F_Path = logObj.Path;
                logEntity.F_Param = logObj.Parms.ToJson();

                try
                {
                    /*************************************************/
                    T_CountEntity countEntity = countApp.FindEntity(o => o.F_Id == CountID);
                    if (countEntity.State != "New") return Error("单据不是新建状态", "");

                    IList<CountDetailModel> CountDetailList = CountDetailEntityListStr.ToObject<IList<CountDetailModel>>();
                    string[] arrayInPage = CountDetailList.Select(o => o.ItemID + o.Lot).ToArray();

                    IList<T_CountDetailEntity> CountDetailInDB = db.FindList<T_CountDetailEntity>(o => o.CountID == CountID);
                    string[] arrayInDB = CountDetailInDB.Select(o => o.ItemID + o.Lot).ToArray();

                    IList<T_CountDetailEntity> needDelList = db.FindList<T_CountDetailEntity>(o => (!arrayInPage.Contains(o.ItemID + o.Lot)) && o.CountID == CountID);
                    IList<CountDetailModel> needInsertList = CountDetailList.Where(o => !arrayInDB.Contains(o.ItemID + o.Lot)).ToList();

                    foreach (CountDetailModel entity in needInsertList)
                    {
                        T_CountDetailEntity countDetail = new T_CountDetailEntity();
                        countDetail.CountID = CountID;
                        T_ItemEntity item = itemApp.FindEntity(o => o.F_Id == entity.ItemID);
                        countDetail.ItemCode = item.ItemCode;
                        countDetail.ItemName = item.ItemName;
                        countDetail.ItemID = item.F_Id;
                        countDetail.Factory = item.Factory;
                        countDetail.Lot = entity.Lot;
                        countDetail.Spec = entity.Spec;
                        countDetail.ItemUnitText = entity.ItemUnitText;
                        countDetail.OverdueDate = entity.OverdueDate;
                        countDetail.SEQ = entity.SEQ;
                        countDetail.Qty = 0;
                        countDetail.CountQty = 0;
                        T_SupplierEntity supplier = supplierApp.GetForm(entity.SupplierUserID);
                        countDetail.SupplierUserID = supplier.F_Id;
                        countDetail.SupplierUserName = supplier.SupplierName;
                        countDetail.F_DeleteMark = false;

                        T_ContainerTypeEntity containerType = containerTypeApp.FindEntity(o => o.ContainerTypeCode == item.ContainerType);
                        string containerKind = containerType.ContainerKind;
                        if (containerKind == "Rack")
                        {
                            T_StationEntity station = stationApp.FindEntity(o => o.StationCode == FixType.Station.StationOut_BigItem.ToString() && o.UseCode.Contains("CountOut"));
                            countDetail.StationID = station.F_Id;
                        }
                        else if (containerKind == "Plastic")
                        {
                            T_StationEntity station = stationApp.FindEntity(o => o.StationCode == FixType.Station.StationOut_Normal.ToString() && o.UseCode.Contains("CountOut"));
                            countDetail.StationID = station.F_Id;
                        }
                        else if (containerKind == "Box")
                        {
                            // 纸箱 在库
                        }
                        else return Error("物料的容器类型未知:" + item.ItemCode, "");

                        countDetail.F_Id = Guid.NewGuid().ToString();
                        countDetail.CountState = "New";
                        countDetail.AuditState = "WaitAudit";
                        db.Insert<T_CountDetailEntity>(countDetail);
                    }

                    foreach (T_CountDetailEntity delcell in needDelList)
                    {
                        db.Delete<T_CountDetailEntity>(delcell);
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

        #region 新建/修改盘点明细
        [HttpPost]
        [HandlerAjaxOnly]
        [ValidateAntiForgeryToken]
        public ActionResult SubmitForm(T_CountDetailEntity CountDetailEntity, string keyValue)
        {
            LogObj logObj = new LogObj();
            logObj.Path = "CountDetailController.SubmitForm";
            logObj.Parms = new { CountDetailEntity = CountDetailEntity, keyValue = keyValue };

            LogEntity logEntity = new LogEntity();
            logEntity.F_ModuleName = "盘点明细";
            logEntity.F_Type = DbLogType.Submit.ToString();
            logEntity.F_Account = OperatorProvider.Provider.GetCurrent().UserCode;
            logEntity.F_NickName = OperatorProvider.Provider.GetCurrent().UserName;
            logEntity.F_Description = "新建/修改盘点明细";
            logEntity.F_Path = logObj.Path;
            logEntity.F_Param = logObj.Parms.ToJson();

            try
            {
                T_ItemEntity item = itemApp.FindEntity(o => o.F_Id == CountDetailEntity.ItemID);
                T_ContainerTypeEntity containerType = containerTypeApp.FindEntity(o => o.ContainerTypeCode == item.ContainerType);
                string containerKind = containerType.ContainerKind;
                if (containerKind == "Rack")
                {
                    T_StationEntity station = stationApp.FindEntity(o => o.StationCode == FixType.Station.StationOut_BigItem.ToString());
                    CountDetailEntity.StationID = station.F_Id;
                }
                else if (containerKind == "Plastic")
                {
                    T_StationEntity t_Station = stationApp.FindEntity(o => o.StationCode == FixType.Station.StationOut_Normal.ToString());
                    CountDetailEntity.StationID = t_Station.F_Id;
                }
                else if (containerKind == "Box")
                {
                    // 纸箱
                }
                else return Error("未知的容器类型", "");

                countDetailApp.SubmitForm(CountDetailEntity, keyValue);


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

        #region 删除盘点明细
        [HttpPost]
        [HandlerAjaxOnly]
        [HandlerAuthorize]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteForm(string keyValue)
        {
            LogObj logObj = new LogObj();
            logObj.Path = "CountDetailController.DeleteForm";
            logObj.Parms = new { keyValue = keyValue };

            LogEntity logEntity = new LogEntity();
            logEntity.F_ModuleName = "盘点单明细";
            logEntity.F_Type = DbLogType.Delete.ToString();
            logEntity.F_Account = OperatorProvider.Provider.GetCurrent().UserCode;
            logEntity.F_NickName = OperatorProvider.Provider.GetCurrent().UserName;
            logEntity.F_Description = "删除盘点单明细";
            logEntity.F_Path = logObj.Path;
            logEntity.F_Param = logObj.Parms.ToJson();

            try
            {
                T_CountDetailEntity data = countDetailApp.GetForm(keyValue);
                if (data.CountState != "New") return Error("非新建状态不可删除", "");

                countDetailApp.DeleteForm(keyValue);

                logObj.Message = "删除成功";
                LogFactory.GetLogger().Info(logObj);

                logEntity.F_Result = true;
                new LogApp().WriteDbLog(logEntity);

                return Success("删除成功。");
            }
            catch (Exception ex)
            {
                logObj.Message = ex;
                LogFactory.GetLogger().Error(logObj);

                logEntity.F_Result = false;
                logEntity.F_Msg = ex.ToJson();
                new LogApp().WriteDbLog(logEntity);

                return Error("删除失败。", ex.ToJson());
            }
        }
        #endregion
    }
}
